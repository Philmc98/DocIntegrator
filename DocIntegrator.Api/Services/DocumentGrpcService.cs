using DocIntegrator.Api.Grpc;
using DocIntegrator.Application.Documents.Dtos;
using DocIntegrator.Application.Documents.Queries.GetAllDocuments;
using MediatR;
using Microsoft.Extensions.Logging;
using GrpcCore = global::Grpc.Core;

namespace DocIntegrator.Api.Services;

/// <summary>
/// gRPC-сервис поверх существующего CQRS-слоя.
/// Показывает, как тот же Application-уровень может обслуживать не только REST, но и gRPC-клиентов.
/// </summary>
public class DocumentGrpcService : Documents.DocumentsBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DocumentGrpcService> _logger;

    /// <summary>Инициализирует gRPC-сервис с MediatR-диспетчером и логгером.</summary>
    /// <param name="mediator">MediatR-диспетчер для отправки запросов в Application-слой.</param>
    /// <param name="logger">Логгер.</param>
    public DocumentGrpcService(IMediator mediator, ILogger<DocumentGrpcService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Возвращает документ по GUID. Тот же CQRS-запрос, что и REST GET /api/documents/{id}.
    /// </summary>
    /// <param name="request">Запрос с полем <c>id</c> (строковый GUID).</param>
    /// <param name="context">gRPC серверный контекст (CancellationToken, metadata и т.д.).</param>
    /// <returns>Заполненный <see cref="DocumentReply"/>.</returns>
    /// <exception cref="GrpcCore.RpcException">InvalidArgument — некорректный GUID; NotFound — документ не найден.</exception>
    public override async Task<DocumentReply> GetDocumentById(GetDocumentByIdRequest request, GrpcCore.ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
        {
            throw new GrpcCore.RpcException(new GrpcCore.Status(GrpcCore.StatusCode.InvalidArgument, "Invalid GUID format"));
        }

        var dto = await _mediator.Send(new GetDocumentByIdQuery(id), context.CancellationToken);
        if (dto is null)
        {
            throw new GrpcCore.RpcException(new GrpcCore.Status(GrpcCore.StatusCode.NotFound, "Document not found"));
        }

        _logger.LogInformation("gRPC запрошен документ Id={DocumentId}", id);

        return MapToReply(dto);
    }

    private static DocumentReply MapToReply(DocumentDto dto) =>
        new()
        {
            Id = dto.Id.ToString(),
            Title = dto.Title,
            Content = dto.Content,
            Status = dto.Status,
            CreatedAt = dto.CreatedAt.ToString("O")
        };
}

