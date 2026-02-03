using System.Collections.Concurrent;

namespace NonProfitFinance.Middleware;

/// <summary>
/// Simple rate limiting middleware to prevent DoS attacks.
/// HIGH-09 fix: Limits requests per IP address within a time window.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, RateLimitInfo> _requestCounts = new();
    
    // Configuration
    private const int MaxRequestsPerWindow = 300; // Max requests per time window (increased for normal usage)
    private const int TimeWindowSeconds = 60; // Time window in seconds
    private const int ImportMaxRequestsPerWindow = 10; // Stricter limit for imports but more lenient

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var path = context.Request.Path.Value ?? "";
        var isImportEndpoint = path.Contains("/api/import", StringComparison.OrdinalIgnoreCase);
        
        // Exclude navigation/shell requests from rate limiting (page reloads)
        if (path == "/" || path == "/index.html" || path.Contains("/_framework/") || path.Contains("_blazor"))
        {
            await _next(context);
            return;
        }
        
        var key = $"{ipAddress}:{(isImportEndpoint ? "import" : "general")}";
        var maxRequests = isImportEndpoint ? ImportMaxRequestsPerWindow : MaxRequestsPerWindow;
        
        var now = DateTime.UtcNow;
        var rateLimitInfo = _requestCounts.GetOrAdd(key, _ => new RateLimitInfo { WindowStart = now, Count = 0 });
        
        // Reset window if expired
        if ((now - rateLimitInfo.WindowStart).TotalSeconds > TimeWindowSeconds)
        {
            rateLimitInfo.WindowStart = now;
            rateLimitInfo.Count = 0;
        }
        
        rateLimitInfo.Count++;
        
        if (rateLimitInfo.Count > maxRequests)
        {
            _logger.LogWarning("Rate limit exceeded for {IP} on {Path}", ipAddress, path);
            
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers["Retry-After"] = TimeWindowSeconds.ToString();
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "Too many requests. Please try again later.",
                retryAfterSeconds = TimeWindowSeconds
            });
            return;
        }
        
        // Add rate limit headers
        context.Response.Headers["X-RateLimit-Limit"] = maxRequests.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = Math.Max(0, maxRequests - rateLimitInfo.Count).ToString();
        context.Response.Headers["X-RateLimit-Reset"] = ((int)(rateLimitInfo.WindowStart.AddSeconds(TimeWindowSeconds) - now).TotalSeconds).ToString();
        
        await _next(context);
    }

    private class RateLimitInfo
    {
        public DateTime WindowStart { get; set; }
        public int Count { get; set; }
    }
}

/// <summary>
/// Extension methods to add rate limiting middleware to the pipeline.
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>();
    }
}
