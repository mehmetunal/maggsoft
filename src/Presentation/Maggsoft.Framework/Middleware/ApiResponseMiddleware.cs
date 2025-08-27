using Maggsoft.Core.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Maggsoft.Framework.Middleware.ApiResponseMiddleware;

[AttributeUsage(AttributeTargets.All)]
public class IgnoreResponseRewindMiddlewareAttribute : Attribute { }
public class ApiResponseMessages
{
    public string ValidationFailed { get; set; } = "Validation failed. Please check your input data.";
    public string AnErrorOccurred { get; set; } = "An error occurred";
    public string MultipleErrorsOccurred { get; set; } = "Multiple errors occurred";
    public string ResponseProcessingError { get; set; } = "An error occurred while processing the response";
    public string JsonParseError { get; set; } = "JSON Parse Error: {0}";
    public string ResponseValidationFailed { get; set; } = "Response format validation failed";
    public string RequestProcessingError { get; set; } = "An error occurred while processing your request";
    public string ProblemDetailsParseError { get; set; } = "ProblemDetails Parse Error: {0}";
    public string ErrorDetailsProcessingFailed { get; set; } = "Error details processing failed";
    
    // HTTP Status Code Messages - User Friendly
    public string BadRequest { get; set; } = "Your request could not be processed";
    public string Unauthorized { get; set; } = "Authentication is required to access this resource";
    public string Forbidden { get; set; } = "You don't have permission to access this resource";
    public string NotFound { get; set; } = "The requested resource was not found";
    public string MethodNotAllowed { get; set; } = "This operation is not allowed";
    public string Conflict { get; set; } = "This request conflicts with the current state";
    public string UnprocessableEntity { get; set; } = "Please check your input data";
    public string TooManyRequests { get; set; } = "Too many requests. Please try again later";
    public string InternalServerError { get; set; } = "An internal error occurred. Please try again later";
    public string NotImplemented { get; set; } = "This feature is not available";
    public string BadGateway { get; set; } = "Service is temporarily unavailable";
    public string ServiceUnavailable { get; set; } = "Service is temporarily unavailable";
    public string GatewayTimeout { get; set; } = "The request timed out. Please try again";
    public string DefaultError { get; set; } = "An error occurred while processing your request";
    
    // HTTP Status Code Messages - Technical
    public string TechnicalBadRequest { get; set; } = "Bad Request - The request was invalid or malformed";
    public string TechnicalUnauthorized { get; set; } = "Unauthorized - Authentication required";
    public string TechnicalForbidden { get; set; } = "Forbidden - Access denied";
    public string TechnicalNotFound { get; set; } = "Not Found - The requested resource was not found";
    public string TechnicalMethodNotAllowed { get; set; } = "Method Not Allowed - HTTP method not supported";
    public string TechnicalConflict { get; set; } = "Conflict - Request conflicts with current state";
    public string TechnicalUnprocessableEntity { get; set; } = "Unprocessable Entity - Validation failed";
    public string TechnicalTooManyRequests { get; set; } = "Too Many Requests - Rate limit exceeded";
    public string TechnicalInternalServerError { get; set; } = "Internal Server Error - An unexpected error occurred";
    public string TechnicalNotImplemented { get; set; } = "Not Implemented - Feature not implemented";
    public string TechnicalBadGateway { get; set; } = "Bad Gateway - Invalid response from upstream server";
    public string TechnicalServiceUnavailable { get; set; } = "Service Unavailable - Service temporarily unavailable";
    public string TechnicalGatewayTimeout { get; set; } = "Gateway Timeout - Upstream server timeout";
    public string TechnicalDefaultError { get; set; } = "HTTP {0} - Request failed";
}

public class IgnoreResponseOption
{
    public string[] IgnoreAcceptHeader { get; set; } = ["image/"];
    public bool UseCamelCase { get; set; } = false;
    public ApiResponseMessages Messages { get; set; } = new();
}

