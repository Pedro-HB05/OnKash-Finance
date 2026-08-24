namespace OnKashFinance.API.DTOs;

public record LimiteUsoResposta(string Chave, string Nome, long Utilizado, long? Limite, string Unidade);

public record PlanoOfertaResposta(
    string Codigo,
    string Nome,
    string Descricao,
    bool Atual,
    bool Disponivel,
    bool Destaque,
    IReadOnlyList<string> Recursos);

public class AssinaturaResumoResposta
{
    public string Plano { get; set; } = "GRATUITO";
    public string NomePlano { get; set; } = "Grátis";
    public string Status { get; set; } = "ATIVA";
    public DateTimeOffset? PeriodoAtualFim { get; set; }
    public IReadOnlyList<LimiteUsoResposta> Uso { get; set; } = [];
    public IReadOnlyList<PlanoOfertaResposta> Planos { get; set; } = [];
    public bool PossuiSolicitacaoPendente { get; set; }
}

public class SolicitarUpgradeRequest
{
    public string Plano { get; set; } = string.Empty;
}
