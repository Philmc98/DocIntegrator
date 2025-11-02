using DocIntegrator.Domain.Entities;

namespace DocIntegrator.Application.Interfaces;

public interface IDocumentRepository
{
    Task<List<Document>> GetAllAsync(CancellationToken cancellationToken);  // нужно?
    Task<Document?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(Document entity, CancellationToken ct);
    Task UpdateAsync(Document entity, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
