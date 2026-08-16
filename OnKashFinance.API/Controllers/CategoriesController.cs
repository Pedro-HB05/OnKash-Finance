using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnKashFinance.Api.Data;
using OnKashFinance.Api.DTOs.Categories;
using OnKashFinance.Api.Entities;
using OnKashFinance.Api.Extensions;
using OnKashFinance.Api.Services;

namespace OnKashFinance.Api.Controllers;

[ApiController]
[Route(
    "api/organizations/{organizationId:guid}/categories")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly OnKashDbContext _context;
    private readonly OrganizationAccessService _accessService;

    public CategoriesController(
        OnKashDbContext context,
        OrganizationAccessService accessService)
    {
        _context = context;
        _accessService = accessService;
    }


    [HttpGet]
    public async Task<IActionResult> GetAll(
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

        var categories =
            await _context.Categories
                .AsNoTracking()
                .Where(x =>
                    x.OrganizationId ==
                    organizationId &&
                    x.IsActive)
                .OrderBy(x => x.Type)
                .ThenBy(x => x.Name)
                .Select(x =>
                    new CategoryResponse
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Type = x.Type,
                        ParentCategoryId =
                            x.ParentCategoryId,
                        CreatedAt = x.CreatedAt
                    })
                .ToListAsync();

        return Ok(categories);
    }


    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
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

        if (membership is null)
            return Forbid();

        var category =
            await _context.Categories
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    x.OrganizationId ==
                    organizationId &&
                    x.IsActive)
                .Select(x =>
                    new CategoryResponse
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Type = x.Type,
                        ParentCategoryId =
                            x.ParentCategoryId,
                        CreatedAt = x.CreatedAt
                    })
                .FirstOrDefaultAsync();

        return category is null
            ? NotFound()
            : Ok(category);
    }


    [HttpPost]
    public async Task<IActionResult> Create(
        Guid organizationId,
        [FromBody]
        CreateCategoryRequest request)
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
            .CanManageFinance(membership.Role))
        {
            return Forbid();
        }

        if (!Enum.IsDefined(request.Type))
        {
            return BadRequest(new
            {
                message = "Tipo inválido."
            });
        }

        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new
            {
                message =
                    "Nome da categoria é obrigatório."
            });
        }

        var duplicate =
            await _context.Categories
                .AnyAsync(x =>
                    x.OrganizationId ==
                    organizationId &&
                    x.Type == request.Type &&
                    x.IsActive &&
                    x.Name.ToLower() ==
                    name.ToLower());

        if (duplicate)
        {
            return Conflict(new
            {
                message =
                    "Categoria já cadastrada."
            });
        }

        if (request.ParentCategoryId.HasValue)
        {
            var parent =
                await _context.Categories
                    .FirstOrDefaultAsync(x =>
                        x.Id ==
                        request.ParentCategoryId.Value &&
                        x.OrganizationId ==
                        organizationId &&
                        x.IsActive);

            if (parent is null ||
                parent.Type != request.Type)
            {
                return BadRequest(new
                {
                    message =
                        "Categoria pai inválida."
                });
            }
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),

            OrganizationId =
                organizationId,

            Name = name,

            Type = request.Type,

            ParentCategoryId =
                request.ParentCategoryId,

            IsActive = true,

            CreatedAt =
                DateTime.UtcNow,

            UpdatedAt =
                DateTime.UtcNow
        };

        _context.Categories.Add(category);

        await _context.SaveChangesAsync();

        return StatusCode(
            StatusCodes.Status201Created,
            new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Type = category.Type,
                ParentCategoryId =
                    category.ParentCategoryId,
                CreatedAt =
                    category.CreatedAt
            });
    }


    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid organizationId,
        Guid id,
        [FromBody]
        UpdateCategoryRequest request)
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

        var category =
            await _context.Categories
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.OrganizationId ==
                    organizationId &&
                    x.IsActive);

        if (category is null)
            return NotFound();

        if (request.ParentCategoryId == id)
        {
            return BadRequest(new
            {
                message =
                    "Categoria não pode ser pai dela mesma."
            });
        }

        if (request.ParentCategoryId.HasValue)
        {
            var parent =
                await _context.Categories
                    .FirstOrDefaultAsync(x =>
                        x.Id ==
                        request.ParentCategoryId &&
                        x.OrganizationId ==
                        organizationId &&
                        x.IsActive);

            if (parent is null ||
                parent.Type != request.Type)
            {
                return BadRequest(new
                {
                    message =
                        "Categoria pai inválida."
                });
            }
        }

        category.Name =
            request.Name.Trim();

        category.Type =
            request.Type;

        category.ParentCategoryId =
            request.ParentCategoryId;

        category.UpdatedAt =
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

        var category =
            await _context.Categories
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.OrganizationId ==
                    organizationId &&
                    x.IsActive);

        if (category is null)
            return NotFound();

        var hasChildren =
            await _context.Categories
                .AnyAsync(x =>
                    x.ParentCategoryId ==
                    category.Id &&
                    x.IsActive);

        if (hasChildren)
        {
            return Conflict(new
            {
                message =
                    "Categoria possui subcategorias."
            });
        }

        category.IsActive = false;

        category.UpdatedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}