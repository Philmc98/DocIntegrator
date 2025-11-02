using MediatR;
using DocIntegrator.Application.Interfaces;
using DocIntegrator.Application.Documents.Dtos;
using DocIntegrator.Application.Documents.Queries;
using DocIntegrator.Application.Documents.Filters;
using DocIntegrator.Application.Common.Models;


namespace DocIntegrator.Application.Documents.Queries.GetAllDocuments;

public class GetAllDocumentsQueryHandler : IRequestHandler<GetAllDocumentsQuery, PagedResult<DocumentDto>>
{
    private readonly IDocumentRepository _repository;

    public GetAllDocumentsQueryHandler(IDocumentRepository repository) => _repository = repository;

    public async Task<PagedResult<DocumentDto>> Handle(GetAllDocumentsQuery request, CancellationToken ct)
    {
        var f = request.Filter;

        f.SortBy ??= "CreatedAt";
        f.SortDir ??= "Desc";
        f.Page = f.Page <= 0 ? 1 : f.Page;
        f.PageSize = f.PageSize <= 0 ? 20 : f.PageSize;

        var docs = await _repository.GetAllAsync(ct);

        // Фильтрация
        if (!string.IsNullOrWhiteSpace(f.Status))
            docs = docs.Where(d => string.Equals(d.Status, f.Status, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!string.IsNullOrWhiteSpace(f.TitleContains))
            docs = docs.Where(d => (d.Title ?? string.Empty).Contains(f.TitleContains!, StringComparison.OrdinalIgnoreCase)).ToList();

        if (f.CreatedFrom.HasValue)
            docs = docs.Where(d => d.CreatedAt >= f.CreatedFrom.Value).ToList();

        if (f.CreatedTo.HasValue)
        {
            var inclusiveTo = f.CreatedTo.Value.Date.AddDays(1);
            docs = docs.Where(d => d.CreatedAt < inclusiveTo).ToList();
        }

        // Сортировка
        bool asc = string.Equals(f.SortDir, "Asc", StringComparison.OrdinalIgnoreCase);
        docs = f.SortBy switch
        {
            "Title" => asc ? docs.OrderBy(d => d.Title).ToList() : docs.OrderByDescending(d => d.Title).ToList(),
            "Status" => asc ? docs.OrderBy(d => d.Status).ToList() : docs.OrderByDescending(d => d.Status).ToList(),
            _ => asc ? docs.OrderBy(d => d.CreatedAt).ToList() : docs.OrderByDescending(d => d.CreatedAt).ToList()
        };

        // Подсчёт до пагинации
        var totalCount = docs.Count;

        // Пагинация
        var page = f.Page;
        var pageSize = f.PageSize;
        var skip = Math.Max(0, (page - 1) * pageSize);
        var pagedDocs = docs.Skip(skip).Take(pageSize).ToList();

        // Возврат
        var items = pagedDocs.Select(d => new DocumentDto
        {
            Id = d.Id,
            Title = d.Title,
            Content = d.Content,
            Status = d.Status,
            CreatedAt = d.CreatedAt
        }).ToList();

        return new PagedResult<DocumentDto>(items, totalCount, page, pageSize);
    }
}
