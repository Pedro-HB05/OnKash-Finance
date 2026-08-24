using Microsoft.EntityFrameworkCore;
using OnKashFinance.API.Autenticacao;
using OnKashFinance.API.Dados;
using OnKashFinance.API.DTOs;
using OnKashFinance.API.Modelos;

namespace OnKashFinance.API.Servicos;

public class DashboardService
{
    private readonly OnKashDbContext _db;
    private readonly UsuarioAtualService _usuarioAtual;

    public DashboardService(
        OnKashDbContext db,
        UsuarioAtualService usuarioAtual)
    {
        _db = db;
        _usuarioAtual = usuarioAtual;
    }

    public async Task<DashboardPessoalResposta> ObterPessoalAsync(
        DateOnly? inicio = null,
        DateOnly? fim = null)
    {
        if (!_usuarioAtual.EhPessoal())
        {
            throw new UnauthorizedAccessException();
        }

        ValidarPeriodo(inicio, fim);

        var usuarioId = _usuarioAtual.ObterUsuarioId();

        var hoje = DateOnly.FromDateTime(DateTime.Today);

        var dataFinalSaldo = fim ?? hoje;

        var saldoInicial =
            await _db.ContasPessoais
                .Where(x =>
                    x.UsuarioId == usuarioId &&
                    x.Ativo)
                .SumAsync(x =>
                    (decimal?)x.SaldoInicial)
            ?? 0;

        var lancamentos =
            _db.LancamentosPessoais
                .Where(x =>
                    x.UsuarioId == usuarioId &&
                    !x.Cancelado);

        var movimentacaoAteDataFinal =
            await lancamentos
                .Where(x =>
                    x.Data <= dataFinalSaldo)
                .SumAsync(x =>
                    (decimal?)
                    (
                        x.Tipo ==
                        TipoLancamentoPessoal.ENTRADA
                            ? x.Valor
                            : -x.Valor
                    ))
            ?? 0;

        var saldo =
            saldoInicial +
            movimentacaoAteDataFinal;

        var entradasQuery =
            lancamentos.Where(x =>
                x.Tipo ==
                TipoLancamentoPessoal.ENTRADA);

        var saidasQuery =
            lancamentos.Where(x =>
                x.Tipo ==
                TipoLancamentoPessoal.SAIDA);

        if (inicio.HasValue)
        {
            entradasQuery =
                entradasQuery.Where(x =>
                    x.Data >= inicio.Value);

            saidasQuery =
                saidasQuery.Where(x =>
                    x.Data >= inicio.Value);
        }

        if (fim.HasValue)
        {
            entradasQuery =
                entradasQuery.Where(x =>
                    x.Data <= fim.Value);

            saidasQuery =
                saidasQuery.Where(x =>
                    x.Data <= fim.Value);
        }

        var entradas =
            await entradasQuery
                .SumAsync(x =>
                    (decimal?)x.Valor)
            ?? 0;

        var saidas =
            await saidasQuery
                .SumAsync(x =>
                    (decimal?)x.Valor)
            ?? 0;

        decimal entradasAnteriores = 0, saidasAnteriores = 0;
        if (inicio.HasValue && fim.HasValue)
        {
            var duracao = fim.Value.DayNumber - inicio.Value.DayNumber + 1;
            var fimAnterior = inicio.Value.AddDays(-1);
            var inicioAnterior = fimAnterior.AddDays(-(duracao - 1));
            entradasAnteriores = await lancamentos.Where(x => x.Tipo == TipoLancamentoPessoal.ENTRADA && x.Data >= inicioAnterior && x.Data <= fimAnterior).SumAsync(x => (decimal?)x.Valor) ?? 0;
            saidasAnteriores = await lancamentos.Where(x => x.Tipo == TipoLancamentoPessoal.SAIDA && x.Data >= inicioAnterior && x.Data <= fimAnterior).SumAsync(x => (decimal?)x.Valor) ?? 0;
        }

        return new DashboardPessoalResposta
        {
            Saldo = saldo,
            Entradas = entradas,
            Saidas = saidas,
            ResultadoMes = entradas - saidas,
            EntradasAnteriores = entradasAnteriores,
            SaidasAnteriores = saidasAnteriores,
            ResultadoAnterior = entradasAnteriores - saidasAnteriores
        };
    }

    public async Task<DashboardEmpresarialResposta> ObterEmpresarialAsync(
        DateOnly? inicio = null,
        DateOnly? fim = null)
    {
        if (!_usuarioAtual.EhEmpresarial())
        {
            throw new UnauthorizedAccessException();
        }

        ValidarPeriodo(inicio, fim);

        var empresaId =
            _usuarioAtual.ExigirEmpresaId();

        var hoje =
            DateOnly.FromDateTime(
                DateTime.Today);

        var dataFinalSaldo =
            fim ?? hoje;

        var saldoInicial =
            await _db.ContasEmpresariais
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    x.Ativo)
                .SumAsync(x =>
                    (decimal?)x.SaldoInicial)
            ?? 0;

