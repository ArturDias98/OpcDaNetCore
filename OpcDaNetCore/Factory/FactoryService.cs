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
        AddItems(group);
    }

    public void AddItems(Group group)
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

    public void ConfigureGroup(ConfigureGroup configure)
    {
        var subscription = FindSubscription(configure.Name);

        if (subscription is not null)
        {
            var state = (SubscriptionState)subscription.State.Clone();
            state.UpdateRate = configure.UpdateRate;
            state.Active = configure.IsActive;

            subscription.ModifyState(-1, state);
        }
    }

    public void RemoveGroup(string groupName)
    {
        var subscription = FindSubscription(groupName);
        if (subscription is not null)
        {
            _server.CancelSubscription(subscription);
        }
    }

    public IEnumerable<BrowseItem> BrowseNode(string? node)
    {
        return Browse(node);
    }

    public Task<IEnumerable<BrowseItem>> BrowseNodeAsync(string? node, CancellationToken cancellationToken = default)
    {
        return Task.Factory.StartNew(() => Browse(node), cancellationToken);
    }

    public IEnumerable<ItemDataValue> Read(string groupName)
    {
        var subscription = FindSubscription(groupName)
            ?? throw new InvalidOperationException("You must create a group");

        var read = _server.Read(subscription.Items);

        return read.Select(i => new ItemDataValue(i.ItemName, i.Value));
    }

    public IEnumerable<ItemDataValue> Read(string groupName, params string[] ids)
    {
        return Read(groupName, ids.ToList());
    }

    public IEnumerable<ItemDataValue> Read(string groupName, IEnumerable<string> ids)
    {
        var subscription = FindSubscription(groupName)
            ?? throw new InvalidOperationException("You must create a group");

        var parse = ids.Select(i => new Item
        {
            ItemName = i,
            ClientHandle = subscription.ClientHandle,
            ServerHandle = subscription.ServerHandle,
        }).ToArray();

        var read = _server.Read(parse);

        return read.Select(i => new ItemDataValue(i.ItemName, i.Value));
    }

    public void Write(string groupName, params ItemDataValue[] items)
    {
        Write(groupName, items.ToList());
    }

    public void Write(string groupName, IEnumerable<ItemDataValue> items)
    {
        var subscription = FindSubscription(groupName)
            ?? throw new InvalidOperationException("You must create a group");

        var itemValues = new ItemValue[items.Count()];
        int i = 0;
        foreach (var item in items)
        {
            var writeItem = new Item
            {
                ItemName = item.ItemName,
                ClientHandle = subscription.ClientHandle,
                ServerHandle = subscription.ServerHandle
            };
            itemValues[i] = new ItemValue(writeItem)
            {
                Value = item.Value,
                Quality = new Quality(qualityBits.good),
            };
            i++;
        }

        _server.Write(itemValues);
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
