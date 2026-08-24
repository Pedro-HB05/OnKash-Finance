using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using OnKashFinance.API.Dados;
using OnKashFinance.API.Modelos;

namespace OnKashFinance.API.Seguranca;

public class PermissoesEmpresaMiddleware
{
    private readonly RequestDelegate _next;
    public PermissoesEmpresaMiddleware(RequestDelegate next) => _next = next;
    public async Task InvokeAsync(HttpContext context, OnKashDbContext db)
    {
        if (context.User.Identity?.IsAuthenticated == true && context.Request.Path.StartsWithSegments("/api"))
        {
            if (!Guid.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId) ||
                !await db.Usuarios.AsNoTracking().AnyAsync(x => x.Id == usuarioId && x.Ativo))
            {
                await Negar(context, StatusCodes.Status401Unauthorized, "Sua conta não está ativa."); return;
            }
            if (context.Request.Path.StartsWithSegments("/api/empresarial"))
            {
                if (!Guid.TryParse(context.User.FindFirstValue("empresa_id"), out var empresaId))
                { await Negar(context, StatusCodes.Status401Unauthorized, "Vínculo empresarial inválido."); return; }
                var vinculo = await db.EmpresaUsuarios.AsNoTracking().Include(x => x.Permissoes)
                    .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId && x.EmpresaId == empresaId && x.Ativo && x.Empresa.Ativo);
                if (vinculo is null) { await Negar(context, StatusCodes.Status401Unauthorized, "Seu vínculo empresarial não está ativo."); return; }
                var permissao = Mapear(context.Request.Path.Value ?? string.Empty);
                if (permissao is not null && vinculo.Perfil != PerfilEmpresa.ADMINISTRADOR && !Permitido(vinculo.Permissoes, permissao))
                { await Negar(context, StatusCodes.Status403Forbidden, "Você não possui permissão para acessar este módulo."); return; }
            }
        }
        await _next(context);
    }

    private static bool Permitido(PermissaoEmpresa? p, string permissao) => p is not null && permissao switch
    {
        "DASHBOARD" => p.Dashboard, "LANCAMENTOS" => p.Lancamentos, "CONTAS" => p.Contas,
        "CLIENTES" => p.Clientes, "FORNECEDORES" => p.Fornecedores, "CONTAS_PAGAR" => p.ContasPagar,
        "CONTAS_RECEBER" => p.ContasReceber, "CATEGORIAS" => p.Categorias, "RELATORIOS" => p.Relatorios,
        "USUARIOS" => p.Usuarios, _ => false
    };
    private static async Task Negar(HttpContext context, int status, string mensagem)
    { context.Response.StatusCode = status; await context.Response.WriteAsJsonAsync(new { status, mensagem }); }

    private static string? Mapear(string caminho)
    {
        if (caminho.Contains("/dashboard", StringComparison.OrdinalIgnoreCase)) return "DASHBOARD";
        if (caminho.Contains("/usuarios", StringComparison.OrdinalIgnoreCase)) return "USUARIOS";
        if (caminho.Contains("/clientes", StringComparison.OrdinalIgnoreCase)) return "CLIENTES";
        if (caminho.Contains("/fornecedores", StringComparison.OrdinalIgnoreCase)) return "FORNECEDORES";
        if (caminho.Contains("/contas-a-pagar", StringComparison.OrdinalIgnoreCase)) return "CONTAS_PAGAR";
        if (caminho.Contains("/contas-a-receber", StringComparison.OrdinalIgnoreCase)) return "CONTAS_RECEBER";
        if (caminho.Contains("/categorias", StringComparison.OrdinalIgnoreCase)) return "CATEGORIAS";
        if (caminho.Contains("/relatorios", StringComparison.OrdinalIgnoreCase) || caminho.Contains("/inteligencia", StringComparison.OrdinalIgnoreCase)) return "RELATORIOS";
        if (caminho.Contains("/lancamentos", StringComparison.OrdinalIgnoreCase)) return "LANCAMENTOS";
        if (caminho.Contains("/contas", StringComparison.OrdinalIgnoreCase)) return "CONTAS";
        return null;
    }
}
