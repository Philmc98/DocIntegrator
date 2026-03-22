using DocIntegrator.Api.Models;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DocIntegrator.Api.Swagger;

/// <summary>
/// Добавляет в Swagger описание и примеры ответов 400, 404, 500 для всех операций.
/// Фронт видит единый формат ошибок и примеры.
/// </summary>
public class ErrorResponseOperationFilter : IOperationFilter
{
    /// <summary>
    /// Добавляет стандартные коды ответов (400, 401, 403, 404, 500) ко всем операциям Swagger.
    /// </summary>
    /// <param name="operation">Операция OpenAPI, к которой добавляются ответы.</param>
    /// <param name="context">Контекст фильтра с доступом к генератору схем.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // 400 — валидация
        operation.Responses.TryAdd("400", new OpenApiResponse
        {
            Description = "Ошибка валидации (неверные данные запроса)",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Schema = context.SchemaGenerator.GenerateSchema(typeof(ErrorResponse), context.SchemaRepository)
                }
            }
        });

        // 401 — не авторизован
        operation.Responses.TryAdd("401", new OpenApiResponse
        {
            Description = "Не авторизован. Укажите JWT в заголовке Authorization: Bearer {token}."
        });

        // 403 — доступ запрещён (роль)
        operation.Responses.TryAdd("403", new OpenApiResponse
        {
            Description = "Доступ запрещён. Требуется роль Admin для PUT/DELETE."
        });

        // 404 — не найдено
        operation.Responses.TryAdd("404", new OpenApiResponse
        {
            Description = "Ресурс не найден",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Schema = context.SchemaGenerator.GenerateSchema(typeof(ErrorResponse), context.SchemaRepository)
                }
            }
        });

        // 500 — внутренняя ошибка
        operation.Responses.TryAdd("500", new OpenApiResponse
        {
            Description = "Внутренняя ошибка сервера",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Schema = context.SchemaGenerator.GenerateSchema(typeof(ErrorResponse), context.SchemaRepository)
                }
            }
        });
    }
}
