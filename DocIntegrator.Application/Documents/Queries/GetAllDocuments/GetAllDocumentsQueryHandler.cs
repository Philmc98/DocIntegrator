using MediatR;
using DocIntegrator.Application.Interfaces;
using DocIntegrator.Application.Documents.Dtos;
using DocIntegrator.Application.Documents.Queries;
using DocIntegrator.Application.Documents.Filters;


namespace DocIntegrator.Application.Documents.Queries.GetAllDocuments;

public class GetAllDocumentsQueryHandler : IRequestHandler<GetAllDocumentsQuery, PagedResult<DocumentDto>>
{
    private readonly IDocumentRepository _repository;

    public GetAllDocumentsQueryHandler(IDocumentRepository repository) => _repository = repository;

    public async Task<PagedResult<DocumentDto>> Handle(GetAllDocumentsQuery request, CancellationToken ct)
    {
        var f = request.Filter; // сокращение для читабельности
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

        // Сортировка: primary + optional secondary
        var ordered = ApplyPrimarySorting(docs, f.PrimarySort, f.PrimarySortOrder);
        if (!string.IsNullOrWhiteSpace(f.SecondarySort))
            ordered = ApplySecondarySorting(ordered, f.SecondarySort, f.SecondarySortOrder);

        var sortedDocs = ordered.ToList();

        // Подсчёт до пагинации
        var totalCount = sortedDocs.Count;

        // Пагинация (если PageSize не задан — возвращаем все)
        var page = f.Page ?? 1;
        var pageSize = f.PageSize ?? totalCount;
        var skip = Math.Max(0, (page - 1) * pageSize);
        sortedDocs = sortedDocs.Skip(skip).Take(pageSize).ToList();

        return new PagedResult<DocumentDto>
        {
            Items = sortedDocs.Select(d => new DocumentDto
            {
                Id = d.Id,
                Title = d.Title,
                Content = d.Content,
                Status = d.Status,
                CreatedAt = d.CreatedAt
            }).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // Primary: OrderBy / OrderByDescending + кастомный порядок статусов
    private IOrderedEnumerable<Domain.Entities.Document> ApplyPrimarySorting(IEnumerable<Domain.Entities.Document> docs, string? sortField, string? sortOrder)
    {
        bool asc = string.Equals(sortOrder, "asc", StringComparison.OrdinalIgnoreCase);
        switch (sortField?.ToLowerInvariant())
        {
            case "title":
                return asc ? docs.OrderBy(d => d.Title) : docs.OrderByDescending(d => d.Title);

            case "status":
                // Кастомный порядок статусов + внутри групп стабильность по дате
                return asc
                    ? docs.OrderBy(d => GetStatusOrder(d.Status)).ThenByDescending(d => d.CreatedAt)
                    : docs.OrderByDescending(d => GetStatusOrder(d.Status)).ThenByDescending(d => d.CreatedAt);

            case "createdat":
            default:
                return asc ? docs.OrderBy(d => d.CreatedAt) : docs.OrderByDescending(d => d.CreatedAt);
        }
    }

    // Secondary: ThenBy / ThenByDescending подключается поверх primary
    private IOrderedEnumerable<Domain.Entities.Document> ApplySecondarySorting(IOrderedEnumerable<Domain.Entities.Document> docs, string? sortField, string? sortOrder)
    {
        bool asc = string.Equals(sortOrder, "asc", StringComparison.OrdinalIgnoreCase);
        switch (sortField?.ToLowerInvariant())
        {
            case "title":
                return asc ? docs.ThenBy(d => d.Title) : docs.ThenByDescending(d => d.Title);

            case "status":
                return asc 
                    ? docs.ThenBy(d => GetStatusOrder(d.Status)).ThenByDescending(d => d.CreatedAt) 
                    : docs.ThenByDescending(d => GetStatusOrder(d.Status)).ThenByDescending(d => d.CreatedAt);

            case "createdat":
            default:
                return asc ? docs.ThenBy(d => d.CreatedAt) : docs.ThenByDescending(d => d.CreatedAt);
        }
    }

    // Кастомный порядок статусов: бизнес-логика вместо алфавита
    private int GetStatusOrder(string? status) =>
        status switch
        {
            "Опубликован" => 1,
            "Черновик" => 2,
            _ => 99
        };
}
