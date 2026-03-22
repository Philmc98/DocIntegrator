using DocIntegrator.Application.Auth;
using DocIntegrator.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocIntegrator.Api.Controllers;

/// <summary>
/// Аутентификация: вход по логину/паролю, получение JWT.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    /// <summary>Инициализирует контроллер с сервисом аутентификации и логгером.</summary>
    /// <param name="authService">Сервис выдачи JWT-токенов.</param>
    /// <param name="logger">Логгер.</param>
    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Вход: логин и пароль. Возвращает JWT для заголовка Authorization: Bearer {token}.
    /// Демо: admin/admin, user/user.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ct);
        if (result == null)
        {
            _logger.LogWarning("Неудачный вход для пользователя {UserName}", request.UserName);
            return Unauthorized(new { message = "Неверный логин или пароль." });
        }
        _logger.LogInformation("Успешный вход: {UserName}, роль {Role}", result.UserName, result.Role);
        return Ok(result);
    }
}
