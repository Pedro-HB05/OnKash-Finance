namespace OnKashFinance.API.Modelos;

public class PermissaoEmpresa
{
    public Guid Id { get; set; }

    public Guid EmpresaUsuarioId { get; set; }

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

    public DateTimeOffset CriadoEm { get; set; }

    public DateTimeOffset AtualizadoEm { get; set; }

    public EmpresaUsuario EmpresaUsuario { get; set; } = null!;
}