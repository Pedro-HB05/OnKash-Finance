using Microsoft.EntityFrameworkCore;
using OnKashFinance.API.Autenticacao;
using OnKashFinance.API.Dados;
using OnKashFinance.API.DTOs;
using OnKashFinance.API.Modelos;

namespace OnKashFinance.API.Servicos;

public class CartaoService
{
    private readonly OnKashDbContext _db;
    private readonly UsuarioAtualService _usuarioAtual;

    public CartaoService(
        OnKashDbContext db,
        UsuarioAtualService usuarioAtual)
    {
        _db = db;
        _usuarioAtual = usuarioAtual;
    }

    private Guid ObterUsuario()
    {
        if (!_usuarioAtual.EhPessoal())
        {
            throw new UnauthorizedAccessException(
                "Função exclusiva do financeiro pessoal.");
        }

        return _usuarioAtual.ObterUsuarioId();
    }

    public async Task<List<CartaoResposta>> ListarAsync()
    {
        var usuarioId = ObterUsuario();

        return await _db.CartoesPessoais
            .AsNoTracking()
            .Where(x => x.UsuarioId == usuarioId)
            .OrderBy(x => x.Nome)
            .Select(x => new CartaoResposta
            {
                Id = x.Id,
                Nome = x.Nome,
                Instituicao = x.Instituicao,
                Limite = x.Limite,
                DataFechamento = x.DataFechamento,
                DataVencimento = x.DataVencimento,
                Ativo = x.Ativo
            })
            .ToListAsync();
    }

    public async Task<CartaoResposta> CriarAsync(
        CriarCartaoRequest request)
    {
        var usuarioId = ObterUsuario();

        ValidarCartao(
            request.Nome,
            request.Instituicao,
            request.Limite,
            request.DataFechamento,
            request.DataVencimento);

        var cartao = new CartaoPessoal
        {
            UsuarioId = usuarioId,
            Nome = request.Nome.Trim(),
            Instituicao = request.Instituicao.Trim(),
            Limite = request.Limite,
            DataFechamento = request.DataFechamento,
            DataVencimento = request.DataVencimento,
            Ativo = true
        };

        _db.CartoesPessoais.Add(cartao);

        await _db.SaveChangesAsync();

        return new CartaoResposta
        {
            Id = cartao.Id,
            Nome = cartao.Nome,
            Instituicao = cartao.Instituicao,
            Limite = cartao.Limite,
            DataFechamento = cartao.DataFechamento,
            DataVencimento = cartao.DataVencimento,
            Ativo = cartao.Ativo
        };
    }

    public async Task AtualizarAsync(
    Guid id,
    AtualizarCartaoRequest request)
    {
        var usuarioId = ObterUsuario();

        ValidarCartao(
            request.Nome,
            request.Instituicao,
            request.Limite,
            request.DataFechamento,
            request.DataVencimento);

        var cartao = await _db.CartoesPessoais
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.UsuarioId == usuarioId);

        if (cartao is null)
        {
            throw new KeyNotFoundException(
                "Cartão não encontrado.");
        }

        var datasAlteradas =
            cartao.DataFechamento != request.DataFechamento ||
            cartao.DataVencimento != request.DataVencimento;

        if (datasAlteradas)
        {
            var possuiFaturas = await _db.FaturasPessoais
                .AsNoTracking()
                .AnyAsync(x => x.CartaoId == cartao.Id);

            if (possuiFaturas)
            {
                throw new InvalidOperationException(
                    "Não é possível alterar as datas de fechamento e vencimento de um cartão que já possui faturas.");
            }
        }

        cartao.Nome = request.Nome.Trim();
        cartao.Instituicao = request.Instituicao.Trim();
        cartao.Limite = request.Limite;
        cartao.DataFechamento = request.DataFechamento;
        cartao.DataVencimento = request.DataVencimento;
        cartao.Ativo = request.Ativo;

