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
        ConfigurarAssinaturas(modelBuilder);
        ConfigurarGovernancaPrivacidade(modelBuilder);
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

    private static void ConfigurarAssinaturas(ModelBuilder modelBuilder)
    {
        var assinatura = modelBuilder.Entity<AssinaturaUsuario>();
        assinatura.ToTable("assinaturas_usuario");
        assinatura.HasKey(x => x.Id);
        assinatura.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        assinatura.Property(x => x.UsuarioId).HasColumnName("usuario_id");
        assinatura.Property(x => x.Plano).HasColumnName("plano").HasMaxLength(20).IsRequired();
        assinatura.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        assinatura.Property(x => x.PeriodoAtualInicio).HasColumnName("periodo_atual_inicio").HasColumnType("timestamp with time zone");
        assinatura.Property(x => x.PeriodoAtualFim).HasColumnName("periodo_atual_fim").HasColumnType("timestamp with time zone");
        assinatura.Property(x => x.Provedor).HasColumnName("provedor").HasMaxLength(40);
        assinatura.Property(x => x.ReferenciaExterna).HasColumnName("referencia_externa").HasMaxLength(180);
        assinatura.Property(x => x.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("NOW()");
        assinatura.Property(x => x.AtualizadoEm).HasColumnName("atualizado_em").HasDefaultValueSql("NOW()");
        assinatura.HasIndex(x => x.UsuarioId).IsUnique().HasDatabaseName("ux_assinaturas_usuario_usuario");
        assinatura.HasOne(x => x.Usuario).WithOne(x => x.Assinatura).HasForeignKey<AssinaturaUsuario>(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);

        var solicitacao = modelBuilder.Entity<SolicitacaoUpgrade>();
        solicitacao.ToTable("solicitacoes_upgrade");
        solicitacao.HasKey(x => x.Id);
        solicitacao.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        solicitacao.Property(x => x.UsuarioId).HasColumnName("usuario_id");
        solicitacao.Property(x => x.EmpresaId).HasColumnName("empresa_id");
        solicitacao.Property(x => x.PlanoDesejado).HasColumnName("plano_desejado").HasMaxLength(20).IsRequired();
        solicitacao.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        solicitacao.Property(x => x.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("NOW()");
        solicitacao.Property(x => x.AtualizadoEm).HasColumnName("atualizado_em").HasDefaultValueSql("NOW()");
        solicitacao.HasIndex(x => new { x.UsuarioId, x.Status }).HasDatabaseName("ix_solicitacoes_upgrade_usuario_status");
    }

    private static void ConfigurarGovernancaPrivacidade(ModelBuilder modelBuilder)
    {
        var aceite = modelBuilder.Entity<AceiteLegal>();
        aceite.ToTable("aceites_legais"); aceite.HasKey(x => x.Id);
        aceite.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        aceite.Property(x => x.UsuarioId).HasColumnName("usuario_id");
        aceite.Property(x => x.PoliticaPrivacidadeVersao).HasColumnName("politica_privacidade_versao").HasMaxLength(30).IsRequired();
        aceite.Property(x => x.TermosUsoVersao).HasColumnName("termos_uso_versao").HasMaxLength(30).IsRequired();
        aceite.Property(x => x.AceitoEm).HasColumnName("aceito_em").HasDefaultValueSql("NOW()");
        aceite.Property(x => x.EnderecoIp).HasColumnName("endereco_ip").HasMaxLength(64);
        aceite.Property(x => x.AgenteUsuario).HasColumnName("agente_usuario").HasMaxLength(300);
        aceite.HasIndex(x => new { x.UsuarioId, x.PoliticaPrivacidadeVersao, x.TermosUsoVersao }).IsUnique().HasDatabaseName("ux_aceites_legais_usuario_versoes");
        aceite.HasOne<Usuario>().WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);

        var solicitacao = modelBuilder.Entity<SolicitacaoPrivacidade>();
        solicitacao.ToTable("solicitacoes_privacidade"); solicitacao.HasKey(x => x.Id);
        solicitacao.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        solicitacao.Property(x => x.Protocolo).HasColumnName("protocolo").HasMaxLength(40).IsRequired();
        solicitacao.Property(x => x.UsuarioId).HasColumnName("usuario_id");
        solicitacao.Property(x => x.Tipo).HasColumnName("tipo").HasMaxLength(30).IsRequired();
        solicitacao.Property(x => x.Detalhes).HasColumnName("detalhes").HasMaxLength(2000);
        solicitacao.Property(x => x.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
        solicitacao.Property(x => x.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("NOW()");
        solicitacao.Property(x => x.AtualizadoEm).HasColumnName("atualizado_em").HasDefaultValueSql("NOW()");
        solicitacao.Property(x => x.ConcluidoEm).HasColumnName("concluido_em");
        solicitacao.HasIndex(x => x.Protocolo).IsUnique().HasDatabaseName("ux_solicitacoes_privacidade_protocolo");
        solicitacao.HasIndex(x => new { x.UsuarioId, x.Status }).HasDatabaseName("ix_solicitacoes_privacidade_usuario_status");
        solicitacao.HasOne<Usuario>().WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);

        var auditoria = modelBuilder.Entity<AuditoriaOperacao>();
        auditoria.ToTable("auditoria_operacoes"); auditoria.HasKey(x => x.Id);
        auditoria.Property(x => x.Id).HasColumnName("id").UseIdentityByDefaultColumn();
        auditoria.Property(x => x.UsuarioId).HasColumnName("usuario_id"); auditoria.Property(x => x.EmpresaId).HasColumnName("empresa_id");
        auditoria.Property(x => x.Metodo).HasColumnName("metodo").HasMaxLength(10).IsRequired();
        auditoria.Property(x => x.Caminho).HasColumnName("caminho").HasMaxLength(300).IsRequired();
        auditoria.Property(x => x.StatusHttp).HasColumnName("status_http");
        auditoria.Property(x => x.EnderecoIp).HasColumnName("endereco_ip").HasMaxLength(64);
        auditoria.Property(x => x.AgenteUsuario).HasColumnName("agente_usuario").HasMaxLength(300);
        auditoria.Property(x => x.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("NOW()");
        auditoria.HasIndex(x => new { x.UsuarioId, x.CriadoEm }).HasDatabaseName("ix_auditoria_operacoes_usuario_data");
    }
}
