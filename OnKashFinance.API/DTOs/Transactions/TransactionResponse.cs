using OnKashFinance.Api.Enums;

namespace OnKashFinance.Api.DTOs.Transactions;

public class TransactionResponse
{
    public Guid Id { get; set; }

    public Guid CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public TransactionType Type { get; set; }

    public decimal Amount { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateOnly TransactionDate { get; set; }

    public DateOnly? DueDate { get; set; }

    public DateTime? CompletedAt { get; set; }

    public TransactionStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
}