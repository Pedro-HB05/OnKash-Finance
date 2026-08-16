namespace OnKashFinance.Api.DTOs.Reports;

public class FinancialReportResponse
{
    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public decimal Income { get; set; }

    public decimal Expenses { get; set; }

    public decimal Result { get; set; }

    public decimal PendingExpenses { get; set; }

    public decimal ExpectedIncome { get; set; }
}