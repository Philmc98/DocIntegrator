using DocIntegrator.Application.Documents.Dtos;
using DocIntegrator.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DocIntegrator.Application.Documents.Queries.GetDocumentStats;

/// <summary>
/// Делегирует запрос аналитики в IDocumentStatsRepository.
/// Реализация зависит от среды: Dapper (PostgreSql/SqlServer) или LINQ (InMemory).
/// </summary>
public class GetDocumentStatsQueryHandler
    : IRequestHandler<GetDocumentStatsQuery, DocumentStatsDto>
{
    private readonly IDocumentStatsRepository _statsRepository;
    private readonly ILogger<GetDocumentStatsQueryHandler> _logger;

    public GetDocumentStatsQueryHandler(
        IDocumentStatsRepository statsRepository,
        ILogger<GetDocumentStatsQueryHandler> logger)
    {
        _statsRepository = statsRepository;
        _logger = logger;
    }

    public async Task<DocumentStatsDto> Handle(GetDocumentStatsQuery request, CancellationToken ct)
    {
        _logger.LogInformation("GetDocumentStats: запрос статистики");
        var stats = await _statsRepository.GetStatsAsync(ct);
        _logger.LogInformation(
            "GetDocumentStats: total={Total}, events={Events}, statuses={StatusCount}",
            stats.TotalDocuments, stats.TotalEvents, stats.ByStatus.Count);
        return stats;
    }
}
