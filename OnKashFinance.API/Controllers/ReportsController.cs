using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnKashFinance.Api.Data;
using OnKashFinance.Api.DTOs.Reports;
using OnKashFinance.Api.Enums;
using OnKashFinance.Api.Extensions;
using OnKashFinance.Api.Services;

namespace OnKashFinance.Api.Controllers;

[ApiController]
[Route(
    "api/organizations/{organizationId:guid}/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly OnKashDbContext _context;
    private readonly OrganizationAccessService _accessService;

    public ReportsController(
        OnKashDbContext context,
        OrganizationAccessService accessService)
    {
        _context = context;
        _accessService = accessService;
    }


    [HttpGet("financial")]
    public async Task<IActionResult> Financial(
        Guid organizationId,
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate)
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

        var today =
            DateOnly.FromDateTime(
                DateTime.UtcNow);

        var start =
            startDate ??
            new DateOnly(
                today.Year,
                today.Month,
                1);

        var end =
            endDate ?? today;

        if (end < start)
        {
            return BadRequest(new
            {
                message =
                    "Data final não pode ser anterior à inicial."
            });
        }

        var baseQuery =
            _context.Transactions
                .AsNoTracking()
                .Where(x =>
                    x.OrganizationId ==
                    organizationId &&

                    x.TransactionDate >=
                    start &&

                    x.TransactionDate <=
                    end);

        var income =
            await baseQuery
                .Where(x =>
                    x.Type ==
                    TransactionType.Income &&

                    x.Status ==
                    TransactionStatus.Completed)
                .SumAsync(x =>
                    (decimal?)x.Amount)
            ?? 0;

        var expenses =
            await baseQuery
                .Where(x =>
                    x.Type ==
                    TransactionType.Expense &&

                    x.Status ==
                    TransactionStatus.Completed)
                .SumAsync(x =>
                    (decimal?)x.Amount)
            ?? 0;

        var pendingExpenses =
            await baseQuery
                .Where(x =>
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
            await baseQuery
                .Where(x =>
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
            new FinancialReportResponse
            {
                StartDate = start,
                EndDate = end,

                Income = income,

                Expenses = expenses,

                Result =
                    income - expenses,

                PendingExpenses =
                    pendingExpenses,

                ExpectedIncome =
                    expectedIncome
            });
    }
}