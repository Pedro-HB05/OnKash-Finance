using Microsoft.EntityFrameworkCore;
using OnKashFinance.API.Modelos;

namespace OnKashFinance.API.Dados;

public static class MapeamentoPlanejamento
{
    public static void Configurar(ModelBuilder modelBuilder)
    {
        var orcamento = modelBuilder.Entity<OrcamentoPessoal>();
        orcamento.ToTable("orcamentos_pessoais");
        orcamento.HasKey(x => x.Id);
        orcamento.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        orcamento.Property(x => x.UsuarioId).HasColumnName("usuario_id");
        orcamento.Property(x => x.CategoriaId).HasColumnName("categoria_id");
        orcamento.Property(x => x.Mes).HasColumnName("mes").HasColumnType("date");
        orcamento.Property(x => x.Limite).HasColumnName("limite").HasPrecision(15, 2);
        Auditoria(orcamento);
        orcamento.HasIndex(x => new { x.UsuarioId, x.CategoriaId, x.Mes }).IsUnique();
        orcamento.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        orcamento.HasOne(x => x.Categoria).WithMany().HasForeignKey(x => x.CategoriaId).OnDelete(DeleteBehavior.Restrict);

        var recorrencia = modelBuilder.Entity<LancamentoRecorrentePessoal>();
        recorrencia.ToTable("lancamentos_recorrentes_pessoais");
        recorrencia.HasKey(x => x.Id);
        recorrencia.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        recorrencia.Property(x => x.UsuarioId).HasColumnName("usuario_id");
        recorrencia.Property(x => x.ContaId).HasColumnName("conta_id");
        recorrencia.Property(x => x.CategoriaId).HasColumnName("categoria_id");
        recorrencia.Property(x => x.Tipo).HasColumnName("tipo").HasColumnType("tipo_lancamento_pessoal");
        recorrencia.Property(x => x.Descricao).HasColumnName("descricao").HasMaxLength(200);
        recorrencia.Property(x => x.Valor).HasColumnName("valor").HasPrecision(15, 2);
        recorrencia.Property(x => x.Frequencia).HasColumnName("frequencia").HasMaxLength(20);
        recorrencia.Property(x => x.ProximaExecucao).HasColumnName("proxima_execucao").HasColumnType("date");
        recorrencia.Property(x => x.Ativo).HasColumnName("ativo").HasDefaultValue(true);
        Auditoria(recorrencia);
        recorrencia.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        recorrencia.HasOne(x => x.Conta).WithMany().HasForeignKey(x => x.ContaId).OnDelete(DeleteBehavior.Restrict);
        recorrencia.HasOne(x => x.Categoria).WithMany().HasForeignKey(x => x.CategoriaId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void Auditoria<TEntity>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> entidade) where TEntity : class
    {
        entidade.Property("CriadoEm").HasColumnName("criado_em").HasColumnType("timestamp with time zone").HasDefaultValueSql("NOW()");
        entidade.Property("AtualizadoEm").HasColumnName("atualizado_em").HasColumnType("timestamp with time zone").HasDefaultValueSql("NOW()");
    }
}
