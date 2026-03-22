using DocIntegrator.Domain.Entities;

namespace DocIntegrator.Application.Interfaces;

/// <summary>
/// Запись аналитических событий по документам в OLAP-хранилище (ClickHouse).
/// Показывает умение работать с отдельной аналитической БД помимо OLTP (PostgreSQL/MS SQL).
/// </summary>
public interface IDocumentAnalyticsWriter
{
    Task WriteDocumentEventAsync(DocumentEvent documentEvent, CancellationToken ct);
}

