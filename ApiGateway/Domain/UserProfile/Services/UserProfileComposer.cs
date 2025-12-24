using ApiGateway.Application.DataSources;
using ApiGateway.Application.Storage;
using ApiGateway.Domain.UserProfile.Contracts;
using ApiGateway.Domain.UserProfile.Models;
using Polly.CircuitBreaker;
using Serilog;

namespace ApiGateway.Domain.UserProfile.Services;

public sealed class UserProfileComposer : IUserProfileComposer
{
    private readonly IUserDataSource _userSource;
    private readonly IOrderDataSource _orderSource;
    private readonly IProductDataSource _productSource;
    private readonly IDistributedStorage _storage;
    private const string StorageKeyTemplate = "user_profile_cache:{0}";

    public UserProfileComposer(
        IUserDataSource userSource,
        IOrderDataSource orderSource,
        IProductDataSource productSource,
        IDistributedStorage storage)
    {
        _userSource = userSource;
        _orderSource = orderSource;
        _productSource = productSource;
        _storage = storage;
    }

    public async Task<CompositeUserProfile?> ComposeUserProfileAsync(int userId)
    {
        var storageKey = string.Format(StorageKeyTemplate, userId);
        
        var cached = await _storage.LoadAsync<CompositeUserProfile>(storageKey);
        if (cached != null)
        {
            Log.Information("Loaded composite profile from storage for userId: {UserId}", userId);
            return cached;
        }

        Log.Information("Composing profile from data sources for userId: {UserId}", userId);

        try
        {
            var userTask = _userSource.RetrieveUserByIdAsync(userId);
            var ordersTask = _orderSource.RetrieveOrdersByUserIdAsync(userId);

            await Task.WhenAll(userTask, ordersTask);

            var user = await userTask;
            var orders = await ordersTask;

            if (user == null && orders.Count == 0)
            {
                Log.Warning("No data available for userId: {UserId}", userId);
                return null;
            }

            var productIds = orders
                .Where(o => o.ProductInfo == null)
                .Select(o => o.ProductId)
                .Distinct()
                .ToList();

            var products = productIds.Count > 0
                ? await _productSource.RetrieveProductsByIdsAsync(productIds)
                : new Dictionary<int, ProductInfo>();

            var enrichedOrders = orders.Select(order =>
            {
                var enriched = new OrderInfo
                {
                    OrderId = order.OrderId,
                    CustomerId = order.CustomerId,
                    ProductId = order.ProductId,
                    ItemQuantity = order.ItemQuantity,
                    OrderTotal = order.OrderTotal,
                    CreatedDateTime = order.CreatedDateTime,
                    ProductInfo = order.ProductInfo
                };

                if (enriched.ProductInfo == null && products.TryGetValue(order.ProductId, out var product))
                {
                    enriched.ProductInfo = product;
                }

                return enriched;
            }).ToList();

            var profile = new CompositeUserProfile
            {
                UserInfo = user,
                OrderHistory = enrichedOrders,
                OrderCount = enrichedOrders.Count,
                TotalPurchaseAmount = enrichedOrders.Sum(o => o.OrderTotal)
            };

            var ttl = user != null ? TimeSpan.FromSeconds(45) : TimeSpan.FromSeconds(20);
            await _storage.SaveAsync(storageKey, profile, ttl);

            Log.Information("Composed and stored profile for userId: {UserId}", userId);
            return profile;
        }
        catch (BrokenCircuitException ex)
        {
            Log.Error(ex, "Circuit breaker active for userId: {UserId}. Using fallback.", userId);
            return await ExecuteFallbackCompositionAsync(userId);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error composing profile for userId: {UserId}", userId);
            return await ExecuteFallbackCompositionAsync(userId);
        }
    }

    private async Task<CompositeUserProfile?> ExecuteFallbackCompositionAsync(int userId)
    {
        try
        {
            var userTask = _userSource.RetrieveUserByIdAsync(userId);
            var ordersTask = _orderSource.RetrieveOrdersByUserIdAsync(userId);

            await Task.WhenAll(userTask, ordersTask);

            var user = await userTask;
            var orders = await ordersTask;

            if (user == null && orders.Count == 0)
            {
                return null;
            }

            var productIds = orders.Select(o => o.ProductId).Distinct().ToList();
            var products = productIds.Count > 0
                ? await _productSource.RetrieveProductsByIdsAsync(productIds)
                : new Dictionary<int, ProductInfo>();

            var enrichedOrders = orders.Select(order =>
            {
                var enriched = new OrderInfo
                {
                    OrderId = order.OrderId,
                    CustomerId = order.CustomerId,
                    ProductId = order.ProductId,
                    ItemQuantity = order.ItemQuantity,
                    OrderTotal = order.OrderTotal,
                    CreatedDateTime = order.CreatedDateTime
                };

                if (products.TryGetValue(order.ProductId, out var product))
                {
                    enriched.ProductInfo = product;
                }

                return enriched;
            }).ToList();

            return new CompositeUserProfile
            {
                UserInfo = user,
                OrderHistory = enrichedOrders,
                OrderCount = enrichedOrders.Count,
                TotalPurchaseAmount = enrichedOrders.Sum(o => o.OrderTotal)
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fallback composition failed for userId: {UserId}", userId);
            return null;
        }
    }
}

