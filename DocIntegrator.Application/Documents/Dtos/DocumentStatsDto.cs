namespace DocIntegrator.Application.Documents.Dtos;

/// <summary>
/// Сводная аналитика по документам.
/// Данные получаются через Dapper (raw SQL) или LINQ — в зависимости от режима БД.
/// </summary>
public class DocumentStatsDto
{
    public int TotalDocuments { get; set; }
    public int TotalEvents { get; set; }
    public DateTime? LatestDocumentCreatedAt { get; set; }
    public List<StatusCountDto> ByStatus { get; set; } = new();
}

/// <summary>Количество документов по конкретному статусу.</summary>
public class StatusCountDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}
