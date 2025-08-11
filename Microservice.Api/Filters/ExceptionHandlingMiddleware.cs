using System.Net;
using System.Text.Json;

public class ExceptionHandlingMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        try { await next(ctx); }
        catch (Exception ex)
        {
            var problem = new
            {
                title = "Unexpected error",
                detail = ex.Message,
                status = (int)HttpStatusCode.InternalServerError
            };
            ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}
