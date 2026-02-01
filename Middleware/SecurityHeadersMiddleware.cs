namespace NonProfitFinance.Middleware;

/// <summary>
/// Middleware that adds security headers to all HTTP responses.
/// Implements Content Security Policy (CSP), X-Content-Type-Options, X-Frame-Options, etc.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers before the response is sent
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;

            // Content Security Policy - controls resource loading
            // Allows inline styles/scripts for Blazor, external fonts, and chart libraries
            if (!headers.ContainsKey("Content-Security-Policy"))
            {
                headers["Content-Security-Policy"] = 
                    "default-src 'self'; " +
                    "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; " +
                    "style-src 'self' 'unsafe-inline' https://cdnjs.cloudflare.com https://fonts.googleapis.com; " +
                    "font-src 'self' https://cdnjs.cloudflare.com https://fonts.gstatic.com; " +
                    "img-src 'self' data: blob:; " +
                    "connect-src 'self' ws: wss:; " +
                    "frame-ancestors 'self'; " +
                    "form-action 'self'; " +
                    "base-uri 'self';";
            }

            // Prevent MIME type sniffing
            if (!headers.ContainsKey("X-Content-Type-Options"))
            {
                headers["X-Content-Type-Options"] = "nosniff";
            }

            // Prevent clickjacking
            if (!headers.ContainsKey("X-Frame-Options"))
            {
                headers["X-Frame-Options"] = "SAMEORIGIN";
            }

            // Enable XSS filter in browsers
            if (!headers.ContainsKey("X-XSS-Protection"))
            {
                headers["X-XSS-Protection"] = "1; mode=block";
            }

            // Control referrer information
            if (!headers.ContainsKey("Referrer-Policy"))
            {
                headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            }

            // Prevent browser features abuse
            if (!headers.ContainsKey("Permissions-Policy"))
            {
                headers["Permissions-Policy"] = 
                    "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()";
            }

            return Task.CompletedTask;
        });

        await _next(context);
    }
}

/// <summary>
/// Extension methods to add security headers middleware to the pipeline.
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
