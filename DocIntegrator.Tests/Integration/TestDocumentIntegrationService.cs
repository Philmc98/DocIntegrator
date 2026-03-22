using DocIntegrator.Application.Documents.Dtos;
using DocIntegrator.Application.Interfaces;

namespace DocIntegrator.Tests.Integration;

/// <summary>
/// Тестовая реализация интеграционного сервиса.
/// Ничего не пишет в Kafka / event store, чтобы интеграционные тесты не зависели от внешней инфраструктуры.
/// </summary>
public class TestDocumentIntegrationService : IDocumentIntegrationService
{
    public Task HandleDocumentCreatedAsync(DocumentDto document, CancellationToken ct) => Task.CompletedTask;

    public Task HandleDocumentUpdatedAsync(DocumentDto document, CancellationToken ct) => Task.CompletedTask;

    public Task HandleDocumentDeletedAsync(Guid documentId, CancellationToken ct) => Task.CompletedTask;
}

