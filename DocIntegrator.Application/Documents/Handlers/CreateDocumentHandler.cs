using MediatR;
using DocIntegrator.Application.Interfaces;
using DocIntegrator.Domain.Entities;

public class CreateDocumentHandler : IRequestHandler<CreateDocumentCommand, Guid>
{
    private readonly IDocumentRepository _repository;

    public CreateDocumentHandler(IDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
    {
        var doc = new Document
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Content = request.Content,
            Status = request.Status,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(doc, cancellationToken);
        return doc.Id;
    }
}
