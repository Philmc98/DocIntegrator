using MediatR;
using DocIntegrator.Application.Interfaces;
using DocIntegrator.Application.Documents.Dtos;
using DocIntegrator.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace DocIntegrator.Application.Documents.Queries.GetAllDocuments;

/// <summary>
/// Хендлер для получения списка документов с фильтрацией, сортировкой и пагинацией.
/// </summary>
public class GetAllDocumentsQueryHandler : IRequestHandler<GetAllDocumentsQuery, PagedResult<DocumentDto>>
{
    private readonly IDocumentRepository _repository;

    public GetAllDocumentsQueryHandler(IDocumentRepository repository) 
        => _repository = repository;

    public async Task<PagedResult<DocumentDto>> Handle(GetAllDocumentsQuery request, CancellationToken ct)
    {
        // Берем фильтр из запроса
        var f = request.Filter;

        // Устанавливаем значения по умолчанию, если они не заданы
        f.SortBy ??= "CreatedAt";
        f.SortDir ??= "Desc";
        f.Page = f.Page <= 0 ? 1 : f.Page;
        f.PageSize = f.PageSize <= 0 ? 20 : f.PageSize;

        // Начинаем с IQueryable (Базовый запрос)
        var query = _repository.Query();

        // Фильтрация
        if (!string.IsNullOrWhiteSpace(f.Status))
            query = query.Where(d => d.Status == f.Status);

        if (!string.IsNullOrWhiteSpace(f.TitleContains))
            query = query.Where(d => d.Title.Contains(f.TitleContains));

        if (f.CreatedFrom.HasValue)
            query = query.Where(d => d.CreatedAt >= f.CreatedFrom.Value);

        if (f.CreatedTo.HasValue)
        {
            query = query.Where(d => d.CreatedAt <= f.CreatedTo.Value);
        }

        // Сортировка
        bool asc = string.Equals(f.SortDir, "Asc", StringComparison.OrdinalIgnoreCase);
        query = f.SortBy?.ToLowerInvariant() switch
        {
            "Title" => asc ? query.OrderBy(d => d.Title) : query.OrderByDescending(d => d.Title),
            "Status" => asc ? query.OrderBy(d => d.Status) : query.OrderByDescending(d => d.Status),
            _ => asc ? query.OrderBy(d => d.CreatedAt) : query.OrderByDescending(d => d.CreatedAt)
        };

        // Подсчёт до пагинации
        var totalCount = await query.CountAsync(ct);

        // Пагинация + выборка
        var skip = (f.Page - 1) * f.PageSize;
        var items = await query.Skip(skip).Take(f.PageSize)
            .Select(d => new DocumentDto
            {
                Id = d.Id,
                Title = d.Title,
                Content = d.Content,
                Status = d.Status,
                CreatedAt = d.CreatedAt
            }).ToListAsync(ct);


        return new PagedResult<DocumentDto>(items, totalCount, f.Page, f.PageSize);
    }
}
