using MediatR;
using DocIntegrator.Application.Documents.Dtos;

namespace DocIntegrator.Application.Documents.Queries.GetAllDocuments;

public record GetDocumentByIdQuery(Guid Id) : IRequest<DocumentDto?>;
