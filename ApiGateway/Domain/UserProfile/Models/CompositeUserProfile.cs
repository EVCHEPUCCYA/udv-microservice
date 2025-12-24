namespace ApiGateway.Domain.UserProfile.Models;

public sealed class CompositeUserProfile
{
    public UserInfo? UserInfo { get; set; }
    public IReadOnlyList<OrderInfo> OrderHistory { get; set; } = Array.Empty<OrderInfo>();
    public int OrderCount { get; set; }
    public decimal TotalPurchaseAmount { get; set; }
}

public sealed class UserInfo
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
}

public sealed class OrderInfo
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public ProductInfo? ProductInfo { get; set; }
    public int ItemQuantity { get; set; }
    public decimal OrderTotal { get; set; }
    public DateTime CreatedDateTime { get; set; }
}

public sealed class ProductInfo
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
}

