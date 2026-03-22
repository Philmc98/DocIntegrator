using DocIntegrator.Application.Documents.Dtos;

namespace DocIntegrator.Application.Interfaces;

/// <summary>
/// Репозиторий для получения аналитики по документам.
/// В режиме PostgreSql/SqlServer реализован через Dapper (raw SQL + GROUP BY).
/// В InMemory-режиме — через LINQ для совместимости с тестами.
/// </summary>
public interface IDocumentStatsRepository
{
    Task<DocumentStatsDto> GetStatsAsync(CancellationToken ct = default);
}
