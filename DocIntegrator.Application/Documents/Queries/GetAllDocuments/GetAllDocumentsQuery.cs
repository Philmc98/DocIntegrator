using MediatR;
using DocIntegrator.Application.Documents.Dtos;
using DocIntegrator.Application.Documents.Filters;
using DocIntegrator.Application.Common.Models;

namespace DocIntegrator.Application.Documents.Queries;

public record GetAllDocumentsQuery(DocumentsFilterDto Filter)
    : IRequest<PagedResult<DocumentDto>>;
