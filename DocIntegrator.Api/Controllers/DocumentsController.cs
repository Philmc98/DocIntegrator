using DocIntegrator.Application.Documents.Commands;
using DocIntegrator.Application.Documents.Dtos;
using DocIntegrator.Application.Documents.Filters;
using DocIntegrator.Application.Documents.Queries;
using DocIntegrator.Application.Documents.Queries.GetAllDocuments;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using DocIntegrator.Application.Common.Models;

namespace DocIntegrator.Api.Controllers;

/// <summary>
/// REST API контроллер для работы с документами.
/// Реализует CRUD-операции: список, получение по Id, создание, обновление, удаление.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DocumentsController> _logger;

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
    /// Обновить существующий документ
    /// </summary>
    [HttpPut("{id:guid}")]
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
    /// Удалить документ
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        _logger.LogInformation("Удаление документа Id = {DocumentId}", id);
        var ok = await _mediator.Send(new DeleteDocumentCommand(id), ct);
        if (!ok) 
        {
            _logger.LogWarning("Документ с Id = {DocumentId} не найден для удаления", id);
            return NotFound();
        } 
        return NoContent();
    }
}
