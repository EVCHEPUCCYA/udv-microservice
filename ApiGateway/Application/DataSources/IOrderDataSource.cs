using ApiGateway.Domain.UserProfile.Models;

namespace ApiGateway.Application.DataSources;

public interface IOrderDataSource
{
    Task<IReadOnlyList<OrderInfo>> RetrieveOrdersByUserIdAsync(int userId);
}

