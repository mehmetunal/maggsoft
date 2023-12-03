namespace WebApplication2.Middleware
{
    public sealed record Response(object Result, bool Success, int StatusCode = StatusCodes.Status200OK, object? Messages = null, List<string>? ValidationMessages = null,
        bool IsError = default, object? SystemError = null);
}