using ProductService.Core.Entities;

namespace ProductService.Core.Contracts;

public interface IProductStore
{
    Task<ProductEntity?> FindByIdAsync(int id);
    Task<IReadOnlyList<ProductEntity>> ListAllAsync();
}

