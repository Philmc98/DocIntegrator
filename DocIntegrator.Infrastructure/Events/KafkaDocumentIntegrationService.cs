using System.Text.Json;
using Confluent.Kafka;
using DocIntegrator.Application.Documents.Dtos;
using DocIntegrator.Application.Interfaces;
using DocIntegrator.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DocIntegrator.Infrastructure.Events;

/// <summary>
/// Реализация интеграционного сервиса, который:
/// - пишет события в event store (таблица DocumentEvents);
/// - публикует события в Kafka для других микросервисов;
/// - в дальнейшем может дополняться записью аудита в ClickHouse.
///
/// Таким образом демонстрируются:
/// - event sourcing (append-only event store),
/// - использование брокера сообщений (Kafka),
/// - интеграция микросервисов через очередь.
/// </summary>
public class KafkaDocumentIntegrationService : IDocumentIntegrationService
{
    private const string TopicName = "documents-events";

    private readonly IDocumentEventStore _eventStore;
    private readonly IDocumentAnalyticsWriter _analyticsWriter;
    private readonly IProducer<string, string> _kafkaProducer;
    private readonly ILogger<KafkaDocumentIntegrationService> _logger;

    public KafkaDocumentIntegrationService(
        IDocumentEventStore eventStore,
        IDocumentAnalyticsWriter analyticsWriter,
        IConfiguration configuration,
        ILogger<KafkaDocumentIntegrationService> logger)
    {
        _eventStore = eventStore;
        _analyticsWriter = analyticsWriter;
        _logger = logger;

        // Конфиг Kafka читаем из appsettings / env-переменных.
        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            // В бою стоит добавить настройки ретраев, acks и т.п.
        };

        _kafkaProducer = new ProducerBuilder<string, string>(config).Build();
    }

    public Task HandleDocumentCreatedAsync(DocumentDto document, CancellationToken ct) =>
        HandleAsync(document, "Created", ct);

    public Task HandleDocumentUpdatedAsync(DocumentDto document, CancellationToken ct) =>
        HandleAsync(document, "Updated", ct);

    public Task HandleDocumentDeletedAsync(Guid documentId, CancellationToken ct) =>
        HandleAsync(new DocumentDto { Id = documentId }, "Deleted", ct);

    private async Task HandleAsync(DocumentDto document, string eventType, CancellationToken ct)
    {
        var payloadJson = JsonSerializer.Serialize(document);

        // 1. Сохраняем событие в event store (append-only таблица DocumentEvents).
        var evt = new DocumentEvent
        {
            Id = Guid.NewGuid(),
            DocumentId = document.Id,
            EventType = eventType,
            PayloadJson = payloadJson,
            OccurredAt = DateTime.UtcNow
        };

        await _eventStore.AppendAsync(evt, ct);

        // Записываем событие в ClickHouse для аналитики (отчёты, дашборды).
        await _analyticsWriter.WriteDocumentEventAsync(evt, ct);

        // 2. Публикуем событие в Kafka, чтобы внешние сервисы могли реагировать.
        // Используем key = DocumentId, чтобы все события по одному документу попадали в одну партицию.
        try
        {
            var message = new Message<string, string>
            {
                Key = document.Id.ToString(),
                Value = payloadJson
            };

            var deliveryResult = await _kafkaProducer.ProduceAsync(TopicName, message, ct);
            _logger.LogInformation(
                "Событие по документу отправлено в Kafka. Topic={Topic}, Partition={Partition}, Offset={Offset}, EventType={EventType}, DocumentId={DocumentId}",
                deliveryResult.Topic, deliveryResult.Partition.Value, deliveryResult.Offset.Value, eventType, document.Id);
        }
        catch (ProduceException<string, string> ex)
        {
            // В реальном проекте можно реализовать повторную отправку / DLQ.
            _logger.LogError(ex, "Ошибка при отправке события в Kafka. EventType={EventType}, DocumentId={DocumentId}", eventType, document.Id);
            // Не пробрасываем исключение дальше, чтобы не ломать основной бизнес-флоу.
        }
    }
}

