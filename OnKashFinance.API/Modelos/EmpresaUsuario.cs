namespace OnKashFinance.API.Modelos;

public class EmpresaUsuario
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid UsuarioId { get; set; }

    public PerfilEmpresa Perfil { get; set; }
        = PerfilEmpresa.FUNCIONARIO;

    public bool Ativo { get; set; } = true;

    public DateTimeOffset CriadoEm { get; set; }

    public DateTimeOffset AtualizadoEm { get; set; }

    public Empresa Empresa { get; set; } = null!;

    public Usuario Usuario { get; set; } = null!;

    public PermissaoEmpresa? Permissoes { get; set; }
}