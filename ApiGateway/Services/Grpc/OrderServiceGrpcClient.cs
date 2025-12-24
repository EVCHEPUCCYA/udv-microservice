using ApiGateway.Models;
using Grpc.Net.Client;
using Serilog;
using OrderService.Protos;

namespace ApiGateway.Services;

public sealed class OrderServiceGrpcClient : IOrderServiceClient
{
    private readonly OrderService.Protos.OrderService.OrderServiceClient _grpcClient;
    private readonly Serilog.ILogger _logger;

    public OrderServiceGrpcClient(IConfiguration configuration, Serilog.ILogger logger)
    {
        var serviceEndpoint = configuration["Services:OrderServiceGrpc"] ?? "http://localhost:5002";
        var grpcChannel = GrpcChannel.ForAddress(serviceEndpoint);
        _grpcClient = new OrderService.Protos.OrderService.OrderServiceClient(grpcChannel);
        _logger = logger;
    }

    public async Task<List<Order>> GetUserOrdersAsync(int userId)
    {
        try
        {
            var grpcRequest = new GetUserOrdersRequest { UserId = userId };
            var grpcResponse = await _grpcClient.GetUserOrdersAsync(grpcRequest);
            
            return grpcResponse.Orders.Select(o => new Order
            {
                Id = o.Id,
                UserId = o.UserId,
                ProductId = o.ProductId,
                Quantity = o.Quantity,
                TotalPrice = (decimal)o.TotalPrice,
                OrderDate = DateTime.Parse(o.OrderDate)
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error loading orders for user {UserId} via gRPC", userId);
            return new List<Order>();
        }
    }
}

