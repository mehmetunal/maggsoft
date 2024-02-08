using Microsoft.AspNetCore.Diagnostics;

namespace WebApplication2.Middleware;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;
    private readonly IHostEnvironment _environment = environment;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, exception.Message, exception.InnerException, exception.InnerException?.Message);

        var errorMessage = _environment.IsDevelopment()
            ? exception.InnerException != null ? exception.InnerException.Message
            : exception.Message : exception.Message;

        if (exception is KeyNotFoundException)
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;


        var isError = httpContext.Response.StatusCode == StatusCodes.Status500InternalServerError;
        Response response = new(
            null,
            false,
            httpContext.Response.StatusCode,
            isError == false ? errorMessage : null,
            new List<string>(),
            isError,
            isError ? errorMessage : null
        );

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        
        return true;
    }
}
