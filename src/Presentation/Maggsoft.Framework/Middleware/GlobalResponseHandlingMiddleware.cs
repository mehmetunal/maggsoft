using Microsoft.AspNetCore.Http;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Maggsoft.Core.Extensions;
using Maggsoft.Core.Base;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System;
using Azure.Core;

namespace Maggsoft.Framework.Middleware;
internal sealed class GlobalResponseHandlingMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private readonly RequestDelegate _next = next;
    private readonly IConfiguration _configuration = configuration;
    private readonly JsonSerializerOptions jsonSettings = new() { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = null, AllowTrailingCommas = true };
    public async Task InvokeAsync(HttpContext context)
    {
        var originalBody = context.Response.Body;
        context.Response.ContentType = MediaTypeNames.Application.Json;

        var json = await FormatResponse(context);

        _ = json.TryParseJson(out Response<object> response);
        if (response == null)
        {
            var majorVersionConfig = _configuration.GetSection("ApiVersion:MajorVersion")?.Value;
            var minorVersionConfig = _configuration.GetSection("ApiVersion:MinorVersion")?.Value;

            response = new()
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Result = JsonSerializer.Deserialize<object>(json, jsonSettings),
                ApiVersion = $"{majorVersionConfig}.{minorVersionConfig}"
            };

            context.Response.StatusCode = response.StatusCode;
        }

        json = JsonSerializer.Serialize(response, jsonSettings);

        context.Response.ContentLength = response != null ? Encoding.UTF8.GetByteCount(json) : 0;

        await using var output = new MemoryStream(Encoding.UTF8.GetBytes(json));
        await output.CopyToAsync(originalBody);
        
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
}
