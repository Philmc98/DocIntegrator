namespace DocIntegrator.Application.Common.Models;

/// <summary>
/// Универсальный результат с пагинацией.
/// Содержит список элементов и метаданные для постраничного вывода.
/// </summary>
/// <typeparam name="T">Тип элементов списка.</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Список элементов текущей страницы.
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Общее количество элементов (без учёта пагинации).
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Номер текущей страницы (начинается с 1).
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Размер страницы (количество элементов).
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Общее количество страниц.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// Есть ли предыдущая страница.
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Есть ли следующая страница.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    public PagedResult(List<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }
}
