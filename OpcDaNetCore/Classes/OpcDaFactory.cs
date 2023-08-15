using Opc.Da;
using OpcDaNetCore.Models;
using OpcDaNetCore.Utilities;

namespace OpcDaNetCore.Classes;

public class OpcDaFactory
{
    private string _serverName;
    private string _ip;
    private readonly List<SubscriptionModel> _subscriptions;
    private Action<IEnumerable<ItemModel>>? onDataChanged;

    public OpcDaFactory()
    {
        _serverName = string.Empty;
        _ip = string.Empty;
        _subscriptions = new();
    }

    private void Subscription_DataChanged(object subscriptionHandle, object requestHandle, ItemValueResult[] values)
    {
        var parse = values.Select(i => new ItemModel(i.ItemName, i.Value));
        onDataChanged?.Invoke(parse);
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

    private void CreateSubscriptions(Server server)
    {
        foreach (SubscriptionModel item in _subscriptions)
        {
            var state = new SubscriptionState()
            {
                Name = item.Name,
                UpdateRate = item.UpdateRate,
                Active = item.IsActive,
                ClientHandle = new List<string> { item.Name }
            };

            var subscription = (Subscription)server.CreateSubscription(state);
            
            var items = item.Items.Distinct().Select(i => new Item()
            {
                ItemName = i,
                ClientHandle = subscription.ClientHandle,
                ServerHandle = subscription.ServerHandle,
            }).ToArray();

            subscription.AddItems(items);
            subscription.DataChanged += Subscription_DataChanged;
        }
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

    public OpcDaFactory WithSubscription(params SubscriptionModel[] subscriptions)
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

    public OpcDaFactory WithDataChangedCallback(Action<IEnumerable<ItemModel>> action)
    {
        onDataChanged = action;

        return this;
    }

    public Server Build()
    {
        ValidateServerParameters();

        var servers = BrowseOpcDaServers.BrowseServers(_ip);
        var server = servers.FirstOrDefault(i => i.ServerName == _serverName);

        ArgumentNullException.ThrowIfNull(server, "Server not found. Please verify that you have specified the correct server name");

        var create = BrowseOpcDaServers.CreateServerAndConnect(server.Url);

        CreateSubscriptions(create);

        return create;
    }

    public async Task<Server> BuildAsync(CancellationToken cancellationToken = default)
    {
        ValidateServerParameters();

        var servers = await BrowseServersAsync(cancellationToken);

        var server = servers.FirstOrDefault(i => i.ServerName == _serverName);

        ArgumentNullException.ThrowIfNull(server, "Server not found. Please verify that you have specified the correct server name");

        var create = BrowseOpcDaServers.CreateServerAndConnect(server.Url);

        CreateSubscriptions(create);

        return create;
    }
}
