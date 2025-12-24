using ApiGateway.Domain.UserProfile.Contracts;
using ApiGateway.Domain.UserProfile.Models;
using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;
using Serilog;

namespace ApiGateway.Api.Handlers;

public static class UserProfileHandler
{
    public static async Task<IResult> GetUserProfile(
        int userId,
        IUserProfileComposer composer)
    {
        try
        {
            var profile = await composer.ComposeUserProfileAsync(userId);
            
            if (profile == null)
            {
                return Results.NotFound(new { error = $"Profile not found for user {userId}" });
            }
            
            return Results.Ok(profile);
        }
        catch (BrokenCircuitException ex)
        {
            Log.Error(ex, "Circuit breaker triggered for userId: {UserId}", userId);
            return Results.StatusCode(503);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error retrieving profile for userId: {UserId}", userId);
            return Results.Problem("An error occurred processing the request");
        }
    }
}

