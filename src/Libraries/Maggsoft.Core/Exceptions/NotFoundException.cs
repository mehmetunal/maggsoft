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
    /// <param name="argument">argument</param>
    /// <param name="format">format</param>
    /// <exception cref="NotFoundException"></exception>
    public static void ThrowIfNull(object argument, string format = "{0}")
    {
        if (argument.IsEmpty())
            Throw(string.Format(format, nameof(argument)));
    }

    [DoesNotReturn]
    internal static void Throw(string? paramName) =>
       throw new NotFoundException(paramName);
}
