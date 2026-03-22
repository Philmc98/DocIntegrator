using ClickHouse.Client.ADO;
using DocIntegrator.Application.Interfaces;
using DocIntegrator.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DocIntegrator.Infrastructure.Analytics;

/// <summary>
/// Пишет события документов в ClickHouse.
/// Это демонстрация работы с отдельным аналитическим хранилищем (OLAP),
/// куда попадают данные из event store / Kafka.
/// </summary>
public class ClickHouseDocumentAnalyticsWriter : IDocumentAnalyticsWriter
{
    private readonly string _connectionString;
    private readonly ILogger<ClickHouseDocumentAnalyticsWriter> _logger;

    public ClickHouseDocumentAnalyticsWriter(IConfiguration configuration, ILogger<ClickHouseDocumentAnalyticsWriter> logger)
    {
        // Пример строки подключения:
        // Host=clickhouse;Port=9000;Username=default;Password=;Database=default
        _connectionString = configuration.GetConnectionString("ClickHouse")
                              ?? "Host=clickhouse;Port=9000;Username=default;Password=;Database=default";
        _logger = logger;
    }

    public async Task WriteDocumentEventAsync(DocumentEvent documentEvent, CancellationToken ct)
    {
        try
        {
            await using var connection = new ClickHouseConnection(_connectionString);
            await connection.OpenAsync(ct);

            // Для простоты предполагается, что таблица document_events уже создана:
            // CREATE TABLE document_events
            // ( EventId UUID, DocumentId UUID, EventType String, OccurredAt DateTime )
            // ENGINE = MergeTree() ORDER BY (OccurredAt, DocumentId);
            await using var command = connection.CreateCommand();

            // ClickHouse использует синтаксис {name:Type} для параметров
            command.CommandText =
                "INSERT INTO document_events (EventId, DocumentId, EventType, OccurredAt) " +
                "VALUES ({eventId:UUID}, {documentId:UUID}, {eventType:String}, {occurredAt:DateTime})";

            AddParameter(command, "eventId", documentEvent.Id.ToString());
            AddParameter(command, "documentId", documentEvent.DocumentId.ToString());
            AddParameter(command, "eventType", documentEvent.EventType);
            AddParameter(command, "occurredAt", documentEvent.OccurredAt);

            await command.ExecuteNonQueryAsync(ct);
        }
        catch (Exception ex)
        {
            // Аналитика не должна ломать боевой флоу — просто логируем ошибку.
            _logger.LogError(ex, "Ошибка при записи события документа в ClickHouse. EventId={EventId}", documentEvent.Id);
        }
    }

    private static void AddParameter(System.Data.Common.DbCommand command, string name, object? value)
    {
        var p = command.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        command.Parameters.Add(p);
    }
}

