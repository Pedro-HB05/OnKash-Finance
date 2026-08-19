namespace OnKashFinance.API.Modelos;

public class FaturaPessoal
{
    public Guid Id { get; set; }

    public Guid CartaoId { get; set; }

    public DateOnly Competencia { get; set; }

    public DateOnly DataFechamento { get; set; }

    public DateOnly DataVencimento { get; set; }

    public StatusFatura Status { get; set; }
        = StatusFatura.ABERTA;

    public DateTimeOffset CriadoEm { get; set; }

    public DateTimeOffset AtualizadoEm { get; set; }

    public CartaoPessoal Cartao { get; set; } = null!;

    public ICollection<ParcelaCartaoPessoal> Parcelas { get; set; }
        = new List<ParcelaCartaoPessoal>();

    public ICollection<LancamentoPessoal> Lancamentos { get; set; }
        = new List<LancamentoPessoal>();
}