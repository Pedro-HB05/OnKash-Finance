using Microsoft.EntityFrameworkCore;
using OnKashFinance.API.Modelos;

namespace OnKashFinance.API.Dados;

public class OnKashDbContext : DbContext
{
    public OnKashDbContext(DbContextOptions<OnKashDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<EmpresaUsuario> EmpresaUsuarios => Set<EmpresaUsuario>();
    public DbSet<PermissaoEmpresa> PermissoesEmpresa => Set<PermissaoEmpresa>();

    public DbSet<ContaPessoal> ContasPessoais => Set<ContaPessoal>();
    public DbSet<CategoriaPessoal> CategoriasPessoais => Set<CategoriaPessoal>();
    public DbSet<CartaoPessoal> CartoesPessoais => Set<CartaoPessoal>();
    public DbSet<FaturaPessoal> FaturasPessoais => Set<FaturaPessoal>();
    public DbSet<CompraCartaoPessoal> ComprasCartaoPessoais => Set<CompraCartaoPessoal>();
    public DbSet<ParcelaCartaoPessoal> ParcelasCartaoPessoais => Set<ParcelaCartaoPessoal>();
    public DbSet<LancamentoPessoal> LancamentosPessoais => Set<LancamentoPessoal>();

    public DbSet<ContaEmpresarial> ContasEmpresariais => Set<ContaEmpresarial>();
    public DbSet<CategoriaEmpresarial> CategoriasEmpresariais => Set<CategoriaEmpresarial>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();
    public DbSet<ContaPagar> ContasPagar => Set<ContaPagar>();
    public DbSet<ContaReceber> ContasReceber => Set<ContaReceber>();
    public DbSet<LancamentoEmpresarial> LancamentosEmpresariais => Set<LancamentoEmpresarial>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        MapeamentoGeral.Configurar(modelBuilder);
        MapeamentoPessoal.Configurar(modelBuilder);
        MapeamentoEmpresarial.Configurar(modelBuilder);
    }
}