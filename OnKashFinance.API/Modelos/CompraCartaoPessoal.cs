namespace OnKashFinance.API.Modelos;

public class CompraCartaoPessoal
{
    public Guid Id { get; set; }

    public Guid CartaoId { get; set; }

    public Guid CategoriaId { get; set; }

    public string Descricao { get; set; } = string.Empty;

    public decimal ValorTotal { get; set; }

    public DateOnly DataCompra { get; set; }

    public int NumeroParcelas { get; set; } = 1;

    public decimal ValorParcela { get; set; }

    public string? Observacao { get; set; }

    public bool Cancelada { get; set; }

    public DateTimeOffset? CanceladaEm { get; set; }

    public DateTimeOffset CriadoEm { get; set; }

    public DateTimeOffset AtualizadoEm { get; set; }

    public CartaoPessoal Cartao { get; set; } = null!;

    public CategoriaPessoal Categoria { get; set; } = null!;

    public ICollection<ParcelaCartaoPessoal> Parcelas { get; set; }
        = new List<ParcelaCartaoPessoal>();
}