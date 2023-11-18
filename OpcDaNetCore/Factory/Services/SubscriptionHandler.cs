using Opc;
using Opc.Da;
using OpcDaNetCore.ValueObjects;
using System.Collections.Generic;
using System.Linq;

namespace OpcDaNetCore.Factory.Services
{
    internal partial class OpcDaService
    {
        private void Subscription_DataChanged(object subscriptionHandle, object requestHandle, ItemValueResult[] values)
        {
            var parse = values.Select(i => new ItemDataValue(i.ItemName, i.Value));

            DataChanged?.Invoke(this, parse);
        }

        private Subscription FindSubscription(string name)
        {
            Subscription find = null;

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

        private void AddItemsAndConfigure(string groupName, IEnumerable<string> items)
        {
            var group = new Group(groupName, 1000, items);
            AddItems(group);
        }

        private void RemoveItemsAndConfigure(string groupName, IEnumerable<string> items)
        {
            var group = new Group(groupName, items);
            RemoveItems(group);
        }

        public void AddItems(string groupName, IEnumerable<string> items)
        {
            AddItemsAndConfigure(groupName, items);
        }

        public void AddItems(string groupName, params string[] items)
        {
            AddItemsAndConfigure(groupName, items);
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

        public void RemoveItems(string groupName, IEnumerable<string> items)
        {
            RemoveItemsAndConfigure(groupName, items);
        }

        public void RemoveItems(string groupName, params string[] items)
        {
            RemoveItemsAndConfigure(groupName, items);
        }

        public void RemoveItems(Group group)
        {
            var subscription = FindSubscription(group.Name);

            if (subscription != null)
            {
                var items = subscription.Items
                    .Where(i => group.Items.Contains(i.ItemName))
                    .Select(i => new ItemIdentifier
                    {
                        ItemName = i.ItemName,
                        ItemPath = i.ItemPath,
                        ClientHandle = i.ClientHandle,
                        ServerHandle = i.ServerHandle,
                    }).ToArray();

                subscription.RemoveItems(items);
            }
        }

        public void ConfigureGroup(ConfigureGroup configure)
        {
            var subscription = FindSubscription(configure.Name);

            if (subscription != null)
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
            if (subscription != null)
            {
                LockServer().CancelSubscription(subscription);
            }
        }
    }
}