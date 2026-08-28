using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OnKashFinance.API.Autenticacao;
using OnKashFinance.API.Dados;
using OnKashFinance.API.DTOs;
using OnKashFinance.API.Modelos;

namespace OnKashFinance.API.Servicos;

public class InteligenciaFinanceiraService
{
    private readonly OnKashDbContext _db;
    private readonly UsuarioAtualService _usuarioAtual;
    public InteligenciaFinanceiraService(OnKashDbContext db, UsuarioAtualService usuarioAtual)
    { _db = db; _usuarioAtual = usuarioAtual; }

    private (string Ambiente, Guid ProprietarioId) Contexto(bool pessoal)
    {
        if (pessoal && !_usuarioAtual.EhPessoal() || !pessoal && !_usuarioAtual.EhEmpresarial())
            throw new UnauthorizedAccessException("Acesso incompatível com o tipo de conta.");
        return pessoal ? ("PESSOAL", _usuarioAtual.ObterUsuarioId()) : ("EMPRESARIAL", _usuarioAtual.ExigirEmpresaId());
    }

    public async Task<ResultadoImportacaoResposta> ImportarAsync(bool pessoal, ImportarExtratoRequest request)
    {
        var (ambiente, proprietarioId) = Contexto(pessoal);
        if (request.Movimentos.Count is < 1 or > 1000) throw new InvalidOperationException("O arquivo deve conter entre 1 e 1.000 movimentos.");
        var contaValida = pessoal
            ? await _db.ContasPessoais.AnyAsync(x => x.Id == request.ContaId && x.UsuarioId == proprietarioId && x.Ativo)
            : await _db.ContasEmpresariais.AnyAsync(x => x.Id == request.ContaId && x.EmpresaId == proprietarioId && x.Ativo);
        if (!contaValida) throw new InvalidOperationException("Selecione uma conta ativa válida.");

        var resposta = new ResultadoImportacaoResposta();
        var hashesProcessados = new HashSet<string>(StringComparer.Ordinal);
        await using var transacao = await _db.Database.BeginTransactionAsync();
        await _db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtext({ambiente + proprietarioId}))");
        foreach (var movimento in request.Movimentos)
        {
            if (movimento.Valor == 0 || string.IsNullOrWhiteSpace(movimento.Descricao)) continue;
            var descricao = movimento.Descricao.Trim();
            var hash = Hash($"{proprietarioId:N}|{request.ContaId:N}|{movimento.Data:yyyy-MM-dd}|{movimento.Valor:0.00}|{descricao.ToUpperInvariant()}");
            if (!hashesProcessados.Add(hash) || await _db.MovimentosImportados.AnyAsync(x => x.Ambiente == ambiente && x.ProprietarioId == proprietarioId && x.Hash == hash))
            { resposta.Duplicados++; continue; }

            Guid lancamentoId;
            if (pessoal)
            {
                var tipo = movimento.Valor > 0 ? TipoLancamentoPessoal.ENTRADA : TipoLancamentoPessoal.SAIDA;
                var valor = Math.Abs(movimento.Valor);
                var existente = await _db.LancamentosPessoais.FirstOrDefaultAsync(x => x.UsuarioId == proprietarioId && x.ContaId == request.ContaId && !x.Cancelado && x.Data == movimento.Data && x.Valor == valor && x.Tipo == tipo && x.Descricao == descricao);
                if (existente is not null) { lancamentoId = existente.Id; resposta.Conciliados++; }
                else
                {
                    lancamentoId = Guid.NewGuid();
                    _db.LancamentosPessoais.Add(new LancamentoPessoal { Id = lancamentoId, UsuarioId = proprietarioId, ContaId = request.ContaId, Tipo = tipo, Descricao = descricao, Valor = valor, Data = movimento.Data, Observacao = $"Importado de {Limitar(request.ArquivoOrigem, 180)}." });
                    resposta.Importados++;
                }
            }
            else
            {
                var tipo = movimento.Valor > 0 ? TipoLancamentoEmpresarial.RECEITA : TipoLancamentoEmpresarial.DESPESA;
                var valor = Math.Abs(movimento.Valor);
                var existente = await _db.LancamentosEmpresariais.FirstOrDefaultAsync(x => x.EmpresaId == proprietarioId && x.ContaId == request.ContaId && !x.Cancelado && x.Data == movimento.Data && x.Valor == valor && x.Tipo == tipo && x.Descricao == descricao);
                if (existente is not null) { lancamentoId = existente.Id; resposta.Conciliados++; }
                else
                {
                    lancamentoId = Guid.NewGuid();
                    _db.LancamentosEmpresariais.Add(new LancamentoEmpresarial { Id = lancamentoId, EmpresaId = proprietarioId, ContaId = request.ContaId, Tipo = tipo, Descricao = descricao, Valor = valor, Data = movimento.Data, Observacao = $"Importado de {Limitar(request.ArquivoOrigem, 180)}." });
                    resposta.Importados++;
                }
            }
            _db.MovimentosImportados.Add(new MovimentoImportado { Ambiente = ambiente, ProprietarioId = proprietarioId, ContaId = request.ContaId, LancamentoId = lancamentoId, Hash = hash, ArquivoOrigem = Limitar(request.ArquivoOrigem, 255), Data = movimento.Data, Descricao = Limitar(descricao, 300), Valor = movimento.Valor });
        }
        await _db.SaveChangesAsync();
        await transacao.CommitAsync();
        return resposta;
    }

    public async Task<ProjecaoCaixaResposta> ProjetarAsync(bool pessoal, int dias)
    {
        var (_, proprietarioId) = Contexto(pessoal);
        dias = Math.Clamp(dias, 7, 365);
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        var fim = hoje.AddDays(dias);
        decimal saldo;
        var eventos = new List<(DateOnly Data, decimal Entrada, decimal Saida)>();
        if (pessoal)
        {
            saldo = await _db.ContasPessoais.Where(x => x.UsuarioId == proprietarioId && x.Ativo).SumAsync(x => (decimal?)x.SaldoInicial) ?? 0;
            saldo += await _db.LancamentosPessoais.Where(x => x.UsuarioId == proprietarioId && !x.Cancelado && x.Data <= hoje).SumAsync(x => (decimal?)(x.Tipo == TipoLancamentoPessoal.ENTRADA ? x.Valor : x.Tipo == TipoLancamentoPessoal.SAIDA ? -x.Valor : 0)) ?? 0;
            var recorrencias = await _db.LancamentosRecorrentesPessoais.AsNoTracking().Where(x => x.UsuarioId == proprietarioId && x.Ativo && x.ProximaExecucao <= fim).ToListAsync();
            foreach (var item in recorrencias)
            {
                var data = item.ProximaExecucao;
                while (data <= fim)
                {
                    if (data > hoje) eventos.Add((data, item.Tipo == TipoLancamentoPessoal.ENTRADA ? item.Valor : 0, item.Tipo == TipoLancamentoPessoal.SAIDA ? item.Valor : 0));
                    data = item.Frequencia switch { "SEMANAL" => data.AddDays(7), "ANUAL" => data.AddYears(1), _ => data.AddMonths(1) };
                }
            }
        }
        else
        {
            saldo = await _db.ContasEmpresariais.Where(x => x.EmpresaId == proprietarioId && x.Ativo).SumAsync(x => (decimal?)x.SaldoInicial) ?? 0;
            saldo += await _db.LancamentosEmpresariais.Where(x => x.EmpresaId == proprietarioId && !x.Cancelado && x.Data <= hoje).SumAsync(x => (decimal?)(x.Tipo == TipoLancamentoEmpresarial.RECEITA ? x.Valor : x.Tipo == TipoLancamentoEmpresarial.DESPESA ? -x.Valor : 0)) ?? 0;
            eventos.AddRange(await _db.ContasReceber.Where(x => x.EmpresaId == proprietarioId && x.Vencimento > hoje && x.Vencimento <= fim && (x.Status == StatusContaReceber.PENDENTE || x.Status == StatusContaReceber.ATRASADO)).Select(x => new ValueTuple<DateOnly, decimal, decimal>(x.Vencimento, x.Valor, 0)).ToListAsync());
            eventos.AddRange(await _db.ContasPagar.Where(x => x.EmpresaId == proprietarioId && x.Vencimento > hoje && x.Vencimento <= fim && (x.Status == StatusContaPagar.PENDENTE || x.Status == StatusContaPagar.ATRASADO)).Select(x => new ValueTuple<DateOnly, decimal, decimal>(x.Vencimento, 0, x.Valor)).ToListAsync());
        }
        var pontos = new List<PontoProjecaoResposta> { new() { Data = hoje, SaldoProjetado = saldo } };
        foreach (var grupo in eventos.GroupBy(x => x.Data).OrderBy(x => x.Key))
        {
            var entradas = grupo.Sum(x => x.Entrada); var saidas = grupo.Sum(x => x.Saida); saldo += entradas - saidas;
            pontos.Add(new PontoProjecaoResposta { Data = grupo.Key, Entradas = entradas, Saidas = saidas, SaldoProjetado = saldo });
        }
        return new ProjecaoCaixaResposta { SaldoAtual = pontos[0].SaldoProjetado, SaldoProjetado = saldo, Pontos = pontos };
    }

    public async Task<DreSimplificadaResposta> ObterDreAsync(DateOnly inicio, DateOnly fim)
    {
        var (_, empresaId) = Contexto(false);
        if (inicio > fim) throw new InvalidOperationException("Período inválido.");
        var dados = _db.LancamentosEmpresariais.AsNoTracking().Where(x => x.EmpresaId == empresaId && !x.Cancelado && x.Data >= inicio && x.Data <= fim && x.Tipo != TipoLancamentoEmpresarial.TRANSFERENCIA);
        var receitas = await dados.Where(x => x.Tipo == TipoLancamentoEmpresarial.RECEITA).GroupBy(x => x.Categoria != null ? x.Categoria.Nome : "Sem categoria").Select(x => new LinhaDreResposta { Categoria = x.Key, Valor = x.Sum(y => y.Valor) }).OrderByDescending(x => x.Valor).ToListAsync();
        var despesas = await dados.Where(x => x.Tipo == TipoLancamentoEmpresarial.DESPESA).GroupBy(x => x.Categoria != null ? x.Categoria.Nome : "Sem categoria").Select(x => new LinhaDreResposta { Categoria = x.Key, Valor = x.Sum(y => y.Valor) }).OrderByDescending(x => x.Valor).ToListAsync();
        var receita = receitas.Sum(x => x.Valor); var despesa = despesas.Sum(x => x.Valor); var resultado = receita - despesa;
        return new DreSimplificadaResposta { Inicio = inicio, Fim = fim, ReceitaBruta = receita, Despesas = despesa, Resultado = resultado, Margem = receita == 0 ? 0 : Math.Round(resultado / receita * 100, 2), ReceitasPorCategoria = receitas, DespesasPorCategoria = despesas };
    }

    public async Task<List<AlertaFinanceiroResposta>> AlertasEmpresariaisAsync()
    {
        var (_, empresaId) = Contexto(false); var hoje = DateOnly.FromDateTime(DateTime.Today); var limite = hoje.AddDays(7); var resposta = new List<AlertaFinanceiroResposta>();
        var saldo = await _db.ContasEmpresariais.Where(x => x.EmpresaId == empresaId && x.Ativo).SumAsync(x => (decimal?)x.SaldoInicial) ?? 0;
        saldo += await _db.LancamentosEmpresariais.Where(x => x.EmpresaId == empresaId && !x.Cancelado && x.Data <= hoje).SumAsync(x => (decimal?)(x.Tipo == TipoLancamentoEmpresarial.RECEITA ? x.Valor : x.Tipo == TipoLancamentoEmpresarial.DESPESA ? -x.Valor : 0)) ?? 0;
        if (saldo < 0) resposta.Add(new AlertaFinanceiroResposta { Tipo = "SALDO", Titulo = "Saldo empresarial negativo", Descricao = $"O saldo consolidado está em R$ {saldo:N2}.", Severidade = "CRITICO", Link = "/empresarial/inteligencia" });
        var pagar = await _db.ContasPagar.AsNoTracking().Where(x => x.EmpresaId == empresaId && x.Vencimento <= limite && x.Status != StatusContaPagar.PAGO && x.Status != StatusContaPagar.CANCELADO).OrderBy(x => x.Vencimento).Take(10).ToListAsync();
        resposta.AddRange(pagar.Select(x => new AlertaFinanceiroResposta { Tipo = "CONTA_PAGAR", Titulo = x.Vencimento < hoje ? "Conta a pagar vencida" : "Pagamento próximo", Descricao = $"{x.Descricao}: R$ {x.Valor:N2} em {x.Vencimento:dd/MM}.", Severidade = x.Vencimento < hoje ? "CRITICO" : "ATENCAO", Link = "/empresarial/contas-a-pagar" }));
        var receber = await _db.ContasReceber.AsNoTracking().Where(x => x.EmpresaId == empresaId && x.Vencimento <= limite && x.Status != StatusContaReceber.RECEBIDO && x.Status != StatusContaReceber.CANCELADO).OrderBy(x => x.Vencimento).Take(10).ToListAsync();
        resposta.AddRange(receber.Select(x => new AlertaFinanceiroResposta { Tipo = "CONTA_RECEBER", Titulo = x.Vencimento < hoje ? "Recebimento atrasado" : "Recebimento próximo", Descricao = $"{x.Descricao}: R$ {x.Valor:N2} em {x.Vencimento:dd/MM}.", Severidade = x.Vencimento < hoje ? "CRITICO" : "INFO", Link = "/empresarial/contas-a-receber" }));
        return resposta;
    }

    public async Task<List<AnexoFinanceiroResposta>> ListarAnexosAsync(bool pessoal, Guid lancamentoId)
    {
        var (ambiente, dono) = Contexto(pessoal); await ValidarLancamentoAsync(pessoal, dono, lancamentoId);
        return await _db.AnexosFinanceiros.AsNoTracking().Where(x => x.Ambiente == ambiente && x.ProprietarioId == dono && x.LancamentoId == lancamentoId).OrderByDescending(x => x.CriadoEm).Select(x => new AnexoFinanceiroResposta { Id = x.Id, NomeArquivo = x.NomeArquivo, TipoConteudo = x.TipoConteudo, Tamanho = x.Tamanho, CriadoEm = x.CriadoEm }).ToListAsync();
    }
    public async Task AdicionarAnexoAsync(bool pessoal, Guid lancamentoId, IFormFile arquivo)
    {
        var (ambiente, dono) = Contexto(pessoal); await ValidarLancamentoAsync(pessoal, dono, lancamentoId);
        if (arquivo.Length is <= 0 or > 5_242_880) throw new InvalidOperationException("O arquivo deve ter no máximo 5 MB.");
        var permitidos = new[] { "application/pdf", "image/png", "image/jpeg", "image/webp" };
        if (!permitidos.Contains(arquivo.ContentType.ToLowerInvariant())) throw new InvalidOperationException("Envie PDF, PNG, JPG ou WEBP.");
        await using var memoria = new MemoryStream(); await arquivo.CopyToAsync(memoria);
        _db.AnexosFinanceiros.Add(new AnexoFinanceiro { Ambiente = ambiente, ProprietarioId = dono, LancamentoId = lancamentoId, NomeArquivo = Limitar(Path.GetFileName(arquivo.FileName), 255), TipoConteudo = arquivo.ContentType, Tamanho = arquivo.Length, Conteudo = memoria.ToArray() });
        await _db.SaveChangesAsync();
    }
    public async Task<AnexoFinanceiro> ObterAnexoAsync(bool pessoal, Guid id)
    {
        var (ambiente, dono) = Contexto(pessoal);
        return await _db.AnexosFinanceiros.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.Ambiente == ambiente && x.ProprietarioId == dono) ?? throw new KeyNotFoundException("Anexo não encontrado.");
    }
    public async Task ExcluirAnexoAsync(bool pessoal, Guid id)
    {
        var (ambiente, dono) = Contexto(pessoal);
        var item = await _db.AnexosFinanceiros.FirstOrDefaultAsync(x => x.Id == id && x.Ambiente == ambiente && x.ProprietarioId == dono) ?? throw new KeyNotFoundException("Anexo não encontrado.");
        _db.AnexosFinanceiros.Remove(item); await _db.SaveChangesAsync();
    }
    private async Task ValidarLancamentoAsync(bool pessoal, Guid dono, Guid id)
    {
        var existe = pessoal ? await _db.LancamentosPessoais.AnyAsync(x => x.Id == id && x.UsuarioId == dono) : await _db.LancamentosEmpresariais.AnyAsync(x => x.Id == id && x.EmpresaId == dono);
        if (!existe) throw new KeyNotFoundException("Lançamento não encontrado.");
    }
    private static string Hash(string valor) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(valor))).ToLowerInvariant();
    private static string Limitar(string? valor, int tamanho) { valor ??= string.Empty; return valor.Length <= tamanho ? valor : valor[..tamanho]; }
}
