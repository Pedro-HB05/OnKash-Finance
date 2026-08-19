namespace OnKashFinance.API.Modelos;

public class ContaReceber
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid? ClienteId { get; set; }

    public string Descricao { get; set; } = string.Empty;

    public decimal Valor { get; set; }

    public DateOnly Vencimento { get; set; }

    public DateOnly? DataRecebimento { get; set; }

    public Guid CategoriaId { get; set; }

    public Guid? ContaId { get; set; }

    public StatusContaReceber Status { get; set; }
        = StatusContaReceber.PENDENTE;

    public string? Observacao { get; set; }

    public DateTimeOffset CriadoEm { get; set; }

    public DateTimeOffset AtualizadoEm { get; set; }

    public Empresa Empresa { get; set; } = null!;

    public Cliente? Cliente { get; set; }

    public CategoriaEmpresarial Categoria { get; set; } = null!;

    public ContaEmpresarial? Conta { get; set; }

    public ICollection<LancamentoEmpresarial> Lancamentos { get; set; }
        = new List<LancamentoEmpresarial>();
}