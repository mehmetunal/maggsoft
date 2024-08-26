using System;
using System.Diagnostics.CodeAnalysis;

namespace Maggsoft.Core.Exceptions
{
    public class ArgumentNullIfException : ArgumentNullException
    {
        public static void ThrowIfEquel(bool ifCondition, object dc, object c, string exceptionMessage = "")
        {
            if (ifCondition)
            {
                ThrowIfEquel(dc, c, exceptionMessage);
            }
        }

        public static void ThrowIfEquel(object dc, object c, string exceptionMessage = "")
        {
            if (!dc.Equals(c))
            {
                Throw(string.IsNullOrEmpty(exceptionMessage) ? $"dc:${dc} not equel c:{c}" : exceptionMessage);
            }
        }

        [DoesNotReturn]
        internal static void Throw(string? paramName) =>
               throw new ArgumentNullException(paramName);
    }
}
