namespace DocIntegrator.Application.Auth;

/// <summary>
/// Запрос на вход (логин).
/// </summary>
public class LoginRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
