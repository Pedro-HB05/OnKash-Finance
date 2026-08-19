using Microsoft.EntityFrameworkCore;
using OnKashFinance.API.Modelos;

namespace OnKashFinance.API.Dados;

public static class MapeamentoPessoal
{
    public static void Configurar(ModelBuilder modelBuilder)
    {
        ConfigurarConta(modelBuilder);
        ConfigurarCategoria(modelBuilder);
        ConfigurarCartao(modelBuilder);
        ConfigurarFatura(modelBuilder);
        ConfigurarCompraCartao(modelBuilder);
        ConfigurarParcelaCartao(modelBuilder);
        ConfigurarLancamento(modelBuilder);
    }

    private static void ConfigurarConta(ModelBuilder modelBuilder)
    {
        var entidade = modelBuilder.Entity<ContaPessoal>();

        entidade.ToTable("contas_pessoais");
        entidade.HasKey(x => x.Id);

        entidade.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        entidade.Property(x => x.UsuarioId).HasColumnName("usuario_id");

        entidade.Property(x => x.Nome)
            .HasColumnName("nome")
            .HasMaxLength(100)
            .IsRequired();

        entidade.Property(x => x.Tipo)
            .HasColumnName("tipo")
            .HasMaxLength(50)
            .IsRequired();

        entidade.Property(x => x.SaldoInicial)
            .HasColumnName("saldo_inicial")
            .HasPrecision(15, 2)
            .HasDefaultValue(0m);

        entidade.Property(x => x.Ativo)
            .HasColumnName("ativo")
            .HasDefaultValue(true);

        ConfigurarAuditoria(entidade);

        entidade.HasIndex(x => new { x.UsuarioId, x.Nome })
            .IsUnique()
            .HasDatabaseName("uq_conta_pessoal_nome");

        entidade.HasOne(x => x.Usuario)
            .WithMany(x => x.ContasPessoais)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigurarCategoria(ModelBuilder modelBuilder)
    {
        var entidade = modelBuilder.Entity<CategoriaPessoal>();

        entidade.ToTable("categorias_pessoais");
        entidade.HasKey(x => x.Id);

        entidade.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        entidade.Property(x => x.UsuarioId).HasColumnName("usuario_id");

        entidade.Property(x => x.Nome)
            .HasColumnName("nome")
            .HasMaxLength(100)
            .IsRequired();

        entidade.Property(x => x.Tipo)
            .HasColumnName("tipo")
            .HasColumnType("tipo_categoria")
            .IsRequired();

        entidade.Property(x => x.Padrao)
            .HasColumnName("padrao")
            .HasDefaultValue(false);

        entidade.Property(x => x.Ativo)
            .HasColumnName("ativo")
            .HasDefaultValue(true);

        ConfigurarAuditoria(entidade);

        entidade.HasOne(x => x.Usuario)
            .WithMany(x => x.CategoriasPessoais)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigurarCartao(ModelBuilder modelBuilder)
    {
        var entidade = modelBuilder.Entity<CartaoPessoal>();

        entidade.ToTable("cartoes_pessoais");
        entidade.HasKey(x => x.Id);

        entidade.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        entidade.Property(x => x.UsuarioId).HasColumnName("usuario_id");

        entidade.Property(x => x.Nome)
            .HasColumnName("nome")
            .HasMaxLength(100)
            .IsRequired();

        entidade.Property(x => x.Instituicao)
            .HasColumnName("instituicao")
            .HasMaxLength(120)
            .IsRequired();

        entidade.Property(x => x.Limite)
            .HasColumnName("limite")
            .HasPrecision(15, 2)
            .HasDefaultValue(0m);

        entidade.Property(x => x.DiaFechamento).HasColumnName("dia_fechamento");
        entidade.Property(x => x.DiaVencimento).HasColumnName("dia_vencimento");

        entidade.Property(x => x.Ativo)
            .HasColumnName("ativo")
            .HasDefaultValue(true);

        ConfigurarAuditoria(entidade);

        entidade.HasIndex(x => new { x.UsuarioId, x.Nome })
            .IsUnique()
            .HasDatabaseName("uq_cartao_pessoal_nome");

        entidade.HasOne(x => x.Usuario)
            .WithMany(x => x.CartoesPessoais)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigurarFatura(ModelBuilder modelBuilder)
    {
        var entidade = modelBuilder.Entity<FaturaPessoal>();

        entidade.ToTable("faturas_pessoais");
        entidade.HasKey(x => x.Id);

        entidade.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        entidade.Property(x => x.CartaoId).HasColumnName("cartao_id");
        entidade.Property(x => x.Competencia).HasColumnName("competencia").HasColumnType("date");
        entidade.Property(x => x.DataFechamento).HasColumnName("data_fechamento").HasColumnType("date");
        entidade.Property(x => x.DataVencimento).HasColumnName("data_vencimento").HasColumnType("date");

        entidade.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("status_fatura")
            .HasDefaultValue(StatusFatura.ABERTA);

        ConfigurarAuditoria(entidade);

        entidade.HasIndex(x => new { x.CartaoId, x.Competencia })
            .IsUnique()
            .HasDatabaseName("uq_fatura_cartao_competencia");

        entidade.HasOne(x => x.Cartao)
            .WithMany(x => x.Faturas)
            .HasForeignKey(x => x.CartaoId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigurarCompraCartao(ModelBuilder modelBuilder)
    {
        var entidade = modelBuilder.Entity<CompraCartaoPessoal>();

        entidade.ToTable("compras_cartao_pessoais");
        entidade.HasKey(x => x.Id);

        entidade.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        entidade.Property(x => x.CartaoId).HasColumnName("cartao_id");
        entidade.Property(x => x.CategoriaId).HasColumnName("categoria_id");

        entidade.Property(x => x.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(200)
            .IsRequired();

        entidade.Property(x => x.ValorTotal)
            .HasColumnName("valor_total")
            .HasPrecision(15, 2);

        entidade.Property(x => x.DataCompra)
            .HasColumnName("data_compra")
            .HasColumnType("date");

        entidade.Property(x => x.NumeroParcelas)
            .HasColumnName("numero_parcelas")
            .HasDefaultValue(1);

        entidade.Property(x => x.ValorParcela)
            .HasColumnName("valor_parcela")
            .HasPrecision(15, 2);

        entidade.Property(x => x.Observacao)
            .HasColumnName("observacao")
            .HasColumnType("text");

        entidade.Property(x => x.Cancelada)
            .HasColumnName("cancelada")
            .HasDefaultValue(false);

        entidade.Property(x => x.CanceladaEm)
            .HasColumnName("cancelada_em")
            .HasColumnType("timestamp with time zone");

        ConfigurarAuditoria(entidade);

        entidade.HasOne(x => x.Cartao)
            .WithMany(x => x.Compras)
            .HasForeignKey(x => x.CartaoId)
            .OnDelete(DeleteBehavior.Cascade);

        entidade.HasOne(x => x.Categoria)
            .WithMany(x => x.ComprasCartao)
            .HasForeignKey(x => x.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigurarParcelaCartao(ModelBuilder modelBuilder)
    {
        var entidade = modelBuilder.Entity<ParcelaCartaoPessoal>();

        entidade.ToTable("parcelas_cartao_pessoais");
        entidade.HasKey(x => x.Id);

        entidade.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        entidade.Property(x => x.CompraId).HasColumnName("compra_id");
        entidade.Property(x => x.FaturaId).HasColumnName("fatura_id");
        entidade.Property(x => x.NumeroParcela).HasColumnName("numero_parcela");

        entidade.Property(x => x.Valor)
            .HasColumnName("valor")
            .HasPrecision(15, 2);

        entidade.Property(x => x.DataVencimento)
            .HasColumnName("data_vencimento")
            .HasColumnType("date");

        ConfigurarAuditoria(entidade);

        entidade.HasIndex(x => new { x.CompraId, x.NumeroParcela })
            .IsUnique()
            .HasDatabaseName("uq_parcela_compra_numero");

        entidade.HasOne(x => x.Compra)
            .WithMany(x => x.Parcelas)
            .HasForeignKey(x => x.CompraId)
            .OnDelete(DeleteBehavior.Cascade);

        entidade.HasOne(x => x.Fatura)
            .WithMany(x => x.Parcelas)
            .HasForeignKey(x => x.FaturaId)
            .OnDelete(DeleteBehavior.SetNull);
    }

    private static void ConfigurarLancamento(ModelBuilder modelBuilder)
    {
        var entidade = modelBuilder.Entity<LancamentoPessoal>();

        entidade.ToTable("lancamentos_pessoais");
        entidade.HasKey(x => x.Id);

        entidade.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        entidade.Property(x => x.UsuarioId).HasColumnName("usuario_id");
        entidade.Property(x => x.ContaId).HasColumnName("conta_id");
        entidade.Property(x => x.CategoriaId).HasColumnName("categoria_id");
        entidade.Property(x => x.FaturaId).HasColumnName("fatura_id");

        entidade.Property(x => x.Tipo)
            .HasColumnName("tipo")
            .HasColumnType("tipo_lancamento_pessoal");

        entidade.Property(x => x.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(200)
            .IsRequired();

        entidade.Property(x => x.Valor)
            .HasColumnName("valor")
            .HasPrecision(15, 2);

        entidade.Property(x => x.Data)
            .HasColumnName("data")
            .HasColumnType("date");

        entidade.Property(x => x.Observacao)
            .HasColumnName("observacao")
            .HasColumnType("text");

        entidade.Property(x => x.Cancelado)
            .HasColumnName("cancelado")
            .HasDefaultValue(false);

        entidade.Property(x => x.CanceladoEm)
            .HasColumnName("cancelado_em")
            .HasColumnType("timestamp with time zone");

        ConfigurarAuditoria(entidade);

        entidade.HasOne(x => x.Usuario)
            .WithMany(x => x.LancamentosPessoais)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        entidade.HasOne(x => x.Conta)
            .WithMany(x => x.Lancamentos)
            .HasForeignKey(x => x.ContaId)
            .OnDelete(DeleteBehavior.Restrict);

        entidade.HasOne(x => x.Categoria)
            .WithMany(x => x.Lancamentos)
            .HasForeignKey(x => x.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        entidade.HasOne(x => x.Fatura)
            .WithMany(x => x.Lancamentos)
            .HasForeignKey(x => x.FaturaId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigurarAuditoria<TEntity>(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> entidade)
        where TEntity : class
    {
        entidade.Property("CriadoEm")
            .HasColumnName("criado_em")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");

        entidade.Property("AtualizadoEm")
            .HasColumnName("atualizado_em")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");
    }
}