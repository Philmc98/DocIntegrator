using MediatR;
using Microsoft.AspNetCore.Mvc;
using DocIntegrator.Application.Interfaces;
using DocIntegrator.Application.Documents.Queries.GetAllDocuments;
using DocIntegrator.Application.Documents.Commands;

namespace DocIntegrator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IDocumentRepository _repository;

    public DocumentsController(IMediator mediator, IDocumentRepository repository)
    {
        _mediator = mediator;
        _repository = repository;
    }

    // GET: api/Documents
    [HttpGet]
    public async Task<IActionResult> GetAll(
    [FromQuery] string? status,
    [FromQuery] string? titleContains,
    [FromQuery] string? primarySort = "createdAt",
    [FromQuery] string? primarySortOrder = "desc",
    [FromQuery] string? secondarySort = null,
    [FromQuery] string? secondarySortOrder = "asc",
    [FromQuery] DateTime? createdFrom = null,
    [FromQuery] DateTime? createdTo = null,
    [FromQuery] int? page = null,
    [FromQuery] int? pageSize = null,
    CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAllDocumentsQuery(
            status,
            titleContains,
            primarySort,
            primarySortOrder,
            secondarySort,
            secondarySortOrder,
            createdFrom,
            createdTo,
            page,
            pageSize
        ), ct);

        return Ok(result);
    }


    // GET: api/Documents/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var doc = await _repository.GetByIdAsync(id, ct);
        return doc is null ? NotFound() : Ok(doc);
    }

    // POST: api/Documents
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDocumentCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(Get), new { id }, new
        {
            id,
            title = command.Title,
            content = command.Content,
            status = command.Status,
            created = DateTime.UtcNow
        });
    }

    // PUT: api/Documents/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDocumentCommand command, CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest("Id в URL и теле запроса не совпадают");

        await _mediator.Send(command, ct);
        return NoContent();
    }

    // DELETE: api/Documents/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteDocumentCommand(id), ct);
        return NoContent();
    }
}
