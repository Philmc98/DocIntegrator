using DocIntegrator.Application.Documents.Dtos;
using DocIntegrator.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DocIntegrator.Application.Documents.Queries.GetDocumentHistory;

/// <summary>
/// Хендлер: читает поток событий из event store и возвращает историю изменений документа.
/// </summary>
public class GetDocumentHistoryQueryHandler
    : IRequestHandler<GetDocumentHistoryQuery, IReadOnlyList<DocumentEventDto>>
{
    private readonly IDocumentEventStore _eventStore;
    private readonly ILogger<GetDocumentHistoryQueryHandler> _logger;

    public GetDocumentHistoryQueryHandler(
        IDocumentEventStore eventStore,
        ILogger<GetDocumentHistoryQueryHandler> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DocumentEventDto>> Handle(
        GetDocumentHistoryQuery request,
        CancellationToken ct)
    {
        _logger.LogInformation("GetDocumentHistory: DocumentId={DocumentId}", request.DocumentId);

        var events = await _eventStore.GetStreamAsync(request.DocumentId, ct);

        _logger.LogInformation("GetDocumentHistory: найдено {Count} событий для DocumentId={DocumentId}",
            events.Count, request.DocumentId);

        return events
            .Select(e => new DocumentEventDto
            {
                Id = e.Id,
                DocumentId = e.DocumentId,
                EventType = e.EventType,
                PayloadJson = e.PayloadJson,
                OccurredAt = e.OccurredAt
            })
            .ToList();
    }
}
