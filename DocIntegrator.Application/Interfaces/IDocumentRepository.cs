using DocIntegrator.Domain.Entities;

namespace DocIntegrator.Application.Interfaces;

/// <summary>
/// Контракт репозитория для работы с документами.
/// Определяет базовые операции CRUD и доступ к IQueryable для гибких LINQ-запросов.
/// </summary>
public interface IDocumentRepository
{
    /// <summary>
    /// Возвращает IQueryable для построения запросов (фильтрация, сортировка, пагинация).
    /// Используем AsNoTracking в реализации, чтобы не тратить ресурсы на трекинг при чтении.
    /// </summary>
    IQueryable<Document> Query();

    /// <summary>
    /// Получить документ по идентификатору.
    /// </summary>
    Task<Document?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Проверить, существует ли документ с указанным идентификатором.
    /// Это полезно для валидации перед обновлением/удалением.
    Task<bool> ExistsAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Добавить новый документ.
    /// </summary>
    Task AddAsync(Document entity, CancellationToken ct);

    /// <summary>
    /// Обновить существующий документ.
    /// </summary>
    Task UpdateAsync(Document entity, CancellationToken ct);

    /// <summary>
    /// Удалить документ по идентификатору.
    /// Возвращает true, если удаление прошло успешно.
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Сохранить изменения вручную (если используем Unit of Work).
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct);
}
