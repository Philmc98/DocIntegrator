using DocIntegrator.Application.Interfaces;
using DocIntegrator.Domain.Entities;
using DocIntegrator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DocIntegrator.Infrastructure.Events;

/// <summary>
/// EF Core-реализация event store для документов.
/// Все события складываются в таблицу DocumentEvents (append-only).
/// </summary>
public class DocumentEventStore : IDocumentEventStore
{
    private readonly DocIntegratorDbContext _dbContext;
    private readonly ILogger<DocumentEventStore> _logger;

    public DocumentEventStore(DocIntegratorDbContext dbContext, ILogger<DocumentEventStore> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task AppendAsync(DocumentEvent documentEvent, CancellationToken ct)
    {
        await _dbContext.DocumentEvents.AddAsync(documentEvent, ct);
        await _dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Событие по документу добавлено в event store. EventId={EventId}, DocumentId={DocumentId}, Type={EventType}",
            documentEvent.Id, documentEvent.DocumentId, documentEvent.EventType);
    }

    public async Task<IReadOnlyList<DocumentEvent>> GetStreamAsync(Guid documentId, CancellationToken ct)
    {
        return await _dbContext.DocumentEvents
            .AsNoTracking()
            .Where(e => e.DocumentId == documentId)
            .OrderBy(e => e.OccurredAt)
            .ToListAsync(ct);
    }
}

