using MediatR;
using DocIntegrator.Application.Interfaces;
using DocIntegrator.Application.Documents.Commands;

namespace DocIntegrator.Application.Documents.Handlers;

public class DeleteDocumentHandler : IRequestHandler<DeleteDocumentCommand, bool>
{
    private readonly IDocumentRepository _repository;

    public DeleteDocumentHandler(IDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteDocumentCommand request, CancellationToken ct)
    {
        await _repository.DeleteAsync(request.Id, ct);
        return true;
    }
}
