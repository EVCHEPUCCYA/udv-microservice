using ApiGateway.Models;
using Grpc.Net.Client;
using Serilog;
using ProductService.Protos;

namespace ApiGateway.Services;

public sealed class ProductServiceGrpcClient : IProductServiceClient
{
    private readonly ProductService.Protos.ProductService.ProductServiceClient _grpcClient;
    private readonly Serilog.ILogger _logger;

    public ProductServiceGrpcClient(IConfiguration configuration, Serilog.ILogger logger)
    {
        var serviceEndpoint = configuration["Services:ProductServiceGrpc"] ?? "http://localhost:5003";
        var grpcChannel = GrpcChannel.ForAddress(serviceEndpoint);
        _grpcClient = new ProductService.Protos.ProductService.ProductServiceClient(grpcChannel);
        _logger = logger;
    }

    public async Task<Dictionary<int, Product>> GetProductsAsync(IEnumerable<int> productIds)
    {
        var result = new Dictionary<int, Product>();
        
        foreach (var productId in productIds)
        {
            try
            {
                var grpcRequest = new GetProductRequest { Id = productId };
                var grpcResponse = await _grpcClient.GetProductAsync(grpcRequest);
                
                result[productId] = new Product
                {
                    Id = grpcResponse.Id,
                    Name = grpcResponse.Name,
                    Description = grpcResponse.Description,
                    Price = (decimal)grpcResponse.Price
                };
            }
            catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
            {
                _logger.Warning("Product {ProductId} not found via gRPC", productId);
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Error fetching product {ProductId} via gRPC", productId);
            }
        }
        
        return result;
    }
}

