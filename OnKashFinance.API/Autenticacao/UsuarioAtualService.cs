using OnKashFinance.API.Modelos;
using System.Security.Claims;

namespace OnKashFinance.API.Autenticacao;

public class UsuarioAtualService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsuarioAtualService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal Usuario
    {
        get
        {
            return _httpContextAccessor
                       .HttpContext?
                       .User
                   ?? throw new UnauthorizedAccessException(
                       "Usuário não autenticado."
                   );
        }
    }

    public Guid ObterUsuarioId()
    {
        var valor = Usuario.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (!Guid.TryParse(valor, out var usuarioId))
        {
            throw new UnauthorizedAccessException(
                "Usuário inválido."
            );
        }

        return usuarioId;
    }

    public string ObterNome()
    {
        return Usuario.FindFirstValue(
                   ClaimTypes.Name
               )
               ?? string.Empty;
    }

    public string ObterEmail()
    {
        return Usuario.FindFirstValue(
                   ClaimTypes.Email
               )
               ?? string.Empty;
    }

    public TipoContaUsuario ObterTipoConta()
    {
        var valor = Usuario.FindFirstValue(
            "tipo_conta"
        );

        if (!Enum.TryParse<TipoContaUsuario>(
                valor,
                true,
                out var tipoConta))
        {
            throw new UnauthorizedAccessException(
                "Tipo de conta inválido."
            );
        }

        return tipoConta;
    }

    public Guid? ObterEmpresaId()
    {
        var valor = Usuario.FindFirstValue(
            "empresa_id"
        );

        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        if (!Guid.TryParse(valor, out var empresaId))
        {
            throw new UnauthorizedAccessException(
                "Empresa inválida."
            );
        }

        return empresaId;
    }

    public Guid ExigirEmpresaId()
    {
        var empresaId = ObterEmpresaId();

        if (!empresaId.HasValue)
        {
            throw new UnauthorizedAccessException(
                "O usuário não está vinculado a uma empresa."
            );
        }

        return empresaId.Value;
    }

    public PerfilEmpresa? ObterPerfilEmpresa()
    {
        var valor = Usuario.FindFirstValue(
            "perfil_empresa"
        );

        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        if (!Enum.TryParse<PerfilEmpresa>(
                valor,
                true,
                out var perfil))
        {
            throw new UnauthorizedAccessException(
                "Perfil empresarial inválido."
            );
        }

        return perfil;
    }

    public bool EhAdministrador()
    {
        return ObterPerfilEmpresa()
            == PerfilEmpresa.ADMINISTRADOR;
    }

    public bool EhPessoal()
    {
        return ObterTipoConta()
            == TipoContaUsuario.PESSOAL;
    }

    public bool EhEmpresarial()
    {
        return ObterTipoConta()
            == TipoContaUsuario.EMPRESARIAL;
    }
}