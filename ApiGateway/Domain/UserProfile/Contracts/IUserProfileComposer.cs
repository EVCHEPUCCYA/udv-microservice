using ApiGateway.Domain.UserProfile.Models;

namespace ApiGateway.Domain.UserProfile.Contracts;

public interface IUserProfileComposer
{
    Task<CompositeUserProfile?> ComposeUserProfileAsync(int userId);
}

