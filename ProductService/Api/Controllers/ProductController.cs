using ProductService.Core.Contracts;

namespace ProductService.Api.Controllers;

public static class ProductController
{
    public static void RegisterProductRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products");

        group.MapGet("/{id:int}", async (int id, IProductStore store) =>
        {
            var entity = await store.FindByIdAsync(id);
            return entity != null
                ? Results.Ok(entity)
                : Results.NotFound(new { error = $"Product with id {id} not found" });
        })
        .WithName("FindProductById")
        .WithOpenApi();

        group.MapGet("/", async (IProductStore store) =>
        {
            var entities = await store.ListAllAsync();
            return Results.Ok(entities);
        })
        .WithName("ListAllProducts")
        .WithOpenApi();
    }
}

