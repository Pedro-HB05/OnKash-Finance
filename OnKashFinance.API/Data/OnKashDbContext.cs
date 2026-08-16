using Microsoft.EntityFrameworkCore;
using OnKashFinance.Api.Entities;
using System.Reflection.Emit;

namespace OnKashFinance.Api.Data;

public class OnKashDbContext : DbContext
{
    public OnKashDbContext(DbContextOptions<OnKashDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<OrganizationUser> OrganizationUsers => Set<OrganizationUser>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<FinancialTransaction> Transactions => Set<FinancialTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUsers(modelBuilder);
        ConfigureOrganizations(modelBuilder);
        ConfigureOrganizationUsers(modelBuilder);
        ConfigureCategories(modelBuilder);
        ConfigureTransactions(modelBuilder);
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<User>();

        entity.ToTable("users");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.Name).HasColumnName("name");
        entity.Property(x => x.Email).HasColumnName("email");
        entity.Property(x => x.PasswordHash).HasColumnName("password_hash");
        entity.Property(x => x.IsActive).HasColumnName("is_active");
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        entity.Property(x => x.LastLoginAt).HasColumnName("last_login_at");
    }

    private static void ConfigureOrganizations(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Organization>();

        entity.ToTable("organizations");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.Name).HasColumnName("name");
        entity.Property(x => x.Type).HasColumnName("type");
        entity.Property(x => x.InitialBalance)
            .HasColumnName("initial_balance")
            .HasPrecision(18, 2);

        entity.Property(x => x.InitialBalanceDate)
            .HasColumnName("initial_balance_date");

        entity.Property(x => x.CreatedBy).HasColumnName("created_by");
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        entity.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureOrganizationUsers(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<OrganizationUser>();

        entity.ToTable("organization_users");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.UserId).HasColumnName("user_id");
        entity.Property(x => x.OrganizationId).HasColumnName("organization_id");
        entity.Property(x => x.Role).HasColumnName("role");
        entity.Property(x => x.IsActive).HasColumnName("is_active");
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");

        entity.HasOne(x => x.User)
            .WithMany(x => x.OrganizationUsers)
            .HasForeignKey(x => x.UserId);

        entity.HasOne(x => x.Organization)
            .WithMany(x => x.OrganizationUsers)
            .HasForeignKey(x => x.OrganizationId);
    }

    private static void ConfigureCategories(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Category>();

        entity.ToTable("categories");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.OrganizationId).HasColumnName("organization_id");
        entity.Property(x => x.Name).HasColumnName("name");
        entity.Property(x => x.Type).HasColumnName("type");
        entity.Property(x => x.ParentCategoryId).HasColumnName("parent_category_id");
        entity.Property(x => x.IsActive).HasColumnName("is_active");
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        entity.HasOne(x => x.Organization)
            .WithMany(x => x.Categories)
            .HasForeignKey(x => x.OrganizationId);

        entity.HasOne(x => x.ParentCategory)
            .WithMany()
            .HasForeignKey(x => x.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureTransactions(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<FinancialTransaction>();

        entity.ToTable("transactions");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.OrganizationId).HasColumnName("organization_id");
        entity.Property(x => x.CategoryId).HasColumnName("category_id");
        entity.Property(x => x.Type).HasColumnName("type");

        entity.Property(x => x.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2);

        entity.Property(x => x.Description).HasColumnName("description");
        entity.Property(x => x.TransactionDate).HasColumnName("transaction_date");
        entity.Property(x => x.DueDate).HasColumnName("due_date");
        entity.Property(x => x.CompletedAt).HasColumnName("completed_at");
        entity.Property(x => x.Status).HasColumnName("status");
        entity.Property(x => x.CreatedBy).HasColumnName("created_by");
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        entity.HasOne(x => x.Organization)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.OrganizationId);

        entity.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}