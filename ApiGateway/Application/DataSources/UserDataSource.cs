using ApiGateway.Domain.UserProfile.Models;
using ApiGateway.Services;
using Serilog;

namespace ApiGateway.Application.DataSources;

public sealed class UserDataSource : IUserDataSource
{
    private readonly IUserServiceClient _serviceClient;

    public UserDataSource(IUserServiceClient serviceClient)
    {
        _serviceClient = serviceClient;
    }

    public async Task<UserInfo?> RetrieveUserByIdAsync(int userId)
    {
        try
        {
            var user = await _serviceClient.GetUserAsync(userId);
            if (user == null)
            {
                return null;
            }

            return new UserInfo
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                EmailAddress = user.Email
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unable to retrieve user data for userId: {UserId}", userId);
            return null;
        }
    }
}

