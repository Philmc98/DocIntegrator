using DocIntegrator.Domain.Entities;

namespace DocIntegrator.Application.Interfaces;

public interface IServiceRepository
{
    Task<Service?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Service>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}
