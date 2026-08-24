using Microsoft.EntityFrameworkCore;
using OnKashFinance.API.Modelos;

namespace OnKashFinance.API.Dados;

public static class MapeamentoInteligenciaFinanceira
{
    public static void Configurar(ModelBuilder modelBuilder)
    {
        var movimento = modelBuilder.Entity<MovimentoImportado>();
        movimento.ToTable("movimentos_importados");
        movimento.HasKey(x => x.Id);
        movimento.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        movimento.Property(x => x.Ambiente).HasColumnName("ambiente").HasMaxLength(20);
        movimento.Property(x => x.ProprietarioId).HasColumnName("proprietario_id");
        movimento.Property(x => x.ContaId).HasColumnName("conta_id");
        movimento.Property(x => x.LancamentoId).HasColumnName("lancamento_id");
        movimento.Property(x => x.Hash).HasColumnName("hash").HasMaxLength(64);
        movimento.Property(x => x.ArquivoOrigem).HasColumnName("arquivo_origem").HasMaxLength(255);
        movimento.Property(x => x.Data).HasColumnName("data").HasColumnType("date");
        movimento.Property(x => x.Descricao).HasColumnName("descricao").HasMaxLength(300);
        movimento.Property(x => x.Valor).HasColumnName("valor").HasPrecision(15, 2);
        movimento.Property(x => x.CriadoEm).HasColumnName("criado_em").HasColumnType("timestamp with time zone").HasDefaultValueSql("NOW()");
        movimento.HasIndex(x => new { x.Ambiente, x.ProprietarioId, x.Hash }).IsUnique().HasDatabaseName("uq_movimento_importado_hash");

        var anexo = modelBuilder.Entity<AnexoFinanceiro>();
        anexo.ToTable("anexos_financeiros");
        anexo.HasKey(x => x.Id);
        anexo.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        anexo.Property(x => x.Ambiente).HasColumnName("ambiente").HasMaxLength(20);
        anexo.Property(x => x.ProprietarioId).HasColumnName("proprietario_id");
        anexo.Property(x => x.LancamentoId).HasColumnName("lancamento_id");
        anexo.Property(x => x.NomeArquivo).HasColumnName("nome_arquivo").HasMaxLength(255);
        anexo.Property(x => x.TipoConteudo).HasColumnName("tipo_conteudo").HasMaxLength(150);
        anexo.Property(x => x.Tamanho).HasColumnName("tamanho");
        anexo.Property(x => x.Conteudo).HasColumnName("conteudo").HasColumnType("bytea");
        anexo.Property(x => x.CriadoEm).HasColumnName("criado_em").HasColumnType("timestamp with time zone").HasDefaultValueSql("NOW()");
        anexo.HasIndex(x => new { x.Ambiente, x.ProprietarioId, x.LancamentoId }).HasDatabaseName("ix_anexo_financeiro_lancamento");
    }
}
