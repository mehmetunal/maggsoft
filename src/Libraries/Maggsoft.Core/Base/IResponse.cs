using System.Collections.Generic;

namespace Maggsoft.Core.Base
{
    public interface IResponse
    {
        string Messages { get; set; }
        List<string> ValidationMessages { get; set; }
        int StatusCode { get; set; }
        bool Success { get; set; }
        bool IsError { get; }
    }
}