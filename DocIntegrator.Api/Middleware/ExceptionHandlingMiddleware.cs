using FluentValidation;
using DocIntegrator.Application.Common.Exceptions;

namespace DocIntegrator.Api.Middleware;

/// <summary>
/// Глобальный middleware для обработки исключений.
/// Перехватывает все ошибки, логирует их и возвращает корректный JSON-ответ клиенту.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    // Конструктор middleware, принимающий следующий делегат запроса и логгер
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Основной метод, который будет вызван для каждого HTTP-запроса
    /// </summary>
    public async Task Invoke(HttpContext context)
    {
        try
        {
            // Передаем управление дальше по конвейеру (к контроллерам и другим middleware)
            await _next(context); 
        }
        catch (ValidationException ex)
        {
            // Ошибка валидации (FluentValidation)
            _logger.LogWarning("Validation failed: {Errors}", ex.Errors);

            // Возвращаем 400 Bad Request с подробным списком ошибок
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var errors = ex.Errors
                .GroupBy(e => e.PropertyName) // Группируем ошибки по имени свойства
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            var result = new
            {
                title = "Validation Failed",
                status = StatusCodes.Status400BadRequest,
                errors
            };

            await context.Response.WriteAsJsonAsync(result);
        }
        catch (NotFoundException ex)
        {
            // Ошибка "не найдено"
            _logger.LogWarning("Resource not found: {Message}", ex.Message);

            // Возвращаем 404 Not Found с деталями
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "application/json";

            var result = new
            {
                title = "Not Found",
                status = StatusCodes.Status404NotFound,
                detail = ex.Message
            };

            await context.Response.WriteAsJsonAsync(result);
        }
        catch (Exception ex)
        {
            // Любая непредвиденная ошибка
            _logger.LogError(ex, "Unhandled exception");

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var result = new
            {
                title = "Internal Server Error",
                status = StatusCodes.Status500InternalServerError,
                detail = "Внутренняя ошибка сервера. Обратитесь к администратору."
            };

            await context.Response.WriteAsJsonAsync(result);
        }
    }
}
