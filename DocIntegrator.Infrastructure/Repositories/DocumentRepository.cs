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

    public async Task<List<Document>> GetAllAsync(CancellationToken ct = default)
        => await _context.Documents.ToListAsync(ct);

    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Documents.FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task AddAsync(Document doc, CancellationToken ct = default)
    {
        _context.Documents.Add(doc);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Document doc, CancellationToken ct = default)
    {
        _context.Documents.Update(doc);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.Documents.FindAsync(new object[] { id }, ct);
        if (entity == null)
        {
            return false;
        }

        _context.Documents.Remove(entity);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
