using OnKashFinance.Api.Enums;

namespace OnKashFinance.Api.DTOs.Categories;

public class CategoryResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public CategoryType Type { get; set; }

    public Guid? ParentCategoryId { get; set; }

    public DateTime CreatedAt { get; set; }
}