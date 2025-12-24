using Microsoft.AspNetCore.Server.Kestrel.Core;
using ProductService.Api.Controllers;
using ProductService.Api.Grpc;
using ProductService.Core.Contracts;
using ProductService.Infrastructure.Observability;
using ProductService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080, o => o.Protocols = HttpProtocols.Http1);
    options.ListenAnyIP(8081, o => o.Protocols = HttpProtocols.Http2);
});

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<IProductStore, MemoryProductStore>();

builder.Services.AddGrpc();

var app = builder.Build();

app.ConfigureMetrics();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.RegisterProductRoutes();

app.MapGrpcService<ProductGrpcHandler>();

app.Run();
