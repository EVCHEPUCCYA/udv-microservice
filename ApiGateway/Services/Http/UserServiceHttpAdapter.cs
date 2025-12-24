using ApiGateway.Models;
using System.Text.Json;
using Serilog;

namespace ApiGateway.Services;

public sealed class UserServiceHttpAdapter : IUserServiceClient
{
    private readonly HttpClient _httpClient;

    public UserServiceHttpAdapter(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("UserService");
    }

    public async Task<User?> GetUserAsync(int userId)
    {
        try
        {
            var httpResponse = await _httpClient.GetAsync($"/api/users/{userId}");
            
            if (httpResponse.IsSuccessStatusCode)
            {
                var jsonContent = await httpResponse.Content.ReadAsStringAsync();
                var dto = JsonSerializer.Deserialize<UserDto>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (dto == null) return null;
                
                return new User
                {
                    Id = dto.Id,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.EmailAddress
                };
            }
            
            Log.Warning("UserService returned status {StatusCode} for userId {UserId}", httpResponse.StatusCode, userId);
            return null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving user {UserId}", userId);
            return null;
        }
    }

    private sealed record UserDto
    {
        public int Id { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string EmailAddress { get; init; } = string.Empty;
    }
}

