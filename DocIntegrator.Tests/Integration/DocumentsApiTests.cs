using System.Net;
using System.Net.Http.Json;
using DocIntegrator.Application.Common.Models;
using DocIntegrator.Application.Documents.Dtos;
using Xunit;

namespace DocIntegrator.Tests.Integration;

/// <summary>
/// Интеграционные тесты API через WebApplicationFactory.
/// Используют CustomWebApplicationFactory с InMemory БД.
/// </summary>
public class DocumentsApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DocumentsApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetDocuments_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/Documents");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<PagedResult<DocumentDto>>();
        Assert.NotNull(json);
        Assert.NotNull(json.Items);
    }

    [Fact]
    public async Task GetDocumentById_WhenNotExists_Returns404()
    {
        var response = await _client.GetAsync($"/api/Documents/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateDocument_Returns201()
    {
        var dto = new CreateDocumentDto
        {
            Title = "Integration Test",
            Content = "Content",
            Status = "Черновик"
        };
        var response = await _client.PostAsJsonAsync("/api/Documents", dto);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<DocumentDto>();
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(dto.Title, created.Title);
    }
}
