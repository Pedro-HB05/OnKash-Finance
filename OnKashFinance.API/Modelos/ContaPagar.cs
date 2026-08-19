namespace OnKashFinance.API.Modelos;

public class ContaPagar
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid? FornecedorId { get; set; }

    public string Descricao { get; set; } = string.Empty;

    public decimal Valor { get; set; }

    public DateOnly Vencimento { get; set; }

    public DateOnly? DataPagamento { get; set; }

    public Guid CategoriaId { get; set; }

    public Guid? ContaId { get; set; }

    public StatusContaPagar Status { get; set; }
        = StatusContaPagar.PENDENTE;

    public string? Observacao { get; set; }

    public DateTimeOffset CriadoEm { get; set; }

    public DateTimeOffset AtualizadoEm { get; set; }

    public Empresa Empresa { get; set; } = null!;

    public Fornecedor? Fornecedor { get; set; }

    public CategoriaEmpresarial Categoria { get; set; } = null!;

    public ContaEmpresarial? Conta { get; set; }

    public ICollection<LancamentoEmpresarial> Lancamentos { get; set; }
        = new List<LancamentoEmpresarial>();
}