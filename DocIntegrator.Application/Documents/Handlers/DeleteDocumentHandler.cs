using MediatR;
using DocIntegrator.Application.Interfaces;
using DocIntegrator.Application.Documents.Commands;
using DocIntegrator.Application.Common.Exceptions;
using Microsoft.Extensions.Logging;

namespace DocIntegrator.Application.Documents.Handlers;

/// <summary>
/// Хендлер для удаления документа по идентификатору.
/// Возвращает true при успешном удалении, выбрасывает NotFoundException если документ не найден.
/// </summary>
public class DeleteDocumentHandler : IRequestHandler<DeleteDocumentCommand, bool>
{
    private readonly IDocumentRepository _repository;
    private readonly IDocumentIntegrationService _integrationService;
    private readonly IDocumentCache _cache;
    private readonly ILogger<DeleteDocumentHandler> _logger;

    /// <summary>
    /// Внедряем репозиторий, интеграционный сервис, кеш и логгер через DI.
    /// </summary>
    public DeleteDocumentHandler(
        IDocumentRepository repository,
        IDocumentIntegrationService integrationService,
        IDocumentCache cache,
        ILogger<DeleteDocumentHandler> logger)
    {
        _repository = repository;
        _integrationService = integrationService;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Обрабатываем команду: удаляем документ, логируем результат.
    /// </summary>
    public async Task<bool> Handle(DeleteDocumentCommand request, CancellationToken ct)
    {
        // Пытаемся удалить документ по идентификатору
        var deleted = await _repository.DeleteAsync(request.Id, ct);

        // Если документ не найден - логируем и выбрасываем исключение
        if (!deleted)
        {
            _logger.LogWarning("Удаление документа не удалось: документ с Id = {DocumentId} не найден.", request.Id);
            throw new NotFoundException(nameof(DocIntegrator.Domain.Entities.Document), request.Id);
        }

        // Инвалидируем кеш Redis, чтобы не отдавать уже удалённый документ.
        await _cache.InvalidateAsync(request.Id, ct);

        // Фиксируем событие удаления в event store + отправляем в Kafka.
        await _integrationService.HandleDocumentDeletedAsync(request.Id, ct);

        // Логируем успешное удаление
        _logger.LogInformation("Документ с Id = {DocumentId} успешно удален.", request.Id);

        return true;
    }
}
