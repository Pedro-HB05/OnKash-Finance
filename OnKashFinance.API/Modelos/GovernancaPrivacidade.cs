namespace OnKashFinance.API.Modelos;

public class AceiteLegal
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string PoliticaPrivacidadeVersao { get; set; } = string.Empty;
    public string TermosUsoVersao { get; set; } = string.Empty;
    public DateTimeOffset AceitoEm { get; set; }
    public string? EnderecoIp { get; set; }
    public string? AgenteUsuario { get; set; }
}

public class SolicitacaoPrivacidade
{
    public Guid Id { get; set; }
    public string Protocolo { get; set; } = string.Empty;
    public Guid UsuarioId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string? Detalhes { get; set; }
    public string Status { get; set; } = "RECEBIDA";
    public DateTimeOffset CriadoEm { get; set; }
    public DateTimeOffset AtualizadoEm { get; set; }
    public DateTimeOffset? ConcluidoEm { get; set; }
}

public class AuditoriaOperacao
{
    public long Id { get; set; }
    public Guid? UsuarioId { get; set; }
    public Guid? EmpresaId { get; set; }
    public string Metodo { get; set; } = string.Empty;
    public string Caminho { get; set; } = string.Empty;
    public int StatusHttp { get; set; }
    public string? EnderecoIp { get; set; }
    public string? AgenteUsuario { get; set; }
    public DateTimeOffset CriadoEm { get; set; }
}
