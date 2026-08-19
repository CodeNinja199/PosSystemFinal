using System.ComponentModel.DataAnnotations;

namespace Pos.Api.Dtos;

public class UpdateCategoryRequest
{
    [Required]
    [MaxLength(60)]
    public string Name { get; set; } = string.Empty;
}
