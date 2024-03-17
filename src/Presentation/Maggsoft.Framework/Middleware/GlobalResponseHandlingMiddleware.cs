using Maggsoft.Core.Base;
using Maggsoft.Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Maggsoft.Framework.Middleware;

[AttributeUsage(AttributeTargets.All)]
public class IgnoreResponseRewindMiddlewareAttribute : Attribute { }

public sealed class GlobalResponseHandlingMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private readonly RequestDelegate _next = next;
    private readonly IConfiguration _configuration = configuration;
    private readonly JsonSerializerOptions jsonSettings = new() { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = null, AllowTrailingCommas = true };
    public async Task InvokeAsync(HttpContext context)
    {
        if (IgnoreResponse(context))
        {
            await _next(context);
            return;
        }

        var originalBody = context.Response.Body;
        context.Response.ContentType = MediaTypeNames.Application.Json;

        var json = await FormatResponse(context);

        _ = json.TryParseJson(out Result<object> response);
        _ = json.TryParseJson(out ProblemDetails problemDetails);
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

        if (problemDetails != null && problemDetails.Status != null)//results.problem error
        {
            response.Message = problemDetails.Detail;
            response.StatusCode = problemDetails.Status.Value;
        }
        else if (response != null && response.Data != null
            && response.Data is not JsonElement && response.Message == null
            && (response.ValidationMessages == null || (response.ValidationMessages != null && response.ValidationMessages.Count() == 0)) &&
            (problemDetails != null && problemDetails.Status == null))//one data
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
}
