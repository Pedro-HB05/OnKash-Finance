using OnKashFinance.Api.Enums;

namespace OnKashFinance.Api.Entities;

public class FinancialTransaction
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public Guid CategoryId { get; set; }

    public TransactionType Type { get; set; }

    public decimal Amount { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateOnly TransactionDate { get; set; }

    public DateOnly? DueDate { get; set; }

    public DateTime? CompletedAt { get; set; }

    public TransactionStatus Status { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Organization Organization { get; set; } = null!;

    public Category Category { get; set; } = null!;

    public User CreatedByUser { get; set; } = null!;
}