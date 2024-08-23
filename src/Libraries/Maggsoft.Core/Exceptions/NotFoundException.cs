using Maggsoft.Core.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Maggsoft.Core.Exceptions;

[Serializable]
public class NotFoundException : Exception
{
    public NotFoundException()
        : base()
    { }

    public NotFoundException(string message)
        : base(message)

    { }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    { }

    protected NotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }

    /// <summary>
    /// ThrowIfNull
    /// </summary>
    /// <param name="argument">object argument</param>
    /// <exception cref="NotFoundException"></exception>
    protected static void ThrowIfNull(object argument)
    {
        if (argument.IsEmpty())
            throw new NotFoundException($"{nameof(argument)}");
    }
} 
