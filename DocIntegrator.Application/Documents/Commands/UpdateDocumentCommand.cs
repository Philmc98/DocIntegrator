using MediatR;

namespace DocIntegrator.Application.Documents.Commands;

public record UpdateDocumentCommand(Guid Id, string Title, string Content, string Status) : IRequest<Unit>;
