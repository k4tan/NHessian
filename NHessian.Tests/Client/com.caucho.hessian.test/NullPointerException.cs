using System;

namespace java.lang
{
    public class NullPointerException : Exception
    {
        public NullPointerException()
        {
        }

        public NullPointerException(string message)
            : base(message)
        {
        }

        public NullPointerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}