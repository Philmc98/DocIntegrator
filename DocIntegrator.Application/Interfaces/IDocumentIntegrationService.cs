using DocIntegrator.Application.Documents.Dtos;

namespace DocIntegrator.Application.Interfaces;

/// <summary>
/// Высокоуровневый фасад для интеграций вокруг документов.
/// Скрывает за собой event store, Kafka, ClickHouse и другие инфраструктурные детали.
/// Хендлеры команд просто вызывают этот сервис, не зная, как именно обрабатываются события.
/// </summary>
public interface IDocumentIntegrationService
{
    Task HandleDocumentCreatedAsync(DocumentDto document, CancellationToken ct);
    Task HandleDocumentUpdatedAsync(DocumentDto document, CancellationToken ct);
    Task HandleDocumentDeletedAsync(Guid documentId, CancellationToken ct);
}

