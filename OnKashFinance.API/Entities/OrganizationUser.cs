using OnKashFinance.Api.Enums;

namespace OnKashFinance.Api.Entities;

public class OrganizationUser
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid OrganizationId { get; set; }

    public OrganizationRole Role { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;

    public Organization Organization { get; set; } = null!;
}