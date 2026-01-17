using Domain.Exception;

namespace API.Middleware;

public class ExceptionMiddleware(RequestDelegate next)
{

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (System.Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }

    }

    private async Task HandleExceptionAsync(HttpContext httpContext, System.Exception exception)
    {
        httpContext.Response.ContentType = "application/json";
        var statusCode = 500;
        object result;

        switch (exception)
        {
            case ValidationException validationEx:
                statusCode = 400;
                result = new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    title = "Validation Error",
                    errors = validationEx.Errors
                };
                break;

            case NotFoundException notFoundEx:
                statusCode = 404;
                result = new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    title = "Resource Not Found",
                    detail = notFoundEx.Message
                };
                break;
            default:
                result = new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    title = "Internal Server Error",
                    detail = exception.Message
                };
                break;
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(result);
    }
}
