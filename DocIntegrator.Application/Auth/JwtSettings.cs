namespace DocIntegrator.Application.Auth;

/// <summary>
/// Настройки JWT (ключ, издатель, аудитория, время жизни).
/// Значения задаются в appsettings или переменных окружения (Jwt:Key и т.д.).
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}
