using UserService.Core.Contracts;
using UserService.Core.Entities;

namespace UserService.Api.Controllers;

public static class UserController
{
    public static void RegisterUserRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users");

        group.MapGet("/{id:int}", async (int id, IUserStore store) =>
        {
            var entity = await store.FindByIdAsync(id);
            return entity != null
                ? Results.Ok(entity)
                : Results.NotFound(new { error = $"User with id {id} not found" });
        })
        .WithName("FindUserById")
        .WithOpenApi();

        group.MapGet("/", async (IUserStore store) =>
        {
            var entities = await store.ListAllAsync();
            return Results.Ok(entities);
        })
        .WithName("ListAllUsers")
        .WithOpenApi();
    }
}

