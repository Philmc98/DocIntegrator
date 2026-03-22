namespace DocIntegrator.Api.Middleware;

/// <summary>
/// Логирует входящий HTTP-запрос (метод, путь) и код ответа после выполнения.
/// Помогает при отладке и аудите без логирования тел запросов/ответов.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    /// <summary>Инициализирует middleware со следующим делегатом запроса и логгером.</summary>
    /// <param name="next">Следующий middleware в конвейере.</param>
    /// <param name="logger">Логгер для записи информации о запросах.</param>
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>Логирует запрос, передаёт управление следующему middleware и логирует код ответа.</summary>
    /// <param name="context">Контекст текущего HTTP-запроса.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path;
        _logger.LogInformation("Запрос: {Method} {Path}", method, path);

        await _next(context);

        var statusCode = context.Response.StatusCode;
        var level = statusCode >= 500 ? LogLevel.Error : statusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
        _logger.Log(level, "Ответ: {Method} {Path} -> {StatusCode}", method, path, statusCode);
    }
}
