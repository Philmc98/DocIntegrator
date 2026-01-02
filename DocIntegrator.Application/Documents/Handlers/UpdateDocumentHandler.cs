using MediatR;
using DocIntegrator.Application.Interfaces;
using DocIntegrator.Application.Documents.Commands;
using DocIntegrator.Application.Documents.Dtos;
using Microsoft.Extensions.Logging;

namespace DocIntegrator.Application.Documents.Handlers;

/// <summary>
/// Хендлер для обновления существующего документа.
/// Проверяет наличие, обновляет поля и возвращает DTO.
/// </summary>
public class UpdateDocumentHandler : IRequestHandler<UpdateDocumentCommand, DocumentDto?>
{
    private readonly IDocumentRepository _repository;
    private readonly ILogger<UpdateDocumentHandler> _logger;

    /// <summary>
    /// Внедряем репозиторий и логгер через DI.
    /// </summary>
    public UpdateDocumentHandler(IDocumentRepository repository, ILogger<UpdateDocumentHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Обрабатываем команду: ищем документ, обновляем поля, сохраняем изменения.
    /// </summary>
    public async Task<DocumentDto?> Handle(UpdateDocumentCommand request, CancellationToken ct)
    {
        // Получаем документ из БД.
        var entity = await _repository.GetByIdAsync(request.Id, ct);

        // Если документ не найден — логируем и возвращаем null (контроллер вернёт 404).
        if (entity == null)
        {
            _logger.LogWarning("Обновление документа не удалось: документ с Id = {DocumentId} не найден.", request.Id);
            return null;
        }

        // Обновляем только допустимые поля.
        entity.Title = request.Document.Title;
        entity.Content = request.Document.Content;
        entity.Status = request.Document.Status;

        // Сохраняем изменения в БД.
        await _repository.UpdateAsync(entity, ct);

        _logger.LogInformation("Документ успешно обновлён. Id = {DocumentId}, Title = {Title}", entity.Id, entity.Title);

        // Возвращаем обновлённый DTO клиенту.
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
