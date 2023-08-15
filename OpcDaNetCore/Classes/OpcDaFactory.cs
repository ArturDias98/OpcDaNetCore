using Opc.Da;
using OpcDaNetCore.Models;
using OpcDaNetCore.Utilities;

namespace OpcDaNetCore.Classes;

public class OpcDaFactory
{
    private string _serverName;
    private string _ip;

    public OpcDaFactory()
    {
        _serverName = string.Empty;
        _ip = string.Empty;
    }

    private Task<IEnumerable<OpcDaServer>> BrowseServersAsync(CancellationToken cancellationToken = default)
    {
        return Task.Factory.StartNew(() => BrowseOpcDaServers.BrowseServers(_ip), cancellationToken);
    }

    private void ValidateServerParameters()
    {
        ArgumentException.ThrowIfNullOrEmpty(_ip, "You must specify the server ip address");
        ArgumentException.ThrowIfNullOrEmpty(_serverName, "You must specify the server name");
    }

    public OpcDaFactory WithServerName(string serverName)
    {
        ArgumentException.ThrowIfNullOrEmpty(serverName, "Invalid server name");

        _serverName = serverName;

        return this;
    }

    public OpcDaFactory WithIp(string ip)
    {
        ArgumentException.ThrowIfNullOrEmpty(ip, "Invalid ip address");

        _ip = ip;

        return this;
    }

    public Server Build()
    {
        ValidateServerParameters();

        var servers = BrowseOpcDaServers.BrowseServers(_ip);
        var server = servers.FirstOrDefault(i => i.ServerName == _serverName);

        ArgumentNullException.ThrowIfNull(server, "Please verify that you have specified the correct server name");

        return BrowseOpcDaServers.CreateServer(server.Url);
    }

    public async Task<Server> BuildAsync(CancellationToken cancellationToken = default)
    {
        ValidateServerParameters();

        var servers = await BrowseServersAsync(cancellationToken);

        var server = servers.FirstOrDefault(i => i.ServerName == _serverName);

        ArgumentNullException.ThrowIfNull(server, "Please verify that you have specified the correct server name");

        return BrowseOpcDaServers.CreateServer(server.Url);
    }
}
