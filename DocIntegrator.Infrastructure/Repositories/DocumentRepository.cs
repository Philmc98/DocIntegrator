using DocIntegrator.Application.Interfaces;
using DocIntegrator.Domain.Entities;
using DocIntegrator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DocIntegrator.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly DocIntegratorDbContext _context;
    private readonly ILogger<DocumentRepository> _logger;
    private const int SlowQueryThresholdMs = 1000;

    public DocumentRepository(DocIntegratorDbContext context, ILogger<DocumentRepository> logger)
    {
        _context = context;
        _logger = logger;
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
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            await _context.Documents.AddAsync(doc, ct);
            await _context.SaveChangesAsync(ct);
            if (sw.ElapsedMilliseconds > SlowQueryThresholdMs)
                _logger.LogWarning("AddAsync выполнен медленно: {ElapsedMs} мс, Id={Id}", sw.ElapsedMilliseconds, doc.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка EF при AddAsync, Id={Id}", doc.Id);
            throw;
        }
    }

    /// <summary>
    /// Обновить существующий документ.
    /// Используем Attach + EntityState.Modified, чтобы избежать проблем с трекингом.
    /// </summary>
    public async Task UpdateAsync(Document doc, CancellationToken ct = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            _context.Attach(doc);
            _context.Entry(doc).State = EntityState.Modified;
            await _context.SaveChangesAsync(ct);
            if (sw.ElapsedMilliseconds > SlowQueryThresholdMs)
                _logger.LogWarning("UpdateAsync выполнен медленно: {ElapsedMs} мс, Id={Id}", sw.ElapsedMilliseconds, doc.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка EF при UpdateAsync, Id={Id}", doc.Id);
            throw;
        }
    }

    /// <summary>
    /// Удалить документ по идентификатору.
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var entity = await _context.Documents.FindAsync(new object[] { id }, ct);
            if (entity == null)
            {
                _logger.LogDebug("DeleteAsync: документ не найден, Id={Id}", id);
                return false;
            }
            _context.Documents.Remove(entity);
            var ok = await _context.SaveChangesAsync(ct) > 0;
            if (sw.ElapsedMilliseconds > SlowQueryThresholdMs)
                _logger.LogWarning("DeleteAsync выполнен медленно: {ElapsedMs} мс, Id={Id}", sw.ElapsedMilliseconds, id);
            return ok;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка EF при DeleteAsync, Id={Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Сохранить изменения вручную (например, в рамках Unit of Work).
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}
