using System.Diagnostics;

namespace NonProfitFinance.Middleware;

/// <summary>
/// Middleware that logs HTTP request and response information for debugging and auditing.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip logging for static files
        if (IsStaticFile(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var requestId = Activity.Current?.Id ?? context.TraceIdentifier;

        // Log request
        _logger.LogInformation(
            "HTTP {Method} {Path} started | RequestId: {RequestId} | IP: {IP}",
            context.Request.Method,
            context.Request.Path,
            requestId,
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown"
        );

        try
        {
            await _next(context);
            stopwatch.Stop();

            // Log response
            var level = context.Response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
            _logger.Log(
                level,
                "HTTP {Method} {Path} completed | Status: {StatusCode} | Duration: {Duration}ms | RequestId: {RequestId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                requestId
            );
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "HTTP {Method} {Path} failed | Duration: {Duration}ms | RequestId: {RequestId}",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds,
                requestId
            );
            throw;
        }
    }

    private static bool IsStaticFile(PathString path)
    {
        var pathValue = path.Value ?? string.Empty;
        return pathValue.StartsWith("/css", StringComparison.OrdinalIgnoreCase) ||
               pathValue.StartsWith("/js", StringComparison.OrdinalIgnoreCase) ||
               pathValue.StartsWith("/lib", StringComparison.OrdinalIgnoreCase) ||
               pathValue.StartsWith("/_framework", StringComparison.OrdinalIgnoreCase) ||
               pathValue.StartsWith("/_blazor", StringComparison.OrdinalIgnoreCase) ||
               pathValue.EndsWith(".ico", StringComparison.OrdinalIgnoreCase) ||
               pathValue.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
               pathValue.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
               pathValue.EndsWith(".woff", StringComparison.OrdinalIgnoreCase) ||
               pathValue.EndsWith(".woff2", StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Extension methods to add request logging middleware to the pipeline.
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
