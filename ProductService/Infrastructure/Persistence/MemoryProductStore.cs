using ProductService.Core.Contracts;
using ProductService.Core.Entities;

namespace ProductService.Infrastructure.Persistence;

public sealed class MemoryProductStore : IProductStore
{
    private readonly Dictionary<int, ProductEntity> _storage = new()
    {
        { 201, new ProductEntity { Id = 201, ProductName = "Игровая консоль", Description = "Новейшая игровая консоль с поддержкой 4K", UnitPrice = 1249.50m } },
        { 202, new ProductEntity { Id = 202, ProductName = "Ультрабук", Description = "Легкий и производительный ультрабук", UnitPrice = 1899.00m } },
        { 203, new ProductEntity { Id = 203, ProductName = "Умные часы", Description = "Фитнес-трекер с GPS и мониторингом здоровья", UnitPrice = 799.00m } },
        { 204, new ProductEntity { Id = 204, ProductName = "Колонка Bluetooth", Description = "Портативная колонка с объемным звуком", UnitPrice = 349.99m } },
        { 205, new ProductEntity { Id = 205, ProductName = "Веб-камера", Description = "4K веб-камера для видеоконференций", UnitPrice = 249.99m } }
    };

    public Task<ProductEntity?> FindByIdAsync(int id)
    {
        _storage.TryGetValue(id, out var entity);
        return Task.FromResult(entity);
    }

    public Task<IReadOnlyList<ProductEntity>> ListAllAsync()
    {
        return Task.FromResult<IReadOnlyList<ProductEntity>>(_storage.Values.ToList());
    }
}

