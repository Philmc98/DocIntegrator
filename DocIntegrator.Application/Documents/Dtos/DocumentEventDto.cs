namespace DocIntegrator.Application.Documents.Dtos;

/// <summary>
/// Событие из event store для конкретного документа.
/// Позволяет просматривать полную историю изменений (event sourcing).
/// </summary>
public class DocumentEventDto
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }

    /// <summary>Created, Updated, Deleted</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>JSON-снимок DTO документа в момент события</summary>
    public string PayloadJson { get; set; } = string.Empty;

    public DateTime OccurredAt { get; set; }
}
