using ApiGateway.Models;

namespace ApiGateway.Services;

public interface IProductServiceClient
{
    Task<Dictionary<int, Product>> GetProductsAsync(IEnumerable<int> productIds);
}

