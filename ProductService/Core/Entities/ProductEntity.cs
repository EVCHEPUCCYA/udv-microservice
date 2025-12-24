namespace ProductService.Core.Entities;

public sealed class ProductEntity
{
    public int Id { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
}

