using OnKashFinance.Api.Enums;

namespace OnKashFinance.Api.Entities;

public class Category
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    public string Name { get; set; } = string.Empty;

    public CategoryType Type { get; set; }

    public Guid? ParentCategoryId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Organization Organization { get; set; } = null!;

    public Category? ParentCategory { get; set; }
}