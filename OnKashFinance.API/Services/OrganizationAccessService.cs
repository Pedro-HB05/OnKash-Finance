using Microsoft.EntityFrameworkCore;
using OnKashFinance.Api.Data;
using OnKashFinance.Api.Entities;
using OnKashFinance.Api.Enums;

namespace OnKashFinance.Api.Services;

public class OrganizationAccessService
{
    private readonly OnKashDbContext _context;

    public OrganizationAccessService(OnKashDbContext context)
    {
        _context = context;
    }

    public async Task<OrganizationUser?> GetMembershipAsync(
        Guid userId,
        Guid organizationId)
    {
        return await _context.OrganizationUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.OrganizationId == organizationId &&
                x.IsActive);
    }

    public static bool CanManageFinance(OrganizationRole role)
    {
        return role == OrganizationRole.Owner ||
               role == OrganizationRole.Admin ||
               role == OrganizationRole.Financial;
    }

    public static bool CanManageOrganization(OrganizationRole role)
    {
        return role == OrganizationRole.Owner ||
               role == OrganizationRole.Admin;
    }

    public static bool CanManageUsers(OrganizationRole role)
    {
        return role == OrganizationRole.Owner;
    }
}