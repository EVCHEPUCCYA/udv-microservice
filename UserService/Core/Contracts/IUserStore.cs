using UserService.Core.Entities;

namespace UserService.Core.Contracts;

public interface IUserStore
{
    Task<UserEntity?> FindByIdAsync(int id);
    Task<IReadOnlyList<UserEntity>> ListAllAsync();
}

