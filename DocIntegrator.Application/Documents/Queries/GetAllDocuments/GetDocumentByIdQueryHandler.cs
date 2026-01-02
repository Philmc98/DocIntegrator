using DocIntegrator.Application.Documents.Dtos;
using DocIntegrator.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DocIntegrator.Application.Documents.Queries.GetAllDocuments;

/// <summary>
/// Хендлер для получения документа по идентификатору.
/// Возвращает DTO или null, если документ не найден (контроллер преобразует в 404).
/// </summary>
public class GetDocumentByIdQueryHandler : IRequestHandler<GetDocumentByIdQuery, DocumentDto?>
{
    private readonly IDocumentRepository _repository;
    private readonly ILogger<GetDocumentByIdQueryHandler> _logger;

    // <summary>
    /// Внедряем репозиторий (работа с БД) и логгер (для мониторинга).
    /// </summary>
    public GetDocumentByIdQueryHandler(IDocumentRepository repository, ILogger<GetDocumentByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Обрабатываем запрос: ищем документ по ID, логируем результат, маппим в DTO.
    /// </summary>
    public async Task<DocumentDto?> Handle(GetDocumentByIdQuery request, CancellationToken ct)
    {
        // Получаем сущность из репозитория.
        // Репозиторий читает с AsNoTracking — это быстрее и экономнее.
        var doc = await _repository.GetByIdAsync(request.Id, ct);

        // Если не найден — логируем и возвращаем null (контроллер вернёт 404).
        if (doc == null)
        {
            _logger.LogWarning("Документ не найден. Id = {DocumentId}", request.Id);
            return null;
        }

        // Маппим сущность домена в безопасный DTO для ответа клиенту.
        return MapToDto(doc);
    }

    /// <summary>
    /// Приватный метод маппинга, чтобы не дублировать логику по всему проекту.
    /// </summary>
    private static DocumentDto MapToDto(DocIntegrator.Domain.Entities.Document doc)
        => new DocumentDto
        {
            Id = doc.Id,
            Title = doc.Title,
            Content = doc.Content,
            Status = doc.Status,
            CreatedAt = doc.CreatedAt
        };
}
