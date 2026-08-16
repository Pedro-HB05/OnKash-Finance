using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnKashFinance.Api.Data;
using OnKashFinance.Api.DTOs.Auth;
using OnKashFinance.Api.Entities;
using OnKashFinance.Api.Enums;
using OnKashFinance.Api.Services;
using System.Security.Claims;

namespace OnKashFinance.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly OnKashDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly JwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        OnKashDbContext context,
        IPasswordHasher<User> passwordHasher,
        JwtService jwtService,
        ILogger<AuthController> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request)
    {
        var name = request.Name.Trim();

        var email = request.Email
            .Trim()
            .ToLowerInvariant();

        var emailExists = await _context.Users
            .AnyAsync(x => x.Email.ToLower() == email);

        if (emailExists)
        {
            return Conflict(new
            {
                message = "Já existe um usuário cadastrado com esse e-mail."
            });
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var now = DateTime.UtcNow;

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = name,
                Email = email,
                PasswordHash = string.Empty,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                request.Password
            );

            _context.Users.Add(user);

            var organization = new Organization
            {
                Id = Guid.NewGuid(),

                Name = $"{user.Name} - Pessoal",

                Type = OrganizationType.Personal,

                InitialBalance = 0,

                InitialBalanceDate =
                    DateOnly.FromDateTime(now),

                CreatedBy = user.Id,

                CreatedAt = now,

                UpdatedAt = now
            };

            _context.Organizations.Add(organization);

            var organizationUser = new OrganizationUser
            {
                Id = Guid.NewGuid(),

                UserId = user.Id,

                OrganizationId = organization.Id,

                Role = OrganizationRole.Owner,

                IsActive = true,

                CreatedAt = now
            };

            _context.OrganizationUsers.Add(organizationUser);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var response = new RegisterResponse
            {
                UserId = user.Id,

                Name = user.Name,

                Email = user.Email,

                OrganizationId = organization.Id,

                OrganizationName = organization.Name
            };

            return StatusCode(
                StatusCodes.Status201Created,
                response
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            _logger.LogError(
                ex,
                "Erro ao cadastrar usuário {Email}",
                email
            );

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message = "Ocorreu um erro ao cadastrar o usuário."
                }
            );
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        var email = request.Email
            .Trim()
            .ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == email);

        if (user is null)
        {
            return Unauthorized(new
            {
                message = "E-mail ou senha inválidos."
            });
        }

        if (!user.IsActive)
        {
            return Unauthorized(new
            {
                message = "Usuário inativo."
            });
        }

        var passwordResult =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password
            );

        if (passwordResult ==
            PasswordVerificationResult.Failed)
        {
            return Unauthorized(new
            {
                message = "E-mail ou senha inválidos."
            });
        }

        var (token, expiresAt) =
            _jwtService.GenerateToken(user);

        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new LoginResponse
        {
            Token = token,

            ExpiresAt = expiresAt,

            User = new UserLoginResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            }
        });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdClaim) ||
            !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new
            {
                message = "Token inválido."
            });
        }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user is null)
        {
            return NotFound(new
            {
                message = "Usuário não encontrado."
            });
        }

        return Ok(new
        {
            id = user.Id,
            name = user.Name,
            email = user.Email
        });
    }
}