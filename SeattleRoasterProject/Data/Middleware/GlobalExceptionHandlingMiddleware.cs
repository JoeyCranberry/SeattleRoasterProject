using System.Net;

namespace SeattleRoasterProject.Data.Middleware;

public class GlobalExceptionHandlingMiddleware(ILogger<GlobalExceptionHandlingMiddleware> logger) : IMiddleware
{
    private readonly ILogger _logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Caller will either receive 200 (OK) or one of the below;
        try
        {
            await next(context);
            // Catch, handle and log known Exceptions below.
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; // 500
            // When catching general Exceptions, we don't want to write them to the response.
            await context.Response.WriteAsync("Internal Server Error. Please Try Again Later.");
            _logger.Log(LogLevel.Error,
                $"Message: {Environment.NewLine + ex.Message} {Environment.NewLine}Trace: {Environment.NewLine + ex.StackTrace ?? string.Empty}");
        }
    }
}