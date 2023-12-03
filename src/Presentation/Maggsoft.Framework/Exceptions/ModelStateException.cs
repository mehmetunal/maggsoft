using System;
using System.Runtime.Serialization;

namespace Maggsoft.Framework.Exceptions;

[Serializable]
public class ModelStateException : Exception
{
    public ModelStateException()
        : base()
    { }

    public ModelStateException(string message)
        : base(message)

    { }

    public ModelStateException(string message, Exception innerException)
        : base(message, innerException)
    { }

    protected ModelStateException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }
}
