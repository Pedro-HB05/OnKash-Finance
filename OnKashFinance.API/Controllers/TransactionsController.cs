using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnKashFinance.Api.Data;
using OnKashFinance.Api.DTOs.Transactions;
using OnKashFinance.Api.Entities;
using OnKashFinance.Api.Enums;
using OnKashFinance.Api.Extensions;
using OnKashFinance.Api.Services;

namespace OnKashFinance.Api.Controllers;

[ApiController]
[Route(
    "api/organizations/{organizationId:guid}/transactions")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly OnKashDbContext _context;
    private readonly OrganizationAccessService _accessService;

    public TransactionsController(
        OnKashDbContext context,
        OrganizationAccessService accessService)
    {
        _context = context;
        _accessService = accessService;
    }


    [HttpGet]
    public async Task<IActionResult> GetAll(
        Guid organizationId,
        [FromQuery]
        TransactionFilterRequest filter)
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

        if (filter.StartDate.HasValue &&
            filter.EndDate.HasValue &&
            filter.EndDate <
            filter.StartDate)
        {
            return BadRequest(new
            {
                message =
                    "Data final não pode ser anterior à inicial."
            });
        }

        var query =
            _context.Transactions
                .AsNoTracking()
                .Where(x =>
                    x.OrganizationId ==
                    organizationId);

        if (filter.Type.HasValue)
        {
            query = query.Where(x =>
                x.Type == filter.Type.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(x =>
                x.Status ==
                filter.Status.Value);
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(x =>
                x.CategoryId ==
                filter.CategoryId.Value);
        }

        if (filter.StartDate.HasValue)
        {
            query = query.Where(x =>
                x.TransactionDate >=
                filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            query = query.Where(x =>
                x.TransactionDate <=
                filter.EndDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(
            filter.Search))
        {
            var search =
                filter.Search.Trim().ToLower();

            query = query.Where(x =>
                x.Description
                    .ToLower()
                    .Contains(search));
        }

        var transactions =
            await query
                .OrderByDescending(x =>
                    x.TransactionDate)
                .ThenByDescending(x =>
                    x.CreatedAt)
                .Select(x =>
                    new TransactionResponse
                    {
                        Id = x.Id,

                        CategoryId =
                            x.CategoryId,

                        CategoryName =
                            x.Category.Name,

                        Type = x.Type,

                        Amount = x.Amount,

                        Description =
                            x.Description,

                        TransactionDate =
                            x.TransactionDate,

                        DueDate =
                            x.DueDate,

                        CompletedAt =
                            x.CompletedAt,

                        Status =
                            x.Status,

                        CreatedAt =
                            x.CreatedAt
                    })
                .ToListAsync();

        return Ok(transactions);
    }


    [HttpGet("payable")]
    public async Task<IActionResult> GetPayable(
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

        var transactions =
            await _context.Transactions
                .AsNoTracking()
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
                .OrderBy(x => x.DueDate)
                .Select(x =>
                    new TransactionResponse
                    {
                        Id = x.Id,
                        CategoryId = x.CategoryId,
                        CategoryName =
                            x.Category.Name,
                        Type = x.Type,
                        Amount = x.Amount,
                        Description =
                            x.Description,
                        TransactionDate =
                            x.TransactionDate,
                        DueDate = x.DueDate,
                        CompletedAt =
                            x.CompletedAt,
                        Status = x.Status,
                        CreatedAt =
                            x.CreatedAt
                    })
                .ToListAsync();

        return Ok(transactions);
    }


    [HttpGet("receivable")]
    public async Task<IActionResult> GetReceivable(
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

        var transactions =
            await _context.Transactions
                .AsNoTracking()
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
                .OrderBy(x => x.DueDate)
                .Select(x =>
                    new TransactionResponse
                    {
                        Id = x.Id,
                        CategoryId = x.CategoryId,
                        CategoryName =
                            x.Category.Name,
                        Type = x.Type,
                        Amount = x.Amount,
                        Description =
                            x.Description,
                        TransactionDate =
                            x.TransactionDate,
                        DueDate = x.DueDate,
                        CompletedAt =
                            x.CompletedAt,
                        Status = x.Status,
                        CreatedAt =
                            x.CreatedAt
                    })
                .ToListAsync();

        return Ok(transactions);
    }


    [HttpPost]
    public async Task<IActionResult> Create(
        Guid organizationId,
        [FromBody]
        CreateTransactionRequest request)
    {
        var userId = User.GetUserId();

        if (userId is null)
            return Unauthorized();

        var membership =
            await _accessService.GetMembershipAsync(
                userId.Value,
                organizationId);

        if (membership is null ||
            !OrganizationAccessService
                .CanManageFinance(membership.Role))
        {
            return Forbid();
        }

        if (request.Amount <= 0)
        {
            return BadRequest(new
            {
                message =
                    "Valor deve ser maior que zero."
            });
        }

        var category =
            await _context.Categories
                .FirstOrDefaultAsync(x =>
                    x.Id ==
                    request.CategoryId &&
                    x.OrganizationId ==
                    organizationId &&
                    x.IsActive);

        if (category is null)
        {
            return BadRequest(new
            {
                message = "Categoria inválida."
            });
        }

        if ((short)category.Type !=
            (short)request.Type)
        {
            return BadRequest(new
            {
                message =
                    "Categoria incompatível com o tipo da movimentação."
            });
        }

        var now = DateTime.UtcNow;

        var transaction =
            new FinancialTransaction
            {
                Id = Guid.NewGuid(),

                OrganizationId =
                    organizationId,

                CategoryId =
                    request.CategoryId,

                Type = request.Type,

                Amount = request.Amount,

                Description =
                    request.Description.Trim(),

                TransactionDate =
                    request.TransactionDate,

                DueDate =
                    request.DueDate,

                Status =
                    request.Status,

                CompletedAt =
                    request.Status ==
                    TransactionStatus.Completed
                        ? now
                        : null,

                CreatedBy =
                    userId.Value,

                CreatedAt = now,

                UpdatedAt = now
            };

        _context.Transactions.Add(
            transaction);

        await _context.SaveChangesAsync();

        return StatusCode(
            StatusCodes.Status201Created,
            new
            {
                transaction.Id
            });
    }


    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid organizationId,
        Guid id,
        [FromBody]
        UpdateTransactionRequest request)
    {
        var userId = User.GetUserId();

        if (userId is null)
            return Unauthorized();

        var membership =
            await _accessService.GetMembershipAsync(
                userId.Value,
                organizationId);

        if (membership is null ||
            !OrganizationAccessService
                .CanManageFinance(membership.Role))
        {
            return Forbid();
        }

        var transaction =
            await _context.Transactions
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.OrganizationId ==
                    organizationId);

        if (transaction is null)
            return NotFound();

        var category =
            await _context.Categories
                .FirstOrDefaultAsync(x =>
                    x.Id ==
                    request.CategoryId &&
                    x.OrganizationId ==
                    organizationId &&
                    x.IsActive);

        if (category is null)
        {
            return BadRequest(new
            {
                message =
                    "Categoria inválida."
            });
        }

        if ((short)category.Type !=
            (short)request.Type)
        {
            return BadRequest(new
            {
                message =
                    "Categoria incompatível."
            });
        }

        var wasCompleted =
            transaction.Status ==
            TransactionStatus.Completed;

        transaction.CategoryId =
            request.CategoryId;

        transaction.Type =
            request.Type;

        transaction.Amount =
            request.Amount;

        transaction.Description =
            request.Description.Trim();

        transaction.TransactionDate =
            request.TransactionDate;

        transaction.DueDate =
            request.DueDate;

        transaction.Status =
            request.Status;

        transaction.UpdatedAt =
            DateTime.UtcNow;

        if (!wasCompleted &&
            request.Status ==
            TransactionStatus.Completed)
        {
            transaction.CompletedAt =
                DateTime.UtcNow;
        }

        if (request.Status !=
            TransactionStatus.Completed)
        {
            transaction.CompletedAt =
                null;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id:guid}/complete")]
    public async Task<IActionResult> Complete(
        Guid organizationId,
        Guid id)
    {
        var userId = User.GetUserId();

        if (userId is null)
            return Unauthorized();

        var membership =
            await _accessService.GetMembershipAsync(
                userId.Value,
                organizationId);

        if (membership is null ||
            !OrganizationAccessService
                .CanManageFinance(membership.Role))
        {
            return Forbid();
        }

        var transaction =
            await _context.Transactions
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.OrganizationId ==
                    organizationId);

        if (transaction is null)
            return NotFound();

        if (transaction.Status ==
            TransactionStatus.Cancelled)
        {
            return BadRequest(new
            {
                message =
                    "Movimentação cancelada não pode ser concluída."
            });
        }

        transaction.Status =
            TransactionStatus.Completed;

        transaction.CompletedAt =
            DateTime.UtcNow;

        transaction.UpdatedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }


    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid organizationId,
        Guid id)
    {
        var userId = User.GetUserId();

        if (userId is null)
            return Unauthorized();

        var membership =
            await _accessService.GetMembershipAsync(
                userId.Value,
                organizationId);

        if (membership is null ||
            !OrganizationAccessService
                .CanManageFinance(membership.Role))
        {
            return Forbid();
        }

        var transaction =
            await _context.Transactions
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.OrganizationId ==
                    organizationId);

        if (transaction is null)
            return NotFound();

        transaction.Status =
            TransactionStatus.Cancelled;

        transaction.CompletedAt = null;

        transaction.UpdatedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}