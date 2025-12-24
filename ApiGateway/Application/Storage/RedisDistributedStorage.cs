using StackExchange.Redis;
using System.Text.Json;
using Serilog;

namespace ApiGateway.Application.Storage;

public sealed class RedisDistributedStorage : IDistributedStorage
{
    private readonly IDatabase _db;
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public RedisDistributedStorage(IConnectionMultiplexer multiplexer)
    {
        _db = multiplexer.GetDatabase();
    }

    public async Task<TValue?> LoadAsync<TValue>(string storageKey) where TValue : class
    {
        try
        {
            var value = await _db.StringGetAsync(storageKey);
            if (!value.HasValue)
            {
                return null;
            }

            return JsonSerializer.Deserialize<TValue>(value!, SerializerOptions);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load from storage. Key: {StorageKey}", storageKey);
            return null;
        }
    }

    public async Task SaveAsync<TValue>(string storageKey, TValue value, TimeSpan ttl) where TValue : class
    {
        try
        {
            var serialized = JsonSerializer.Serialize(value, SerializerOptions);
            await _db.StringSetAsync(storageKey, serialized, ttl);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save to storage. Key: {StorageKey}", storageKey);
        }
    }

    public async Task InvalidateAsync(string storageKey)
    {
        try
        {
            await _db.KeyDeleteAsync(storageKey);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to invalidate storage key: {StorageKey}", storageKey);
        }
    }

    public async Task<bool> ContainsKeyAsync(string storageKey)
    {
        try
        {
            return await _db.KeyExistsAsync(storageKey);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to check storage key existence: {StorageKey}", storageKey);
            return false;
        }
    }
}

