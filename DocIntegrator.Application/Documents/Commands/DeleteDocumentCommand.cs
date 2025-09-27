using MediatR;

namespace DocIntegrator.Application.Documents.Commands;

public record DeleteDocumentCommand(Guid Id) : IRequest<Unit>;
