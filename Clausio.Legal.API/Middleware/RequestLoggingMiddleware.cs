namespace Clausio.Legal.API.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate                    _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(
            RequestDelegate                    next,
            ILogger<RequestLoggingMiddleware> logger)
        {
            _next   = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var start     = DateTime.UtcNow;
            var requestId = context.Items["RequestId"]?.ToString() ?? Guid.NewGuid().ToString()[..8];
            var userId    = context.User?.FindFirst("sub")?.Value ?? "anonymous";

            _logger.LogInformation(
                "[{RequestId}] → {Method} {Path} | User: {UserId} | IP: {IP}",
                requestId,
                context.Request.Method,
                context.Request.Path,
                userId,
                context.Connection.RemoteIpAddress
            );

            await _next(context);

            var duration = (DateTime.UtcNow - start).TotalMilliseconds;
            var logLevel = context.Response.StatusCode >= 500
                ? LogLevel.Error
                : context.Response.StatusCode >= 400
                    ? LogLevel.Warning
                    : LogLevel.Information;

            _logger.Log(logLevel,
                "[{RequestId}] ← {Method} {Path} | Status: {Status} | Duration: {Duration}ms",
                requestId,
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                Math.Round(duration)
            );
        }
    }
}