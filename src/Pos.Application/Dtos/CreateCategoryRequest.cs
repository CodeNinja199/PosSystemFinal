using System.ComponentModel.DataAnnotations;

namespace Pos.Application.Dtos;

public class CreateCategoryRequest
{
    [Required]
    [MaxLength(60)]
    public string Name { get; set; } = string.Empty;
}
