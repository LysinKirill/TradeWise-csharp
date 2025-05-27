using System;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TradeWiseBackend.Api.Middlewares;

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
        var request = context.Request;
        _logger.LogInformation("Handling request: {Method} {Path}", request.Method, request.Path);

        context.Request.EnableBuffering();

        var buffer = new byte[Convert.ToInt32(context.Request.ContentLength ?? 0)];
        await context.Request.Body.ReadAsync(buffer.AsMemory(0, buffer.Length));

        var bodyAsText = Encoding.UTF8.GetString(buffer);
        context.Request.Body.Seek(0, SeekOrigin.Begin);

        _logger.LogInformation("Request Body: {Body}", bodyAsText);

        await _next(context);

        _logger.LogInformation("Finished handling request.");
    }
}