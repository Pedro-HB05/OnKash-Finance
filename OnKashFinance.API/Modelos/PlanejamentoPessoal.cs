namespace OnKashFinance.API.Modelos;

public class OrcamentoPessoal
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid CategoriaId { get; set; }
    public DateOnly Mes { get; set; }
    public decimal Limite { get; set; }
    public DateTimeOffset CriadoEm { get; set; }
    public DateTimeOffset AtualizadoEm { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public CategoriaPessoal Categoria { get; set; } = null!;
}

public class LancamentoRecorrentePessoal
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid ContaId { get; set; }
    public Guid CategoriaId { get; set; }
    public TipoLancamentoPessoal Tipo { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Frequencia { get; set; } = "MENSAL";
    public DateOnly ProximaExecucao { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTimeOffset CriadoEm { get; set; }
    public DateTimeOffset AtualizadoEm { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public ContaPessoal Conta { get; set; } = null!;
    public CategoriaPessoal Categoria { get; set; } = null!;
}
