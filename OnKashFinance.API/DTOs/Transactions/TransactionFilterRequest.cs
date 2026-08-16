using OnKashFinance.Api.Enums;

namespace OnKashFinance.Api.DTOs.Transactions;

public class TransactionFilterRequest
{
    public TransactionType? Type { get; set; }

    public TransactionStatus? Status { get; set; }

    public Guid? CategoryId { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Search { get; set; }
}