using Microsoft.AspNetCore.Server.Kestrel.Core;
using OrderService.Api.Controllers;
using OrderService.Api.Grpc;
using OrderService.Core.Contracts;
using OrderService.Infrastructure.Observability;
using OrderService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080, o => o.Protocols = HttpProtocols.Http1);
    options.ListenAnyIP(8081, o => o.Protocols = HttpProtocols.Http2);
});

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<IOrderStore, MemoryOrderStore>();

builder.Services.AddGrpc();

var app = builder.Build();

app.ConfigureMetrics();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.RegisterOrderRoutes();

app.MapGrpcService<OrderGrpcHandler>();

app.Run();
