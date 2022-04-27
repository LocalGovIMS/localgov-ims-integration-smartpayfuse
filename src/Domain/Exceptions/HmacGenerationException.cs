using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Domain.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class HmacGenerationException : Exception
    {
        public HmacGenerationException()
        {
        }

        public HmacGenerationException(string message)
            : base(message)
        {
        }

        public HmacGenerationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected HmacGenerationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
