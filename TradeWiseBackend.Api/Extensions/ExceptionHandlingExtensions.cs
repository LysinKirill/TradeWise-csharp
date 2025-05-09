using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TradeWiseBackend.Api.Middlewares;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace TradeWiseBackend.Api.Extensions;
using HellangProblemDetails = Hellang.Middleware.ProblemDetails.ProblemDetailsExtensions;

public static class ExceptionHandlingExtensions
{
    public static IServiceCollection AddExceptionHandlingMiddleware(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddExceptionHandler<BadRequestExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        HellangProblemDetails.AddProblemDetails(services);
        services.Configure<Hellang.Middleware.ProblemDetails.ProblemDetailsOptions>(options =>
        {
            options.Map<ValidationException>(ex =>
                new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation Failed",
                    Detail = ex.Message
                });
            options.Map<RpcException>(ex =>
            {
                var statusCode = ex.StatusCode switch
                {
                    StatusCode.InvalidArgument => StatusCodes.Status400BadRequest,
                    StatusCode.NotFound => StatusCodes.Status404NotFound,
                    StatusCode.PermissionDenied => StatusCodes.Status403Forbidden,
                    _ => StatusCodes.Status500InternalServerError
                };
                return new StatusCodeProblemDetails(statusCode);
            });
        });

        return services;
    }
}