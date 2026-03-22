using DocIntegrator.Application.Documents.Commands;
using DocIntegrator.Application.Documents.Dtos;
using DocIntegrator.Application.Documents.Filters;
using DocIntegrator.Application.Documents.Queries;
using DocIntegrator.Application.Documents.Queries.GetAllDocuments;
using DocIntegrator.Application.Documents.Queries.GetDocumentHistory;
using DocIntegrator.Application.Documents.Queries.GetDocumentStats;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DocIntegrator.Application.Common.Models;

namespace DocIntegrator.Api.Controllers;

/// <summary>
/// REST API контроллер для работы с документами.
/// Требуется аутентификация (JWT). PUT и DELETE — только роль Admin.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DocumentsController> _logger;

    /// <summary>Инициализирует контроллер с MediatR-диспетчером и логгером.</summary>
    /// <param name="mediator">MediatR-диспетчер для отправки команд и запросов.</param>
    /// <param name="logger">Логгер.</param>
    public DocumentsController(IMediator mediator, ILogger<DocumentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // GET: api/Documents
    /// <summary>
    /// Получить список документов с фильтрацией, сортировкой и пагинацией.
    /// </summary>
    /// <param name="filter">Фильтр документов</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Список документов</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<DocumentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<DocumentDto>>> GetDocuments([FromQuery] DocumentsFilterDto filter, CancellationToken ct)
    {
        _logger.LogInformation("Запрос списка документов с фильтром {@Filter}", filter);
        var result = await _mediator.Send(new GetAllDocumentsQuery(filter), ct);
        return Ok(result);
    }

    // GET: api/Documents/{id}
    /// <summary>
    /// Получить документ по идентификатору
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        _logger.LogInformation("Запрос документа по Id = {DocumentId}", id);
        var result = await _mediator.Send(new GetDocumentByIdQuery(id), ct);
        if (result == null) 
        {
            _logger.LogWarning("Документ с Id = {DocumentId} не найден", id);
            return NotFound();
        }
        return Ok(result);
    }

    // POST: api/Documents
    /// <summary>
    /// Создать новый документ
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateDocumentDto dto, CancellationToken ct)
    {
        _logger.LogInformation("Создание нового документа Title = {Title}", dto.Title);
        var result = await _mediator.Send(new CreateDocumentCommand(dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // PUT: api/Documents/{id}
    /// <summary>
    /// Обновить существующий документ. Только роль Admin.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDocumentDto dto, CancellationToken ct)
    {
        _logger.LogInformation("Обновление документа Id = {DocumentId}", id);
        var result = await _mediator.Send(new UpdateDocumentCommand(id, dto), ct);
        if (result == null) 
        {
            _logger.LogWarning("Документ с Id = {DocumentId} не найден для обновления", id);
            return NotFound();
        } 
        return NoContent();
    }

    // DELETE: api/Documents/{id}
    /// <summary>
    /// Удалить документ. Только роль Admin.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        _logger.LogInformation("Удаление документа Id = {DocumentId}", id);
        await _mediator.Send(new DeleteDocumentCommand(id), ct);
        return NoContent();
    }

    // GET: api/Documents/{id}/history
    /// <summary>
    /// История изменений документа из event store (event sourcing).
    /// Возвращает все события: Created, Updated, Deleted — в хронологическом порядке.
    /// </summary>
    [HttpGet("{id:guid}/history")]
    [ProducesResponseType(typeof(IReadOnlyList<DocumentEventDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(Guid id, CancellationToken ct)
    {
        _logger.LogInformation("Запрос истории событий для DocumentId = {DocumentId}", id);
        var result = await _mediator.Send(new GetDocumentHistoryQuery(id), ct);
        return Ok(result);
    }

    // GET: api/Documents/stats
    /// <summary>
    /// Аналитика: общее количество документов, разбивка по статусам, количество событий.
    /// В режиме PostgreSql/SqlServer — через Dapper (raw SQL + GROUP BY).
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(DocumentStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        _logger.LogInformation("Запрос статистики по документам");
        var result = await _mediator.Send(new GetDocumentStatsQuery(), ct);
        return Ok(result);
    }
}
