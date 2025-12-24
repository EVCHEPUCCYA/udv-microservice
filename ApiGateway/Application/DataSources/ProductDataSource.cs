using ApiGateway.Domain.UserProfile.Models;
using ApiGateway.Services;
using Serilog;

namespace ApiGateway.Application.DataSources;

public sealed class ProductDataSource : IProductDataSource
{
    private readonly IProductServiceClient _serviceClient;

    public ProductDataSource(IProductServiceClient serviceClient)
    {
        _serviceClient = serviceClient;
    }

    public async Task<Dictionary<int, ProductInfo>> RetrieveProductsByIdsAsync(IEnumerable<int> productIds)
    {
        var result = new Dictionary<int, ProductInfo>();
        
        try
        {
            var products = await _serviceClient.GetProductsAsync(productIds);
            
            foreach (var kvp in products)
            {
                result[kvp.Key] = new ProductInfo
                {
                    Id = kvp.Value.Id,
                    ProductName = kvp.Value.Name,
                    Description = kvp.Value.Description,
                    UnitPrice = kvp.Value.Price
                };
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unable to retrieve products for ids: {ProductIds}", string.Join(", ", productIds));
        }
        
        return result;
    }
}

