using Maggsoft.Core.Extensions;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace WebApplication2.Middleware;

public class GlobalResponseHandlingMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var originalBody = context.Response.Body;
        context.Response.ContentType = MediaTypeNames.Application.Json;

        var json = await FormatResponse(context);

        _ = json.TryParseJson(out Response response);
        if (response == null)
        {
            response = new(JsonSerializer.Deserialize<object>(json), true, context.Response.StatusCode, null, []);
            json = JsonSerializer.Serialize(response);
        }

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