namespace ApiGateway.Application.Storage;

public interface IDistributedStorage
{
    Task<TValue?> LoadAsync<TValue>(string storageKey) where TValue : class;
    Task SaveAsync<TValue>(string storageKey, TValue value, TimeSpan ttl) where TValue : class;
    Task InvalidateAsync(string storageKey);
    Task<bool> ContainsKeyAsync(string storageKey);
}

