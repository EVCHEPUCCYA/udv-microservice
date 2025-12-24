using ApiGateway.Domain.UserProfile.Models;

namespace ApiGateway.Application.DataSources;

public interface IUserDataSource
{
    Task<UserInfo?> RetrieveUserByIdAsync(int userId);
}

