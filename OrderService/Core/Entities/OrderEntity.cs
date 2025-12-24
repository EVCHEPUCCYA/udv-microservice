namespace OrderService.Core.Entities;

public sealed class OrderEntity
{
    public int Id { get; init; }
    public int CustomerId { get; init; }
    public int ProductId { get; init; }
    public int ItemQuantity { get; init; }
    public decimal TotalAmount { get; init; }
    public DateTime CreatedAt { get; init; }
}

