using DocIntegrator.Application.Documents.Commands;
using DocIntegrator.Application.Documents.Dtos;
using DocIntegrator.Application.Documents.Filters;
using DocIntegrator.Application.Documents.Queries;
using DocIntegrator.Application.Documents.Queries.GetAllDocuments;
using DocIntegrator.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using DocIntegrator.Application.Common.Models;


namespace DocIntegrator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET: api/Documents
    [HttpGet]
    public async Task<ActionResult<PagedResult<DocumentDto>>> GetDocuments([FromQuery] DocumentsFilterDto filter, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllDocumentsQuery(filter), ct);
        return Ok(result);
    }

    // GET: api/Documents/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDocumentByIdQuery(id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // POST: api/Documents
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDocumentDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateDocumentCommand(dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // PUT: api/Documents/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDocumentDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateDocumentCommand(id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // DELETE: api/Documents/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var ok = await _mediator.Send(new DeleteDocumentCommand(id), ct);
        if (!ok) return NotFound();
        return NoContent();
    }
}
