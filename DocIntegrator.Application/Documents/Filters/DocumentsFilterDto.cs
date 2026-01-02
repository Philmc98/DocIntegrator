namespace DocIntegrator.Application.Documents.Filters;

/// <summary>
/// DTO для фильтрации, сортировки и пагинации списка документов.
/// Используется в запросах GetAllDocumentsQuery.
/// </summary>
public class DocumentsFilterDto
{
    /// <summary>
    /// Фильтр по статусу документа (например: Черновик, Опубликован, На согласовании)
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Фильтр по части заголовка (поиск по подстроке).
    /// </summary>
    public string? TitleContains { get; set; }

    // <summary>
    /// Фильтр по дате создания: начиная с этой даты.
    /// </summary>
    public DateTime? CreatedFrom { get; set; }

    // <summary>
    /// Фильтр по дате создания: до этой даты.
    /// </summary>
    public DateTime? CreatedTo { get; set; }

    /// <summary>
    /// Поле для сортировки (например: Title, CreatedAt, Status).
    /// По умолчанию сортируем по CreatedAt.
    /// </summary>
    public string SortBy { get; set; }

    /// <summary>
    /// Направление сортировки: Asc или Desc.
    /// По умолчанию Desc.
    /// </summary>
    public string SortDir { get; set; }

    /// <summary>
    /// Номер страницы (начинается с 1).
    /// По умолчанию 1.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Размер страницы (количество элементов).
    /// По умолчанию 20.
    /// </summary>
    public int PageSize { get; set; }

    public DocumentsFilterDto()
    {
        SortBy = "CreatedAt";
        SortDir = "Desc";
        Page = 1;
        PageSize = 20;
    }
}
