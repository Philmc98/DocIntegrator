using DocIntegrator.Application.Documents.Dtos;
using MediatR;

namespace DocIntegrator.Application.Documents.Queries.GetDocumentHistory;

/// <summary>
/// Запрос истории событий конкретного документа из event store.
/// Демонстрирует event sourcing: все изменения хранятся как append-only события.
/// </summary>
public record GetDocumentHistoryQuery(Guid DocumentId) : IRequest<IReadOnlyList<DocumentEventDto>>;
