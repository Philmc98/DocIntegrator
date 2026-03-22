using Dapper;
using DocIntegrator.Application.Documents.Dtos;
using DocIntegrator.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data.Common;

namespace DocIntegrator.Infrastructure.Repositories;

/// <summary>
/// Dapper-реализация репозитория статистики.
/// Использует raw SQL с агрегатными функциями (COUNT, MAX, GROUP BY) напрямую
/// через ADO.NET-соединение — без ORM-абстракции EF Core.
/// Это демонстрирует навык работы с Dapper для сложных аналитических запросов,
/// где ручной SQL эффективнее, чем LINQ-to-Entities.
/// </summary>
public class DocumentStatsRepository : IDocumentStatsRepository
{
    private readonly string _connectionString;
    private readonly string _dbProvider;
    private readonly ILogger<DocumentStatsRepository> _logger;

    public DocumentStatsRepository(IConfiguration configuration, ILogger<DocumentStatsRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        _dbProvider = configuration["Database:Provider"] ?? "PostgreSql";
        _logger = logger;
    }

    public async Task<DocumentStatsDto> GetStatsAsync(CancellationToken ct = default)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(ct);

        _logger.LogInformation("DocumentStatsRepository: выполняем Dapper-запрос статистики через {Provider}", _dbProvider);

        // Dapper: агрегированный запрос — итоги и последняя дата создания
        const string totalsSql = """
            SELECT
                COUNT(*)               AS TotalDocuments,
                MAX("CreatedAt")       AS LatestDocumentCreatedAt
            FROM "Documents"
            """;

        // Dapper: группировка по статусу — аналог OLAP GROUP BY
        const string byStatusSql = """
            SELECT
                "Status",
                COUNT(*) AS Count
            FROM "Documents"
            GROUP BY "Status"
            ORDER BY Count DESC
            """;

        // Dapper: количество событий в event store
        const string eventCountSql = """
            SELECT COUNT(*) FROM "DocumentEvents"
            """;

        var totals = await connection.QuerySingleAsync<(int TotalDocuments, DateTime? LatestDocumentCreatedAt)>(totalsSql);
        var byStatus = (await connection.QueryAsync<StatusCountDto>(byStatusSql)).ToList();
        var totalEvents = await connection.ExecuteScalarAsync<int>(eventCountSql);

        return new DocumentStatsDto
        {
            TotalDocuments = totals.TotalDocuments,
            LatestDocumentCreatedAt = totals.LatestDocumentCreatedAt,
            ByStatus = byStatus,
            TotalEvents = totalEvents
        };
    }

    private DbConnection CreateConnection()
    {
        if (_dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            // MS SQL Server: используем SqlConnection + Dapper — аналог работы с on-premise SQL Server
            var sqlConnStr = _connectionString
                .Replace("Host=", "Server=", StringComparison.OrdinalIgnoreCase);
            return new SqlConnection(_connectionString);
        }

        // PostgreSQL: NpgsqlConnection + Dapper — стандарт для облачных постгрес-инстансов
        return new NpgsqlConnection(_connectionString);
    }
}
