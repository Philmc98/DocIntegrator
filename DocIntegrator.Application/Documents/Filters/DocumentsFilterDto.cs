namespace DocIntegrator.Application.Documents.Filters;

public class DocumentsFilterDto
{
    public string? Status { get; set; }
    public string? TitleContains { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }

    public string? PrimarySort { get; set; } = "createdAt";
    public string? PrimarySortOrder { get; set; } = "desc";
    public string? SecondarySort { get; set; } = null;
    public string? SecondarySortOrder { get; set; } = "asc";

    public int? Page { get; set; } = 1;
    public int? PageSize { get; set; } = null;
}
