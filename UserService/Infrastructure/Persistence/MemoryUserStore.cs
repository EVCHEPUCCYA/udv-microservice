using UserService.Core.Contracts;
using UserService.Core.Entities;

namespace UserService.Infrastructure.Persistence;

public sealed class MemoryUserStore : IUserStore
{
    private readonly Dictionary<int, UserEntity> _storage = new()
    {
        { 1, new UserEntity { Id = 1, FirstName = "Александр", LastName = "Кузнецов", EmailAddress = "alex.kuznetsov@mail.ru" } },
        { 2, new UserEntity { Id = 2, FirstName = "Елена", LastName = "Волкова", EmailAddress = "elena.volkova@gmail.com" } },
        { 3, new UserEntity { Id = 3, FirstName = "Дмитрий", LastName = "Новиков", EmailAddress = "dmitry.novikov@yandex.ru" } },
        { 4, new UserEntity { Id = 4, FirstName = "Ольга", LastName = "Соколова", EmailAddress = "olga.sokolova@outlook.com" } }
    };

    public Task<UserEntity?> FindByIdAsync(int id)
    {
        _storage.TryGetValue(id, out var entity);
        return Task.FromResult(entity);
    }

    public Task<IReadOnlyList<UserEntity>> ListAllAsync()
    {
        return Task.FromResult<IReadOnlyList<UserEntity>>(_storage.Values.ToList());
    }
}

