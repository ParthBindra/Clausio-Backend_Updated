using System.Collections.Concurrent;
using System.Text.Json;

namespace Clausio.Legal.API.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate                  _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;

        private static readonly ConcurrentDictionary<string, RateLimitEntry>
            _requestCounts = new();

        private const int MAX_REQUESTS    = 100;
        private const int WINDOW_SECONDS  = 60;
        private const int MAX_AUTH        = 10;
        private const int AUTH_WINDOW     = 300;

        public RateLimitingMiddleware(
            RequestDelegate                  next,
            ILogger<RateLimitingMiddleware> logger)
        {
            _next   = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ip        = GetClientIp(context);
            var isAuth    = context.Request.Path.StartsWithSegments("/api/auth/login");
            var now       = DateTime.UtcNow;
            var maxReq    = isAuth ? MAX_AUTH       : MAX_REQUESTS;
            var windowSec = isAuth ? AUTH_WINDOW    : WINDOW_SECONDS;

            var entry = _requestCounts.GetOrAdd(ip, _ =>
                new RateLimitEntry(0, now.AddSeconds(windowSec)));

            if (now > entry.WindowExpiry)
                entry = new RateLimitEntry(0, now.AddSeconds(windowSec));

            entry = entry with { Count = entry.Count + 1 };
            _requestCounts[ip] = entry;

            context.Response.Headers["X-RateLimit-Limit"]     = maxReq.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = Math.Max(0, maxReq - entry.Count).ToString();

            if (entry.Count > maxReq)
            {
                _logger.LogWarning("Rate limit exceeded for IP: {IP}", ip);
                context.Response.StatusCode  = 429;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    message    = isAuth
                        ? "Too many login attempts. Try again in 5 minutes."
                        : "Too many requests. Please slow down.",
                    retryAfter = (int)(entry.WindowExpiry - now).TotalSeconds,
                    status     = 429
                }));
                return;
            }

            await _next(context);
        }

        private static string GetClientIp(HttpContext context)
        {
            var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwarded))
                return forwarded.Split(',')[0].Trim();
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }

    public record RateLimitEntry(int Count, DateTime WindowExpiry);
}