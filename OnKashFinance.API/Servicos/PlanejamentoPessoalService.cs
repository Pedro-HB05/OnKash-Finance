using Microsoft.EntityFrameworkCore;
using OnKashFinance.API.Autenticacao;
using OnKashFinance.API.Dados;
using OnKashFinance.API.DTOs;
using OnKashFinance.API.Modelos;

namespace OnKashFinance.API.Servicos;

public class PlanejamentoPessoalService
{
    private readonly OnKashDbContext _db;
    private readonly UsuarioAtualService _usuarioAtual;

    public PlanejamentoPessoalService(OnKashDbContext db, UsuarioAtualService usuarioAtual)
    {
        _db = db;
        _usuarioAtual = usuarioAtual;
    }

    private Guid Usuario()
    {
        if (!_usuarioAtual.EhPessoal()) throw new UnauthorizedAccessException("Função exclusiva do financeiro pessoal.");
        return _usuarioAtual.ObterUsuarioId();
    }

    public async Task<List<OrcamentoResposta>> ListarOrcamentosAsync(DateOnly? mes)
    {
        var usuarioId = Usuario();
        var referencia = new DateOnly((mes ?? DateOnly.FromDateTime(DateTime.Today)).Year, (mes ?? DateOnly.FromDateTime(DateTime.Today)).Month, 1);
        var fim = referencia.AddMonths(1).AddDays(-1);
        var itens = await _db.OrcamentosPessoais.AsNoTracking()
            .Where(x => x.UsuarioId == usuarioId && x.Mes == referencia)
            .OrderBy(x => x.Categoria.Nome).ToListAsync();

        var resposta = new List<OrcamentoResposta>();
        foreach (var item in itens)
        {
            var utilizado = await _db.LancamentosPessoais
                .Where(x => x.UsuarioId == usuarioId && x.CategoriaId == item.CategoriaId && x.Tipo == TipoLancamentoPessoal.SAIDA && !x.Cancelado && x.Data >= referencia && x.Data <= fim)
                .SumAsync(x => (decimal?)x.Valor) ?? 0;
            resposta.Add(new OrcamentoResposta
            {
                Id = item.Id, CategoriaId = item.CategoriaId, Categoria = item.Categoria.Nome,
                Mes = item.Mes, Limite = item.Limite, Utilizado = utilizado,
                Percentual = item.Limite > 0 ? Math.Round(utilizado / item.Limite * 100, 1) : 0
            });
        }
        return resposta;
    }

    public async Task SalvarOrcamentoAsync(SalvarOrcamentoRequest request)
    {
        var usuarioId = Usuario();
        if (request.Limite <= 0) throw new InvalidOperationException("O limite deve ser maior que zero.");
        var mes = new DateOnly(request.Mes.Year, request.Mes.Month, 1);
        var categoriaValida = await _db.CategoriasPessoais.AnyAsync(x => x.Id == request.CategoriaId && (x.UsuarioId == usuarioId || x.UsuarioId == null) && x.Tipo == TipoCategoria.SAIDA && x.Ativo);
        if (!categoriaValida) throw new InvalidOperationException("Selecione uma categoria de saída ativa.");
        var item = await _db.OrcamentosPessoais.FirstOrDefaultAsync(x => x.UsuarioId == usuarioId && x.CategoriaId == request.CategoriaId && x.Mes == mes);
        if (item is null)
        {
            item = new OrcamentoPessoal { UsuarioId = usuarioId, CategoriaId = request.CategoriaId, Mes = mes };
            _db.OrcamentosPessoais.Add(item);
        }
        item.Limite = request.Limite;
        await _db.SaveChangesAsync();
    }

