using System;

namespace OpcDaNetCore.Exceptions
{
    public class InvalidIpAddressException : Exception
    {
        public InvalidIpAddressException()
        {
        }

        public InvalidIpAddressException(string message) : base(message)
        {
        }

        public InvalidIpAddressException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public static void ThrowIfInvalidAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                new InvalidIpAddressException("You must specify a valid ip address");
            }
        }
    }
}
