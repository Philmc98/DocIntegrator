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
    private readonly ILogger<CreateDocumentHandler> _logger;

    /// <summary>
    /// Внедряем репозиторий и логгер через DI.
    /// </summary>
    public CreateDocumentHandler(IDocumentRepository repository, ILogger<CreateDocumentHandler> logger)
    {
        _repository = repository;
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

        // Сохраняем в БД через репозиторий.
        await _repository.AddAsync(entity, ct);

        // Логируем успешное создание.
        _logger.LogInformation("Документ создан. Id = {DocumentId}, Title = {Title}", entity.Id, entity.Title);

        // Возвращаем DTO клиенту.
        return MapToDto(entity);
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
