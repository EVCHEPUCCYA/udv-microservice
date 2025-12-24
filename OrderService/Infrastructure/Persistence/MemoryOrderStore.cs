using OrderService.Core.Contracts;
using OrderService.Core.Entities;

namespace OrderService.Infrastructure.Persistence;

public sealed class MemoryOrderStore : IOrderStore
{
    private readonly Dictionary<int, List<OrderEntity>> _storage = new()
    {
        { 1, new List<OrderEntity> 
            { 
                new OrderEntity { Id = 1, CustomerId = 1, ProductId = 201, ItemQuantity = 1, TotalAmount = 1249.50m, CreatedAt = DateTime.UtcNow.AddDays(-7) }, 
                new OrderEntity { Id = 2, CustomerId = 1, ProductId = 203, ItemQuantity = 2, TotalAmount = 1598.00m, CreatedAt = DateTime.UtcNow.AddDays(-3) },
                new OrderEntity { Id = 3, CustomerId = 1, ProductId = 204, ItemQuantity = 1, TotalAmount = 349.99m, CreatedAt = DateTime.UtcNow.AddDays(-1) }
            } 
        },
        { 2, new List<OrderEntity> 
            { 
                new OrderEntity { Id = 4, CustomerId = 2, ProductId = 202, ItemQuantity = 1, TotalAmount = 1899.00m, CreatedAt = DateTime.UtcNow.AddDays(-12) },
                new OrderEntity { Id = 5, CustomerId = 2, ProductId = 201, ItemQuantity = 1, TotalAmount = 1249.50m, CreatedAt = DateTime.UtcNow.AddDays(-8) }
            } 
        },
        { 3, new List<OrderEntity> 
            { 
                new OrderEntity { Id = 6, CustomerId = 3, ProductId = 203, ItemQuantity = 1, TotalAmount = 799.00m, CreatedAt = DateTime.UtcNow.AddDays(-15) },
                new OrderEntity { Id = 7, CustomerId = 3, ProductId = 204, ItemQuantity = 3, TotalAmount = 1049.97m, CreatedAt = DateTime.UtcNow.AddDays(-4) }
            } 
        },
        { 4, new List<OrderEntity> 
            { 
                new OrderEntity { Id = 8, CustomerId = 4, ProductId = 201, ItemQuantity = 2, TotalAmount = 2499.00m, CreatedAt = DateTime.UtcNow.AddDays(-6) }
            } 
        }
    };

    public Task<OrderEntity?> FindByIdAsync(int id)
    {
        var entity = _storage.Values
            .SelectMany(o => o)
            .FirstOrDefault(o => o.Id == id);
        
        return Task.FromResult(entity);
    }

    public Task<IReadOnlyList<OrderEntity>> FindByCustomerIdAsync(int customerId)
    {
        _storage.TryGetValue(customerId, out var orders);
        return Task.FromResult<IReadOnlyList<OrderEntity>>(orders?.ToList() ?? new List<OrderEntity>());
    }
}

