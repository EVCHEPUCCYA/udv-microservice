using ApiGateway.Models;
using System.Text.Json;
using Serilog;

namespace ApiGateway.Services;

public sealed class OrderServiceHttpAdapter : IOrderServiceClient
{
    private readonly HttpClient _httpClient;

    public OrderServiceHttpAdapter(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("OrderService");
    }

    public async Task<List<Order>> GetUserOrdersAsync(int userId)
    {
        try
        {
            var httpResponse = await _httpClient.GetAsync($"/api/orders/user/{userId}");
            
            if (httpResponse.IsSuccessStatusCode)
            {
                var jsonContent = await httpResponse.Content.ReadAsStringAsync();
                var orderDtos = JsonSerializer.Deserialize<List<OrderDto>>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (orderDtos == null) return new List<Order>();
                
                return orderDtos.Select(dto => new Order
                {
                    Id = dto.Id,
                    UserId = dto.CustomerId,
                    ProductId = dto.ProductId,
                    Quantity = dto.ItemQuantity,
                    TotalPrice = dto.TotalAmount,
                    OrderDate = dto.CreatedAt
                }).ToList();
            }
            
            Log.Warning("OrderService returned status {StatusCode} for userId {UserId}", httpResponse.StatusCode, userId);
            return new List<Order>();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error loading orders for userId {UserId}", userId);
            return new List<Order>();
        }
    }

    private sealed record OrderDto
    {
        public int Id { get; init; }
        public int CustomerId { get; init; }
        public int ProductId { get; init; }
        public int ItemQuantity { get; init; }
        public decimal TotalAmount { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}

