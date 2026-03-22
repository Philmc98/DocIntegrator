using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DocIntegrator.Application.Behaviors;

/// <summary>
/// Pipeline behavior: замеряет время выполнения запроса и логирует предупреждение при превышении порога.
/// Удобно для выявления медленных операций без профилировщика.
/// </summary>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private const int ThresholdMs = 500;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var requestName = typeof(TRequest).Name;

        var response = await next();

        sw.Stop();
        if (sw.ElapsedMilliseconds > ThresholdMs)
            _logger.LogWarning("Медленный запрос {RequestName}: {ElapsedMs} мс", requestName, sw.ElapsedMilliseconds);

        return response;
    }
}
