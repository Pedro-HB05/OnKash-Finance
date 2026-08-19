namespace OnKashFinance.API.Modelos;

public class CategoriaEmpresarial
{
    public Guid Id { get; set; }

    public Guid? EmpresaId { get; set; }

    public string Nome { get; set; } = string.Empty;

    public TipoCategoria Tipo { get; set; }

    public bool Padrao { get; set; }

    public bool Ativo { get; set; } = true;

    public DateTimeOffset CriadoEm { get; set; }

    public DateTimeOffset AtualizadoEm { get; set; }

    public Empresa? Empresa { get; set; }

    public ICollection<ContaPagar> ContasPagar { get; set; }
        = new List<ContaPagar>();

    public ICollection<ContaReceber> ContasReceber { get; set; }
        = new List<ContaReceber>();

    public ICollection<LancamentoEmpresarial> Lancamentos { get; set; }
        = new List<LancamentoEmpresarial>();
}