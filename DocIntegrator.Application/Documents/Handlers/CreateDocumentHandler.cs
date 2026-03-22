using MediatR;
using DocIntegrator.Application.Interfaces;
using DocIntegrator.Application.Documents.Dtos;
using DocIntegrator.Application.Documents.Commands;
using Microsoft.Extensions.Logging;

namespace DocIntegrator.Application.Documents.Handlers;

/// <summary>
/// Хендлер для создания нового документа.
/// Получает DTO, валидирует, сохраняет в БД и возвращает DTO с Id и CreatedAt.
/// </summary>
public class CreateDocumentHandler : IRequestHandler<CreateDocumentCommand, DocumentDto>
{
    private readonly IDocumentRepository _repository;
    private readonly IDocumentIntegrationService _integrationService;
    private readonly IDocumentCache _cache;
    private readonly ILogger<CreateDocumentHandler> _logger;

    /// <summary>
    /// Внедряем репозиторий, интеграционный сервис (Kafka + event store) и кеш, а также логгер через DI.
    /// </summary>
    public CreateDocumentHandler(
        IDocumentRepository repository,
        IDocumentIntegrationService integrationService,
        IDocumentCache cache,
        ILogger<CreateDocumentHandler> logger)
    {
        _repository = repository;
        _integrationService = integrationService;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Обрабатываем команду: создаём сущность, сохраняем, возвращаем DTO.
    /// </summary>
    public async Task<DocumentDto> Handle(CreateDocumentCommand request, CancellationToken ct)
    {
        var dto = request.Document;

        // Создаем сущность документа.
        var entity = new Domain.Entities.Document
        {
            Id = Guid.NewGuid(), // Генерируем уникальный идентификатор.
            Title = dto.Title,
            Content = dto.Content,
            Status = dto.Status,
            CreatedAt = DateTime.UtcNow  // Фиксируем момент создания.
        };

        // Сохраняем в БД через репозиторий (основное состояние).
        await _repository.AddAsync(entity, ct);

        // Маппим сущность в DTO для ответа и интеграций.
        var createdDto = MapToDto(entity);

        // Отправляем интеграционное событие:
        // - запишется в event store (event sourcing / аудит),
        // - уйдёт в Kafka для других микросервисов.
        await _integrationService.HandleDocumentCreatedAsync(createdDto, ct);

        // Кладём свежесозданный документ в кеш Redis, чтобы ускорить последующие чтения.
        await _cache.SetAsync(createdDto, ct);

        // Логируем успешное создание.
        _logger.LogInformation("Документ создан. Id = {DocumentId}, Title = {Title}", entity.Id, entity.Title);

        // Возвращаем DTO клиенту.
        return createdDto;
    }

    /// <summary>
    /// Приватный метод маппинга сущности в DTO.
    /// </summary>
    private DocumentDto MapToDto(DocIntegrator.Domain.Entities.Document doc)
        => new DocumentDto
        {
            Id = doc.Id,
            Title = doc.Title,
            Content = doc.Content,
            Status = doc.Status,
            CreatedAt = doc.CreatedAt
        };
}
