using Microsoft.AspNetCore.Server.Kestrel.Core;
using UserService.Api.Controllers;
using UserService.Api.Grpc;
using UserService.Core.Contracts;
using UserService.Infrastructure.Observability;
using UserService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080, o => o.Protocols = HttpProtocols.Http1);
    options.ListenAnyIP(8081, o => o.Protocols = HttpProtocols.Http2);
});

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<IUserStore, MemoryUserStore>();

builder.Services.AddGrpc();

var app = builder.Build();

app.ConfigureMetrics();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.RegisterUserRoutes();

app.MapGrpcService<UserGrpcHandler>();

app.Run();
