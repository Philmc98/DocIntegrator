using DocIntegrator.Domain.Entities;

namespace DocIntegrator.Application.Interfaces;

/// <summary>
/// Контракт event store для документов.
/// Отвечает за долговременное хранение доменных событий (event sourcing).
/// </summary>
public interface IDocumentEventStore
{
    /// <summary>
    /// Добавить новое событие в ленту (append-only).
    /// </summary>
    Task AppendAsync(DocumentEvent documentEvent, CancellationToken ct);

    /// <summary>
    /// Получить полную ленту событий по документу.
    /// Это позволяет восстановить состояние из истории.
    /// </summary>
    Task<IReadOnlyList<DocumentEvent>> GetStreamAsync(Guid documentId, CancellationToken ct);
}

