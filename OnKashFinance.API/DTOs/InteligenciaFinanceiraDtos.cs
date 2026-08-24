namespace OnKashFinance.API.DTOs;

public class MovimentoImportacaoRequest
{
    public DateOnly Data { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}

public class ImportarExtratoRequest
{
    public Guid ContaId { get; set; }
    public string ArquivoOrigem { get; set; } = string.Empty;
    public List<MovimentoImportacaoRequest> Movimentos { get; set; } = [];
}

public class ResultadoImportacaoResposta
{
    public int Importados { get; set; }
    public int Conciliados { get; set; }
    public int Duplicados { get; set; }
}

public class PontoProjecaoResposta
{
    public DateOnly Data { get; set; }
    public decimal Entradas { get; set; }
    public decimal Saidas { get; set; }
    public decimal SaldoProjetado { get; set; }
}

public class ProjecaoCaixaResposta
{
    public decimal SaldoAtual { get; set; }
    public decimal SaldoProjetado { get; set; }
    public List<PontoProjecaoResposta> Pontos { get; set; } = [];
}

public class LinhaDreResposta
{
    public string Categoria { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}

public class DreSimplificadaResposta
{
    public DateOnly Inicio { get; set; }
    public DateOnly Fim { get; set; }
    public decimal ReceitaBruta { get; set; }
    public decimal Despesas { get; set; }
    public decimal Resultado { get; set; }
    public decimal Margem { get; set; }
    public List<LinhaDreResposta> ReceitasPorCategoria { get; set; } = [];
    public List<LinhaDreResposta> DespesasPorCategoria { get; set; } = [];
}

public class AnexoFinanceiroResposta
{
    public Guid Id { get; set; }
    public string NomeArquivo { get; set; } = string.Empty;
    public string TipoConteudo { get; set; } = string.Empty;
    public long Tamanho { get; set; }
    public DateTimeOffset CriadoEm { get; set; }
}
