using OnKashFinance.API.Modelos;

namespace OnKashFinance.API.DTOs;

public class AdicionarUsuarioEmpresaRequest
{
    public Guid UsuarioId { get; set; }

    public PerfilEmpresa Perfil { get; set; }
        = PerfilEmpresa.FUNCIONARIO;
}

public class AtualizarPerfilEmpresaRequest
{
    public PerfilEmpresa Perfil { get; set; }

    public bool Ativo { get; set; }
}

public class AtualizarPermissoesEmpresaRequest
{
    public bool Dashboard { get; set; }
    public bool Lancamentos { get; set; }
    public bool Contas { get; set; }
    public bool Clientes { get; set; }
    public bool Fornecedores { get; set; }
    public bool ContasPagar { get; set; }
    public bool ContasReceber { get; set; }
    public bool Categorias { get; set; }
    public bool Relatorios { get; set; }
    public bool Usuarios { get; set; }
}

public class UsuarioEmpresaResposta
{
    public Guid EmpresaUsuarioId { get; set; }

    public Guid UsuarioId { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public PerfilEmpresa Perfil { get; set; }

    public bool Ativo { get; set; }

    public AtualizarPermissoesEmpresaRequest? Permissoes { get; set; }
}