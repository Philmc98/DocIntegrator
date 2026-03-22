using DocIntegrator.Application.Auth;

namespace DocIntegrator.Application.Interfaces;

/// <summary>
/// Сервис аутентификации: проверка учётных данных и выдача JWT.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Проверить логин/пароль и выдать JWT при успехе.
    /// </summary>
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
