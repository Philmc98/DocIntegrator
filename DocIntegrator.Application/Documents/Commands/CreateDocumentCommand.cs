using MediatR;
using DocIntegrator.Application.Documents.Dtos;

namespace DocIntegrator.Application.Documents.Commands;

public record CreateDocumentCommand(CreateDocumentDto Document) : IRequest<DocumentDto>;