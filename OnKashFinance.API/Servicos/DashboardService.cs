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

    public async Task<DashboardPessoalResposta>
        ObterPessoalAsync()
    {
        if (!_usuarioAtual.EhPessoal())
            throw new UnauthorizedAccessException();

        var usuarioId =
            _usuarioAtual.ObterUsuarioId();

        var hoje =
            DateOnly.FromDateTime(
                DateTime.Today
            );

        var inicioMes =
            new DateOnly(
                hoje.Year,
                hoje.Month,
                1
            );

        var fimMes =
            inicioMes
                .AddMonths(1)
                .AddDays(-1);

        var saldoInicial =
            await _db.ContasPessoais
                .Where(x =>
                    x.UsuarioId == usuarioId &&
                    x.Ativo)
                .SumAsync(x =>
                    (decimal?)x.SaldoInicial)
            ?? 0;

        var entradasTotais =
            await _db.LancamentosPessoais
                .Where(x =>
                    x.UsuarioId == usuarioId &&
                    !x.Cancelado &&
                    x.Tipo ==
                    TipoLancamentoPessoal.ENTRADA)
                .SumAsync(x =>
                    (decimal?)x.Valor)
            ?? 0;

        var saidasTotais =
            await _db.LancamentosPessoais
                .Where(x =>
                    x.UsuarioId == usuarioId &&
                    !x.Cancelado &&
                    x.Tipo ==
                    TipoLancamentoPessoal.SAIDA)
                .SumAsync(x =>
                    (decimal?)x.Valor)
            ?? 0;

        var entradasMes =
            await _db.LancamentosPessoais
                .Where(x =>
                    x.UsuarioId == usuarioId &&
                    !x.Cancelado &&
                    x.Tipo ==
                    TipoLancamentoPessoal.ENTRADA &&
                    x.Data >= inicioMes &&
                    x.Data <= fimMes)
                .SumAsync(x =>
                    (decimal?)x.Valor)
            ?? 0;

        var saidasMes =
            await _db.LancamentosPessoais
                .Where(x =>
                    x.UsuarioId == usuarioId &&
                    !x.Cancelado &&
                    x.Tipo ==
                    TipoLancamentoPessoal.SAIDA &&
                    x.Data >= inicioMes &&
                    x.Data <= fimMes)
                .SumAsync(x =>
                    (decimal?)x.Valor)
            ?? 0;

        return new DashboardPessoalResposta
        {
            Saldo =
                saldoInicial +
                entradasTotais -
                saidasTotais,

            Entradas =
                entradasMes,

            Saidas =
                saidasMes,

            ResultadoMes =
                entradasMes -
                saidasMes
        };
    }

    public async Task<DashboardEmpresarialResposta>
        ObterEmpresarialAsync()
    {
        if (!_usuarioAtual.EhEmpresarial())
            throw new UnauthorizedAccessException();

        var empresaId =
            _usuarioAtual.ExigirEmpresaId();

        var hoje =
            DateOnly.FromDateTime(
                DateTime.Today
            );

        var inicioMes =
            new DateOnly(
                hoje.Year,
                hoje.Month,
                1
            );

        var fimMes =
            inicioMes
                .AddMonths(1)
                .AddDays(-1);

        var saldoInicial =
            await _db.ContasEmpresariais
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    x.Ativo)
                .SumAsync(x =>
                    (decimal?)x.SaldoInicial)
            ?? 0;

        var receitasTotais =
            await _db.LancamentosEmpresariais
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    !x.Cancelado &&
                    x.Tipo ==
                    TipoLancamentoEmpresarial.RECEITA)
                .SumAsync(x =>
                    (decimal?)x.Valor)
            ?? 0;

        var despesasTotais =
            await _db.LancamentosEmpresariais
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    !x.Cancelado &&
                    x.Tipo ==
                    TipoLancamentoEmpresarial.DESPESA)
                .SumAsync(x =>
                    (decimal?)x.Valor)
            ?? 0;

        var transferenciasSaida =
            await _db.LancamentosEmpresariais
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    !x.Cancelado &&
                    x.Tipo ==
                    TipoLancamentoEmpresarial.TRANSFERENCIA)
                .SumAsync(x =>
                    (decimal?)x.Valor)
            ?? 0;

        var transferenciasEntrada =
            transferenciasSaida;

        var entradasMes =
            await _db.LancamentosEmpresariais
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    !x.Cancelado &&
                    x.Tipo ==
                    TipoLancamentoEmpresarial.RECEITA &&
                    x.Data >= inicioMes &&
                    x.Data <= fimMes)
                .SumAsync(x =>
                    (decimal?)x.Valor)
            ?? 0;

        var saidasMes =
            await _db.LancamentosEmpresariais
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    !x.Cancelado &&
                    x.Tipo ==
                    TipoLancamentoEmpresarial.DESPESA &&
                    x.Data >= inicioMes &&
                    x.Data <= fimMes)
                .SumAsync(x =>
                    (decimal?)x.Valor)
            ?? 0;

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
                    x.Status !=
                    StatusContaPagar.PAGO &&
                    x.Status !=
                    StatusContaPagar.CANCELADO)
                .SumAsync(x =>
                    (decimal?)x.Valor)
            ?? 0;

        var receberVencido =
            await _db.ContasReceber
                .Where(x =>
                    x.EmpresaId == empresaId &&
                    x.Vencimento < hoje &&
                    x.Status !=
                    StatusContaReceber.RECEBIDO &&
                    x.Status !=
                    StatusContaReceber.CANCELADO)
                .SumAsync(x =>
                    (decimal?)x.Valor)
            ?? 0;

        var saldo =
            saldoInicial +
            receitasTotais -
            despesasTotais -
            transferenciasSaida +
            transferenciasEntrada;

        return new DashboardEmpresarialResposta
        {
            Saldo = saldo,

            Entradas =
                entradasMes,

            Saidas =
                saidasMes,

            Resultado =
                entradasMes -
                saidasMes,

            ContasAPagar =
                contasPagar,

            ContasAReceber =
                contasReceber,

            PagarVencido =
                pagarVencido,

            ReceberVencido =
                receberVencido,

            ValoresVencidos =
                pagarVencido +
                receberVencido
        };
    }
}