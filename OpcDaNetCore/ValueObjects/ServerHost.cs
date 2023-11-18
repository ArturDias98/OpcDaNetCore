using OpcDaNetCore.Exceptions;

namespace OpcDaNetCore.ValueObjects
{
    public class ServerHost
    {
        public ServerHost(string serverName, string host, string url)
        {
            InvalidServerException.ThrowIfInvalidServer(serverName);

            ServerName = serverName;
            Host = host;
            Url = url;
        }

        public string ServerName { get; }
        public string Host { get; }
        public string Url { get; }
    }
}