    public async Task ExcluirOrcamentoAsync(Guid id)
    {
        var usuarioId = Usuario();
        var item = await _db.OrcamentosPessoais.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == usuarioId) ?? throw new KeyNotFoundException("Orçamento não encontrado.");
        _db.OrcamentosPessoais.Remove(item);
        await _db.SaveChangesAsync();
    }

    public async Task<List<RecorrenciaResposta>> ListarRecorrenciasAsync()
    {
        var usuarioId = Usuario();
        await ProcessarRecorrenciasAsync(usuarioId);
        return await _db.LancamentosRecorrentesPessoais.AsNoTracking().Where(x => x.UsuarioId == usuarioId)
            .OrderBy(x => x.ProximaExecucao).Select(x => new RecorrenciaResposta
            {
                Id = x.Id, ContaId = x.ContaId, Conta = x.Conta.Nome, CategoriaId = x.CategoriaId,
                Categoria = x.Categoria.Nome, Tipo = x.Tipo, Descricao = x.Descricao, Valor = x.Valor,
                Frequencia = x.Frequencia, ProximaExecucao = x.ProximaExecucao, Ativo = x.Ativo
            }).ToListAsync();
    }

    public async Task SalvarRecorrenciaAsync(Guid? id, SalvarRecorrenciaRequest request)
    {
        var usuarioId = Usuario();
        if (string.IsNullOrWhiteSpace(request.Descricao) || request.Valor <= 0) throw new InvalidOperationException("Informe descrição e valor válido.");
        var frequencias = new[] { "SEMANAL", "MENSAL", "ANUAL" };
        var frequencia = request.Frequencia.Trim().ToUpperInvariant();
        if (!frequencias.Contains(frequencia)) throw new InvalidOperationException("Frequência inválida.");
        if (!await _db.ContasPessoais.AnyAsync(x => x.Id == request.ContaId && x.UsuarioId == usuarioId && x.Ativo)) throw new InvalidOperationException("Conta inválida ou inativa.");
        var tipoCategoria = request.Tipo == TipoLancamentoPessoal.ENTRADA ? TipoCategoria.ENTRADA : TipoCategoria.SAIDA;
        if (!await _db.CategoriasPessoais.AnyAsync(x => x.Id == request.CategoriaId && (x.UsuarioId == usuarioId || x.UsuarioId == null) && x.Ativo && x.Tipo == tipoCategoria)) throw new InvalidOperationException("A categoria deve corresponder ao tipo do lançamento.");
        var item = id.HasValue
            ? await _db.LancamentosRecorrentesPessoais.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == usuarioId) ?? throw new KeyNotFoundException("Recorrência não encontrada.")
            : new LancamentoRecorrentePessoal { UsuarioId = usuarioId };
        if (!id.HasValue) _db.LancamentosRecorrentesPessoais.Add(item);
        item.ContaId = request.ContaId; item.CategoriaId = request.CategoriaId; item.Tipo = request.Tipo;
        item.Descricao = request.Descricao.Trim(); item.Valor = request.Valor; item.Frequencia = frequencia;
        item.ProximaExecucao = request.ProximaExecucao; item.Ativo = request.Ativo;
        await _db.SaveChangesAsync();
    }

    public async Task ExcluirRecorrenciaAsync(Guid id)
    {
        var usuarioId = Usuario();
        var item = await _db.LancamentosRecorrentesPessoais.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == usuarioId) ?? throw new KeyNotFoundException("Recorrência não encontrada.");
        _db.LancamentosRecorrentesPessoais.Remove(item);
        await _db.SaveChangesAsync();
    }

    public async Task<List<AlertaFinanceiroResposta>> ListarAlertasAsync()
    {
        var usuarioId = Usuario();
        await ProcessarRecorrenciasAsync(usuarioId);
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        var alertas = new List<AlertaFinanceiroResposta>();
        var orcamentos = await ListarOrcamentosAsync(hoje);
        foreach (var item in orcamentos.Where(x => x.Percentual >= 80))
            alertas.Add(new AlertaFinanceiroResposta { Tipo = "ORCAMENTO", Titulo = item.Percentual >= 100 ? "Orçamento excedido" : "Orçamento próximo do limite", Descricao = $"{item.Categoria}: {item.Percentual:N0}% utilizado.", Severidade = item.Percentual >= 100 ? "CRITICO" : "ATENCAO", Link = "/pessoal/planejamento" });
        var proximas = await _db.LancamentosRecorrentesPessoais.AsNoTracking().Where(x => x.UsuarioId == usuarioId && x.Ativo && x.ProximaExecucao >= hoje && x.ProximaExecucao <= hoje.AddDays(7)).ToListAsync();
        foreach (var item in proximas)
            alertas.Add(new AlertaFinanceiroResposta { Tipo = "RECORRENCIA", Titulo = "Lançamento recorrente próximo", Descricao = $"{item.Descricao} em {item.ProximaExecucao:dd/MM}.", Severidade = "INFO", Link = "/pessoal/planejamento" });
        if (!await _db.ContasPessoais.AnyAsync(x => x.UsuarioId == usuarioId && x.Ativo))
            alertas.Add(new AlertaFinanceiroResposta { Tipo = "CONTA", Titulo = "Nenhuma conta ativa", Descricao = "Ative ou cadastre uma conta para registrar movimentações.", Severidade = "ATENCAO", Link = "/pessoal/contas" });
        var contas = await _db.ContasPessoais.AsNoTracking().Where(x => x.UsuarioId == usuarioId && x.Ativo).ToListAsync();
        foreach (var conta in contas)
        {
            var movimentacao = await _db.LancamentosPessoais.Where(x => x.UsuarioId == usuarioId && x.ContaId == conta.Id && !x.Cancelado && x.Data <= hoje).SumAsync(x => (decimal?)(x.Tipo == TipoLancamentoPessoal.ENTRADA ? x.Valor : -x.Valor)) ?? 0;
            if (conta.SaldoInicial + movimentacao < 0) alertas.Add(new AlertaFinanceiroResposta { Tipo = "SALDO", Titulo = "Conta com saldo negativo", Descricao = $"{conta.Nome}: R$ {conta.SaldoInicial + movimentacao:N2}.", Severidade = "CRITICO", Link = "/pessoal/contas" });
        }
        return alertas;
    }

    private async Task ProcessarRecorrenciasAsync(Guid usuarioId)
    {
        await using var transacao = await _db.Database.BeginTransactionAsync();
        await _db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtext({usuarioId.ToString()}))");
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        var itens = await _db.LancamentosRecorrentesPessoais.Where(x => x.UsuarioId == usuarioId && x.Ativo && x.ProximaExecucao <= hoje).ToListAsync();
        foreach (var item in itens)
        {
            while (item.ProximaExecucao <= hoje)
            {
                _db.LancamentosPessoais.Add(new LancamentoPessoal { UsuarioId = usuarioId, ContaId = item.ContaId, CategoriaId = item.CategoriaId, Tipo = item.Tipo, Descricao = item.Descricao, Valor = item.Valor, Data = item.ProximaExecucao, Observacao = "Gerado automaticamente por recorrência." });
                item.ProximaExecucao = item.Frequencia switch { "SEMANAL" => item.ProximaExecucao.AddDays(7), "ANUAL" => item.ProximaExecucao.AddYears(1), _ => item.ProximaExecucao.AddMonths(1) };
            }
        }
        if (itens.Count > 0) await _db.SaveChangesAsync();
        await transacao.CommitAsync();
    }
}
