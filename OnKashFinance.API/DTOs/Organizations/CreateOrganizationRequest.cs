using System.ComponentModel.DataAnnotations;

namespace OnKashFinance.Api.DTOs.Organizations;

public class CreateOrganizationRequest
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public decimal InitialBalance { get; set; }
}