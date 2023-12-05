using Maggsoft.Core.Model;
using System.Collections.Generic;

namespace Maggsoft.Core.Base;

public interface IResult
{
    object Message { get; set; }
    List<string> ValidationMessages { get; set; }
    int StatusCode { get; set; }
    bool IsSuccess { get; set; }
    bool IsFailure { get; }
}