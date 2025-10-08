using MediatR;
using DocIntegrator.Application.Interfaces;
using DocIntegrator.Application.Documents.Dtos;
using DocIntegrator.Application.Documents.Commands;

namespace DocIntegrator.Application.Documents.Handlers;

public class CreateDocumentHandler : IRequestHandler<CreateDocumentCommand, DocumentDto>
{
    private readonly IDocumentRepository _repository;

    public CreateDocumentHandler(IDocumentRepository repository)
        => _repository = repository;

    public async Task<DocumentDto> Handle(CreateDocumentCommand request, CancellationToken ct)
    {
        var entity = new Domain.Entities.Document
        {
            Id = Guid.NewGuid(),
            Title = request.Document.Title,
            Content = request.Document.Content,
            Status = request.Document.Status,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(entity, ct);

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
