using MediatR;
using DocIntegrator.Application.Interfaces;
using DocIntegrator.Application.Documents.Commands;

namespace DocIntegrator.Application.Documents.Handlers;

public class DeleteDocumentHandler : IRequestHandler<DeleteDocumentCommand, Unit>
{
    private readonly IDocumentRepository _repository;

    public DeleteDocumentHandler(IDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        await _repository.DeleteAsync(request.Id, cancellationToken);
        return Unit.Value;
    }
}
