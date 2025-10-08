using MediatR;
using DocIntegrator.Application.Documents.Dtos;

namespace DocIntegrator.Application.Documents.Commands;

public record UpdateDocumentCommand(Guid Id, UpdateDocumentDto Document) : IRequest<DocumentDto?>;
