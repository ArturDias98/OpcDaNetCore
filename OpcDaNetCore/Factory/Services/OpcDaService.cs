﻿using Opc.Da;
using OpcDaNetCore.Contracts;
using OpcDaNetCore.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpcDaNetCore.Factory.Services
{

    internal partial class OpcDaService : IOpcDaService
    {
        private static readonly object _syncLock = new object();

        private readonly Server _server;

        public OpcDaService(Server server, IEnumerable<Group> groups, Action<IEnumerable<ItemDataValue>> onDataChanged)
        {
            _server = server;

            foreach (var group in groups)
            {
                CreateSubscription(group);
            }

            DataChanged += (_, items) => onDataChanged?.Invoke(items);
        }

        private Server LockServer()
        {
            lock (_syncLock)
            {
                return _server;
            }
        }

        private IEnumerable<BrowseItem> Browse(string node)
        {
            var itemID = node is null ? new Opc.ItemIdentifier() : new Opc.ItemIdentifier(node);
            var filters = new BrowseFilters { BrowseFilter = browseFilter.all };

            var browseElements = LockServer().Browse(itemID, filters, out _);
            return browseElements?.Select(i => new BrowseItem(i.Name, i.ItemName, i.HasChildren)) ?? Enumerable.Empty<BrowseItem>();
        }

        public event EventHandler<IEnumerable<ItemDataValue>> DataChanged;

        public bool IsConnected => _server.IsConnected;

        public string ServerName => _server.Name;

        public string Host => _server.Url.HostName;

        public IEnumerable<BrowseItem> BrowseNode(string node)
        {
            return Browse(node);
        }

        public Task<IEnumerable<BrowseItem>> BrowseNodeAsync(string node, CancellationToken cancellationToken = default)
        {
            return Task.Factory.StartNew(() => Browse(node), cancellationToken);
        }

        public IEnumerable<ItemDataValue> Read(string groupName)
        {
            var subscription = FindSubscription(groupName)
                ?? throw new InvalidOperationException("You must create a group");

            var read = LockServer().Read(subscription.Items);

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

            var read = LockServer().Read(parse);

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

            LockServer().Write(itemValues);
        }
    }
}