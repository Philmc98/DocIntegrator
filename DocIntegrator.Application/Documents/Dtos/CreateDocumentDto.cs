namespace DocIntegrator.Application.Documents.Dtos;

public class CreateDocumentDto
{
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string Status { get; set; } = "Черновик";
}
