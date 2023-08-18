using Opc.Da;
using OpcDaNetCore.ValueObjects;

namespace OpcDaNetCore.Factory.Services;

internal partial class OpcDaService
{
    private void Subscription_DataChanged(object subscriptionHandle, object requestHandle, ItemValueResult[] values)
    {
        var parse = values.Select(i => new ItemDataValue(i.ItemName, i.Value));

        DataChanged?.Invoke(this, parse);
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
            LockServer().CancelSubscription(subscription);
        }
    }
}
