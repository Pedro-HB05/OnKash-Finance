namespace OnKashFinance.API.Modelos;

public class ParcelaCartaoPessoal
{
    public Guid Id { get; set; }

    public Guid CompraId { get; set; }

    public Guid? FaturaId { get; set; }

    public int NumeroParcela { get; set; }

    public decimal Valor { get; set; }

    public DateOnly DataVencimento { get; set; }

    public DateTimeOffset CriadoEm { get; set; }

    public DateTimeOffset AtualizadoEm { get; set; }

    public CompraCartaoPessoal Compra { get; set; } = null!;

    public FaturaPessoal? Fatura { get; set; }
}