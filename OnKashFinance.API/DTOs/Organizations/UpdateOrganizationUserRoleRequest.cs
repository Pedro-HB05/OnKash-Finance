using OnKashFinance.Api.Enums;

namespace OnKashFinance.Api.DTOs.Organizations;

public class UpdateOrganizationUserRoleRequest
{
    public OrganizationRole Role { get; set; }
}