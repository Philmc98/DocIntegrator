using DocIntegrator.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocIntegrator.Infrastructure.Data;

/// <summary>
/// Основной DbContext приложения.
/// Хранит состояние документов (таблица Documents) и ленту событий (таблица DocumentEvents)
/// для поддержки event sourcing / аудита.
/// </summary>
public class DocIntegratorDbContext : DbContext
{
    public DocIntegratorDbContext(DbContextOptions<DocIntegratorDbContext> options)
        : base(options) { }

    /// <summary>
    /// Текущие актуальные состояния документов.
    /// Это привычная CRUD-таблица, с которой работает основное API.
    /// </summary>
    public DbSet<Document> Documents => Set<Document>();

    /// <summary>
    /// Лента доменных событий по документам.
    /// Каждое создание/обновление/удаление документа порождает запись в этом наборе.
    /// </summary>
    public DbSet<DocumentEvent> DocumentEvents => Set<DocumentEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Маппинг сущности Document.
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasIndex(e => e.Status).HasDatabaseName("IX_Documents_Status");
            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_Documents_CreatedAt");
        });

        // Маппинг сущности DocumentEvent (event sourcing / аудит).
        modelBuilder.Entity<DocumentEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PayloadJson).IsRequired();
            entity.Property(e => e.OccurredAt).IsRequired();

            entity.HasIndex(e => e.DocumentId).HasDatabaseName("IX_DocumentEvents_DocumentId");
            entity.HasIndex(e => e.OccurredAt).HasDatabaseName("IX_DocumentEvents_OccurredAt");
        });
    }
}

