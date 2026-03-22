namespace DocIntegrator.Application.Auth;

/// <summary>
/// Ответ после успешного входа: JWT и данные пользователя.
/// </summary>
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
