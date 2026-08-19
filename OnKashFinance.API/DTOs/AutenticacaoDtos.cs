using OnKashFinance.API.Modelos;

namespace OnKashFinance.API.DTOs;

public class CadastroRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public TipoContaUsuario TipoConta { get; set; }
    public string? NomeEmpresa { get; set; }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class LoginResposta
{
    public string Token { get; set; } = string.Empty;
    public Guid UsuarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public TipoContaUsuario TipoConta { get; set; }
    public Guid? EmpresaId { get; set; }
    public PerfilEmpresa? Perfil { get; set; }
}