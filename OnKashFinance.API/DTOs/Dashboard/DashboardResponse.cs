namespace OnKashFinance.Api.DTOs.Dashboard;

public class DashboardResponse
{
    public decimal InitialBalance { get; set; }

    public decimal CurrentBalance { get; set; }

    public decimal MonthlyIncome { get; set; }

    public decimal MonthlyExpenses { get; set; }

    public decimal MonthlyResult { get; set; }

    public decimal PendingExpenses { get; set; }

    public decimal ExpectedIncome { get; set; }
}