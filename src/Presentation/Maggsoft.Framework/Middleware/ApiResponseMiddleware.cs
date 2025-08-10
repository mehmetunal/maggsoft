using Maggsoft.Core.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Maggsoft.Framework.Middleware.ApiResponseMiddleware;

[AttributeUsage(AttributeTargets.All)]
public class IgnoreResponseRewindMiddlewareAttribute : Attribute { }
public class IgnoreResponseOption
{
    public string[] IgnoreAcceptHeader { get; set; } = ["image/"];
    public bool UseCamelCase { get; set; } = false;
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
        if (context.GetEndpoint()?.Metadata.GetOrderedMetadata<IgnoreResponseRewindMiddlewareAttribute>().Count > 0)
            return true;

        // Image dosyaları kontrolü
        if (!string.IsNullOrEmpty(acceptHeaders) && acceptHeaders.Any(h => h.Contains("image/")))
            return true;

        // Custom ignore options kontrolü
        if (_options?.IgnoreAcceptHeader != null && !string.IsNullOrEmpty(acceptHeaders))
        {
            return _options.IgnoreAcceptHeader.Any(opt =>
                acceptHeaders.Any(h => h.Contains(opt)));
        }

        return false;
    }

    private Result<object> ParseResponse(string json, int statusCode)
    {
        try
        {
            // JSON'ı parse et
            using var jsonDocument = JsonDocument.Parse(json);
            var root = jsonDocument.RootElement;

            // Maggsoft Result formatında mı kontrol et
            if (IsMaggsoftResultFormat(root))
            {
                var result = JsonSerializer.Deserialize<Result<object>>(json, _jsonSettings);
                if (result != null)
                {
                    result.StatusCode = statusCode;
                    return result;
                }
            }

            // Normal response formatında ise Result<object>'e çevir
            return CreateResultFromResponse(json, statusCode);
        }
        catch
        {
            // Parse hatası durumunda basit bir Result oluştur
            return new Result<object>
            {
                IsSuccess = statusCode == StatusCodes.Status200OK,
                StatusCode = statusCode,
                Data = json,
                Message = "Response parse hatası"
            };
        }
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
        return root.TryGetProperty("timeStamp", out _) ||
               root.TryGetProperty("TimeStamp", out _) ||
               root.TryGetProperty("isSuccess", out _) ||
               root.TryGetProperty("IsSuccess", out _) ||
               root.TryGetProperty("statusCode", out _) ||
               root.TryGetProperty("StatusCode", out _);
    }

    private Result<object> CreateResultFromResponse(string json, int statusCode)
    {
        object data;
        try
        {
            data = JsonSerializer.Deserialize<object>(json, _jsonSettings) ?? json;
        }
        catch
        {
            data = json;
        }

        var majorVersion = configuration.GetSection("ApiVersion:MajorVersion")?.Value;
        var minorVersion = configuration.GetSection("ApiVersion:MinorVersion")?.Value;

        return new Result<object>
        {
            IsSuccess = statusCode == StatusCodes.Status200OK,
            StatusCode = statusCode,
            Data = data,
            ApiVersion = !string.IsNullOrEmpty(majorVersion) && !string.IsNullOrEmpty(minorVersion)
                ? $"{majorVersion}.{minorVersion}"
                : null
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

    private static void LogResponse(HttpContext context, string json)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]: {context.Request.Method} {context.Request.Path} - ContentLength: {context.Request.ContentLength}");
        Console.WriteLine($"Response: {json}");
        Console.ForegroundColor = ConsoleColor.White;
    }
}