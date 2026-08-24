using System.Security.Claims;
using OnKashFinance.API.Dados;
using OnKashFinance.API.Modelos;

namespace OnKashFinance.API.Seguranca;

public class AuditoriaOperacoesMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditoriaOperacoesMiddleware> _logger;
    public AuditoriaOperacoesMiddleware(RequestDelegate next, ILogger<AuditoriaOperacoesMiddleware> logger)
    { _next = next; _logger = logger; }

    public async Task InvokeAsync(HttpContext context, OnKashDbContext db)
    {
        await _next(context);
        if (context.User.Identity?.IsAuthenticated != true || !DeveAuditar(context.Request)) return;
        try
        {
            Guid? usuarioId = Guid.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : null;
            Guid? empresaId = Guid.TryParse(context.User.FindFirstValue("empresa_id"), out var eid) ? eid : null;
            db.AuditoriasOperacoes.Add(new AuditoriaOperacao
            {
                UsuarioId = usuarioId, EmpresaId = empresaId,
                Metodo = context.Request.Method,
                Caminho = Limitar(context.Request.Path.Value ?? "/", 300),
                StatusHttp = context.Response.StatusCode,
                EnderecoIp = Limitar(context.Connection.RemoteIpAddress?.ToString() ?? string.Empty, 64),
                AgenteUsuario = Limitar(context.Request.Headers.UserAgent.ToString() ?? string.Empty, 300),
                CriadoEm = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }
        catch (Exception ex) { _logger.LogError(ex, "Não foi possível registrar a auditoria da operação."); }
    }
    private static bool DeveAuditar(HttpRequest request) =>
        request.Method is "POST" or "PUT" or "PATCH" or "DELETE" || request.Path.StartsWithSegments("/api/privacidade/exportacao");
    private static string Limitar(string valor, int limite) => valor.Length <= limite ? valor : valor[..limite];
}
