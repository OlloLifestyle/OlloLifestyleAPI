using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Net;

namespace OlloLifestyleAPI.Configuration;

public static class RateLimiterExtensions
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            // Configure global rate limiting policy
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientIdentifier(httpContext),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = GetPermitLimit(configuration),
                        Window = TimeSpan.FromMinutes(1)
                    }));

            // Configure rejection response
            options.OnRejected = OnRejectedAsync;

            // Add named policies for specific endpoints
            AddNamedPolicies(options, configuration);
        });

        return services;
    }

    private static string GetClientIdentifier(HttpContext httpContext)
    {
        // Get client IP address with proxy support
        var clientIP = GetClientIPAddress(httpContext);
        
        // For authenticated users, you can combine IP + UserID for more granular control
        var userId = httpContext.User?.Identity?.Name;
        
        return string.IsNullOrEmpty(userId) 
            ? $"ip:{clientIP}" 
            : $"user:{userId}:ip:{clientIP}";
    }

    private static string GetClientIPAddress(HttpContext httpContext)
    {
        // Check for forwarded IP first (for load balancers/proxies)
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var firstIP = forwardedFor.Split(',')[0].Trim();
            if (IPAddress.TryParse(firstIP, out _))
                return firstIP;
        }

        // Check for real IP header
        var realIP = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIP) && IPAddress.TryParse(realIP, out _))
            return realIP;

        // Fall back to connection remote IP
        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static int GetPermitLimit(IConfiguration configuration)
    {
        return configuration.GetValue<int>("RateLimiting:PermitLimit", 100);
    }

    private static void AddNamedPolicies(RateLimiterOptions options, IConfiguration configuration)
    {
        // Authentication endpoints - more restrictive
        options.AddPolicy("AuthPolicy", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: GetClientIdentifier(httpContext),
                factory: partition => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = configuration.GetValue<int>("RateLimiting:AuthPermitLimit", 10),
                    Window = TimeSpan.FromMinutes(1)
                }));

        // API endpoints - standard limit
        options.AddPolicy("ApiPolicy", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: GetClientIdentifier(httpContext),
                factory: partition => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = configuration.GetValue<int>("RateLimiting:ApiPermitLimit", 100),
                    Window = TimeSpan.FromMinutes(1)
                }));

        // File upload endpoints - very restrictive
        options.AddPolicy("UploadPolicy", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: GetClientIdentifier(httpContext),
                factory: partition => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = configuration.GetValue<int>("RateLimiting:UploadPermitLimit", 5),
                    Window = TimeSpan.FromMinutes(1)
                }));

        // Public endpoints - generous but still limited
        options.AddPolicy("PublicPolicy", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: GetClientIdentifier(httpContext),
                factory: partition => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = configuration.GetValue<int>("RateLimiting:PublicPermitLimit", 200),
                    Window = TimeSpan.FromMinutes(1)
                }));
    }

    private static async ValueTask OnRejectedAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        var httpContext = context.HttpContext;
        
        // Set response status and headers
        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        httpContext.Response.ContentType = "application/json";

        // Add rate limit headers for better client experience
        TimeSpan? retryAfter = null;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue) && retryAfterValue is TimeSpan ts)
        {
            retryAfter = ts;
            httpContext.Response.Headers["Retry-After"] = ts.TotalSeconds.ToString();
        }

        var retryAfterSeconds = retryAfter?.TotalSeconds ?? 60;

        // Add custom headers
        httpContext.Response.Headers["X-RateLimit-Policy"] = "Global";
        httpContext.Response.Headers["X-RateLimit-Limit"] = "100";
        httpContext.Response.Headers["X-RateLimit-Remaining"] = "0";
        httpContext.Response.Headers["X-RateLimit-Reset"] = DateTimeOffset.UtcNow.AddSeconds(retryAfterSeconds).ToUnixTimeSeconds().ToString();

        // Create structured error response
        var errorResponse = new
        {
            error = "rate_limit_exceeded",
            message = "Rate limit exceeded. Please try again later.",
            details = new
            {
                limit = 100,
                windowSize = "1 minute",
                retryAfter = retryAfterSeconds,
                policy = "Global"
            },
            timestamp = DateTimeOffset.UtcNow,
            traceId = httpContext.TraceIdentifier
        };

        // Log rate limit violation
        var logger = httpContext.RequestServices.GetService<ILogger>();
        logger?.LogWarning("Rate limit exceeded for {ClientIdentifier} on {Path}", 
            GetClientIdentifier(httpContext), 
            httpContext.Request.Path);

        // Write JSON response
        await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);
    }
}

/// <summary>
/// Extension methods for applying rate limiting to controllers and actions
/// </summary>
public static class RateLimitingControllerExtensions
{
    /// <summary>
    /// Apply rate limiting policy to a controller action
    /// </summary>
    public static RouteHandlerBuilder WithRateLimit(this RouteHandlerBuilder builder, string policyName)
    {
        return builder.RequireRateLimiting(policyName);
    }
}