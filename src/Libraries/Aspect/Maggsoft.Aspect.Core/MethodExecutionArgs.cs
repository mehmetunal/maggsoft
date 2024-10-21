using System;
using System.Reflection;

namespace Maggsoft.Aspect.Core;

public class MethodExecutionArgs
{ 
    public MethodExecutionArgs()
    {  
    }

    public MethodExecutionArgs( 
        MethodInfo method,
        object[] arguments)
    {
        Method = method;
        Arguments = arguments;
    }

    /// <summary>
    /// Method info
    /// </summary>
    public MethodInfo Method { get; }

    /// <summary>
    /// Method arguments
    /// </summary>
    public object[] Arguments { get; }

    /// <summary>
    /// Error occurring within the method.
    /// </summary>
    public Exception Exception { get; internal set; }

    /// <summary>
    /// Method result
    /// </summary>
    public object ReturnValue { get; set; }
}