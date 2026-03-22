using DocIntegrator.Application.Documents.Dtos;
using DocIntegrator.Application.Interfaces;

namespace DocIntegrator.Infrastructure.Caching;

/// <summary>
/// Заглушка кеша, которая ничего не делает.
/// Удобно использовать в тестах или при отсутствии Redis.
/// </summary>
public class NoOpDocumentCache : IDocumentCache
{
    public Task<DocumentDto?> GetAsync(Guid id, CancellationToken ct) =>
        Task.FromResult<DocumentDto?>(null);

    public Task SetAsync(DocumentDto document, CancellationToken ct) =>
        Task.CompletedTask;

    public Task InvalidateAsync(Guid id, CancellationToken ct) =>
        Task.CompletedTask;
}

