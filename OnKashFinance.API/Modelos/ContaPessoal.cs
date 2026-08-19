namespace OnKashFinance.API.Modelos;

public class ContaPessoal
{
    public Guid Id { get; set; }

    public Guid UsuarioId { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;

    public decimal SaldoInicial { get; set; }

    public bool Ativo { get; set; } = true;

    public DateTimeOffset CriadoEm { get; set; }

    public DateTimeOffset AtualizadoEm { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public ICollection<LancamentoPessoal> Lancamentos { get; set; }
        = new List<LancamentoPessoal>();
}