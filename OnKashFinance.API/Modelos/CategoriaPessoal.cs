namespace OnKashFinance.API.Modelos;

public class CategoriaPessoal
{
    public Guid Id { get; set; }

    public Guid? UsuarioId { get; set; }

    public string Nome { get; set; } = string.Empty;

    public TipoCategoria Tipo { get; set; }

    public bool Padrao { get; set; }

    public bool Ativo { get; set; } = true;

    public DateTimeOffset CriadoEm { get; set; }

    public DateTimeOffset AtualizadoEm { get; set; }

    public Usuario? Usuario { get; set; }

    public ICollection<CompraCartaoPessoal> ComprasCartao { get; set; }
        = new List<CompraCartaoPessoal>();

    public ICollection<LancamentoPessoal> Lancamentos { get; set; }
        = new List<LancamentoPessoal>();
}