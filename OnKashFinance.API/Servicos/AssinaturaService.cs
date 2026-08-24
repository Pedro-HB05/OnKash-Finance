using Microsoft.EntityFrameworkCore;
using OnKashFinance.API.Autenticacao;
using OnKashFinance.API.Dados;
using OnKashFinance.API.DTOs;
using OnKashFinance.API.Modelos;

namespace OnKashFinance.API.Servicos;

public class AssinaturaService
{
    private readonly OnKashDbContext _db;
    private readonly UsuarioAtualService _usuarioAtual;

    public AssinaturaService(OnKashDbContext db, UsuarioAtualService usuarioAtual)
    {
        _db = db;
        _usuarioAtual = usuarioAtual;
    }

    public async Task<AssinaturaResumoResposta> ObterResumoAsync()
    {
        var usuarioId = _usuarioAtual.ObterUsuarioId();
        var empresaId = _usuarioAtual.ObterEmpresaId();
        var titularId = await ObterTitularIdAsync(usuarioId, empresaId);
        var assinatura = await _db.AssinaturasUsuario.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UsuarioId == titularId);
        var plano = assinatura?.Plano ?? "GRATUITO";
        var inicioMes = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var fimMes = inicioMes.AddMonths(1);

        long contas;
        long lancamentos;
        long usuarios;
        long anexosBytes;
        if (empresaId.HasValue)
        {
            contas = await _db.ContasEmpresariais.LongCountAsync(x => x.EmpresaId == empresaId);
            lancamentos = await _db.LancamentosEmpresariais.LongCountAsync(x => x.EmpresaId == empresaId && x.Data >= inicioMes && x.Data < fimMes);
            usuarios = await _db.EmpresaUsuarios.LongCountAsync(x => x.EmpresaId == empresaId && x.Ativo);
            anexosBytes = await _db.AnexosFinanceiros.Where(x => x.Ambiente == "EMPRESARIAL" && x.ProprietarioId == empresaId).SumAsync(x => (long?)x.Tamanho) ?? 0;
        }
        else
        {
            contas = await _db.ContasPessoais.LongCountAsync(x => x.UsuarioId == usuarioId);
            lancamentos = await _db.LancamentosPessoais.LongCountAsync(x => x.UsuarioId == usuarioId && x.Data >= inicioMes && x.Data < fimMes);
            usuarios = 1;
            anexosBytes = await _db.AnexosFinanceiros.Where(x => x.Ambiente == "PESSOAL" && x.ProprietarioId == usuarioId).SumAsync(x => (long?)x.Tamanho) ?? 0;
        }

        var limites = Limites(plano);
        var pendente = await _db.SolicitacoesUpgrade.AnyAsync(x => x.UsuarioId == titularId && x.Status == "PENDENTE");
        return new AssinaturaResumoResposta
        {
            Plano = plano,
            NomePlano = NomePlano(plano),
            Status = assinatura?.Status ?? "ATIVA",
            PeriodoAtualFim = assinatura?.PeriodoAtualFim,
            PossuiSolicitacaoPendente = pendente,
            Uso =
            [
                new("CONTAS", "Contas financeiras", contas, limites.Contas, "contas"),
                new("LANCAMENTOS", "Lançamentos neste mês", lancamentos, limites.Lancamentos, "lançamentos"),
                new("USUARIOS", "Usuários da equipe", usuarios, limites.Usuarios, "usuários"),
                new("ARMAZENAMENTO", "Comprovantes armazenados", anexosBytes / 1024 / 1024, limites.ArmazenamentoMb, "MB")
            ],
            Planos = CriarOfertas(plano)
        };
    }

    public async Task SolicitarUpgradeAsync(string planoDesejado)
    {
        var plano = planoDesejado.Trim().ToUpperInvariant();
        if (plano is not ("PRO" or "BUSINESS"))
            throw new InvalidOperationException("Escolha um plano válido para receber novidades.");

        var usuarioId = _usuarioAtual.ObterUsuarioId();
        var empresaId = _usuarioAtual.ObterEmpresaId();
        if (empresaId.HasValue && !_usuarioAtual.EhAdministrador())
            throw new InvalidOperationException("Somente o administrador da empresa pode solicitar uma mudança de plano.");
        var titularId = await ObterTitularIdAsync(usuarioId, empresaId);
        var existente = await _db.SolicitacoesUpgrade
            .FirstOrDefaultAsync(x => x.UsuarioId == titularId && x.Status == "PENDENTE");
        if (existente is not null)
        {
            existente.PlanoDesejado = plano;
            existente.AtualizadoEm = DateTimeOffset.UtcNow;
        }
        else
        {
            _db.SolicitacoesUpgrade.Add(new SolicitacaoUpgrade
            {
                UsuarioId = titularId,
                EmpresaId = empresaId,
                PlanoDesejado = plano,
                Status = "PENDENTE"
            });
        }
        await _db.SaveChangesAsync();
    }

    private async Task<Guid> ObterTitularIdAsync(Guid usuarioId, Guid? empresaId)
    {
        if (!empresaId.HasValue) return usuarioId;
        return await _db.EmpresaUsuarios.AsNoTracking()
            .Where(x => x.EmpresaId == empresaId && x.Perfil == PerfilEmpresa.ADMINISTRADOR)
            .OrderBy(x => x.CriadoEm)
            .Select(x => x.UsuarioId)
            .FirstOrDefaultAsync() is var id && id != Guid.Empty ? id : usuarioId;
    }

    private static string NomePlano(string plano) => plano switch
    {
        "PRO" => "Pro",
        "BUSINESS" => "Business",
        _ => "Grátis"
    };

    private static (long? Contas, long? Lancamentos, long? Usuarios, long? ArmazenamentoMb) Limites(string plano) => plano switch
    {
        "PRO" => (10, 2000, 3, 200),
        "BUSINESS" => (null, null, 10, 2048),
        _ => (2, 150, 1, 10)
    };

    private static IReadOnlyList<PlanoOfertaResposta> CriarOfertas(string atual) =>
    [
        new("GRATUITO", "Grátis", "Para organizar as finanças e conhecer a plataforma.", atual == "GRATUITO", true, false,
            ["Até 2 contas", "150 lançamentos por mês", "Relatórios e planejamento", "10 MB para comprovantes"]),
        new("PRO", "Pro", "Mais automação e espaço para uma rotina financeira completa.", atual == "PRO", false, true,
            ["Até 10 contas", "2.000 lançamentos por mês", "Importação e inteligência financeira", "200 MB para comprovantes"]),
        new("BUSINESS", "Business", "Controle financeiro colaborativo para empresas em crescimento.", atual == "BUSINESS", false, false,
            ["Contas e lançamentos ilimitados", "Até 10 usuários", "Permissões por equipe", "2 GB para comprovantes"])
    ];
}
