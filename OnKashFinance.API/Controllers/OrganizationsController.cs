using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnKashFinance.Api.Data;
using OnKashFinance.Api.DTOs.Organizations;
using OnKashFinance.Api.Entities;
using OnKashFinance.Api.Enums;
using OnKashFinance.Api.Extensions;
using OnKashFinance.Api.Services;

namespace OnKashFinance.Api.Controllers;

[ApiController]
[Route("api/organizations")]
[Authorize]
public class OrganizationsController : ControllerBase
{
    private readonly OnKashDbContext _context;
    private readonly OrganizationAccessService _accessService;

    public OrganizationsController(
        OnKashDbContext context,
        OrganizationAccessService accessService)
    {
        _context = context;
        _accessService = accessService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.GetUserId();

        if (userId is null)
            return Unauthorized();

        var organizations = await (
            from membership in _context.OrganizationUsers
            join organization in _context.Organizations
                on membership.OrganizationId equals organization.Id

            where
                membership.UserId == userId.Value &&
                membership.IsActive

            orderby organization.Type, organization.Name

            select new OrganizationListResponse
            {
                Id = organization.Id,
                Name = organization.Name,
                Type = organization.Type,
                InitialBalance = organization.InitialBalance,
                Role = membership.Role
            }
        )
        .AsNoTracking()
        .ToListAsync();

        return Ok(organizations);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrganizationRequest request)
    {
        var userId = User.GetUserId();

        if (userId is null)
            return Unauthorized();

        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new
            {
                message = "O nome da empresa é obrigatório."
            });
        }

        await using var dbTransaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var now = DateTime.UtcNow;

            var organization = new Organization
            {
                Id = Guid.NewGuid(),

                Name = name,

                Type = OrganizationType.Business,

                InitialBalance = request.InitialBalance,

                InitialBalanceDate =
                    DateOnly.FromDateTime(now),

                CreatedBy = userId.Value,

                CreatedAt = now,

                UpdatedAt = now
            };

            var membership = new OrganizationUser
            {
                Id = Guid.NewGuid(),

                UserId = userId.Value,

                OrganizationId = organization.Id,

                Role = OrganizationRole.Owner,

                IsActive = true,

                CreatedAt = now
            };

            _context.Organizations.Add(organization);
            _context.OrganizationUsers.Add(membership);

            await _context.SaveChangesAsync();

            await dbTransaction.CommitAsync();

            return StatusCode(
                StatusCodes.Status201Created,
                new OrganizationListResponse
                {
                    Id = organization.Id,
                    Name = organization.Name,
                    Type = organization.Type,
                    InitialBalance = organization.InitialBalance,
                    Role = OrganizationRole.Owner
                });
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    [HttpPut("{organizationId:guid}/initial-balance")]
    public async Task<IActionResult> UpdateInitialBalance(
        Guid organizationId,
        [FromBody] UpdateInitialBalanceRequest request)
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

        if (!OrganizationAccessService
            .CanManageOrganization(membership.Role))
        {
            return Forbid();
        }

        var organization =
            await _context.Organizations
                .FirstOrDefaultAsync(x =>
                    x.Id == organizationId);

        if (organization is null)
            return NotFound();

        organization.InitialBalance =
            request.InitialBalance;

        organization.InitialBalanceDate =
            DateOnly.FromDateTime(DateTime.UtcNow);

        organization.UpdatedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            organization.Id,
            organization.Name,
            organization.InitialBalance,
            organization.InitialBalanceDate
        });
    }

    [HttpGet("{organizationId:guid}/users")]
    public async Task<IActionResult> GetUsers(
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

        var users = await (
            from member in _context.OrganizationUsers
            join user in _context.Users
                on member.UserId equals user.Id

            where
                member.OrganizationId == organizationId &&
                member.IsActive

            select new OrganizationUserResponse
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = member.Role
            }
        )
        .AsNoTracking()
        .ToListAsync();

        return Ok(users);
    }

    [HttpPost("{organizationId:guid}/users")]
    public async Task<IActionResult> AddUser(
        Guid organizationId,
        [FromBody] AddOrganizationUserRequest request)
    {
        var loggedUserId = User.GetUserId();

        if (loggedUserId is null)
            return Unauthorized();

        var membership =
            await _accessService.GetMembershipAsync(
                loggedUserId.Value,
                organizationId);

        if (membership is null)
            return Forbid();

        if (!OrganizationAccessService
            .CanManageUsers(membership.Role))
        {
            return Forbid();
        }

        var organization =
            await _context.Organizations
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == organizationId);

        if (organization is null)
            return NotFound();

        if (organization.Type !=
            OrganizationType.Business)
        {
            return BadRequest(new
            {
                message =
                    "Não é possível adicionar usuários ao espaço pessoal."
            });
        }

        if (request.Role == OrganizationRole.Owner)
        {
            return BadRequest(new
            {
                message =
                    "Não é possível adicionar outro proprietário."
            });
        }

        var email = request.Email
            .Trim()
            .ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(x =>
                x.Email == email &&
                x.IsActive);

        if (user is null)
        {
            return NotFound(new
            {
                message =
                    "Nenhum usuário cadastrado com esse e-mail."
            });
        }

        var exists =
            await _context.OrganizationUsers
                .AnyAsync(x =>
                    x.OrganizationId == organizationId &&
                    x.UserId == user.Id &&
                    x.IsActive);

        if (exists)
        {
            return Conflict(new
            {
                message =
                    "Esse usuário já pertence à empresa."
            });
        }

        var newMembership =
            new OrganizationUser
            {
                Id = Guid.NewGuid(),

                UserId = user.Id,

                OrganizationId = organizationId,

                Role = request.Role,

                IsActive = true,

                CreatedAt = DateTime.UtcNow
            };

        _context.OrganizationUsers.Add(
            newMembership);

        await _context.SaveChangesAsync();

        return StatusCode(
            StatusCodes.Status201Created,
            new OrganizationUserResponse
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = request.Role
            });
    }

    [HttpPut(
        "{organizationId:guid}/users/{targetUserId:guid}/role")]
    public async Task<IActionResult> UpdateUserRole(
        Guid organizationId,
        Guid targetUserId,
        [FromBody]
        UpdateOrganizationUserRoleRequest request)
    {
        var loggedUserId = User.GetUserId();

        if (loggedUserId is null)
            return Unauthorized();

        var loggedMembership =
            await _accessService.GetMembershipAsync(
                loggedUserId.Value,
                organizationId);

        if (loggedMembership is null ||
            !OrganizationAccessService
                .CanManageUsers(loggedMembership.Role))
        {
            return Forbid();
        }

        if (request.Role == OrganizationRole.Owner)
        {
            return BadRequest(new
            {
                message =
                    "A função Owner não pode ser atribuída desta forma."
            });
        }

        var membership =
            await _context.OrganizationUsers
                .FirstOrDefaultAsync(x =>
                    x.OrganizationId == organizationId &&
                    x.UserId == targetUserId &&
                    x.IsActive);

        if (membership is null)
            return NotFound();

        if (membership.Role ==
            OrganizationRole.Owner)
        {
            return BadRequest(new
            {
                message =
                    "Não é possível alterar o proprietário."
            });
        }

        membership.Role = request.Role;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete(
        "{organizationId:guid}/users/{targetUserId:guid}")]
    public async Task<IActionResult> RemoveUser(
        Guid organizationId,
        Guid targetUserId)
    {
        var loggedUserId = User.GetUserId();

        if (loggedUserId is null)
            return Unauthorized();

        var loggedMembership =
            await _accessService.GetMembershipAsync(
                loggedUserId.Value,
                organizationId);

        if (loggedMembership is null ||
            !OrganizationAccessService
                .CanManageUsers(loggedMembership.Role))
        {
            return Forbid();
        }

        var membership =
            await _context.OrganizationUsers
                .FirstOrDefaultAsync(x =>
                    x.OrganizationId == organizationId &&
                    x.UserId == targetUserId &&
                    x.IsActive);

        if (membership is null)
            return NotFound();

        if (membership.Role ==
            OrganizationRole.Owner)
        {
            return BadRequest(new
            {
                message =
                    "O proprietário não pode ser removido."
            });
        }

        membership.IsActive = false;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}