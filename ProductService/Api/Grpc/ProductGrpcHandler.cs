using Grpc.Core;
using ProductService.Core.Contracts;
using ProductService.Protos;

namespace ProductService.Api.Grpc;

public sealed class ProductGrpcHandler : Protos.ProductService.ProductServiceBase
{
    private readonly IProductStore _store;

    public ProductGrpcHandler(IProductStore store)
    {
        _store = store;
    }

    public override async Task<ProductResponse> GetProduct(GetProductRequest request, ServerCallContext context)
    {
        var entity = await _store.FindByIdAsync(request.Id);
        
        if (entity == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Product with id {request.Id} not found"));
        }

        return new ProductResponse
        {
            Id = entity.Id,
            Name = entity.ProductName,
            Description = entity.Description,
            Price = (double)entity.UnitPrice
        };
    }

    public override async Task<GetAllProductsResponse> GetAllProducts(GetAllProductsRequest request, ServerCallContext context)
    {
        var entities = await _store.ListAllAsync();
        
        var response = new GetAllProductsResponse();
        response.Products.AddRange(entities.Select(e => new ProductResponse
        {
            Id = e.Id,
            Name = e.ProductName,
            Description = e.Description,
            Price = (double)e.UnitPrice
        }));

        return response;
    }
}

