namespace DocIntegrator.Application.Documents.Filters;

public class DocumentsFilterDto
{
    public string? Status { get; set; }
    public string? TitleContains { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }

    public string? SortBy { get; set; }
    public string? SortDir { get; set; }
   
    public int Page { get; set; }
    public int PageSize { get; set; }

    public DocumentsFilterDto()
    {
        SortBy = "CreatedAt";
        SortDir = "Desc";
        Page = 1;
        PageSize = 20;
    }
}
