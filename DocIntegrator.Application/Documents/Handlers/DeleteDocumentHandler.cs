using MediatR;
using DocIntegrator.Application.Interfaces;
using DocIntegrator.Application.Documents.Commands;
using DocIntegrator.Application.Common.Exceptions;
using System.Reflection.Metadata;

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
        var deleted = await _repository.DeleteAsync(request.Id, ct);

        if (!deleted)
        {
            throw new NotFoundException(nameof(Document), request.Id);
        }

        return true;
    }
}
