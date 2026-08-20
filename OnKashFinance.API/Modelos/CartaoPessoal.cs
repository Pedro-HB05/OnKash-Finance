namespace OnKashFinance.API.Modelos;

public class CartaoPessoal
{
    public Guid Id { get; set; }

    public Guid UsuarioId { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Instituicao { get; set; } = string.Empty;

    public decimal Limite { get; set; }

    public DateOnly DataFechamento { get; set; }

    public DateOnly DataVencimento { get; set; }

    public bool Ativo { get; set; } = true;

    public DateTimeOffset CriadoEm { get; set; }

    public DateTimeOffset AtualizadoEm { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public ICollection<FaturaPessoal> Faturas { get; set; }
        = new List<FaturaPessoal>();

    public ICollection<CompraCartaoPessoal> Compras { get; set; }
        = new List<CompraCartaoPessoal>();
}