using DocIntegrator.Domain.Entities;
using DocIntegrator.Infrastructure.Repositories;
using Xunit;

namespace DocIntegrator.Tests.Repositories;

public class InMemoryDocumentRepositoryTests
{
    private readonly InMemoryDocumentRepository _repo = new();

    [Fact]
    public async Task AddAsync_GetByIdAsync_ReturnsDocument()
    {
        var doc = new Document
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            Content = "Content",
            Status = "Черновик",
            CreatedAt = DateTime.UtcNow
        };
        await _repo.AddAsync(doc);
        var found = await _repo.GetByIdAsync(doc.Id);
        Assert.NotNull(found);
        Assert.Equal(doc.Id, found.Id);
        Assert.Equal(doc.Title, found.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotExists_ReturnsNull()
    {
        var found = await _repo.GetByIdAsync(Guid.NewGuid());
        Assert.Null(found);
    }

    [Fact]
    public async Task DeleteAsync_RemovesDocument()
    {
        var doc = new Document
        {
            Id = Guid.NewGuid(),
            Title = "ToDelete",
            Content = "x",
            Status = "Черновик",
            CreatedAt = DateTime.UtcNow
        };
        await _repo.AddAsync(doc);
        var ok = await _repo.DeleteAsync(doc.Id);
        Assert.True(ok);
        var found = await _repo.GetByIdAsync(doc.Id);
        Assert.Null(found);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotExists_ReturnsFalse()
    {
        var ok = await _repo.DeleteAsync(Guid.NewGuid());
        Assert.False(ok);
    }

    [Fact]
    public async Task Query_ReturnsFilteredResults()
    {
        var doc1 = new Document
        {
            Id = Guid.NewGuid(),
            Title = "A",
            Content = "x",
            Status = "Черновик",
            CreatedAt = DateTime.UtcNow
        };
        var doc2 = new Document
        {
            Id = Guid.NewGuid(),
            Title = "B",
            Content = "x",
            Status = "Опубликован",
            CreatedAt = DateTime.UtcNow
        };
        await _repo.AddAsync(doc1);
        await _repo.AddAsync(doc2);
        var list = _repo.Query().Where(d => d.Status == "Черновик").ToList();
        Assert.Single(list);
        Assert.Equal("A", list[0].Title);
    }
}
