namespace OnKashFinance.API.DTOs;

public class SolicitarDireitoRequest
{
    public string Tipo { get; set; } = string.Empty;
    public string? Detalhes { get; set; }
}

public class CorrigirPerfilRequest
{
    public string Nome { get; set; } = string.Empty;
}

public record SolicitacaoPrivacidadeResposta(
    string Protocolo,
    string Tipo,
    string Status,
    string? Detalhes,
    DateTimeOffset CriadoEm,
    DateTimeOffset? ConcluidoEm);

public class PrivacidadeResumoResposta
{
    public string Controlador { get; set; } = "Pedro Henrique Benvento";
    public string Marca { get; set; } = "OnKash Finance";
    public string Localizacao { get; set; } = "Curitiba/PR";
    public string Canal { get; set; } = "onkashfinance@gmail.com";
    public string VersaoAtual { get; set; } = GovernancaPrivacidade.VersaoAtual;
    public bool AceiteAtual { get; set; }
    public DateTimeOffset? AceitoEm { get; set; }
    public IReadOnlyList<SolicitacaoPrivacidadeResposta> Solicitacoes { get; set; } = [];
}

public static class GovernancaPrivacidade
{
    public const string VersaoAtual = "2026-08-24";
    public static readonly string[] TiposSolicitacao =
    ["ACESSO", "CORRECAO", "EXCLUSAO", "ANONIMIZACAO", "BLOQUEIO", "PORTABILIDADE", "REVOGACAO", "INFORMACOES"];
}
