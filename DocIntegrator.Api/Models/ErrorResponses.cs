namespace DocIntegrator.Api.Models;

/// <summary>
/// Единый формат ответа API при ошибке.
/// Используется для документирования в Swagger (400, 404, 500).
/// </summary>
public class ErrorResponse
{
    /// <summary>Краткое название ошибки (например, "Validation Failed", "Not Found").</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>HTTP-код ответа.</summary>
    public int Status { get; set; }

    /// <summary>Текстовое описание для 404/500. Для 400 не используется.</summary>
    public string? Detail { get; set; }

    /// <summary>Ошибки валидации по полям. Только для 400.</summary>
    public Dictionary<string, string[]>? Errors { get; set; }

    /// <summary>Идентификатор трассировки запроса для поиска в логах.</summary>
    public string? TraceId { get; set; }
}
