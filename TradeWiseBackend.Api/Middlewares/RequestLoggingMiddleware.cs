using System;
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

        await _next(context);

        _logger.LogInformation("Finished handling request.");
    }
}