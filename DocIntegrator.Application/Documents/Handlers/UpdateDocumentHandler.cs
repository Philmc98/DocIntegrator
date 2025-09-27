using MediatR;
using DocIntegrator.Application.Interfaces;
using DocIntegrator.Application.Documents.Commands;

namespace DocIntegrator.Application.Documents.Handlers;

public class UpdateDocumentHandler : IRequestHandler<UpdateDocumentCommand, Unit>
{
    private readonly IDocumentRepository _repository;

    public UpdateDocumentHandler(IDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(UpdateDocumentCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (existing is null)
            throw new KeyNotFoundException("Документ не найден");

        existing.Title = request.Title;
        existing.Content = request.Content;
        existing.Status = request.Status;

        await _repository.UpdateAsync(existing, cancellationToken);
        return Unit.Value;
    }
}
