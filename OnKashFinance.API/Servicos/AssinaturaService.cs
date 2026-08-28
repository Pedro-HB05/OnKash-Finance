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
        var empresarial = empresaId.HasValue;
        var plano = NormalizarPlano(assinatura?.Plano, empresarial);
        var inicioMes = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var fimMes = inicioMes.AddMonths(1);

        long contas;
        long lancamentos;
        long terceiroIndicador;
        long anexosBytes;
        if (empresarial)
        {
            contas = await _db.ContasEmpresariais.LongCountAsync(x => x.EmpresaId == empresaId);
            lancamentos = await _db.LancamentosEmpresariais.LongCountAsync(x => x.EmpresaId == empresaId && x.Data >= inicioMes && x.Data < fimMes);
            terceiroIndicador = await _db.EmpresaUsuarios.LongCountAsync(x => x.EmpresaId == empresaId && x.Ativo);
            anexosBytes = await _db.AnexosFinanceiros.Where(x => x.Ambiente == "EMPRESARIAL" && x.ProprietarioId == empresaId).SumAsync(x => (long?)x.Tamanho) ?? 0;
        }
        else
        {
            contas = await _db.ContasPessoais.LongCountAsync(x => x.UsuarioId == usuarioId);
            lancamentos = await _db.LancamentosPessoais.LongCountAsync(x => x.UsuarioId == usuarioId && x.Data >= inicioMes && x.Data < fimMes);
            terceiroIndicador = await _db.CartoesPessoais.LongCountAsync(x => x.UsuarioId == usuarioId && x.Ativo);
            anexosBytes = await _db.AnexosFinanceiros.Where(x => x.Ambiente == "PESSOAL" && x.ProprietarioId == usuarioId).SumAsync(x => (long?)x.Tamanho) ?? 0;
        }

        var limites = Limites(plano, empresarial);
        var pendente = await _db.SolicitacoesUpgrade.AnyAsync(x => x.UsuarioId == titularId && x.Status == "PENDENTE");
        return new AssinaturaResumoResposta
        {
            Plano = plano,
            NomePlano = NomePlano(plano, empresarial),
            Status = assinatura?.Status ?? "ATIVA",
            PeriodoAtualFim = assinatura?.PeriodoAtualFim,
            PossuiSolicitacaoPendente = pendente,
            Uso =
            [
                new("CONTAS", "Contas financeiras", contas, limites.Contas, "contas"),
                new("LANCAMENTOS", "Lançamentos neste mês", lancamentos, limites.Lancamentos, "lançamentos"),
                empresarial
                    ? new("USUARIOS", "Usuários da equipe", terceiroIndicador, limites.TerceiroIndicador, "usuários")
                    : new("CARTOES", "Cartões ativos", terceiroIndicador, limites.TerceiroIndicador, "cartões"),
                new("ARMAZENAMENTO", "Comprovantes armazenados", anexosBytes / 1024 / 1024, limites.ArmazenamentoMb, "MB")
            ],
            Planos = CriarOfertas(plano, empresarial)
        };
    }

    public async Task SolicitarUpgradeAsync(string planoDesejado)
    {
        var plano = planoDesejado.Trim().ToUpperInvariant();
        var usuarioId = _usuarioAtual.ObterUsuarioId();
        var empresaId = _usuarioAtual.ObterEmpresaId();
        var empresarial = empresaId.HasValue;
        var planosPermitidos = empresarial
            ? new[] { "EMPRESA_GESTAO", "EMPRESA_PRO" }
            : new[] { "PESSOAL_PLUS", "PESSOAL_PREMIUM" };

        if (!planosPermitidos.Contains(plano))
            throw new InvalidOperationException(
                empresarial
                    ? "Escolha um plano empresarial válido."
                    : "Escolha um plano pessoal válido.");

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

    private static string NormalizarPlano(string? plano, bool empresarial) => (plano ?? "GRATUITO").ToUpperInvariant() switch
    {
        "GRATUITO" => empresarial ? "EMPRESA_GRATIS" : "PESSOAL_GRATIS",
        "PRO" => empresarial ? "EMPRESA_GESTAO" : "PESSOAL_PLUS",
        "BUSINESS" => empresarial ? "EMPRESA_PRO" : "PESSOAL_PREMIUM",
        var atual => atual
    };

    private static string NomePlano(string plano, bool empresarial) => plano switch
    {
        "PESSOAL_PLUS" => "Pessoal Plus",
        "PESSOAL_PREMIUM" => "Pessoal Premium",
        "EMPRESA_GESTAO" => "Empresa Gestão",
        "EMPRESA_PRO" => "Empresa Pro",
        _ => empresarial ? "Empresa Essencial" : "Pessoal Essencial"
    };

    private static (long? Contas, long? Lancamentos, long? TerceiroIndicador, long? ArmazenamentoMb)
        Limites(string plano, bool empresarial)
    {
        if (empresarial)
        {
            return plano switch
            {
                "EMPRESA_GESTAO" => (10, 5000, 3, 500),
                "EMPRESA_PRO" => (null, null, 10, 2048),
                _ => (2, 300, 1, 25)
            };
        }

        return plano switch
        {
            "PESSOAL_PLUS" => (10, 2000, 5, 200),
            "PESSOAL_PREMIUM" => (null, null, null, 1024),
            _ => (2, 150, 2, 10)
        };
    }

    private static IReadOnlyList<PlanoOfertaResposta> CriarOfertas(string atual, bool empresarial)
    {
        if (empresarial)
        {
            return
            [
                new("EMPRESA_GRATIS", "Empresa Essencial", "Para iniciar a organização financeira do negócio.", atual == "EMPRESA_GRATIS", true, false,
                    ["Até 2 contas financeiras", "300 lançamentos por mês", "Contas a pagar e receber", "Clientes e fornecedores", "1 usuário e 25 MB para comprovantes"]),
                new("EMPRESA_GESTAO", "Empresa Gestão", "Controle, análise e colaboração para empresas em operação.", atual == "EMPRESA_GESTAO", false, true,
                    ["Até 10 contas financeiras", "5.000 lançamentos por mês", "DRE, projeção e importação bancária", "Até 3 usuários", "500 MB para comprovantes"]),
                new("EMPRESA_PRO", "Empresa Pro", "Escala, governança e controle avançado para equipes.", atual == "EMPRESA_PRO", false, false,
                    ["Contas e lançamentos ilimitados", "Até 10 usuários", "Permissões individuais por módulo", "Relatórios e inteligência completos", "2 GB para comprovantes"])
            ];
        }

        return
        [
            new("PESSOAL_GRATIS", "Pessoal Essencial", "Para organizar o dia a dia e começar a controlar seu dinheiro.", atual == "PESSOAL_GRATIS", true, false,
                ["Até 2 contas financeiras", "150 lançamentos por mês", "Até 2 cartões e controle de faturas", "Dashboard e relatórios básicos", "10 MB para comprovantes"]),
            new("PESSOAL_PLUS", "Pessoal Plus", "Mais automação e planejamento para uma rotina financeira completa.", atual == "PESSOAL_PLUS", false, true,
                ["Até 10 contas e 5 cartões", "2.000 lançamentos por mês", "Orçamentos e lançamentos recorrentes", "Importação e conciliação bancária", "200 MB para comprovantes"]),
            new("PESSOAL_PREMIUM", "Pessoal Premium", "Visão completa para planejar, antecipar e evoluir suas finanças.", atual == "PESSOAL_PREMIUM", false, false,
                ["Contas, cartões e lançamentos ilimitados", "Projeção financeira e alertas avançados", "Relatórios e exportações completos", "Inteligência financeira", "1 GB para comprovantes"])
        ];
    }
}
