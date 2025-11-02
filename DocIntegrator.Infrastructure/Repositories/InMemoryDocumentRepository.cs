using DocIntegrator.Application.Interfaces;
using DocIntegrator.Domain.Entities;

namespace DocIntegrator.Infrastructure.Repositories;

public class InMemoryDocumentRepository : IDocumentRepository
{
    private readonly List<Document> _docs = new();

    public Task<List<Document>> GetAllAsync(CancellationToken cancellationToken) => Task.FromResult(_docs);
    public Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_docs.FirstOrDefault(d => d.Id == id));

    public Task AddAsync(Document doc, CancellationToken ct = default)
    {
        _docs.Add(doc);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Document doc, CancellationToken ct = default)
    {
        var index = _docs.FindIndex(d => d.Id == doc.Id);
        if (index >= 0) _docs[index] = doc;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var removed = _docs.RemoveAll(d => d.Id == id);
        return Task.FromResult(removed > 0);
    }
}