/*
public sealed class ApiResponseMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private readonly RequestDelegate _next = next;
    private readonly IConfiguration _configuration = configuration;
    private readonly JsonSerializerOptions jsonSettings = new() { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = null, AllowTrailingCommas = true };
    private IgnoreResponseOption options;
    public async Task InvokeAsync(HttpContext context, IOptions<IgnoreResponseOption> options)
    {
        this.options = options?.Value;
        //image/
        context.Request.Headers.TryGetValue("Accept", out StringValues acceptHeaders);

        if (IgnoreResponse(context) || (!string.IsNullOrEmpty(acceptHeaders) && acceptHeaders.Any(predicate: c => c.Contains("image/"))))
        {
            await _next(context);
            return;
        }

        if (this.options != null && !string.IsNullOrEmpty(acceptHeaders))
        {
            bool flag = false;

            foreach (var opt in this.options.IgnoreAcceptHeader)
            {
                flag = acceptHeaders.Any(predicate: c => c.Contains(opt));

                if (flag)
                {
                    await _next(context);
                    return;
                }
            }
        }

        var originalBody = context.Response.Body;
        context.Response.ContentType = MediaTypeNames.Application.Json;

        var json = await FormatResponse(context);

        _ = json.TryParseJson(out Result<object> response);
        if (response == null)//lsit data
        {
            var majorVersionConfig = _configuration.GetSection("ApiVersion:MajorVersion")?.Value;
            var minorVersionConfig = _configuration.GetSection("ApiVersion:MinorVersion")?.Value;
            object jObjectData = json;
            try
            {
                jObjectData = JsonSerializer.Deserialize<object>(json, jsonSettings);
            }
            catch { }
            response = new()
            {
                IsSuccess = context.Response.StatusCode == StatusCodes.Status200OK,
                StatusCode = context.Response.StatusCode,
                Data = jObjectData,
                ApiVersion = majorVersionConfig != null && minorVersionConfig != null ? $"{majorVersionConfig}.{minorVersionConfig}" : null
            };
        }
        else if (response != null && response.Data != null
            && response.Data is not JsonElement && response.Message == null
            && (response.ValidationMessages == null || (response.ValidationMessages != null && response.ValidationMessages.Count() == 0)))//one data
        {
            response.Data = JsonSerializer.Deserialize<object>(json, jsonSettings);
            response.IsSuccess = context.Response.StatusCode == StatusCodes.Status200OK;
        }

        if (response.StatusCode == default)
        {
            response.StatusCode = StatusCodes.Status200OK;
        }

        context.Response.StatusCode = response.StatusCode;

        json = JsonSerializer.Serialize(response, jsonSettings);

        context.Response.ContentLength = response != null ? Encoding.UTF8.GetByteCount(json) : 0;

        await using var output = new MemoryStream(Encoding.UTF8.GetBytes(json));
        await output.CopyToAsync(originalBody);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}]:{context.Request.Method}-{context.Request.Path}-size limit violation : {context.Request.ContentLength}");
        Console.WriteLine($"Response : {json}");

        Console.ForegroundColor = ConsoleColor.White;

        context.Response.Body = originalBody;
    }
    private async Task<string> FormatResponse(HttpContext context)
    {
        string responseBody;
        await using (var memStream = new MemoryStream())
        {
            context.Response.Body = memStream;
            await _next(context);
            memStream.Position = 0;
            responseBody = await new StreamReader(memStream).ReadToEndAsync();
        }

        return responseBody;
    }


    /// <summary>
    /// IgnoreResponseRewindMiddlewareAttribute
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private static bool IgnoreResponse(HttpContext context)
    {
        return context.GetEndpoint() != null && context.GetEndpoint().Metadata.GetOrderedMetadata<IgnoreResponseRewindMiddlewareAttribute>().Count > 0;
    }
}*/

