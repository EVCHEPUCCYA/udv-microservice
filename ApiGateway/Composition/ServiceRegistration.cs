using ApiGateway.Application.DataSources;
using ApiGateway.Application.Storage;
using ApiGateway.Domain.UserProfile.Contracts;
using ApiGateway.Domain.UserProfile.Services;
using ApiGateway.Services;
using Microsoft.Extensions.Http;
using Ocelot.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Serilog;
using StackExchange.Redis;

namespace ApiGateway.Composition;

public static class ServiceRegistration
{
    public static IServiceCollection RegisterApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.RegisterDistributedStorage(config);
        services.RegisterResilientHttpClients(config);
        services.RegisterDataSources(config);
        services.RegisterDomainServices();
        
        return services;
    }

    private static IServiceCollection RegisterDistributedStorage(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config["Redis:ConnectionString"] ?? "localhost:6379";
        
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(connectionString));
        
        services.AddScoped<IDistributedStorage, RedisDistributedStorage>();
        
        return services;
    }

    private static IServiceCollection RegisterResilientHttpClients(this IServiceCollection services, IConfiguration config)
    {
        var userServiceBaseUrl = config["Services:UserService"] ?? "http://localhost:5001";
        var orderServiceBaseUrl = config["Services:OrderService"] ?? "http://localhost:5002";
        var productServiceBaseUrl = config["Services:ProductService"] ?? "http://localhost:5003";

        var retryStrategy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 4,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(1.8, attempt)),
                onRetry: (outcome, timespan, attempt, context) =>
                {
                    Log.Warning("HTTP retry attempt {Attempt} after {Delay}ms", attempt, timespan.TotalMilliseconds);
                });

        var breakerStrategy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 6,
                durationOfBreak: TimeSpan.FromSeconds(45),
                onBreak: (result, duration) =>
                {
                    Log.Error("Circuit breaker opened for {Duration}s", duration.TotalSeconds);
                },
                onReset: () =>
                {
                    Log.Information("Circuit breaker reset");
                });

        var combinedPolicy = Policy.WrapAsync(retryStrategy, breakerStrategy);

        services.AddHttpClient("UserService", client =>
        {
            client.BaseAddress = new Uri(userServiceBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(8);
        }).AddPolicyHandler(combinedPolicy);

        services.AddHttpClient("OrderService", client =>
        {
            client.BaseAddress = new Uri(orderServiceBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(7);
        }).AddPolicyHandler(combinedPolicy);

        services.AddHttpClient("ProductService", client =>
        {
            client.BaseAddress = new Uri(productServiceBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(6);
        }).AddPolicyHandler(combinedPolicy);

        return services;
    }

    private static IServiceCollection RegisterDataSources(this IServiceCollection services, IConfiguration config)
    {
        var useGrpc = config.GetValue<bool>("Services:UseGrpc", true);
        
        if (useGrpc)
        {
            services.AddScoped<IUserServiceClient>(sp => 
                new UserServiceGrpcClient(config, Log.Logger));
            services.AddScoped<IOrderServiceClient>(sp => 
                new OrderServiceGrpcClient(config, Log.Logger));
            services.AddScoped<IProductServiceClient>(sp => 
                new ProductServiceGrpcClient(config, Log.Logger));
        }
        else
        {
            services.AddScoped<IUserServiceClient, UserServiceHttpAdapter>();
            services.AddScoped<IOrderServiceClient, OrderServiceHttpAdapter>();
            services.AddScoped<IProductServiceClient, ProductServiceHttpAdapter>();
        }
        
        services.AddScoped<IUserDataSource, UserDataSource>();
        services.AddScoped<IOrderDataSource, OrderDataSource>();
        services.AddScoped<IProductDataSource, ProductDataSource>();
        
        return services;
    }

    private static IServiceCollection RegisterDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IUserProfileComposer, UserProfileComposer>();
        
        return services;
    }
}

