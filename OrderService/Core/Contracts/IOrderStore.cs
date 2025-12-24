using OrderService.Core.Entities;

namespace OrderService.Core.Contracts;

public interface IOrderStore
{
    Task<OrderEntity?> FindByIdAsync(int id);
    Task<IReadOnlyList<OrderEntity>> FindByCustomerIdAsync(int customerId);
}