        await _db.SaveChangesAsync();
    }

    public async Task ExcluirAsync(Guid id)
    {
        var usuarioId = ObterUsuario();
        var cartao = await _db.CartoesPessoais.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == usuarioId);

        if (cartao is null)
            throw new KeyNotFoundException("Cartão não encontrado.");

        if (cartao.Ativo)
            throw new InvalidOperationException("Desative o cartão antes de excluí-lo.");

        var faturasIds = await _db.FaturasPessoais.Where(x => x.CartaoId == id).Select(x => x.Id).ToListAsync();
        var lancamentos = await _db.LancamentosPessoais.Where(x => x.FaturaId.HasValue && faturasIds.Contains(x.FaturaId.Value)).ToListAsync();
        _db.LancamentosPessoais.RemoveRange(lancamentos);
        _db.CartoesPessoais.Remove(cartao);
        await _db.SaveChangesAsync();
    }

    public async Task<CompraCartaoResposta> CriarCompraAsync(
        CriarCompraCartaoRequest request)
    {
        var usuarioId = ObterUsuario();

        if (string.IsNullOrWhiteSpace(request.Descricao))
        {
            throw new InvalidOperationException(
                "A descrição da compra é obrigatória.");
        }

        if (request.ValorTotal <= 0)
        {
            throw new InvalidOperationException(
                "Valor inválido.");
        }

        if (request.NumeroParcelas < 1)
        {
            throw new InvalidOperationException(
                "Número de parcelas inválido.");
        }

        var cartao = await _db.CartoesPessoais
            .FirstOrDefaultAsync(x =>
                x.Id == request.CartaoId &&
                x.UsuarioId == usuarioId &&
                x.Ativo);

        if (cartao is null)
        {
            throw new InvalidOperationException(
                "Cartão inválido.");
        }

        var categoria = await _db.CategoriasPessoais
            .FirstOrDefaultAsync(x =>
                x.Id == request.CategoriaId &&
                x.Ativo &&
                x.Tipo == TipoCategoria.SAIDA &&
                (x.UsuarioId == null ||
                 x.UsuarioId == usuarioId));

        if (categoria is null)
        {
            throw new InvalidOperationException(
                "Categoria inválida.");
        }

        var valorParcela = Math.Round(
            request.ValorTotal / request.NumeroParcelas,
            2,
            MidpointRounding.AwayFromZero);

        var compra = new CompraCartaoPessoal
        {
            CartaoId = cartao.Id,
            CategoriaId = categoria.Id,
            Descricao = request.Descricao.Trim(),
            ValorTotal = request.ValorTotal,
            DataCompra = request.DataCompra,
            NumeroParcelas = request.NumeroParcelas,
            ValorParcela = valorParcela,
            Observacao = request.Observacao,
            Cancelada = false
        };

        _db.ComprasCartaoPessoais.Add(compra);

        var primeiraFatura = ObterPrimeiraFatura(
            cartao,
            request.DataCompra);

        for (var numero = 1;
             numero <= request.NumeroParcelas;
             numero++)
        {
            var mesFechamento = new DateOnly(
                    primeiraFatura.Fechamento.Year,
                    primeiraFatura.Fechamento.Month,
                    1)
                .AddMonths(numero - 1);

            var fechamento = CriarData(
                mesFechamento.Year,
                mesFechamento.Month,
                cartao.DataFechamento.Day);

            var vencimento = CalcularVencimento(
                cartao,
                fechamento);

            var competencia = new DateOnly(
                vencimento.Year,
                vencimento.Month,
                1);

            var fatura = await _db.FaturasPessoais
                .FirstOrDefaultAsync(x =>
                    x.CartaoId == cartao.Id &&
                    x.Competencia == competencia);

            if (fatura is null)
            {
                fatura = new FaturaPessoal
                {
                    CartaoId = cartao.Id,
                    Competencia = competencia,
                    DataFechamento = fechamento,
                    DataVencimento = vencimento,
                    Status = StatusFatura.ABERTA
                };

                _db.FaturasPessoais.Add(fatura);
            }

            var valor =
                numero == request.NumeroParcelas
                    ? request.ValorTotal -
                      valorParcela *
                      (request.NumeroParcelas - 1)
                    : valorParcela;

            var parcela = new ParcelaCartaoPessoal
            {
                Compra = compra,
                Fatura = fatura,
                NumeroParcela = numero,
                Valor = valor,
                DataVencimento = vencimento
            };

            _db.ParcelasCartaoPessoais.Add(parcela);
        }

        await _db.SaveChangesAsync();

        return new CompraCartaoResposta
        {
            Id = compra.Id,
            CartaoId = compra.CartaoId,
            CategoriaId = compra.CategoriaId,
            Descricao = compra.Descricao,
            ValorTotal = compra.ValorTotal,
            DataCompra = compra.DataCompra,
            NumeroParcelas = compra.NumeroParcelas,
            ValorParcela = compra.ValorParcela,
            Cancelada = compra.Cancelada
        };
    }

    public async Task<List<FaturaResposta>> ListarFaturasAsync(
        Guid? cartaoId = null)
    {
        var usuarioId = ObterUsuario();

        var query = _db.FaturasPessoais
            .AsNoTracking()
            .Where(x =>
                x.Cartao.UsuarioId == usuarioId);

        if (cartaoId.HasValue)
        {
            query = query.Where(
                x => x.CartaoId == cartaoId.Value);
        }

        return await query
            .OrderByDescending(x => x.Competencia)
            .Select(x => new FaturaResposta
            {
                Id = x.Id,
                CartaoId = x.CartaoId,
                Cartao = x.Cartao.Nome,
                Competencia = x.Competencia,
                DataFechamento = x.DataFechamento,
                DataVencimento = x.DataVencimento,
                Status = x.Status,
                ValorTotal = x.Parcelas
                    .Where(p => !p.Compra.Cancelada)
                    .Sum(p => (decimal?)p.Valor) ?? 0
            })
            .ToListAsync();
    }

    public async Task PagarFaturaAsync(
        Guid faturaId,
        PagarFaturaRequest request)
    {
        var usuarioId = ObterUsuario();

        var fatura = await _db.FaturasPessoais
            .Include(x => x.Cartao)
            .Include(x => x.Parcelas)
                .ThenInclude(x => x.Compra)
            .FirstOrDefaultAsync(x =>
                x.Id == faturaId &&
                x.Cartao.UsuarioId == usuarioId);

        if (fatura is null)
        {
            throw new KeyNotFoundException(
                "Fatura não encontrada.");
        }

        if (fatura.Status == StatusFatura.PAGA)
        {
            throw new InvalidOperationException(
                "A fatura já foi paga.");
        }

        var pagamentoExistente = await _db.LancamentosPessoais
            .AnyAsync(x =>
                x.FaturaId == fatura.Id &&
                !x.Cancelado);

        if (pagamentoExistente)
        {
            throw new InvalidOperationException(
                "Já existe pagamento ativo para esta fatura.");
        }

        var conta = await _db.ContasPessoais
            .FirstOrDefaultAsync(x =>
                x.Id == request.ContaId &&
                x.UsuarioId == usuarioId &&
                x.Ativo);

        if (conta is null)
        {
            throw new InvalidOperationException(
                "Conta inválida.");
        }

        var valor = fatura.Parcelas
            .Where(x => !x.Compra.Cancelada)
            .Sum(x => x.Valor);

        if (valor <= 0)
        {
            throw new InvalidOperationException(
                "A fatura não possui valor para pagamento.");
        }

        await ExigirSaldoSuficienteAsync(usuarioId, conta, valor);

        var lancamento = new LancamentoPessoal
        {
            UsuarioId = usuarioId,
            ContaId = conta.Id,
            FaturaId = fatura.Id,
            CategoriaId = null,
            Tipo = TipoLancamentoPessoal.SAIDA,
            Descricao =
                $"Pagamento da fatura - {fatura.Cartao.Nome}",
            Valor = valor,
            Data = request.DataPagamento,
            Observacao = request.Observacao,
            Cancelado = false
        };

        _db.LancamentosPessoais.Add(lancamento);

        fatura.Status = StatusFatura.PAGA;

        await _db.SaveChangesAsync();
    }

    private async Task ExigirSaldoSuficienteAsync(
        Guid usuarioId,
        ContaPessoal conta,
        decimal valor)
    {
        var movimentos = await _db.LancamentosPessoais
            .Where(x =>
                x.UsuarioId == usuarioId &&
                x.ContaId == conta.Id &&
                !x.Cancelado)
            .SumAsync(x => (decimal?)(
                x.Tipo == TipoLancamentoPessoal.ENTRADA
                    ? x.Valor
                    : -x.Valor)) ?? 0;

        var transferenciasRecebidas = await _db.LancamentosPessoais
            .Where(x =>
                x.UsuarioId == usuarioId &&
                x.ContaDestinoId == conta.Id &&
                x.Tipo == TipoLancamentoPessoal.TRANSFERENCIA &&
                !x.Cancelado)
            .SumAsync(x => (decimal?)x.Valor) ?? 0;

        var saldo = conta.SaldoInicial + movimentos + transferenciasRecebidas;

        if (saldo < valor)
            throw new InvalidOperationException(
                $"Saldo insuficiente na conta {conta.Nome}. Saldo disponível: R$ {saldo:N2}.");
    }

    public async Task CancelarCompraAsync(
    Guid compraId)
    {
        var usuarioId = ObterUsuario();

        var compra = await _db.ComprasCartaoPessoais
            .Include(x => x.Parcelas)
                .ThenInclude(x => x.Fatura)
            .FirstOrDefaultAsync(x =>
                x.Id == compraId &&
                x.Cartao.UsuarioId == usuarioId);

        if (compra is null)
        {
            throw new KeyNotFoundException(
                "Compra não encontrada.");
        }

        if (compra.Cancelada)
        {
            return;
        }

        var possuiFaturaPaga = compra.Parcelas.Any(
            parcela =>
                parcela.Fatura != null &&
                parcela.Fatura.Status == StatusFatura.PAGA);

        if (possuiFaturaPaga)
        {
            throw new InvalidOperationException(
                "Não é possível cancelar uma compra que possui parcela em fatura paga.");
        }

        compra.Cancelada = true;
        compra.CanceladaEm = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();
    }

    private static void ValidarCartao(
        string nome,
        string instituicao,
        decimal limite,
        DateOnly dataFechamento,
        DateOnly dataVencimento)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new InvalidOperationException(
                "O nome do cartão é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(instituicao))
        {
            throw new InvalidOperationException(
                "A instituição é obrigatória.");
        }

        if (limite < 0)
        {
            throw new InvalidOperationException(
                "O limite não pode ser negativo.");
        }

        if (dataFechamento == default)
        {
            throw new InvalidOperationException(
                "A data de fechamento é obrigatória.");
        }

        if (dataVencimento == default)
        {
            throw new InvalidOperationException(
                "A data de vencimento é obrigatória.");
        }

        if (dataVencimento <= dataFechamento)
        {
            throw new InvalidOperationException(
                "A data de vencimento deve ser posterior à data de fechamento.");
        }
    }

    private static (
        DateOnly Fechamento,
        DateOnly Vencimento)
        ObterPrimeiraFatura(
            CartaoPessoal cartao,
            DateOnly dataCompra)
    {
        if (dataCompra <= cartao.DataFechamento)
        {
            return (
                cartao.DataFechamento,
                cartao.DataVencimento);
        }

        var meses = DiferencaMeses(
            cartao.DataFechamento,
            dataCompra);

        var mesReferencia = new DateOnly(
                cartao.DataFechamento.Year,
                cartao.DataFechamento.Month,
                1)
            .AddMonths(meses);

        var fechamento = CriarData(
            mesReferencia.Year,
            mesReferencia.Month,
            cartao.DataFechamento.Day);

        if (dataCompra > fechamento)
        {
            mesReferencia = mesReferencia.AddMonths(1);

            fechamento = CriarData(
                mesReferencia.Year,
                mesReferencia.Month,
                cartao.DataFechamento.Day);
        }

        var vencimento = CalcularVencimento(
            cartao,
            fechamento);

        return (
            fechamento,
            vencimento);
    }

    private static DateOnly CalcularVencimento(
        CartaoPessoal cartao,
        DateOnly fechamento)
    {
        var diferencaMeses = DiferencaMeses(
            cartao.DataFechamento,
            cartao.DataVencimento);

        var mesVencimento = new DateOnly(
                fechamento.Year,
                fechamento.Month,
                1)
            .AddMonths(diferencaMeses);

        return CriarData(
            mesVencimento.Year,
            mesVencimento.Month,
            cartao.DataVencimento.Day);
    }

    private static int DiferencaMeses(
        DateOnly inicio,
        DateOnly fim)
    {
        return (fim.Year - inicio.Year) * 12 +
               fim.Month -
               inicio.Month;
    }

    private static DateOnly CriarData(
        int ano,
        int mes,
        int dia)
    {
        var ultimoDia =
            DateTime.DaysInMonth(
                ano,
                mes);

        return new DateOnly(
            ano,
            mes,
            Math.Min(dia, ultimoDia));
    }
}
