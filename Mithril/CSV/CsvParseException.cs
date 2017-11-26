using System;
using System.Runtime.Serialization;

namespace Mithril
{
    [Serializable]
    public sealed class CsvParseException : Exception
    {
        public CsvParseException(String message)
            : base(message)
        {
        }

        public CsvParseException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        internal CsvParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}