using MediatR;
using Microsoft.Extensions.Logging;

namespace DocIntegrator.Application.Behaviors;

/// <summary>
/// Pipeline behavior: логирует имя запроса/команды перед выполнением и после.
/// Полезно для трассировки и аудита без дублирования логов в каждом хендлере.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("MediatR: обработка {RequestName}", requestName);

        var response = await next();

        _logger.LogInformation("MediatR: завершён {RequestName}", requestName);
        return response;
    }
}
