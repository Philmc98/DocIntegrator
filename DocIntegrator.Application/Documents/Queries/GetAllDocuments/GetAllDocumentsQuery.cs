using MediatR;
using DocIntegrator.Application.Documents.Dtos;

namespace DocIntegrator.Application.Documents.Queries.GetAllDocuments;

public record GetAllDocumentsQuery(
    string? Status = null,
    string? TitleContains = null,
    string? PrimarySort = "createdAt", // поле для первой сортировки
    string? PrimarySortOrder = "desc", // asc или desc
    string? SecondarySort = null, // поле для второй сортировки
    string? SecondarySortOrder = "asc",
    DateTime? CreatedFrom = null,  // с какой даты
    DateTime? CreatedTo = null,    // по какую дату
    int? Page = null,
    int? PageSize = null
) : IRequest<PagedResult<DocumentDto>>;
