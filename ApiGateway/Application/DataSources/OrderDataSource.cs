using ApiGateway.Domain.UserProfile.Models;
using ApiGateway.Models;
using ApiGateway.Services;
using Serilog;

namespace ApiGateway.Application.DataSources;

public sealed class OrderDataSource : IOrderDataSource
{
    private readonly IOrderServiceClient _serviceClient;

    public OrderDataSource(IOrderServiceClient serviceClient)
    {
        _serviceClient = serviceClient;
    }

    public async Task<IReadOnlyList<OrderInfo>> RetrieveOrdersByUserIdAsync(int userId)
    {
        try
        {
            var orders = await _serviceClient.GetUserOrdersAsync(userId);
            
            return orders.Select(MapToOrderInfo).ToList();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unable to retrieve orders for userId: {UserId}", userId);
            return Array.Empty<OrderInfo>();
        }
    }

    private static OrderInfo MapToOrderInfo(Order order)
    {
        return new OrderInfo
        {
            OrderId = order.Id,
            CustomerId = order.UserId,
            ProductId = order.ProductId,
            ItemQuantity = order.Quantity,
            OrderTotal = order.TotalPrice,
            CreatedDateTime = order.OrderDate
        };
    }
}

