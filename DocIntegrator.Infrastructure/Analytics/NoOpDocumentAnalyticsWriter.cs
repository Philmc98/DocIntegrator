using DocIntegrator.Application.Interfaces;
using DocIntegrator.Domain.Entities;

namespace DocIntegrator.Infrastructure.Analytics;

/// <summary>
/// Заглушка для IDocumentAnalyticsWriter в InMemory-режиме.
/// ClickHouse не используется — не нужен запущенный сервер.
/// </summary>
public class NoOpDocumentAnalyticsWriter : IDocumentAnalyticsWriter
{
    public Task WriteDocumentEventAsync(DocumentEvent documentEvent, CancellationToken ct = default)
        => Task.CompletedTask;
}
