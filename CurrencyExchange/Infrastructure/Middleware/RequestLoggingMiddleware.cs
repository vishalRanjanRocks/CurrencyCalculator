namespace CurrencyExchange.Infrastructure.Middleware
{
    using System.Diagnostics;
    using System.Security.Claims;

    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            var clientIp = context.Connection.RemoteIpAddress?.ToString();
            var method = context.Request.Method;
            var path = context.Request.Path;

            var clientId = context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "anonymous";

            var correlationId = Guid.NewGuid().ToString();
            context.Items["CorrelationId"] = correlationId;

            context.Response.OnStarting(() =>
            {
                context.Response.Headers.Add("X-Correlation-Id", correlationId);
                return Task.CompletedTask;
            });

            await _next(context);

            sw.Stop();
            var statusCode = context.Response.StatusCode;

            _logger.LogInformation("ClientIP: {ClientIP} | ClientId: {ClientId} | {Method} {Path} | Status: {Status} | Time: {Time}ms | CorrelationId: {CorrelationId}",
                clientIp, clientId, method, path, statusCode, sw.ElapsedMilliseconds, correlationId);
        }
    }

}
