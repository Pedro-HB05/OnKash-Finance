using System.ComponentModel.DataAnnotations;
using OnKashFinance.Api.Enums;

namespace OnKashFinance.Api.DTOs.Organizations;

public class AddOrganizationUserRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public OrganizationRole Role { get; set; }
}