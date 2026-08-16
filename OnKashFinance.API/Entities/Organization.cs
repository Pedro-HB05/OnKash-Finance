using OnKashFinance.Api.Enums;

namespace OnKashFinance.Api.Entities;

public class Organization
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public OrganizationType Type { get; set; }

    public decimal InitialBalance { get; set; }

    public DateOnly InitialBalanceDate { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public User CreatedByUser { get; set; } = null!;

    public ICollection<OrganizationUser> OrganizationUsers { get; set; }
        = new List<OrganizationUser>();

    public ICollection<Category> Categories { get; set; }
        = new List<Category>();

    public ICollection<FinancialTransaction> Transactions { get; set; }
        = new List<FinancialTransaction>();
}