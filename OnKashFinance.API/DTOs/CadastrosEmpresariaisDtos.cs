using OnKashFinance.API.Modelos;

namespace OnKashFinance.API.DTOs;

public class CriarContaEmpresarialRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public decimal SaldoInicial { get; set; }
}

public class AtualizarContaEmpresarialRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}

public class ContaEmpresarialResposta
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public decimal SaldoInicial { get; set; }
    public decimal SaldoAtual { get; set; }
    public bool Ativo { get; set; }
}

public class CriarCategoriaEmpresarialRequest
{
    public string Nome { get; set; } = string.Empty;
    public TipoCategoria Tipo { get; set; }
}

public class AtualizarCategoriaEmpresarialRequest
{
    public string Nome { get; set; } = string.Empty;
    public TipoCategoria Tipo { get; set; }
    public bool Ativo { get; set; }
}

public class CategoriaEmpresarialResposta
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public TipoCategoria Tipo { get; set; }
    public bool Padrao { get; set; }
    public bool Ativo { get; set; }
}

public class CriarClienteRequest
{
    public string NomeRazaoSocial { get; set; } = string.Empty;
    public string? CpfCnpj { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Observacao { get; set; }
}

public class AtualizarClienteRequest
{
    public string NomeRazaoSocial { get; set; } = string.Empty;
    public string? CpfCnpj { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Observacao { get; set; }
    public bool Ativo { get; set; }
}

public class ClienteResposta
{
    public Guid Id { get; set; }
    public string NomeRazaoSocial { get; set; } = string.Empty;
    public string? CpfCnpj { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Observacao { get; set; }
    public bool Ativo { get; set; }
}

public class CriarFornecedorRequest
{
    public string NomeRazaoSocial { get; set; } = string.Empty;
    public string? CpfCnpj { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Observacao { get; set; }
}

public class AtualizarFornecedorRequest
{
    public string NomeRazaoSocial { get; set; } = string.Empty;
    public string? CpfCnpj { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Observacao { get; set; }
    public bool Ativo { get; set; }
}

public class FornecedorResposta
{
    public Guid Id { get; set; }
    public string NomeRazaoSocial { get; set; } = string.Empty;
    public string? CpfCnpj { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Observacao { get; set; }
    public bool Ativo { get; set; }
}