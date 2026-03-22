using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DocIntegrator.Application.Auth;
using DocIntegrator.Application.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DocIntegrator.Infrastructure.Auth;

/// <summary>
/// Сервис аутентификации: проверка учётных данных (in-memory для демо) и выдача JWT.
/// В продакшене пользователи хранятся в БД, пароли — хэш BCrypt.
/// </summary>
public class AuthService : IAuthService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IReadOnlyList<AuthUser> _users;

    public AuthService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
        // Демо: два пользователя. В продакшене — таблица Users в БД.
        _users = new List<AuthUser>
        {
            new("admin", BCrypt.Net.BCrypt.HashPassword("admin"), "Admin"),
            new("user", BCrypt.Net.BCrypt.HashPassword("user"), "User")
        };
    }

    public Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = _users.FirstOrDefault(u =>
            string.Equals(u.UserName, request.UserName, StringComparison.OrdinalIgnoreCase));

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Task.FromResult<LoginResponse?>(null);

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);
        var token = GenerateJwt(user.UserName, user.Role, user.UserName, expiresAt);

        return Task.FromResult<LoginResponse?>(new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            UserName = user.UserName,
            Role = user.Role
        });
    }

    private string GenerateJwt(string userId, string role, string userName, DateTime expiresAt)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, userName),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed record AuthUser(string UserName, string PasswordHash, string Role);
}
