using OrderService.Core.Contracts;

namespace OrderService.Api.Controllers;

public static class OrderController
{
    public static void RegisterOrderRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders");

        group.MapGet("/user/{customerId:int}", async (int customerId, IOrderStore store) =>
        {
            var entities = await store.FindByCustomerIdAsync(customerId);
            return Results.Ok(entities);
        })
        .WithName("FindOrdersByCustomerId")
        .WithOpenApi();

        group.MapGet("/{id:int}", async (int id, IOrderStore store) =>
        {
            var entity = await store.FindByIdAsync(id);
            return entity != null
                ? Results.Ok(entity)
                : Results.NotFound(new { error = $"Order with id {id} not found" });
        })
        .WithName("FindOrderById")
        .WithOpenApi();
    }
}

