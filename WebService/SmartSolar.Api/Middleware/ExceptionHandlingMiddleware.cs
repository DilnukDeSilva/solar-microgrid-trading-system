/*
 * File: ExceptionHandlingMiddleware.cs
 * Description: Converts unhandled exceptions into the contract `{ code, message }` JSON body.
 * Author: DE SILVA R K D H (IT22001252)
 * Created: 20/09/2026
 */

using System.Text.Json;
using SmartSolar.Api.Common;
using SmartSolar.Api.DTOs;

namespace SmartSolar.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    // Captures the next middleware and the logger used for unexpected failures.
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    // Runs the pipeline and maps exceptions to HTTP status + contract error JSON.
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiException ex)
        {
            await WriteErrorAsync(context, ex.StatusCode, ex.Code, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing {Path}", context.Request.Path);
            await WriteErrorAsync(
                context,
                StatusCodes.Status500InternalServerError,
                ErrorCodes.InternalError,
                "An unexpected error occurred.");
        }
    }

    // Writes a JSON error body only if the response has not already started.
    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string code, string message)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        var payload = JsonSerializer.Serialize(new ApiErrorDto(code, message), JsonDefaults.Options);
        await context.Response.WriteAsync(payload);
    }
}
