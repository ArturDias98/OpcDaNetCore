namespace OpcDaNetCore.ValueObjects
{
    public class ServerHost
    {
        public ServerHost(string serverName, string host, string url)
        {
            ServerName = serverName;
            Host = host;
            Url = url;
        }

        public string ServerName { get; }
        public string Host { get; }
        public string Url { get; }
    }
}