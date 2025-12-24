using StackExchange.Redis;
using Serilog;

namespace ApiGateway.Api.Middleware;

public sealed class RequestThrottlingMiddleware
{
    private readonly RequestDelegate _next;
    private const int MaxRequestsPerMinute = 120;
    private const int ThrottleWindowSeconds = 60;

    public RequestThrottlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IConnectionMultiplexer redis)
    {
        var clientIdentifier = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var endpoint = context.Request.Path.Value ?? "";
        var throttleKey = $"rate_limit:{clientIdentifier}:{endpoint}";
        
        try
        {
            var db = redis.GetDatabase();
            var currentCount = await db.StringIncrementAsync(throttleKey);
            
            if (currentCount == 1)
            {
                await db.KeyExpireAsync(throttleKey, TimeSpan.FromSeconds(ThrottleWindowSeconds));
            }
            
            if (currentCount > MaxRequestsPerMinute)
            {
                context.Response.StatusCode = 429;
                context.Response.Headers.Append("X-Throttle-Limit", MaxRequestsPerMinute.ToString());
                context.Response.Headers.Append("X-Throttle-Remaining", "0");
                await context.Response.WriteAsJsonAsync(new { error = "Request rate limit exceeded" });
                return;
            }
            
            context.Response.Headers.Append("X-Throttle-Limit", MaxRequestsPerMinute.ToString());
            context.Response.Headers.Append("X-Throttle-Remaining", (MaxRequestsPerMinute - currentCount).ToString());
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Throttling check failed, allowing request");
        }
        
        await _next(context);
    }
}

