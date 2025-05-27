using System.ComponentModel.DataAnnotations;
using Grpc.Core;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TradeWiseBackend.Api.Middlewares;
using ProblemDetailsOptions = Hellang.Middleware.ProblemDetails.ProblemDetailsOptions;

namespace TradeWiseBackend.Api.Extensions;

using HellangProblemDetails = ProblemDetailsExtensions;

public static class ExceptionHandlingExtensions
{
    public static IServiceCollection AddExceptionHandlingMiddleware(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddExceptionHandler<BadRequestExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        HellangProblemDetails.AddProblemDetails(services);
        services.Configure<ProblemDetailsOptions>(options =>
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