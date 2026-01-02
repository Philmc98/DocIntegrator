using DocIntegrator.Application.Interfaces;
using DocIntegrator.Domain.Entities;
using DocIntegrator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DocIntegrator.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly DocIntegratorDbContext _context;

    public DocumentRepository(DocIntegratorDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Возвращает IQueryable для построения запросов.
    /// Используем AsNoTracking, чтобы ускорить чтение (EF не будет отслеживать изменения).
    /// </summary>
    public IQueryable<Document> Query() => _context.Documents.AsNoTracking();

    /// <summary>
    /// Получить документ по идентификатору.
    /// </summary>
    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Documents.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, ct);

    /// <summary>
    /// Проверить, существует ли документ с указанным идентификатором.
    /// </summary>
    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => await _context.Documents.AnyAsync(d => d.Id == id, ct);

    /// <summary>
    /// Добавить новый документ.
    /// </summary>
    public async Task AddAsync(Document doc, CancellationToken ct = default)
    {
        await _context.Documents.AddAsync(doc, ct);
        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Обновить существующий документ.
    /// Используем Attach + EntityState.Modified, чтобы избежать проблем с трекингом.
    /// </summary>
    public async Task UpdateAsync(Document doc, CancellationToken ct = default)
    {
        _context.Attach(doc);
        _context.Entry(doc).State = EntityState.Modified;
        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Удалить документ по идентификатору.
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.Documents.FindAsync(new object[] { id }, ct);
        if (entity == null)
        {
            return false;
        }

        _context.Documents.Remove(entity);
        return await _context.SaveChangesAsync(ct) > 0;
    }

    /// <summary>
    /// Сохранить изменения вручную (например, в рамках Unit of Work).
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}
