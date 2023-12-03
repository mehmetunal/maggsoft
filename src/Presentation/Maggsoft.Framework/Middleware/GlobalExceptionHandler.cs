using Maggsoft.Core.Base;
using Maggsoft.Core.Exceptions;
using Maggsoft.Framework.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Maggsoft.Framework.Middleware;
internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment, IConfiguration configuration) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;
    private readonly IHostEnvironment _environment = environment;
    private readonly IConfiguration _configuration = configuration;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            exception.Message,
            exception.InnerException,
            exception.InnerException?.Message);

        Response<object> response = new();

        if (exception is ArgumentException || exception is ArgumentNullException)
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        else if (exception is ModelStateException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            if (!string.IsNullOrEmpty(exception.Message))
            {
                try
                {
                    foreach (var item in JsonSerializer.Deserialize<List<string>>(exception.Message))
                        response.ValidationMessages.Add(item);
                }
                catch
                {
                    response.ValidationMessages.Add(exception.Message);
                }
            }
        }
        else if (exception is ApiVersioningException)
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        else if (exception is NotFoundException || exception is KeyNotFoundException ||exception is NotFoundException)
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        else if (exception is Exception || exception is FileLoadException || exception is MaggsoftException)
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        else if (exception is ForbiddenExtension)
            httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        else if (exception is UnauthorizedAccessException)
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;

        var isError = httpContext.Response.StatusCode == StatusCodes.Status500InternalServerError;

        var errorMessage = _environment.IsDevelopment()
            ? exception.InnerException != null ? exception.InnerException.Message
            : exception.Message : exception.Message;

        var majorVersionConfig = _configuration.GetSection("ApiVersion:MajorVersion")?.Value;
        var minorVersionConfig = _configuration.GetSection("ApiVersion:MinorVersion")?.Value;

        response.ApiVersion = $"{majorVersionConfig}.{minorVersionConfig}";
        response.StatusCode = httpContext.Response.StatusCode;
        response.Messages = isError == false ? errorMessage : null;
        response.SystemError = isError ? errorMessage : null;

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}