namespace DocIntegrator.Domain.Entities;

public class Document
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Status {get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
