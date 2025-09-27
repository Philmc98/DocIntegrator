using DocIntegrator.Domain.Entities;

namespace DocIntegrator.Application.Interfaces;

public interface IDocumentRepository
{
    Task<List<Document>> GetAllAsync(CancellationToken cancellationToken);
    Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Document doc, CancellationToken ct = default);
    Task UpdateAsync(Document doc, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
