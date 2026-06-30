using Serilog.Context;
using System.Text;

namespace Octacom.API.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Guid.NewGuid().ToString();
        context.Response.Headers["X-Correlation-Id"] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            var requestBody = await ReadRequestBodyAsync(context.Request);

            _logger.LogInformation(
                "Incoming Request: {Method} {Path}{QueryString} | Body: {Body}",
                context.Request.Method,
                context.Request.Path,
                context.Request.QueryString,
                string.IsNullOrWhiteSpace(requestBody) ? "(empty)" : requestBody
            );

            var originalResponseBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            await _next(context);

            var responseBody = await ReadResponseBodyAsync(context.Response);

            _logger.LogInformation(
                "Outgoing Response: {Method} {Path} | StatusCode: {StatusCode} | Body: {Body}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                string.IsNullOrWhiteSpace(responseBody) ? "(empty)" : responseBody
            );

            await responseBodyStream.CopyToAsync(originalResponseBodyStream);
        }
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        if (!request.Body.CanSeek)
        {
            request.EnableBuffering();
        }

        request.Body.Position = 0;
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        return body;
    }

    private static async Task<string> ReadResponseBodyAsync(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(response.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);

        return body;
    }
}