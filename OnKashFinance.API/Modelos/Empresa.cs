namespace OnKashFinance.API.Modelos;

public class Empresa
{
    public Guid Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;

    public DateTimeOffset CriadoEm { get; set; }

    public DateTimeOffset AtualizadoEm { get; set; }

    public ICollection<EmpresaUsuario> Usuarios { get; set; }
        = new List<EmpresaUsuario>();

    public ICollection<ContaEmpresarial> Contas { get; set; }
        = new List<ContaEmpresarial>();

    public ICollection<CategoriaEmpresarial> Categorias { get; set; }
        = new List<CategoriaEmpresarial>();

    public ICollection<Cliente> Clientes { get; set; }
        = new List<Cliente>();

    public ICollection<Fornecedor> Fornecedores { get; set; }
        = new List<Fornecedor>();

    public ICollection<ContaPagar> ContasPagar { get; set; }
        = new List<ContaPagar>();

    public ICollection<ContaReceber> ContasReceber { get; set; }
        = new List<ContaReceber>();

    public ICollection<LancamentoEmpresarial> Lancamentos { get; set; }
        = new List<LancamentoEmpresarial>();
}