namespace OnKashFinance.API.Modelos;

public class MovimentoImportado
{
    public Guid Id { get; set; }
    public string Ambiente { get; set; } = string.Empty;
    public Guid ProprietarioId { get; set; }
    public Guid ContaId { get; set; }
    public Guid LancamentoId { get; set; }
    public string Hash { get; set; } = string.Empty;
    public string ArquivoOrigem { get; set; } = string.Empty;
    public DateOnly Data { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTimeOffset CriadoEm { get; set; }
}

public class AnexoFinanceiro
{
    public Guid Id { get; set; }
    public string Ambiente { get; set; } = string.Empty;
    public Guid ProprietarioId { get; set; }
    public Guid LancamentoId { get; set; }
    public string NomeArquivo { get; set; } = string.Empty;
    public string TipoConteudo { get; set; } = "application/octet-stream";
    public long Tamanho { get; set; }
    public byte[] Conteudo { get; set; } = [];
    public DateTimeOffset CriadoEm { get; set; }
}
