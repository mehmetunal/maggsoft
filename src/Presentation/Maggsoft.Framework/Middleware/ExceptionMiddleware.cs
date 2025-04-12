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
public sealed class ExceptionMiddleware(
    ILogger<ExceptionMiddleware> logger,
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


/*
 public class ApiResponseMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiResponseMiddleware> _logger;

    public ApiResponseMiddleware(RequestDelegate next, ILogger<ApiResponseMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;

        try
        {
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            // Continue down the pipeline
            await _next(context);

            // Skip if response is already a Result type or if it's not a JSON response
            if (ShouldSkipWrapping(context))
            {
                await CopyToOriginalStream(memoryStream, originalBodyStream);
                return;
            }

            // Read the response
            memoryStream.Position = 0;
            var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

            // Create the wrapped response
            var wrappedResponse = CreateWrappedResponse(context, responseBody);

            // Write the wrapped response
            context.Response.Body = originalBodyStream;
            await WriteResponseAsync(context, wrappedResponse);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private static readonly HashSet<string> SkippedPaths = ["/swagger", "/healthcheck"];
    private static EventId ex;

    private static bool ShouldSkipWrapping(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint != null)
        {
            var controllerActionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
            if (controllerActionDescriptor != null)
            {
                // Check method return type
                var returnType = controllerActionDescriptor.MethodInfo.ReturnType;
                
                // Handle Task<T>
                if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    returnType = returnType.GetGenericArguments()[0];
                }

                // Check if return type is Result or Result<T>
                if (returnType == typeof(Result) || 
                    (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Result<>)))
                {
                    return true;
                }

                // Check for SkipApiResponse attribute
                if (controllerActionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(SkipApiResponseAttribute), true).Length != 0 ||
                    controllerActionDescriptor.MethodInfo.GetCustomAttributes(typeof(SkipApiResponseAttribute), true).Length != 0)
                {
                    return true;
                }
            }
        }

        var contentType = context.Response.ContentType?.ToLower();
        
        // Skip if content type is null
        if (string.IsNullOrEmpty(contentType))
            return true;

        // Skip for non-JSON content types
        if (!contentType.Contains("application/json"))
        {
            // Common file formats to skip
            var skipContentTypes = new[]
            {
                "application/pdf",
                "application/msword",
                "application/vnd.ms-excel",
                "application/vnd.openxmlformats-officedocument",
                "application/zip",
                "application/x-rar-compressed",
                "application/octet-stream",
                "image/jpeg",
                "image/png",
                "image/gif",
                "image/bmp",
                "image/webp",
                "image/svg+xml",
                "audio/",
                "video/",
                "text/csv",
                "text/plain"
            };

            if (skipContentTypes.Any(t => contentType.Contains(t)))
                return true;
        }


        // Skip for file download responses
        if (context.Response.Headers.ContainsKey("Content-Disposition") &&
            context.Response.Headers["Content-Disposition"].ToString().Contains("attachment")) 
            return true;

        // Skip for specific paths
        var path = context.Request.Path.Value?.ToLower();
        if (path != null && SkippedPaths.Any(path.Contains)) 
            return true;

        return false;
    }

    private static async Task CopyToOriginalStream(MemoryStream memoryStream, Stream originalBodyStream)
    {
        memoryStream.Position = 0;
        await memoryStream.CopyToAsync(originalBodyStream);
    }

    private static object CreateWrappedResponse(HttpContext context, string responseBody)
    {
        var statusCode = context.Response.StatusCode;
        var isSuccess = statusCode >= 200 && statusCode < 400;

        // Try to deserialize the response body
        object? data = null;
        if (!string.IsNullOrEmpty(responseBody))
        {
            try
            {
                data = JsonSerializer.Deserialize<object>(responseBody);
            }
            catch
            {
                // If deserialization fails, use the raw response
                data = responseBody;
            }
        }

        // Create appropriate Result object
        if (isSuccess)
        {
            return new Result<object>
            {
                IsSuccess = true,
                Data = data,
                Message = GetDefaultSuccessMessage(statusCode),
                StatusCode = (System.Net.HttpStatusCode)statusCode
            };
        }

        return new Result
        {
            IsSuccess = false,
            Message = GetDefaultErrorMessage(statusCode),
            StatusCode = (System.Net.HttpStatusCode)statusCode
        };
    }

    private static async Task WriteResponseAsync(HttpContext context, object response)
    {
        context.Response.ContentType = "application/json";

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }

    private static string GetDefaultSuccessMessage(int statusCode) => statusCode switch
    {
        200 => "İşlem başarıyla tamamlandı",
        201 => "Kayıt başarıyla oluşturuldu",
        204 => "İşlem başarılı, içerik yok",
        _ => "İşlem başarılı"
    };

    private static string GetDefaultErrorMessage(int statusCode) => statusCode switch
    {
        400 => "Geçersiz istek",
        401 => "Yetkisiz erişim",
        403 => "Erişim reddedildi",
        404 => "Kaynak bulunamadı",
        409 => "İşlem çakışması",
        422 => "İşlenemeyen varlık",
        429 => "Çok fazla istek",
        500 => "Sunucu hatası",
        502 => "Geçersiz ağ geçidi",
        503 => "Servis kullanılamıyor",
        504 => "Ağ geçidi zaman aşımı",
        _ => "Bir hata oluştu"
    };
}
*/