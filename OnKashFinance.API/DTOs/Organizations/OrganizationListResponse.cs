using OnKashFinance.Api.Enums;

namespace OnKashFinance.Api.DTOs.Organizations;

public class OrganizationListResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public OrganizationType Type { get; set; }

    public decimal InitialBalance { get; set; }

    public OrganizationRole Role { get; set; }
}