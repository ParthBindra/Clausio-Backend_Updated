namespace Clausio.Legal.API.Middleware
{
    public class RequestIdMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestId = context.Request.Headers["X-Request-Id"]
                .FirstOrDefault() ?? Guid.NewGuid().ToString();

            context.Items["RequestId"]               = requestId;
            context.Response.Headers["X-Request-Id"] = requestId;

            await _next(context);
        }
    }
}