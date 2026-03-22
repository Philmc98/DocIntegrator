using DocIntegrator.Application.Documents.Dtos;
using DocIntegrator.Application.Interfaces;
using DocIntegrator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DocIntegrator.Infrastructure.Repositories;

/// <summary>
/// LINQ-реализация репозитория статистики для InMemory-режима (тесты, локальная разработка без Docker).
/// Семантически эквивалентна Dapper-версии, но использует EF Core IQueryable.
/// </summary>
public class InMemoryDocumentStatsRepository : IDocumentStatsRepository
{
    private readonly DocIntegratorDbContext _context;

    public InMemoryDocumentStatsRepository(DocIntegratorDbContext context)
    {
        _context = context;
    }

    public async Task<DocumentStatsDto> GetStatsAsync(CancellationToken ct = default)
    {
        var total = await _context.Documents.CountAsync(ct);
        var latest = await _context.Documents
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => (DateTime?)d.CreatedAt)
            .FirstOrDefaultAsync(ct);

        var byStatus = await _context.Documents
            .GroupBy(d => d.Status)
            .Select(g => new StatusCountDto
            {
                Status = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(s => s.Count)
            .ToListAsync(ct);

        var totalEvents = await _context.DocumentEvents.CountAsync(ct);

        return new DocumentStatsDto
        {
            TotalDocuments = total,
            LatestDocumentCreatedAt = latest,
            ByStatus = byStatus,
            TotalEvents = totalEvents
        };
    }
}
