using OpcDaNetCore.Contracts;
using OpcDaNetCore.Utilities;
using OpcDaNetCore.ValueObjects;

namespace OpcDaNetCore.Factory;

public class OpcDaFactory
{
    private string _serverName;
    private string _ip;
    private readonly List<Group> _subscriptions;
    private Action<IEnumerable<ItemDataValue>>? onDataChanged;

    public OpcDaFactory()
    {
        _serverName = string.Empty;
        _ip = string.Empty;
        _subscriptions = new();
    }

    private IOpcDaService BuildAndConnect(ServerHost? host)
    {
        ArgumentNullException.ThrowIfNull(host, "Server not found. Please verify that you have specified the correct server name");

        var create = BrowseOpcDaServers.CreateServerAndConnect(host.Url);

        return new OpcDaService(create, _subscriptions, onDataChanged);
    }

    private Task<IEnumerable<ServerHost>> BrowseServersAsync(CancellationToken cancellationToken = default)
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

    public OpcDaFactory WithServer(ServerHost server)
    {
        _ip = server.Host;
        _serverName = server.ServerName;

        return this;
    }

    public OpcDaFactory WithGroup(params Group[] subscriptions)
    {
        foreach (var subscription in subscriptions)
        {
            if (_subscriptions.Any(i => i.Name == subscription.Name))
            {
                throw new ArgumentException("One subscription with same name already exists");
            }

            _subscriptions.Add(subscription);
        }

        return this;
    }

    public OpcDaFactory WithDataChangedCallback(Action<IEnumerable<ItemDataValue>> action)
    {
        onDataChanged = action;

        return this;
    }

    public IOpcDaService Build()
    {
        ValidateServerParameters();

        var servers = BrowseOpcDaServers.BrowseServers(_ip);
        var server = servers.FirstOrDefault(i => i.ServerName == _serverName);

        return BuildAndConnect(server);
    }

    public async Task<IOpcDaService> BuildAsync(CancellationToken cancellationToken = default)
    {
        ValidateServerParameters();

        var servers = await BrowseServersAsync(cancellationToken);

        var server = servers.FirstOrDefault(i => i.ServerName == _serverName);

        return BuildAndConnect(server);
    }
}
