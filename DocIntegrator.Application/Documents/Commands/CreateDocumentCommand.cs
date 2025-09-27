using MediatR;

public record CreateDocumentCommand(string Title, string Content, string Status) : IRequest<Guid>;