public sealed class ApiResponseMiddleware(RequestDelegate next, IConfiguration configuration,
    IHostEnvironment environment,
    IOptions<IgnoreResponseOption>? options = null)
{
    private readonly JsonSerializerOptions _jsonSettings = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = options?.Value?.UseCamelCase == true ? JsonNamingPolicy.CamelCase : null,
        AllowTrailingCommas = true
    };
    private readonly IgnoreResponseOption? _options = options?.Value;

    public async Task InvokeAsync(HttpContext context)
    {
        // Accept header'ını kontrol et
        context.Request.Headers.TryGetValue("Accept", out var acceptHeaders);

        // Image dosyaları ve ignore edilecek response'ları kontrol et
        if (ShouldIgnoreResponse(context, acceptHeaders))
        {
            await next(context);
            return;
        }

        // Response body'yi güvenli şekilde yönet
        var originalBody = context.Response.Body;
        try
        {
            context.Response.ContentType = MediaTypeNames.Application.Json;

            var json = await FormatResponse(context);

            // JSON'ı parse et
            var response = ParseResponse(json, context.Response.StatusCode);

            // Response'u serialize et ve gönder
            var serializedJson = JsonSerializer.Serialize(response, _jsonSettings);
            context.Response.ContentLength = Encoding.UTF8.GetByteCount(serializedJson);

            await using var output = new MemoryStream(Encoding.UTF8.GetBytes(serializedJson));
            await output.CopyToAsync(originalBody);

            // Log response
            LogResponse(context, serializedJson);
        }
        finally
        {
            // Her durumda original body'yi geri yükle
            context.Response.Body = originalBody;
        }
    }

    private bool ShouldIgnoreResponse(HttpContext context, StringValues acceptHeaders)
    {
        // IgnoreResponseRewindMiddlewareAttribute kontrolü
        var endpoint = context.GetEndpoint();
        if (endpoint != null && endpoint.Metadata.GetOrderedMetadata<IgnoreResponseRewindMiddlewareAttribute>().Count > 0)
            return true;

        // Image dosyaları kontrolü
        if (!string.IsNullOrEmpty(acceptHeaders) && acceptHeaders.Any(h => h != null && h.Contains("image/")))
            return true;

        // Custom ignore options kontrolü
        if (_options?.IgnoreAcceptHeader != null && !string.IsNullOrEmpty(acceptHeaders))
        {
            return _options.IgnoreAcceptHeader.Any(opt =>
                opt != null && acceptHeaders.Any(h => h != null && h.Contains(opt)));
        }

        return false;
    }

    private Result<object> ParseResponse(string json, int statusCode)
    {
        try
        {
            // Boş yanıt kontrolü
            if (string.IsNullOrWhiteSpace(json))
            {
                return CreateEmptyResponse(statusCode);
            }
            
            // JSON'ı parse et
            using var jsonDocument = JsonDocument.Parse(json);
            var root = jsonDocument.RootElement;

            // ProblemDetails formatında mı kontrol et
            if (IsProblemDetailsFormat(root))
            {
                // ProblemDetails'i Result formatına dönüştür
                return CreateResultFromProblemDetails(json, statusCode);
            }

            // Maggsoft Result formatında mı kontrol et
            if (IsMaggsoftResultFormat(root))
            {
                var result = JsonSerializer.Deserialize<Result<object>>(json, _jsonSettings);
                if (result != null)
                {
                    // Eğer Errors dolu ama Message boş ise, Message'ı doldur
                    if (!result.IsSuccess && 
                        string.IsNullOrEmpty(result.Message) && 
                        result.Errors != null && 
                        result.Errors.Count > 0)
                    {
                        // Validation hatalarını tespit et
                        bool isValidationError = IsValidationError(result.Errors, statusCode);
                        
                        if (isValidationError)
                        {
                            result.Message = _options?.Messages?.ValidationFailed ?? "Validation failed. Please check your input data.";
                        }
                        else
                        {
                            result.Message = result.Errors.Count == 1 
                                ? (_options?.Messages?.AnErrorOccurred ?? "An error occurred") 
                                : (_options?.Messages?.MultipleErrorsOccurred ?? "Multiple errors occurred");
                        }
                    }
                    
                    return result;
                }
            }

            // Normal response formatında ise Result<object>'e çevir
            return CreateResultFromResponse(json, statusCode);
        }
        catch (Exception ex)
        {
            // Parse hatası durumında basit bir Result oluştur
            var isSuccess = statusCode is >= 200 and < 300;
            var userMessage = _options?.Messages?.ResponseProcessingError ?? "An error occurred while processing the response";
            var technicalError = environment.IsDevelopment() 
                ? string.Format(_options?.Messages?.JsonParseError ?? "JSON Parse Error: {0}", ex.Message)
                : (_options?.Messages?.ResponseValidationFailed ?? "Response format validation failed");
                
            return new Result<object>
            {
                IsSuccess = isSuccess,
                Data = isSuccess ? json : null, // Hata durumlarında her zaman null
                Message = isSuccess ? string.Empty : userMessage,
                Errors = isSuccess ? [] : [technicalError]
            };
        }
    }
    
    /// <summary>
    /// Boş yanıt için standart bir Result oluşturur
    /// </summary>
    private Result<object> CreateEmptyResponse(int statusCode)
    {
        var isSuccess = statusCode is >= 200 and < 300;
        return new Result<object>
        {
            IsSuccess = isSuccess,
            Data = isSuccess ? new object() : null, // Başarılıysa empty object, hatalıysa null
            Message = isSuccess ? string.Empty : GetUserFriendlyMessage(statusCode),
            Errors = isSuccess ? [] : [GetStatusCodeMessage(statusCode)]
        };
    }

    private bool IsMaggsoftResultFormat(JsonElement root)
    {
        // Array ise Maggsoft Result formatında değildir
        if (root.ValueKind == JsonValueKind.Array)
            return false;

        // Object değilse Maggsoft Result formatında değildir
        if (root.ValueKind != JsonValueKind.Object)
            return false;

        // Maggsoft Result formatının özelliklerini kontrol et
        return root.TryGetProperty("isSuccess", out _) ||
               root.TryGetProperty("IsSuccess", out _) ||
               root.TryGetProperty("message", out _) ||
               root.TryGetProperty("Message", out _) ||
               root.TryGetProperty("errors", out _) ||
               root.TryGetProperty("Errors", out _);
    }
    
    /// <summary>
    /// ProblemDetails formatında olup olmadığını kontrol eder
    /// </summary>
    private bool IsProblemDetailsFormat(JsonElement root)
    {
        // Array ise ProblemDetails formatında değildir
        if (root.ValueKind == JsonValueKind.Array)
            return false;

        // Object değilse ProblemDetails formatında değildir
        if (root.ValueKind != JsonValueKind.Object)
            return false;

        // ValidationProblemDetails formatını kontrol et (errors property'si var mı?)
        if (root.TryGetProperty("errors", out _) && 
            root.TryGetProperty("type", out _) && 
            root.TryGetProperty("title", out _) && 
            root.TryGetProperty("status", out _))
        {
            return true;
        }

        // ProblemDetails formatının özelliklerini kontrol et
        // RFC 7807 standardına göre ProblemDetails formatı
        return root.TryGetProperty("type", out _) &&
               root.TryGetProperty("title", out _) &&
               root.TryGetProperty("status", out _) &&
               root.TryGetProperty("detail", out _);
    }
    
    /// <summary>
    /// ProblemDetails formatındaki yanıtı Result formatına dönüştürür
    /// </summary>
    private Result<object> CreateResultFromProblemDetails(string json, int statusCode)
    {
        try
        {
            using var jsonDocument = JsonDocument.Parse(json);
            var root = jsonDocument.RootElement;
            
            // ProblemDetails alanlarını al
            string? title = null;
            string? detail = null;
            string? type = null;
            object? exception = null;
            object? errors = null;
            
            if (root.TryGetProperty("title", out var titleElement))
                title = titleElement.GetString();
                
            if (root.TryGetProperty("detail", out var detailElement))
                detail = detailElement.GetString();
                
            if (root.TryGetProperty("type", out var typeElement))
                type = typeElement.GetString();
                
            // ValidationProblemDetails için errors alanını al
            if (root.TryGetProperty("errors", out var errorsElement))
            {
                errors = JsonSerializer.Deserialize<Dictionary<string, string[]>>(errorsElement.GetRawText(), _jsonSettings);
                
                // Validation mesajlarını birleştirerek message oluştur
                if (errors is Dictionary<string, string[]> errorDict && errorDict.Count > 0)
                {
                    var validationMessages = 
                        (from error in errorDict 
                            from message in error.Value select $"{error.Key}: {message}")
                        .ToList();

                    // Validation mesajlarını detail olarak kullan
                    if (validationMessages.Count > 0)
                    {
                        detail = string.Join(", ", validationMessages);
                    }
                }
            }
                
            // Exception bilgisi varsa al
            if (root.TryGetProperty("exception", out var exceptionElement))
                exception = JsonSerializer.Deserialize<object>(exceptionElement.GetRawText(), _jsonSettings);
            
            // Result oluştur
            var validationErrors = errors != null ? ExtractValidationMessages(errors) : [];
            var userMessage = validationErrors.Count > 0 
                ? (_options?.Messages?.ValidationFailed ?? "Validation failed. Please check your input data.")
                : (!string.IsNullOrEmpty(detail) ? detail : title);
                
            var result = new Result<object>
            {
                IsSuccess = false,
                Message = userMessage,
                Errors = validationErrors.Count > 0 ? validationErrors : [!string.IsNullOrEmpty(detail) ? detail : title ?? "An error occurred"],
                Data = null // Hata durumlarında Data her zaman null olmalı
            };

            // Hata durumlarında Data her zaman null kalır
            // Exception detayları sadece loglarda görünür
            
            return result;
        }
        catch (Exception ex)
        {
            // ProblemDetails parse edilemezse sadece hata mesajı döner
            var userMessage = _options?.Messages?.RequestProcessingError ?? "An error occurred while processing your request";
            var technicalError = environment.IsDevelopment() 
                ? string.Format(_options?.Messages?.ProblemDetailsParseError ?? "ProblemDetails Parse Error: {0}", ex.Message)
                : (_options?.Messages?.ErrorDetailsProcessingFailed ?? "Error details processing failed");
                
            return new Result<object>
            {
                IsSuccess = false,
                Data = null, // Hata durumlarında her zaman null
                Message = userMessage,
                Errors = [technicalError]
            };
        }
    }
    
    /// <summary>
    /// Validation errors'dan mesajları çıkarır
    /// </summary>
    private List<string> ExtractValidationMessages(object errors)
    {
        var messages = new List<string>();
        
        if (errors is Dictionary<string, string[]> errorDict)
        {
            messages.AddRange(from error in errorDict 
                from message in error.Value select $"{error.Key}: {message}");
        }
        
        return messages;
    }

    private Result<object> CreateResultFromResponse(string json, int statusCode)
    {
        var isSuccess = statusCode is >= 200 and < 300;
        object? data = null;
        
        if (isSuccess)
        {
            try
            {
                data = JsonSerializer.Deserialize<object>(json, _jsonSettings) ?? json;
            }
            catch
            {
                data = json;
            }
        }

        return new Result<object>
        {
            IsSuccess = isSuccess,
            Data = data, // Başarılıysa deserialize edilmiş data, hatalıysa null
            Message = string.Empty,
            Errors = []
        };
    }

    private async Task<string> FormatResponse(HttpContext context)
    {
        await using var memStream = new MemoryStream();
        context.Response.Body = memStream;

        await next(context);

        memStream.Position = 0;
        return await new StreamReader(memStream).ReadToEndAsync();
    }

    /// <summary>
    /// Validation hatası olup olmadığını kontrol eder
    /// </summary>
    private static bool IsValidationError(List<string> errors, int statusCode)
    {
        // HTTP 400 (Bad Request) veya 422 (Unprocessable Entity) validation hataları için kullanılır
        if (statusCode != 400 && statusCode != 422)
            return false;

        // Validation hata kalıplarını kontrol et
        foreach (var error in errors)
        {
            var lowerError = error.ToLower();
            
            // Validation anahtar kelimeleri
            if (lowerError.Contains("validation") ||
                lowerError.Contains("required") ||
                lowerError.Contains("invalid") ||
                lowerError.Contains("must be") ||
                lowerError.Contains("should be") ||
                lowerError.Contains("cannot be") ||
                lowerError.Contains("length") ||
                lowerError.Contains("format") ||
                lowerError.Contains("range") ||
                lowerError.Contains("field") ||
                lowerError.Contains("email") ||
                lowerError.Contains("password") ||
                lowerError.Contains("gerekli") ||
                lowerError.Contains("geçersiz") ||
                lowerError.Contains("olmalı") ||
                lowerError.Contains("uzunluk") ||
                lowerError.Contains("karakter") ||
                lowerError.Contains(":")) // Field: Error formatı (Email: Email gerekli)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// HTTP status koduna göre kullanıcı dostu mesaj döner
    /// </summary>
    private string GetUserFriendlyMessage(int statusCode)
    {
        var messages = _options?.Messages;
        return statusCode switch
        {
            400 => messages?.BadRequest ?? "Your request could not be processed",
            401 => messages?.Unauthorized ?? "Authentication is required to access this resource",
            403 => messages?.Forbidden ?? "You don't have permission to access this resource", 
            404 => messages?.NotFound ?? "The requested resource was not found",
            405 => messages?.MethodNotAllowed ?? "This operation is not allowed",
            409 => messages?.Conflict ?? "This request conflicts with the current state",
            422 => messages?.UnprocessableEntity ?? "Please check your input data",
            429 => messages?.TooManyRequests ?? "Too many requests. Please try again later",
            500 => messages?.InternalServerError ?? "An internal error occurred. Please try again later",
            501 => messages?.NotImplemented ?? "This feature is not available",
            502 => messages?.BadGateway ?? "Service is temporarily unavailable",
            503 => messages?.ServiceUnavailable ?? "Service is temporarily unavailable",
            504 => messages?.GatewayTimeout ?? "The request timed out. Please try again",
            _ => messages?.DefaultError ?? "An error occurred while processing your request"
        };
    }

    /// <summary>
    /// HTTP status koduna göre teknik açıklayıcı mesaj döner
    /// </summary>
    private string GetStatusCodeMessage(int statusCode)
    {
        var messages = _options?.Messages;
        return statusCode switch
        {
            400 => messages?.TechnicalBadRequest ?? "Bad Request - The request was invalid or malformed",
            401 => messages?.TechnicalUnauthorized ?? "Unauthorized - Authentication required",
            403 => messages?.TechnicalForbidden ?? "Forbidden - Access denied", 
            404 => messages?.TechnicalNotFound ?? "Not Found - The requested resource was not found",
            405 => messages?.TechnicalMethodNotAllowed ?? "Method Not Allowed - HTTP method not supported",
            409 => messages?.TechnicalConflict ?? "Conflict - Request conflicts with current state",
            422 => messages?.TechnicalUnprocessableEntity ?? "Unprocessable Entity - Validation failed",
            429 => messages?.TechnicalTooManyRequests ?? "Too Many Requests - Rate limit exceeded",
            500 => messages?.TechnicalInternalServerError ?? "Internal Server Error - An unexpected error occurred",
            501 => messages?.TechnicalNotImplemented ?? "Not Implemented - Feature not implemented",
            502 => messages?.TechnicalBadGateway ?? "Invalid response from upstream server",
            503 => messages?.TechnicalServiceUnavailable ?? "Service temporarily unavailable",
            504 => messages?.TechnicalGatewayTimeout ?? "Upstream server timeout",
            _ => string.Format(messages?.TechnicalDefaultError ?? "HTTP {0} - Request failed", statusCode)
        };
    }

    private static void LogResponse(HttpContext context, string json)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]: {context.Request.Method} {context.Request.Path} - ContentLength: {context.Request.ContentLength}");
        Console.WriteLine($"Response: {json}");
        Console.ForegroundColor = ConsoleColor.White;
    }
}
