using System.Net;
using System.Text.Json;
using ECommerce.Application.Common.Exceptions;

namespace ECommerce.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (status, title, detail, errors) = exception switch
        {
            NotFoundException nf => (HttpStatusCode.NotFound, "Not Found", nf.Message, (IDictionary<string, string[]>?)null),
            ForbiddenAccessException fb => (HttpStatusCode.Forbidden, "Forbidden", fb.Message, null),
            AppValidationException ve => (HttpStatusCode.BadRequest, "Validation Failed", "One or more validation errors occurred.", ve.Errors),
            _ => (HttpStatusCode.InternalServerError, "Server Error",
                _environment.IsDevelopment() ? exception.Message : "An unexpected error occurred.", null)
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)status;

        var problem = new
        {
            type = $"https://httpstatuses.com/{(int)status}",
            title,
            status = (int)status,
            detail,
            errors,
            traceId = context.TraceIdentifier
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }
}
