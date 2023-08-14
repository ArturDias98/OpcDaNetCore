namespace OpcDaNetCore.Models;

public class OpcDaServer
{
    public OpcDaServer(string serverName, string host, string url)
    {
        ServerName = serverName;
        Host = host;
        Url = url;
    }

    public string ServerName { get; set; }
    public string Host { get; set; }
    public string Url { get; set; }
}
