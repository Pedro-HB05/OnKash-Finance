namespace OnKashFinance.API.Modelos;

public class LancamentoPessoal
{
    public Guid Id { get; set; }

    public Guid UsuarioId { get; set; }

    public Guid ContaId { get; set; }

    public Guid? ContaDestinoId { get; set; }

    public Guid? CategoriaId { get; set; }

    public Guid? FaturaId { get; set; }

    public TipoLancamentoPessoal Tipo { get; set; }

    public string Descricao { get; set; } = string.Empty;

    public decimal Valor { get; set; }

    public DateOnly Data { get; set; }

    public string? Observacao { get; set; }

    public bool Cancelado { get; set; }

    public DateTimeOffset? CanceladoEm { get; set; }

    public DateTimeOffset CriadoEm { get; set; }

    public DateTimeOffset AtualizadoEm { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public ContaPessoal Conta { get; set; } = null!;

    public ContaPessoal? ContaDestino { get; set; }

    public CategoriaPessoal? Categoria { get; set; }

    public FaturaPessoal? Fatura { get; set; }
}
