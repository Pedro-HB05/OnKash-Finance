namespace OnKashFinance.API.Modelos;

public class Cliente
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public string NomeRazaoSocial { get; set; } = string.Empty;

    public string? CpfCnpj { get; set; }

    public string? Telefone { get; set; }

    public string? Email { get; set; }

    public string? Observacao { get; set; }

    public bool Ativo { get; set; } = true;

    public DateTimeOffset CriadoEm { get; set; }

    public DateTimeOffset AtualizadoEm { get; set; }

    public Empresa Empresa { get; set; } = null!;

    public ICollection<ContaReceber> ContasReceber { get; set; }
        = new List<ContaReceber>();

    public ICollection<LancamentoEmpresarial> Lancamentos { get; set; }
        = new List<LancamentoEmpresarial>();
}