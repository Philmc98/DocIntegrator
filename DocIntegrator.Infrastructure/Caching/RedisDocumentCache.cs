using System.Text.Json;
using DocIntegrator.Application.Documents.Dtos;
using DocIntegrator.Application.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace DocIntegrator.Infrastructure.Caching;

/// <summary>
/// Реализация кеша документов на базе Redis.
/// Используется для ускорения чтения (особенно GetById).
/// </summary>
public class RedisDocumentCache : IDocumentCache
{
    private readonly IDatabase _database;
    private readonly ILogger<RedisDocumentCache> _logger;
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(5);

    public RedisDocumentCache(IConnectionMultiplexer connection, ILogger<RedisDocumentCache> logger)
    {
        _database = connection.GetDatabase();
        _logger = logger;
    }

    private static string BuildKey(Guid id) => $"docs:{id}";

    public async Task<DocumentDto?> GetAsync(Guid id, CancellationToken ct)
    {
        // Redis-клиент StackExchange.Redis не поддерживает токен отмены,
        // поэтому просто пробрасываем его "для совместимости" сигнатуры.
        var value = await _database.StringGetAsync(BuildKey(id));
        if (!value.HasValue)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<DocumentDto>(value!);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось десериализовать документ из Redis, key={Key}", BuildKey(id));
            // При ошибке десериализации просто инвалидируем ключ, чтобы не мешать нормальной работе.
            await _database.KeyDeleteAsync(BuildKey(id));
            return null;
        }
    }

    public async Task SetAsync(DocumentDto document, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(document);
        await _database.StringSetAsync(BuildKey(document.Id), json, DefaultTtl);
    }

    public async Task InvalidateAsync(Guid id, CancellationToken ct)
    {
        await _database.KeyDeleteAsync(BuildKey(id));
    }
}

