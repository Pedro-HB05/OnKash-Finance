using System.ComponentModel.DataAnnotations;
using OnKashFinance.Api.Enums;

namespace OnKashFinance.Api.DTOs.Transactions;

public class CreateTransactionRequest
{
    [Required]
    public Guid CategoryId { get; set; }

    [Required]
    public TransactionType Type { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(255)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateOnly TransactionDate { get; set; }

    public DateOnly? DueDate { get; set; }

    [Required]
    public TransactionStatus Status { get; set; }
}