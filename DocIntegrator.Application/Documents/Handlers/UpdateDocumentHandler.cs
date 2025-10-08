using MediatR;
using DocIntegrator.Application.Interfaces;
using DocIntegrator.Application.Documents.Commands;
using DocIntegrator.Application.Documents.Dtos;

namespace DocIntegrator.Application.Documents.Handlers;

public class UpdateDocumentHandler : IRequestHandler<UpdateDocumentCommand, DocumentDto?>
{
    private readonly IDocumentRepository _repository;

    public UpdateDocumentHandler(IDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<DocumentDto?> Handle(UpdateDocumentCommand request, CancellationToken ct)
    {
        var entity = await _repository.GetByIdAsync(request.Id, ct);
        if (entity == null) return null;

        entity.Title = request.Document.Title;
        entity.Content = request.Document.Content;
        entity.Status = request.Document.Status;

        await _repository.UpdateAsync(entity, ct);

        return new DocumentDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Content = entity.Content,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt
        };
    }
}
