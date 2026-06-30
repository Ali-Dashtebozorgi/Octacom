using Microsoft.EntityFrameworkCore;
using Octacom.API.Common;
using Octacom.Domain.Base;
using System.Net;
using System.Text.Json;

namespace Octacom.API.Middleware;
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, logLevel) = MapException(exception);

        _logger.Log(logLevel, exception, "Exception handled: {Message}", message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse<object>.Fail(message);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private static (HttpStatusCode StatusCode, string Message, LogLevel LogLevel) MapException(Exception exception) =>
        exception switch
        {
            ConferenceNotFoundException ex => (HttpStatusCode.NotFound, ex.Message, LogLevel.Warning),
            BookingNotFoundException ex => (HttpStatusCode.NotFound, ex.Message, LogLevel.Warning),
            DuplicateBookingException ex => (HttpStatusCode.Conflict, ex.Message, LogLevel.Warning),
            ArgumentException ex => (HttpStatusCode.BadRequest, ex.Message, LogLevel.Warning),
            InvalidOperationException ex => (HttpStatusCode.BadRequest, ex.Message, LogLevel.Warning),
            DbUpdateConcurrencyException _ => (HttpStatusCode.Conflict, "The resource was modified by another request. Please try again.", LogLevel.Warning),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", LogLevel.Error)
        };
}