using OnKashFinance.API.Modelos;

namespace OnKashFinance.API.DTOs;

public class SalvarOrcamentoRequest
{
    public Guid CategoriaId { get; set; }
    public DateOnly Mes { get; set; }
    public decimal Limite { get; set; }
}

public class OrcamentoResposta
{
    public Guid Id { get; set; }
    public Guid CategoriaId { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public DateOnly Mes { get; set; }
    public decimal Limite { get; set; }
    public decimal Utilizado { get; set; }
    public decimal Percentual { get; set; }
}

public class SalvarRecorrenciaRequest
{
    public Guid ContaId { get; set; }
    public Guid CategoriaId { get; set; }
    public TipoLancamentoPessoal Tipo { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Frequencia { get; set; } = "MENSAL";
    public DateOnly ProximaExecucao { get; set; }
    public bool Ativo { get; set; } = true;
}

public class RecorrenciaResposta
{
    public Guid Id { get; set; }
    public Guid ContaId { get; set; }
    public string Conta { get; set; } = string.Empty;
    public Guid CategoriaId { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public TipoLancamentoPessoal Tipo { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Frequencia { get; set; } = string.Empty;
    public DateOnly ProximaExecucao { get; set; }
    public bool Ativo { get; set; }
}

public class AlertaFinanceiroResposta
{
    public string Tipo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Severidade { get; set; } = "INFO";
    public string? Link { get; set; }
}
