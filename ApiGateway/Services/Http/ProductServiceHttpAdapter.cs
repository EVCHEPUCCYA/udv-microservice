using ApiGateway.Models;
using System.Text.Json;
using Serilog;

namespace ApiGateway.Services;

public sealed class ProductServiceHttpAdapter : IProductServiceClient
{
    private readonly HttpClient _httpClient;

    public ProductServiceHttpAdapter(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ProductService");
    }

    public async Task<Dictionary<int, Product>> GetProductsAsync(IEnumerable<int> productIds)
    {
        var productCatalog = new Dictionary<int, Product>();
        
        foreach (var productId in productIds)
        {
            try
            {
                var httpResponse = await _httpClient.GetAsync($"/api/products/{productId}");
                
                if (httpResponse.IsSuccessStatusCode)
                {
                    var jsonContent = await httpResponse.Content.ReadAsStringAsync();
                    var dto = JsonSerializer.Deserialize<ProductDto>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    if (dto != null)
                    {
                        productCatalog[productId] = new Product
                        {
                            Id = dto.Id,
                            Name = dto.ProductName,
                            Description = dto.Description,
                            Price = dto.UnitPrice
                        };
                    }
                }
                else
                {
                    Log.Warning("ProductService returned status {StatusCode} for productId {ProductId}", httpResponse.StatusCode, productId);
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error fetching product {ProductId}", productId);
            }
        }
        
        return productCatalog;
    }

    private sealed record ProductDto
    {
        public int Id { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public decimal UnitPrice { get; init; }
    }
}

