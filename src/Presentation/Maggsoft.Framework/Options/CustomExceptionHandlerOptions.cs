using System;

namespace Maggsoft.Framework.Options;

public class CustomExceptionHandlerOptions
{
    public bool IncludeExceptionDetails { get; set; }
    public string ResponseContentType { get; set; } = "application/json";
    public int ResponseStatusCode { get; set; } = 500;
    public Func<Exception, (int StatusCode, string Title)>? CustomExceptionMapping { get; set; }

}