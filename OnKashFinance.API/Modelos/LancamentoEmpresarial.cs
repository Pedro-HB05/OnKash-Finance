namespace OnKashFinance.API.Modelos;

public class LancamentoEmpresarial
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public TipoLancamentoEmpresarial Tipo { get; set; }

    public Guid ContaId { get; set; }

    public Guid? ContaDestinoId { get; set; }

    public Guid? CategoriaId { get; set; }

    public Guid? ClienteId { get; set; }

    public Guid? FornecedorId { get; set; }

    public Guid? ContaPagarId { get; set; }

    public Guid? ContaReceberId { get; set; }

    public string Descricao { get; set; } = string.Empty;

    public decimal Valor { get; set; }

    public DateOnly Data { get; set; }

    public string? Observacao { get; set; }

    public bool Cancelado { get; set; }

    public DateTimeOffset? CanceladoEm { get; set; }

    public DateTimeOffset CriadoEm { get; set; }

    public DateTimeOffset AtualizadoEm { get; set; }

    public Empresa Empresa { get; set; } = null!;

    public ContaEmpresarial Conta { get; set; } = null!;

    public ContaEmpresarial? ContaDestino { get; set; }

    public CategoriaEmpresarial? Categoria { get; set; }

    public Cliente? Cliente { get; set; }

    public Fornecedor? Fornecedor { get; set; }

    public ContaPagar? ContaPagar { get; set; }

    public ContaReceber? ContaReceber { get; set; }
}