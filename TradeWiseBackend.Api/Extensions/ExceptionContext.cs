using Microsoft.AspNetCore.Http;

namespace TradeWiseBackend.Api.Extensions;

public class ExceptionContext
{
    public ExceptionContext(HttpContext httpContext, Exception exception)
    {
        HttpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }

    public HttpContext HttpContext { get; }

    public Exception Exception { get; }

    public bool Handled { get; set; } = false;
}