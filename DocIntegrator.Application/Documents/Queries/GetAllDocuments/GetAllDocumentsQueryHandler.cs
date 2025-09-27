using DocIntegrator.Application.Documents.Dtos;
using DocIntegrator.Application.Documents.Queries.GetAllDocuments;
using DocIntegrator.Application.Interfaces;
using MediatR;

namespace DocIntegrator.Application.Queries;

public class GetAllDocumentsQueryHandler : IRequestHandler<GetAllDocumentsQuery, PagedResult<DocumentDto>>
{
    private readonly IDocumentRepository _repository;

    public GetAllDocumentsQueryHandler(IDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<DocumentDto>> Handle(GetAllDocumentsQuery request, CancellationToken ct)
    {
        // 1. Загружаем все документы из репозитория
        var docs = await _repository.GetAllAsync(ct);

        // 2. Фильтрация по статусу
        if (!string.IsNullOrWhiteSpace(request.Status))
            docs = docs.Where(d => d.Status.Equals(request.Status, StringComparison.OrdinalIgnoreCase)).ToList();

        // 3. Поиск по части заголовка
        if (!string.IsNullOrWhiteSpace(request.TitleContains))
            docs = docs.Where(d => d.Title != null && d.Title.Contains(request.TitleContains, StringComparison.OrdinalIgnoreCase)).ToList();

        // 4. Фильтрация по диапазону дат
        if (request.CreatedFrom.HasValue)
            docs = docs.Where(d => d.CreatedAt >= request.CreatedFrom.Value).ToList();

        if (request.CreatedTo.HasValue)
            docs = docs.Where(d => d.CreatedAt <= request.CreatedTo.Value).ToList();

        // 5. Сортировка: сначала основная, потом вторичная
        IOrderedEnumerable<Domain.Entities.Document>? ordered = ApplyPrimarySorting(docs, request.PrimarySort, request.PrimarySortOrder);

        if (!string.IsNullOrWhiteSpace(request.SecondarySort))
            ordered = ApplySecondarySorting(ordered, request.SecondarySort, request.SecondarySortOrder);

        var sortedDocs = (ordered ?? docs.OrderBy(d => d.Id)).ToList();

        // 6. Подсчёт общего количества до пагинации
        var totalCount = sortedDocs.Count;

        // 7. Пагинация
        if (request.Page.HasValue && request.PageSize.HasValue)
        {
            var skip = (request.Page.Value - 1) * request.PageSize.Value;
            sortedDocs = sortedDocs.Skip(skip).Take(request.PageSize.Value).ToList();
        }

        // 8. Маппинг в DTO
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
            Page = request.Page ?? 1,
            PageSize = request.PageSize ?? totalCount
        };
    }

    // Основная сортировка (OrderBy / OrderByDescending)
    private IOrderedEnumerable<Domain.Entities.Document> ApplyPrimarySorting(
        IEnumerable<Domain.Entities.Document> docs,
        string? sortField,
        string? sortOrder)
    {
        bool asc = sortOrder?.ToLower() == "asc";

        return sortField?.ToLower() switch
        {
            "title" => asc ? docs.OrderBy(d => d.Title) : docs.OrderByDescending(d => d.Title),

            // Кастомный порядок статусов: Черновик → Manual approval → Опубликован → остальные
            "status" => asc
                ? docs.OrderBy(d => GetStatusOrder(d.Status))
                : docs.OrderByDescending(d => GetStatusOrder(d.Status)),

            "createdat" => asc ? docs.OrderBy(d => d.CreatedAt) : docs.OrderByDescending(d => d.CreatedAt),

            _ => docs.OrderBy(d => d.Id) // fallback
        };
    }

    // Вторичная сортировка (ThenBy / ThenByDescending)
    private IOrderedEnumerable<Domain.Entities.Document> ApplySecondarySorting(
        IOrderedEnumerable<Domain.Entities.Document> docs,
        string? sortField,
        string? sortOrder)
    {
        bool asc = sortOrder?.ToLower() == "asc";

        return sortField?.ToLower() switch
        {
            "title" => asc ? docs.ThenBy(d => d.Title) : docs.ThenByDescending(d => d.Title),
            "status" => asc
                ? docs.ThenBy(d => GetStatusOrder(d.Status))
                : docs.ThenByDescending(d => GetStatusOrder(d.Status)),
            "createdat" => asc ? docs.ThenBy(d => d.CreatedAt) : docs.ThenByDescending(d => d.CreatedAt),
            _ => docs
        };
    }

    // Метод для кастомного порядка статусов
    private int GetStatusOrder(string? status)
    {
        return status switch
        {
            "Черновик" => 1,
            "Manual approval" => 2,
            "Опубликован" => 3,
            _ => 99 // все остальные статусы идут в конец
        };
    }
}
