using FluentValidation;
using System.Net;
using System.Text.Json;

public class ExceptionHandlingMiddleware : IMiddleware
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web) { WriteIndented = false };
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        try
        {
            await next(ctx);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed");
            var errors = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
            await WriteProblem(ctx, HttpStatusCode.BadRequest, "Validation failed",
                detail: "One or more validation errors occurred.",
                extensions: new() { ["errors"] = errors });
        }
        catch (OperationCanceledException ex) when (!ctx.RequestAborted.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Operation was canceled");
            await WriteProblem(ctx, HttpStatusCode.BadRequest, "Operation canceled");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized");
            await WriteProblem(ctx, HttpStatusCode.Unauthorized, "Unauthorized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            var detail = _env.IsDevelopment() ? ex.Message : "Unexpected error";
            await WriteProblem(ctx, HttpStatusCode.InternalServerError, "Unexpected error", detail);
        }
    }

    private static async Task WriteProblem(
        HttpContext ctx,
        HttpStatusCode status,
        string title,
        string? detail = null,
        Dictionary<string, object>? extensions = null)
    {
        if (ctx.Response.HasStarted) return;

        ctx.Response.StatusCode = (int)status;
        ctx.Response.ContentType = "application/json";

        var payload = new Dictionary<string, object?>
        {
            ["type"] = $"https://httpstatuses.com/{(int)status}",
            ["title"] = title,
            ["status"] = (int)status,
            ["detail"] = detail,
            ["instance"] = ctx.Request.Path.Value,
            ["traceId"] = ctx.TraceIdentifier
        };

        if (extensions is not null)
        {
            foreach (var kv in extensions)
                payload[kv.Key] = kv.Value;
        }

        await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOpts));
    }
}
