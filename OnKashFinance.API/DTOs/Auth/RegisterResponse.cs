namespace OnKashFinance.Api.DTOs.Auth;

public class RegisterResponse
{
    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public Guid OrganizationId { get; set; }

    public string OrganizationName { get; set; } = string.Empty;
}