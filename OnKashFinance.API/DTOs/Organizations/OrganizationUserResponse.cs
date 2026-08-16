using OnKashFinance.Api.Enums;

namespace OnKashFinance.Api.DTOs.Organizations;

public class OrganizationUserResponse
{
    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public OrganizationRole Role { get; set; }
}