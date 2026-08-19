namespace OnKashFinance.API.Modelos;

public class Usuario
{
    public Guid Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string SenhaHash { get; set; } = string.Empty;

    public TipoContaUsuario TipoConta { get; set; }

    public bool Ativo { get; set; } = true;

    public DateTimeOffset CriadoEm { get; set; }

    public DateTimeOffset AtualizadoEm { get; set; }

    public ICollection<EmpresaUsuario> EmpresasUsuario { get; set; }
        = new List<EmpresaUsuario>();

    public ICollection<ContaPessoal> ContasPessoais { get; set; }
        = new List<ContaPessoal>();

    public ICollection<CategoriaPessoal> CategoriasPessoais { get; set; }
        = new List<CategoriaPessoal>();

    public ICollection<CartaoPessoal> CartoesPessoais { get; set; }
        = new List<CartaoPessoal>();

    public ICollection<LancamentoPessoal> LancamentosPessoais { get; set; }
        = new List<LancamentoPessoal>();
}