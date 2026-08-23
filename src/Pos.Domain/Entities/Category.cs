namespace Pos.Domain.Entities;

public class Category
{
    public int Id { get; set; }

    public int StoreId { get; set; }

    public string Name { get; set; } = string.Empty;
}