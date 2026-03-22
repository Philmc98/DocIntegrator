namespace DocIntegrator.Domain.Entities;

/// <summary>
/// Доменное событие для документа.
/// Используется для event sourcing / интеграции с внешними системами.
/// События складываются в отдельное хранилище (event store),
/// а также публикуются во внешние брокеры (Kafka, ClickHouse и т.д.).
/// </summary>
public class DocumentEvent
{
    /// <summary>
    /// Уникальный идентификатор события (для идемпотентности и трассировки).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор документа, к которому относится событие.
    /// </summary>
    public Guid DocumentId { get; set; }

    /// <summary>
    /// Тип события: Created, Updated, Deleted и т.п.
    /// Храним как строку, чтобы проще было расширять типы без миграций enum.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Полезная нагрузка события в виде JSON.
    /// Содержит снимок DTO/сущности на момент события.
    /// </summary>
    public string PayloadJson { get; set; } = string.Empty;

    /// <summary>
    /// Время возникновения события (UTC).
    /// </summary>
    public DateTime OccurredAt { get; set; }
}

