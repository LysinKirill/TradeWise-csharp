using System;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TradeWiseBackend.Api.Middlewares;
using Microsoft.AspNetCore.Http;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Diagnostics;

namespace TradeWiseBackend.Api.Extensions;

public static class ExceptionHandlingExtensions
{
    public static IServiceCollection AddExceptionHandlingMiddleware(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddExceptionHandler<BadRequestExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddProblemDetails(options =>
        {
            options.Map<RpcException>(ex =>
            {
                var statusCode = ex.StatusCode switch
                {
                    StatusCode.InvalidArgument => StatusCodes.Status400BadRequest,
                    StatusCode.NotFound => StatusCodes.Status404NotFound,
                    StatusCode.PermissionDenied => StatusCodes.Status403Forbidden,
                    _ => StatusCodes.Status500InternalServerError,
                };
                return new StatusCodeProblemDetails(statusCode);
            });
        });

        return services;
    }
}
