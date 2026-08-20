using OnKashFinance.API.Modelos;

namespace OnKashFinance.API.DTOs;

public class CriarCartaoRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Instituicao { get; set; } = string.Empty;
    public decimal Limite { get; set; }
    public DateOnly DataFechamento { get; set; }
    public DateOnly DataVencimento { get; set; }
}

public class AtualizarCartaoRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Instituicao { get; set; } = string.Empty;
    public decimal Limite { get; set; }
    public DateOnly DataFechamento { get; set; }
    public DateOnly DataVencimento { get; set; }
    public bool Ativo { get; set; }
}

public class CartaoResposta
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Instituicao { get; set; } = string.Empty;
    public decimal Limite { get; set; }
    public DateOnly DataFechamento { get; set; }
    public DateOnly DataVencimento { get; set; }
    public bool Ativo { get; set; }
}

public class CriarCompraCartaoRequest
{
    public Guid CartaoId { get; set; }
    public Guid CategoriaId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public DateOnly DataCompra { get; set; }
    public int NumeroParcelas { get; set; } = 1;
    public string? Observacao { get; set; }
}

public class CompraCartaoResposta
{
    public Guid Id { get; set; }
    public Guid CartaoId { get; set; }
    public Guid CategoriaId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public DateOnly DataCompra { get; set; }
    public int NumeroParcelas { get; set; }
    public decimal ValorParcela { get; set; }
    public bool Cancelada { get; set; }
}

public class FaturaResposta
{
    public Guid Id { get; set; }
    public Guid CartaoId { get; set; }
    public string Cartao { get; set; } = string.Empty;
    public DateOnly Competencia { get; set; }
    public DateOnly DataFechamento { get; set; }
    public DateOnly DataVencimento { get; set; }
    public StatusFatura Status { get; set; }
    public decimal ValorTotal { get; set; }
}

public class PagarFaturaRequest
{
    public Guid ContaId { get; set; }
    public DateOnly DataPagamento { get; set; }
    public string? Observacao { get; set; }
}