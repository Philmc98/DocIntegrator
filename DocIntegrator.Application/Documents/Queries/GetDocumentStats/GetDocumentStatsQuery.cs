using DocIntegrator.Application.Documents.Dtos;
using MediatR;

namespace DocIntegrator.Application.Documents.Queries.GetDocumentStats;

/// <summary>
/// Запрос агрегированной статистики по документам.
/// В режиме PostgreSql/SqlServer данные получаются через Dapper (GROUP BY, агрегатные функции).
/// </summary>
public record GetDocumentStatsQuery : IRequest<DocumentStatsDto>;
