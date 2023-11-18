using System;

namespace OpcDaNetCore.Exceptions
{
    public class InvalidServerException : Exception
    {
        public InvalidServerException()
        {
        }

        public InvalidServerException(string message) : base(message)
        {
        }

        public InvalidServerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public static void ThrowIfInvalidServer(string server)
        {
            if (string.IsNullOrWhiteSpace(server))
            {
                throw new InvalidServerException("You must specify the server name");
            }
        }
    }
}
