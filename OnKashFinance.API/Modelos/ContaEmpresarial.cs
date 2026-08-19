namespace OnKashFinance.API.Modelos;

public class ContaEmpresarial
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;

    public decimal SaldoInicial { get; set; }

    public bool Ativo { get; set; } = true;

    public DateTimeOffset CriadoEm { get; set; }

    public DateTimeOffset AtualizadoEm { get; set; }

    public Empresa Empresa { get; set; } = null!;

    public ICollection<ContaPagar> ContasPagar { get; set; }
        = new List<ContaPagar>();

    public ICollection<ContaReceber> ContasReceber { get; set; }
        = new List<ContaReceber>();

    public ICollection<LancamentoEmpresarial> LancamentosOrigem { get; set; }
        = new List<LancamentoEmpresarial>();

    public ICollection<LancamentoEmpresarial> LancamentosDestino { get; set; }
        = new List<LancamentoEmpresarial>();
}