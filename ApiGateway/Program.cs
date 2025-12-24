using ApiGateway.Api.Handlers;
using ApiGateway.Api.Middleware;
using ApiGateway.Composition;
using ApiGateway.Domain.UserProfile.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

builder.Services
    .RegisterApplicationServices(builder.Configuration)
    .ConfigureSecurity(builder.Configuration);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.ConfigureObservability();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/api/auth/login", (string username, IConfiguration config) => 
    AuthenticationHandler.GenerateToken(username, config))
    .WithTags("Authentication")
    .WithName("Login")
    .WithOpenApi();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<RequestThrottlingMiddleware>();

app.MapGet("/api/profile/{userId:int}", UserProfileHandler.GetUserProfile)
    .WithTags("UserProfile")
    .RequireAuthorization()
    .WithName("GetUserProfile")
    .WithOpenApi()
    .Produces<CompositeUserProfile>()
    .Produces(404)
    .Produces(503);

app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/api/auth") && 
               !context.Request.Path.StartsWithSegments("/api/profile") &&
               !context.Request.Path.StartsWithSegments("/metrics") &&
               !context.Request.Path.StartsWithSegments("/openapi"),
    appBuilder => appBuilder.UseOcelot().Wait());

app.Run();
