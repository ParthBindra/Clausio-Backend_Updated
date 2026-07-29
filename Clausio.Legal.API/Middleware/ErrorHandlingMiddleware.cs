using System.Net;
using System.Text.Json;

namespace Clausio.Legal.API.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate                   _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(
            RequestDelegate                   next,
            ILogger<ErrorHandlingMiddleware> logger)
        {
            _next   = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unhandled exception on {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode  = ex switch
            {
                InvalidOperationException   => (int)HttpStatusCode.BadRequest,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                KeyNotFoundException        => (int)HttpStatusCode.NotFound,
                ArgumentException           => (int)HttpStatusCode.BadRequest,
                _                           => (int)HttpStatusCode.InternalServerError
            };

            var response = new
            {
                message = ex.Message,
                status    = context.Response.StatusCode,
                timestamp = DateTime.UtcNow,
                requestId = context.Items["RequestId"]?.ToString()
            };

            return context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }
}