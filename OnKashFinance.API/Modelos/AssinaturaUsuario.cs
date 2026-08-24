namespace OnKashFinance.API.Modelos;

public class AssinaturaUsuario
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string Plano { get; set; } = "GRATUITO";
    public string Status { get; set; } = "ATIVA";
    public DateTimeOffset? PeriodoAtualInicio { get; set; }
    public DateTimeOffset? PeriodoAtualFim { get; set; }
    public string? Provedor { get; set; }
    public string? ReferenciaExterna { get; set; }
    public DateTimeOffset CriadoEm { get; set; }
    public DateTimeOffset AtualizadoEm { get; set; }
    public Usuario Usuario { get; set; } = null!;
}

public class SolicitacaoUpgrade
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid? EmpresaId { get; set; }
    public string PlanoDesejado { get; set; } = string.Empty;
    public string Status { get; set; } = "PENDENTE";
    public DateTimeOffset CriadoEm { get; set; }
    public DateTimeOffset AtualizadoEm { get; set; }
}
