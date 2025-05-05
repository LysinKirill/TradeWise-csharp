using System;
using Grpc.Net.Client;

namespace TradeWiseBackend.Domain.ServiceModels;

public class GrpcCallResult
{
    public bool IsSuccess { get; set; }
    public Grpc.Core.StatusCode? StatusCode { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    public static GrpcCallResult Success() => new GrpcCallResult
    {
        IsSuccess = true,
    };
    public static GrpcCallResult Fail(Grpc.Core.StatusCode code, string message) => new GrpcCallResult
    {
        IsSuccess = false,
        StatusCode = code,
        ErrorMessage = message
    };
}

