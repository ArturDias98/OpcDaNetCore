using Opc.Da;
using OpcDaNetCore.Contracts;
using OpcDaNetCore.ValueObjects;

namespace OpcDaNetCore.Factory;

internal class FactoryService : IOpcDaService
{
    private static readonly object _syncLock = new();

    private readonly Server _server;

    public FactoryService(Server server, IEnumerable<Group> groups, Action<IEnumerable<ItemDataValue>>? onDataChanged)
    {
        _server = server;

        foreach (var group in groups)
        {
            CreateSubscription(group);
        }

        DataChanged += (_, items) => onDataChanged?.Invoke(items);
    }

    private void Subscription_DataChanged(object subscriptionHandle, object requestHandle, ItemValueResult[] values)
    {
        var parse = values.Select(i => new ItemDataValue(i.ItemName, i.Value));

        DataChanged?.Invoke(this, parse);
    }

    private Server LockServer()
    {
        lock (_syncLock)
        {
            return _server;
        }
    }

    private bool TryConnect()
    {
        try
        {
            LockServer().Connect();

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private Subscription? FindSubscription(string name)
    {
        Subscription? find = null;

        foreach (Subscription subscription in LockServer().Subscriptions)
        {
            if (subscription.Name == name)
            {
                find = subscription;
                break;
            }
        }

        return find;
    }

    private void CreateSubscription(Group group)
    {
        var state = new SubscriptionState()
        {
            Name = group.Name,
            UpdateRate = group.UpdateRate,
            Active = true,
            ClientHandle = new List<string> { group.Name }
        };

        var subscription = (Subscription)LockServer().CreateSubscription(state);

        var items = group.Items.Select(i => new Item
        {
            ItemName = i,
            ClientHandle = subscription.ClientHandle,
            ServerHandle = subscription.ServerHandle,
        }).ToArray();

        subscription.AddItems(items);

        subscription.DataChanged += Subscription_DataChanged;
    }

    private void HandleSubscription(Group group)
    {
        var subscription = FindSubscription(group.Name);

        if (subscription is null)
        {
            CreateSubscription(group);
        }
        else
        {
            var items = group.Items.Select(i => new Item
            {
                ItemName = i,
                ClientHandle = subscription.ClientHandle,
                ServerHandle = subscription.ServerHandle,
            }).ToArray();

            subscription.AddItems(items);
            subscription.Refresh();
        }
    }

    private IEnumerable<BrowseItem> Browse(string? node)
    {
        var itemID = node is null ? new Opc.ItemIdentifier() : new Opc.ItemIdentifier(node);
        var filters = new BrowseFilters { BrowseFilter = browseFilter.all };

        var browseElements = LockServer().Browse(itemID, filters, out _);
        return browseElements?.Select(i => new BrowseItem(i.Name, i.ItemName, i.HasChildren)) ?? Enumerable.Empty<BrowseItem>();
    }

    public event EventHandler<IEnumerable<ItemDataValue>>? DataChanged;

    public bool IsConnected => _server.IsConnected;

    public string ServerName => _server.Name;

    public string Host => _server.Url.HostName;

    public void AddItems(string groupName, IEnumerable<string> items)
    {
        var group = new Group(groupName, 1000, items);
        HandleSubscription(group);
    }

    public void AddItems(Group group)
    {
        HandleSubscription(group);
    }

    public IEnumerable<BrowseItem> BrowseNode(string? node)
    {
        return Browse(node);
    }

    public Task<IEnumerable<BrowseItem>> BrowseNodeAsync(string? node, CancellationToken cancellationToken = default)
    {
        return Task.Factory.StartNew(() => Browse(node), cancellationToken);
    }

    public bool Connect()
    {
        if (IsConnected)
        {
            throw new Exception("The system is already connected");
        }

        return TryConnect();
    }

    public Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        return Task.Factory.StartNew(TryConnect, cancellationToken);
    }

    public bool Disconnect()
    {
        if (!IsConnected)
        {
            throw new Exception("The system is not connected");
        }

        try
        {
            LockServer().Disconnect();

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public void Dispose()
    {
        foreach (Subscription item in LockServer().Subscriptions)
        {
            item.DataChanged -= Subscription_DataChanged;
            item.Dispose();
        }

        LockServer().Dispose();
    }
}
