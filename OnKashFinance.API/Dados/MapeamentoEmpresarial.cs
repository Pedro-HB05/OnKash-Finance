using Microsoft.EntityFrameworkCore;
using OnKashFinance.API.Modelos;

namespace OnKashFinance.API.Dados;

public static class MapeamentoEmpresarial
{
    public static void Configurar(ModelBuilder modelBuilder)
    {
        ConfigurarConta(modelBuilder);
        ConfigurarCategoria(modelBuilder);
        ConfigurarCliente(modelBuilder);
        ConfigurarFornecedor(modelBuilder);
        ConfigurarContaPagar(modelBuilder);
        ConfigurarContaReceber(modelBuilder);
        ConfigurarLancamento(modelBuilder);
    }

    private static void ConfigurarConta(ModelBuilder modelBuilder)
    {
        var entidade = modelBuilder.Entity<ContaEmpresarial>();

        entidade.ToTable("contas_empresariais");
        entidade.HasKey(x => x.Id);

        entidade.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        entidade.Property(x => x.EmpresaId).HasColumnName("empresa_id");

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

        entidade.HasIndex(x => new { x.EmpresaId, x.Nome })
            .IsUnique()
            .HasDatabaseName("uq_conta_empresarial_nome");

        entidade.HasOne(x => x.Empresa)
            .WithMany(x => x.Contas)
            .HasForeignKey(x => x.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigurarCategoria(ModelBuilder modelBuilder)
    {
        var entidade = modelBuilder.Entity<CategoriaEmpresarial>();

        entidade.ToTable("categorias_empresariais");
        entidade.HasKey(x => x.Id);

        entidade.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        entidade.Property(x => x.EmpresaId).HasColumnName("empresa_id");

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

        entidade.HasOne(x => x.Empresa)
            .WithMany(x => x.Categorias)
            .HasForeignKey(x => x.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigurarCliente(ModelBuilder modelBuilder)
    {
        var entidade = modelBuilder.Entity<Cliente>();

        entidade.ToTable("clientes");
        entidade.HasKey(x => x.Id);

        entidade.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        entidade.Property(x => x.EmpresaId).HasColumnName("empresa_id");

        entidade.Property(x => x.NomeRazaoSocial)
            .HasColumnName("nome_razao_social")
            .HasMaxLength(200)
            .IsRequired();

        entidade.Property(x => x.CpfCnpj)
            .HasColumnName("cpf_cnpj")
            .HasMaxLength(20);

        entidade.Property(x => x.Telefone)
            .HasColumnName("telefone")
            .HasMaxLength(30);

        entidade.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(255);

        entidade.Property(x => x.Observacao)
            .HasColumnName("observacao")
            .HasColumnType("text");

        entidade.Property(x => x.Ativo)
            .HasColumnName("ativo")
            .HasDefaultValue(true);

        ConfigurarAuditoria(entidade);

        entidade.HasOne(x => x.Empresa)
            .WithMany(x => x.Clientes)
            .HasForeignKey(x => x.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigurarFornecedor(ModelBuilder modelBuilder)
    {
        var entidade = modelBuilder.Entity<Fornecedor>();

        entidade.ToTable("fornecedores");
        entidade.HasKey(x => x.Id);

        entidade.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        entidade.Property(x => x.EmpresaId).HasColumnName("empresa_id");

        entidade.Property(x => x.NomeRazaoSocial)
            .HasColumnName("nome_razao_social")
            .HasMaxLength(200)
            .IsRequired();

        entidade.Property(x => x.CpfCnpj)
            .HasColumnName("cpf_cnpj")
            .HasMaxLength(20);

        entidade.Property(x => x.Telefone)
            .HasColumnName("telefone")
            .HasMaxLength(30);

        entidade.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(255);

        entidade.Property(x => x.Observacao)
            .HasColumnName("observacao")
            .HasColumnType("text");

        entidade.Property(x => x.Ativo)
            .HasColumnName("ativo")
            .HasDefaultValue(true);

        ConfigurarAuditoria(entidade);

        entidade.HasOne(x => x.Empresa)
            .WithMany(x => x.Fornecedores)
            .HasForeignKey(x => x.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigurarContaPagar(ModelBuilder modelBuilder)
    {
        var entidade = modelBuilder.Entity<ContaPagar>();

        entidade.ToTable("contas_pagar");
        entidade.HasKey(x => x.Id);

        entidade.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        entidade.Property(x => x.EmpresaId).HasColumnName("empresa_id");
        entidade.Property(x => x.FornecedorId).HasColumnName("fornecedor_id");

        entidade.Property(x => x.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(200)
            .IsRequired();

        entidade.Property(x => x.Valor)
            .HasColumnName("valor")
            .HasPrecision(15, 2);

        entidade.Property(x => x.Vencimento)
            .HasColumnName("vencimento")
            .HasColumnType("date");

        entidade.Property(x => x.DataPagamento)
            .HasColumnName("data_pagamento")
            .HasColumnType("date");

        entidade.Property(x => x.CategoriaId).HasColumnName("categoria_id");
        entidade.Property(x => x.ContaId).HasColumnName("conta_id");

        entidade.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("status_conta_pagar")
            .HasDefaultValue(StatusContaPagar.PENDENTE);

        entidade.Property(x => x.Observacao)
            .HasColumnName("observacao")
            .HasColumnType("text");

        ConfigurarAuditoria(entidade);

        entidade.HasOne(x => x.Empresa)
            .WithMany(x => x.ContasPagar)
            .HasForeignKey(x => x.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        entidade.HasOne(x => x.Fornecedor)
            .WithMany(x => x.ContasPagar)
            .HasForeignKey(x => x.FornecedorId)
            .OnDelete(DeleteBehavior.Restrict);

        entidade.HasOne(x => x.Categoria)
            .WithMany(x => x.ContasPagar)
            .HasForeignKey(x => x.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        entidade.HasOne(x => x.Conta)
            .WithMany(x => x.ContasPagar)
            .HasForeignKey(x => x.ContaId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigurarContaReceber(ModelBuilder modelBuilder)
    {
        var entidade = modelBuilder.Entity<ContaReceber>();

        entidade.ToTable("contas_receber");
        entidade.HasKey(x => x.Id);

        entidade.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        entidade.Property(x => x.EmpresaId).HasColumnName("empresa_id");
        entidade.Property(x => x.ClienteId).HasColumnName("cliente_id");

        entidade.Property(x => x.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(200)
            .IsRequired();

        entidade.Property(x => x.Valor)
            .HasColumnName("valor")
            .HasPrecision(15, 2);

        entidade.Property(x => x.Vencimento)
            .HasColumnName("vencimento")
            .HasColumnType("date");

        entidade.Property(x => x.DataRecebimento)
            .HasColumnName("data_recebimento")
            .HasColumnType("date");

        entidade.Property(x => x.CategoriaId).HasColumnName("categoria_id");
        entidade.Property(x => x.ContaId).HasColumnName("conta_id");

        entidade.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("status_conta_receber")
            .HasDefaultValue(StatusContaReceber.PENDENTE);

        entidade.Property(x => x.Observacao)
            .HasColumnName("observacao")
            .HasColumnType("text");

        ConfigurarAuditoria(entidade);

        entidade.HasOne(x => x.Empresa)
            .WithMany(x => x.ContasReceber)
            .HasForeignKey(x => x.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        entidade.HasOne(x => x.Cliente)
            .WithMany(x => x.ContasReceber)
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        entidade.HasOne(x => x.Categoria)
            .WithMany(x => x.ContasReceber)
            .HasForeignKey(x => x.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        entidade.HasOne(x => x.Conta)
            .WithMany(x => x.ContasReceber)
            .HasForeignKey(x => x.ContaId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigurarLancamento(ModelBuilder modelBuilder)
    {
        var entidade = modelBuilder.Entity<LancamentoEmpresarial>();

        entidade.ToTable("lancamentos_empresariais");
        entidade.HasKey(x => x.Id);

        entidade.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        entidade.Property(x => x.EmpresaId).HasColumnName("empresa_id");

        entidade.Property(x => x.Tipo)
            .HasColumnName("tipo")
            .HasColumnType("tipo_lancamento_empresarial");

        entidade.Property(x => x.ContaId).HasColumnName("conta_id");
        entidade.Property(x => x.ContaDestinoId).HasColumnName("conta_destino_id");
        entidade.Property(x => x.CategoriaId).HasColumnName("categoria_id");
        entidade.Property(x => x.ClienteId).HasColumnName("cliente_id");
        entidade.Property(x => x.FornecedorId).HasColumnName("fornecedor_id");
        entidade.Property(x => x.ContaPagarId).HasColumnName("conta_pagar_id");
        entidade.Property(x => x.ContaReceberId).HasColumnName("conta_receber_id");

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

        entidade.HasOne(x => x.Empresa)
            .WithMany(x => x.Lancamentos)
            .HasForeignKey(x => x.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        entidade.HasOne(x => x.Conta)
            .WithMany(x => x.LancamentosOrigem)
            .HasForeignKey(x => x.ContaId)
            .OnDelete(DeleteBehavior.Restrict);

        entidade.HasOne(x => x.ContaDestino)
            .WithMany(x => x.LancamentosDestino)
            .HasForeignKey(x => x.ContaDestinoId)
            .OnDelete(DeleteBehavior.Restrict);

        entidade.HasOne(x => x.Categoria)
            .WithMany(x => x.Lancamentos)
            .HasForeignKey(x => x.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        entidade.HasOne(x => x.Cliente)
            .WithMany(x => x.Lancamentos)
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        entidade.HasOne(x => x.Fornecedor)
            .WithMany(x => x.Lancamentos)
            .HasForeignKey(x => x.FornecedorId)
            .OnDelete(DeleteBehavior.Restrict);

        entidade.HasOne(x => x.ContaPagar)
            .WithMany(x => x.Lancamentos)
            .HasForeignKey(x => x.ContaPagarId)
            .OnDelete(DeleteBehavior.Restrict);

        entidade.HasOne(x => x.ContaReceber)
            .WithMany(x => x.Lancamentos)
            .HasForeignKey(x => x.ContaReceberId)
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