using OnKashFinance.API.Modelos;

namespace OnKashFinance.API.DTOs;

public class CriarContaPagarRequest
{
    public Guid? FornecedorId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateOnly Vencimento { get; set; }
    public Guid CategoriaId { get; set; }
    public string? Observacao { get; set; }
}

public class AtualizarContaPagarRequest
{
    public Guid? FornecedorId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateOnly Vencimento { get; set; }
    public Guid CategoriaId { get; set; }
    public string? Observacao { get; set; }
}

public class PagarContaRequest
{
    public Guid ContaId { get; set; }
    public DateOnly DataPagamento { get; set; }
}

public class ContaPagarResposta
{
    public Guid Id { get; set; }
    public Guid? FornecedorId { get; set; }
    public string? Fornecedor { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateOnly Vencimento { get; set; }
    public DateOnly? DataPagamento { get; set; }
    public Guid CategoriaId { get; set; }
    public Guid? ContaId { get; set; }
    public StatusContaPagar Status { get; set; }
    public string? Observacao { get; set; }
}

public class CriarContaReceberRequest
{
    public Guid? ClienteId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateOnly Vencimento { get; set; }
    public Guid CategoriaId { get; set; }
    public string? Observacao { get; set; }
}

public class AtualizarContaReceberRequest
{
    public Guid? ClienteId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateOnly Vencimento { get; set; }
    public Guid CategoriaId { get; set; }
    public string? Observacao { get; set; }
}

public class ReceberContaRequest
{
    public Guid ContaId { get; set; }
    public DateOnly DataRecebimento { get; set; }
}

public class ContaReceberResposta
{
    public Guid Id { get; set; }
    public Guid? ClienteId { get; set; }
    public string? Cliente { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateOnly Vencimento { get; set; }
    public DateOnly? DataRecebimento { get; set; }
    public Guid CategoriaId { get; set; }
    public Guid? ContaId { get; set; }
    public StatusContaReceber Status { get; set; }
    public string? Observacao { get; set; }
}

public class CriarLancamentoEmpresarialRequest
{
    public TipoLancamentoEmpresarial Tipo { get; set; }
    public Guid ContaId { get; set; }
    public Guid? ContaDestinoId { get; set; }
    public Guid? CategoriaId { get; set; }
    public Guid? ClienteId { get; set; }
    public Guid? FornecedorId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateOnly Data { get; set; }
    public string? Observacao { get; set; }
}

public class AtualizarLancamentoEmpresarialRequest
{
    public TipoLancamentoEmpresarial Tipo { get; set; }
    public Guid ContaId { get; set; }
    public Guid? ContaDestinoId { get; set; }
    public Guid? CategoriaId { get; set; }
    public Guid? ClienteId { get; set; }
    public Guid? FornecedorId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateOnly Data { get; set; }
    public string? Observacao { get; set; }
}

public class LancamentoEmpresarialResposta
{
    public Guid Id { get; set; }
    public TipoLancamentoEmpresarial Tipo { get; set; }
    public Guid ContaId { get; set; }
    public Guid? ContaDestinoId { get; set; }
    public Guid? CategoriaId { get; set; }
    public Guid? ClienteId { get; set; }
    public Guid? FornecedorId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateOnly Data { get; set; }
    public string? Observacao { get; set; }
    public bool Cancelado { get; set; }
}