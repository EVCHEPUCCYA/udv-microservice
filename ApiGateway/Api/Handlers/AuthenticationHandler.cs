using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiGateway.Api.Handlers;

public static class AuthenticationHandler
{
    public static IResult GenerateToken(string username, IConfiguration config)
    {
        var jwtKey = config["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
        var jwtIssuer = config["Jwt:Issuer"] ?? "ApiGateway";
        var jwtAudience = config["Jwt:Audience"] ?? "ApiGateway";

        var handler = new JwtSecurityTokenHandler();
        var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
        
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, username)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = jwtIssuer,
            Audience = jwtAudience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = handler.CreateToken(descriptor);
        var tokenString = handler.WriteToken(token);

        return Results.Ok(new { token = tokenString, expiresInSeconds = 3600 });
    }
}

