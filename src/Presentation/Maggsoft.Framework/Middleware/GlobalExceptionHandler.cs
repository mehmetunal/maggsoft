using Maggsoft.Core.Base;
using Maggsoft.Core.Exceptions;
using Maggsoft.Core.Extensions;
using Maggsoft.Core.Model;
using Maggsoft.Framework.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Maggsoft.Framework.Middleware;
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment,
    IConfiguration configuration) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(
            exception,
            exception.Message,
            exception.InnerException,
            exception.InnerException?.Message);

        var response = new Result<object>();
        httpContext.Response.StatusCode = GetStatusCodeForException(exception);

        if (exception is ModelStateException modelStateException)
        {
            response.ValidationMessages = GetValidationMessages(modelStateException);
        }
        else if (response.ValidationMessages.Count == 0)
        {
            response.Message = GetErrorMessage(exception);
        }

        response.ApiVersion = GetApiVersion();
        response.StatusCode = httpContext.Response.StatusCode;
        response.IsSuccess = false;

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }

    private int GetStatusCodeForException(Exception exception)
    {
        return exception switch
        {
            ArgumentException or ArgumentNullException => StatusCodes.Status404NotFound,
            ModelStateException => StatusCodes.Status200OK,
            ApiVersioningException => StatusCodes.Status400BadRequest,
            NotFoundException or KeyNotFoundException => StatusCodes.Status404NotFound,
            ForbiddenExtension => StatusCodes.Status403Forbidden,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            FileLoadException or MaggsoftException => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private List<string> GetValidationMessages(ModelStateException exception)
    {
        var messages = new List<string>();
        if (!string.IsNullOrEmpty(exception.Message))
        {
            try
            {
                var deserializedMessages = JsonSerializer.Deserialize<List<string>>(exception.Message);
                if (deserializedMessages != null)
                {
                    messages.AddRange(deserializedMessages);
                }
            }
            catch
            {
                messages.Add(exception.Message);
            }
        }
        return messages;
    }

    private object GetErrorMessage(Exception exception)
    {
        var errorMessage = environment.IsDevelopment()
            ? exception.InnerException?.Message ?? exception.Message
            : exception.Message;

        if (errorMessage.TryParseJson(out Error error))
        {
            return error;
        }
        return errorMessage;
    }

    private string GetApiVersion()
    {
        var majorVersion = configuration["ApiVersion:MajorVersion"];
        var minorVersion = configuration["ApiVersion:MinorVersion"];
        return $"{majorVersion}.{minorVersion}";
    }
}

/*Old
 public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment, IConfiguration configuration) : IExceptionHandler
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

        Result<object> response = new();

        if (exception is ArgumentException || exception is ArgumentNullException)
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        else if (exception is ModelStateException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status200OK;
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
        else if (exception is NotFoundException || exception is KeyNotFoundException || exception is NotFoundException)
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        else if (exception is ForbiddenExtension)
            httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        else if (exception is UnauthorizedAccessException)
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        else if (exception is Exception || exception is FileLoadException || exception is MaggsoftException)
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        else
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        if (response.ValidationMessages.Count == 0)
        {
            var errorMessage = _environment.IsDevelopment()
                ? exception.InnerException != null ? exception.InnerException.Message
                : exception.Message : exception.Message;

            if (errorMessage.TryParseJson(out Error error))
            {
                response.Message = error;
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
            else
                response.Message = errorMessage;
        }

        var majorVersionConfig = _configuration.GetSection("ApiVersion:MajorVersion")?.Value;
        var minorVersionConfig = _configuration.GetSection("ApiVersion:MinorVersion")?.Value;

        response.ApiVersion = $"{majorVersionConfig}.{minorVersionConfig}";
        response.StatusCode = httpContext.Response.StatusCode;
        response.IsSuccess = false;

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
 
 */