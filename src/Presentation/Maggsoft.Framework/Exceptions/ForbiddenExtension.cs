using System;
using System.Runtime.Serialization;

namespace Maggsoft.Framework.Exceptions
{
    [Serializable]
    public class ForbiddenExtension : Exception
    {
        public ForbiddenExtension()
            : base()
        { }

        public ForbiddenExtension(string message)
            : base(message)

        { }

        public ForbiddenExtension(string message, Exception innerException)
            : base(message, innerException)
        { }

        protected ForbiddenExtension(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