        var lancamentos =
            _db.LancamentosEmpresariais
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    !x.Cancelado);

        var movimentacaoAteDataFinal =
            await lancamentos
                .Where(x =>
                    x.Data <= dataFinalSaldo)
                .SumAsync(x =>
                    (decimal?)
                    (
                        x.Tipo ==
                        TipoLancamentoEmpresarial.RECEITA
                            ? x.Valor
                            : x.Tipo ==
                              TipoLancamentoEmpresarial.DESPESA
                                ? -x.Valor
                                : 0
                    ))
            ?? 0;

        var saldo =
            saldoInicial +
            movimentacaoAteDataFinal;

        var entradasQuery =
            lancamentos.Where(x =>
                x.Tipo ==
                TipoLancamentoEmpresarial.RECEITA);

        var saidasQuery =
            lancamentos.Where(x =>
                x.Tipo ==
                TipoLancamentoEmpresarial.DESPESA);

        if (inicio.HasValue)
        {
            entradasQuery =
                entradasQuery.Where(x =>
                    x.Data >= inicio.Value);

            saidasQuery =
                saidasQuery.Where(x =>
                    x.Data >= inicio.Value);
        }

        if (fim.HasValue)
        {
            entradasQuery =
                entradasQuery.Where(x =>
                    x.Data <= fim.Value);

            saidasQuery =
                saidasQuery.Where(x =>
                    x.Data <= fim.Value);
        }

        var entradas =
            await entradasQuery
                .SumAsync(x =>
                    (decimal?)x.Valor)
            ?? 0;

        var saidas =
            await saidasQuery
                .SumAsync(x =>
                    (decimal?)x.Valor)
            ?? 0;

        decimal entradasAnteriores = 0, saidasAnteriores = 0;
        if (inicio.HasValue && fim.HasValue)
        {
            var duracao = fim.Value.DayNumber - inicio.Value.DayNumber + 1;
            var fimAnterior = inicio.Value.AddDays(-1);
            var inicioAnterior = fimAnterior.AddDays(-(duracao - 1));
            entradasAnteriores = await lancamentos.Where(x => x.Tipo == TipoLancamentoEmpresarial.RECEITA && x.Data >= inicioAnterior && x.Data <= fimAnterior).SumAsync(x => (decimal?)x.Valor) ?? 0;
            saidasAnteriores = await lancamentos.Where(x => x.Tipo == TipoLancamentoEmpresarial.DESPESA && x.Data >= inicioAnterior && x.Data <= fimAnterior).SumAsync(x => (decimal?)x.Valor) ?? 0;
        }

        var contasPagar =
            await _db.ContasPagar
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    (
                        x.Status ==
                            StatusContaPagar.PENDENTE ||
                        x.Status ==
                            StatusContaPagar.ATRASADO
                    ))
                .SumAsync(x =>
                    (decimal?)x.Valor)
            ?? 0;

        var contasReceber =
            await _db.ContasReceber
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    (
                        x.Status ==
                            StatusContaReceber.PENDENTE ||
                        x.Status ==
                            StatusContaReceber.ATRASADO
                    ))
                .SumAsync(x =>
                    (decimal?)x.Valor)
            ?? 0;

        var pagarVencido =
            await _db.ContasPagar
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    x.Vencimento < hoje &&
                    x.Status != StatusContaPagar.PAGO &&
                    x.Status != StatusContaPagar.CANCELADO)
                .SumAsync(x =>
                    (decimal?)x.Valor)
            ?? 0;

        var receberVencido =
            await _db.ContasReceber
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    x.Vencimento < hoje &&
                    x.Status != StatusContaReceber.RECEBIDO &&
                    x.Status != StatusContaReceber.CANCELADO)
                .SumAsync(x =>
                    (decimal?)x.Valor)
            ?? 0;

        return new DashboardEmpresarialResposta
        {
            Saldo = saldo,
            Entradas = entradas,
            Saidas = saidas,
            Resultado = entradas - saidas,
            ContasAPagar = contasPagar,
            ContasAReceber = contasReceber,
            PagarVencido = pagarVencido,
            ReceberVencido = receberVencido,
            ValoresVencidos =
                pagarVencido +
                receberVencido,
            EntradasAnteriores = entradasAnteriores,
            SaidasAnteriores = saidasAnteriores,
            ResultadoAnterior = entradasAnteriores - saidasAnteriores
        };
    }

    private static void ValidarPeriodo(
        DateOnly? inicio,
        DateOnly? fim)
    {
        if (
            inicio.HasValue &&
            fim.HasValue &&
            inicio.Value > fim.Value)
        {
            throw new ArgumentException(
                "A data inicial não pode ser maior que a data final.");
        }
    }
}
