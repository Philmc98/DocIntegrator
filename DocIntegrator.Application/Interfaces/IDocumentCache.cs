using DocIntegrator.Application.Documents.Dtos;

namespace DocIntegrator.Application.Interfaces;

/// <summary>
/// Абстракция кеша документов.
/// В проде реализуется через Redis, в тестах может быть no-op ("No Operation") реализация.
/// </summary>
public interface IDocumentCache
{
    /// <summary>
    /// Попробовать получить документ из кеша по идентификатору.
    /// </summary>
    Task<DocumentDto?> GetAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Сохранить/обновить документ в кеш.
    /// </summary>
    Task SetAsync(DocumentDto document, CancellationToken ct);

    /// <summary>
    /// Удалить документ из кеша (например, после удаления или изменения).
    /// </summary>
    Task InvalidateAsync(Guid id, CancellationToken ct);
}

