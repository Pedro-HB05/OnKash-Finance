using OnKashFinance.API.Modelos;

namespace OnKashFinance.API.DTOs;

public class CriarContaPessoalRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public decimal SaldoInicial { get; set; }
}

public class AtualizarContaPessoalRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}

public class ContaPessoalResposta
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public decimal SaldoInicial { get; set; }
    public decimal SaldoAtual { get; set; }
    public bool Ativo { get; set; }
}

public class CriarCategoriaPessoalRequest
{
    public string Nome { get; set; } = string.Empty;
    public TipoCategoria Tipo { get; set; }
}

public class AtualizarCategoriaPessoalRequest
{
    public string Nome { get; set; } = string.Empty;
    public TipoCategoria Tipo { get; set; }
    public bool Ativo { get; set; }
}

public class CategoriaPessoalResposta
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public TipoCategoria Tipo { get; set; }
    public bool Padrao { get; set; }
    public bool Ativo { get; set; }
}

public class CriarLancamentoPessoalRequest
{
    public Guid ContaId { get; set; }
    public Guid? CategoriaId { get; set; }
    public TipoLancamentoPessoal Tipo { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateOnly Data { get; set; }
    public string? Observacao { get; set; }
}

public class AtualizarLancamentoPessoalRequest
{
    public Guid ContaId { get; set; }
    public Guid? CategoriaId { get; set; }
    public TipoLancamentoPessoal Tipo { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateOnly Data { get; set; }
    public string? Observacao { get; set; }
}

public class LancamentoPessoalResposta
{
    public Guid Id { get; set; }
    public Guid ContaId { get; set; }
    public string Conta { get; set; } = string.Empty;
    public Guid? CategoriaId { get; set; }
    public string? Categoria { get; set; }
    public TipoLancamentoPessoal Tipo { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateOnly Data { get; set; }
    public string? Observacao { get; set; }
    public bool Cancelado { get; set; }
}