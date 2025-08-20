using Maggsoft.Core.Model;
using System.Collections.Generic;

namespace Maggsoft.Core.Base;

public interface IResult  
{
    string Message { get; set; }
    List<string> Errors { get; set; }
    bool IsSuccess { get; set; }
    
}