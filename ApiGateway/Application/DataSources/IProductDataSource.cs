using ApiGateway.Domain.UserProfile.Models;

namespace ApiGateway.Application.DataSources;

public interface IProductDataSource
{
    Task<Dictionary<int, ProductInfo>> RetrieveProductsByIdsAsync(IEnumerable<int> productIds);
}

