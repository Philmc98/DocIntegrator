using DocIntegrator.Application.Documents.Dtos;
using DocIntegrator.Application.Interfaces;
using MediatR;

namespace DocIntegrator.Application.Documents.Queries.GetAllDocuments;

public class GetDocumentByIdQueryHandler : IRequestHandler<GetDocumentByIdQuery, DocumentDto?>
{
    private readonly IDocumentRepository _repository;

    public GetDocumentByIdQueryHandler(IDocumentRepository repository)
        => _repository = repository;

    public async Task<DocumentDto?> Handle(GetDocumentByIdQuery request, CancellationToken ct)
    {
        var doc = await _repository.GetByIdAsync(request.Id, ct);
        if (doc == null) return null;

        return new DocumentDto
        {
            Id = doc.Id,
            Title = doc.Title,
            Content = doc.Content,
            Status = doc.Status,
            CreatedAt = doc.CreatedAt
        };
    }
}
