using System.Text.Json;
using DocIntegrator.Application.Documents.Dtos;
using DocIntegrator.Application.Interfaces;
using DocIntegrator.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DocIntegrator.Infrastructure.Events;

/// <summary>
/// Интеграционный сервис для локальной разработки и InMemory-режима.
/// Записывает события только в event store (EF Core InMemory).
/// Kafka и ClickHouse НЕ используются — не требуют запущенной инфраструктуры.
/// В production используется KafkaDocumentIntegrationService.
/// </summary>
public class LocalDocumentIntegrationService : IDocumentIntegrationService
{
    private readonly IDocumentEventStore _eventStore;
    private readonly ILogger<LocalDocumentIntegrationService> _logger;

    public LocalDocumentIntegrationService(
        IDocumentEventStore eventStore,
        ILogger<LocalDocumentIntegrationService> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
    }

    public Task HandleDocumentCreatedAsync(DocumentDto document, CancellationToken ct) =>
        AppendEventAsync(document.Id, "Created", document, ct);

    public Task HandleDocumentUpdatedAsync(DocumentDto document, CancellationToken ct) =>
        AppendEventAsync(document.Id, "Updated", document, ct);

    public Task HandleDocumentDeletedAsync(Guid documentId, CancellationToken ct) =>
        AppendEventAsync(documentId, "Deleted", new DocumentDto { Id = documentId }, ct);

    private async Task AppendEventAsync(Guid documentId, string eventType, DocumentDto payload, CancellationToken ct)
    {
        var evt = new DocumentEvent
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            EventType = eventType,
            PayloadJson = JsonSerializer.Serialize(payload),
            OccurredAt = DateTime.UtcNow
        };

        await _eventStore.AppendAsync(evt, ct);

        _logger.LogInformation(
            "[Local] Событие записано в event store. EventType={EventType}, DocumentId={DocumentId}. " +
            "Kafka и ClickHouse отключены в InMemory-режиме.",
            eventType, documentId);
    }
}
