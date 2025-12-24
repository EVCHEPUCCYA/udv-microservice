using Grpc.Core;
using OrderService.Core.Contracts;
using OrderService.Protos;

namespace OrderService.Api.Grpc;

public sealed class OrderGrpcHandler : Protos.OrderService.OrderServiceBase
{
    private readonly IOrderStore _store;

    public OrderGrpcHandler(IOrderStore store)
    {
        _store = store;
    }

    public override async Task<GetUserOrdersResponse> GetUserOrders(GetUserOrdersRequest request, ServerCallContext context)
    {
        var entities = await _store.FindByCustomerIdAsync(request.UserId);
        
        var response = new GetUserOrdersResponse();
        response.Orders.AddRange(entities.Select(e => new OrderResponse
        {
            Id = e.Id,
            UserId = e.CustomerId,
            ProductId = e.ProductId,
            Quantity = e.ItemQuantity,
            TotalPrice = (double)e.TotalAmount,
            OrderDate = e.CreatedAt.ToString("O")
        }));

        return response;
    }

    public override async Task<OrderResponse> GetOrder(GetOrderRequest request, ServerCallContext context)
    {
        var entity = await _store.FindByIdAsync(request.Id);
        
        if (entity == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Order with id {request.Id} not found"));
        }

        return new OrderResponse
        {
            Id = entity.Id,
            UserId = entity.CustomerId,
            ProductId = entity.ProductId,
            Quantity = entity.ItemQuantity,
            TotalPrice = (double)entity.TotalAmount,
            OrderDate = entity.CreatedAt.ToString("O")
        };
    }
}

