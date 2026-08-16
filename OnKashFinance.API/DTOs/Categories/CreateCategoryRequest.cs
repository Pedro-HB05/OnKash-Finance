using System.ComponentModel.DataAnnotations;
using OnKashFinance.Api.Enums;

namespace OnKashFinance.Api.DTOs.Categories;

public class CreateCategoryRequest
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public CategoryType Type { get; set; }

    public Guid? ParentCategoryId { get; set; }
}