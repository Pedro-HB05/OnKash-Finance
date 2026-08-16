using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnKashFinance.Api.Data;
using OnKashFinance.Api.DTOs.Dashboard;
using OnKashFinance.Api.Enums;
using OnKashFinance.Api.Extensions;
using OnKashFinance.Api.Services;

namespace OnKashFinance.Api.Controllers;

[ApiController]
[Route(
    "api/organizations/{organizationId:guid}/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly OnKashDbContext _context;
    private readonly OrganizationAccessService _accessService;

    public DashboardController(
        OnKashDbContext context,
        OrganizationAccessService accessService)
    {
        _context = context;
        _accessService = accessService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        Guid organizationId)
    {
        var userId = User.GetUserId();

        if (userId is null)
            return Unauthorized();

        var membership =
            await _accessService.GetMembershipAsync(
                userId.Value,
                organizationId);

        if (membership is null)
            return Forbid();

        var organization =
            await _context.Organizations
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == organizationId);

        if (organization is null)
            return NotFound();

        var today =
            DateOnly.FromDateTime(
                DateTime.UtcNow);

        var firstDay =
            new DateOnly(
                today.Year,
                today.Month,
                1);

        var nextMonth =
            firstDay.AddMonths(1);

        var completedAfterInitialDate =
            _context.Transactions
                .Where(x =>
                    x.OrganizationId ==
                    organizationId &&

                    x.Status ==
                    TransactionStatus.Completed &&

                    x.TransactionDate >=
                    organization.InitialBalanceDate);

        var totalIncome =
            await completedAfterInitialDate
                .Where(x =>
                    x.Type ==
                    TransactionType.Income)
                .SumAsync(x =>
                    (decimal?)x.Amount)
            ?? 0;

        var totalExpenses =
            await completedAfterInitialDate
                .Where(x =>
                    x.Type ==
                    TransactionType.Expense)
                .SumAsync(x =>
                    (decimal?)x.Amount)
            ?? 0;

        var currentBalance =
            organization.InitialBalance +
            totalIncome -
            totalExpenses;

        var monthlyIncome =
            await _context.Transactions
                .Where(x =>
                    x.OrganizationId ==
                    organizationId &&

                    x.Type ==
                    TransactionType.Income &&

                    x.Status ==
                    TransactionStatus.Completed &&

                    x.TransactionDate >=
                    firstDay &&

                    x.TransactionDate <
                    nextMonth)
                .SumAsync(x =>
                    (decimal?)x.Amount)
            ?? 0;

        var monthlyExpenses =
            await _context.Transactions
                .Where(x =>
                    x.OrganizationId ==
                    organizationId &&

                    x.Type ==
                    TransactionType.Expense &&

                    x.Status ==
                    TransactionStatus.Completed &&

                    x.TransactionDate >=
                    firstDay &&

                    x.TransactionDate <
                    nextMonth)
                .SumAsync(x =>
                    (decimal?)x.Amount)
            ?? 0;

        var pendingExpenses =
            await _context.Transactions
                .Where(x =>
                    x.OrganizationId ==
                    organizationId &&

                    x.Type ==
                    TransactionType.Expense &&

                    (
                        x.Status ==
                        TransactionStatus.Pending ||

                        x.Status ==
                        TransactionStatus.Overdue
                    ))
                .SumAsync(x =>
                    (decimal?)x.Amount)
            ?? 0;

        var expectedIncome =
            await _context.Transactions
                .Where(x =>
                    x.OrganizationId ==
                    organizationId &&

                    x.Type ==
                    TransactionType.Income &&

                    (
                        x.Status ==
                        TransactionStatus.Pending ||

                        x.Status ==
                        TransactionStatus.Overdue
                    ))
                .SumAsync(x =>
                    (decimal?)x.Amount)
            ?? 0;

        return Ok(
            new DashboardResponse
            {
                InitialBalance =
                    organization.InitialBalance,

                CurrentBalance =
                    currentBalance,

                MonthlyIncome =
                    monthlyIncome,

                MonthlyExpenses =
                    monthlyExpenses,

                MonthlyResult =
                    monthlyIncome -
                    monthlyExpenses,

                PendingExpenses =
                    pendingExpenses,

                ExpectedIncome =
                    expectedIncome
            });
    }
}