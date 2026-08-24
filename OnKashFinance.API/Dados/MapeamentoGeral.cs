using Microsoft.EntityFrameworkCore;
using OnKashFinance.API.Modelos;

namespace OnKashFinance.API.Dados;

public static class MapeamentoGeral
{
    public static void Configurar(ModelBuilder modelBuilder)
    {
        ConfigurarUsuario(modelBuilder);
        ConfigurarEmpresa(modelBuilder);
        ConfigurarEmpresaUsuario(modelBuilder);
        ConfigurarPermissaoEmpresa(modelBuilder);
    }

    private static void ConfigurarUsuario(ModelBuilder modelBuilder)
    {
        var entidade = modelBuilder.Entity<Usuario>();

        entidade.ToTable("usuarios");

        entidade.HasKey(x => x.Id);

        entidade.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        entidade.Property(x => x.Nome)
            .HasColumnName("nome")
            .HasMaxLength(150)
            .IsRequired();

        entidade.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        entidade.Property(x => x.SenhaHash)
            .HasColumnName("senha_hash")
            .HasColumnType("text")
            .IsRequired();

        entidade.Property(x => x.TipoConta)
            .HasColumnName("tipo_conta")
            .HasColumnType("tipo_conta_usuario")
            .IsRequired();

        entidade.Property(x => x.Ativo)
            .HasColumnName("ativo")
            .HasDefaultValue(true);

        entidade.Property(x => x.EmailVerificado)
            .HasColumnName("email_verificado")
            .HasDefaultValue(false)
            .IsRequired();

        entidade.Property(x => x.CodigoVerificacaoEmail)
            .HasColumnName("codigo_verificacao_email")
            .HasMaxLength(6);

        entidade.Property(x => x.CodigoVerificacaoExpiraEm)
            .HasColumnName("codigo_verificacao_expira_em")
            .HasColumnType("timestamp with time zone");

        entidade.Property(x => x.CriadoEm)
            .HasColumnName("criado_em")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");

        entidade.Property(x => x.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");

        entidade.HasIndex(x => x.Email)
            .HasDatabaseName("ux_usuarios_email");
    }

    private static void ConfigurarEmpresa(ModelBuilder modelBuilder)
    {
        var entidade = modelBuilder.Entity<Empresa>();

        entidade.ToTable("empresas");

        entidade.HasKey(x => x.Id);

        entidade.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        entidade.Property(x => x.Nome)
            .HasColumnName("nome")
            .HasMaxLength(200)
            .IsRequired();

        entidade.Property(x => x.Ativo)
            .HasColumnName("ativo")
            .HasDefaultValue(true);

        entidade.Property(x => x.CriadoEm)
            .HasColumnName("criado_em")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");

        entidade.Property(x => x.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");
    }

    private static void ConfigurarEmpresaUsuario(ModelBuilder modelBuilder)
    {
        var entidade = modelBuilder.Entity<EmpresaUsuario>();

        entidade.ToTable("empresa_usuarios");

        entidade.HasKey(x => x.Id);

        entidade.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        entidade.Property(x => x.EmpresaId)
            .HasColumnName("empresa_id");

        entidade.Property(x => x.UsuarioId)
            .HasColumnName("usuario_id");

        entidade.Property(x => x.Perfil)
            .HasColumnName("perfil")
            .HasColumnType("perfil_empresa");

        entidade.Property(x => x.Ativo)
            .HasColumnName("ativo")
            .HasDefaultValue(true);

        entidade.Property(x => x.CriadoEm)
            .HasColumnName("criado_em")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");

        entidade.Property(x => x.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");

        entidade.HasIndex(x => new { x.EmpresaId, x.UsuarioId })
            .IsUnique()
            .HasDatabaseName("uq_empresa_usuario");

        entidade.HasOne(x => x.Empresa)
            .WithMany(x => x.Usuarios)
            .HasForeignKey(x => x.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        entidade.HasOne(x => x.Usuario)
            .WithMany(x => x.EmpresasUsuario)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigurarPermissaoEmpresa(ModelBuilder modelBuilder)
    {
        var entidade = modelBuilder.Entity<PermissaoEmpresa>();

        entidade.ToTable("permissoes_empresa");

        entidade.HasKey(x => x.Id);

        entidade.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        entidade.Property(x => x.EmpresaUsuarioId)
            .HasColumnName("empresa_usuario_id");

        entidade.Property(x => x.Dashboard)
            .HasColumnName("dashboard");

        entidade.Property(x => x.Lancamentos)
            .HasColumnName("lancamentos");

        entidade.Property(x => x.Contas)
            .HasColumnName("contas");

        entidade.Property(x => x.Clientes)
            .HasColumnName("clientes");

        entidade.Property(x => x.Fornecedores)
            .HasColumnName("fornecedores");

        entidade.Property(x => x.ContasPagar)
            .HasColumnName("contas_pagar");

        entidade.Property(x => x.ContasReceber)
            .HasColumnName("contas_receber");

        entidade.Property(x => x.Categorias)
            .HasColumnName("categorias");

        entidade.Property(x => x.Relatorios)
            .HasColumnName("relatorios");

        entidade.Property(x => x.Usuarios)
            .HasColumnName("usuarios");

        entidade.Property(x => x.CriadoEm)
            .HasColumnName("criado_em")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");

        entidade.Property(x => x.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");

        entidade.HasIndex(x => x.EmpresaUsuarioId)
            .IsUnique();

        entidade.HasOne(x => x.EmpresaUsuario)
            .WithOne(x => x.Permissoes)
            .HasForeignKey<PermissaoEmpresa>(
                x => x.EmpresaUsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}