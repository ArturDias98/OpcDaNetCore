using OpcDaNetCore.Contracts;
using OpcDaNetCore.Factory.Services;
using OpcDaNetCore.Utilities;
using OpcDaNetCore.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpcDaNetCore.Factory
{
    public class OpcDaFactory
    {
        private string _serverName;
        private string _ip;
        private readonly List<Group> _subscriptions;
        private Action<IEnumerable<ItemDataValue>> onDataChanged;

        public OpcDaFactory()
        {
            _serverName = string.Empty;
            _ip = string.Empty;
            _subscriptions = new List<Group>();
        }

        private void ValidateIpAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new ArgumentNullException("You must specify the server ip address");
            }
        }

        private void ValidateServerName(string server)
        {
            if (string.IsNullOrWhiteSpace(server))
            {
                throw new ArgumentNullException("You must specify the server name");
            }
        }

        private IOpcDaService BuildAndConnect(ServerHost host)
        {
            if (host is null)
            {
                throw new ArgumentNullException("Server not found. Please verify that you have specified the correct server name");
            }

            var create = BrowseOpcDaServers.CreateServerAndConnect(host.Url);

            return new OpcDaService(create, _subscriptions, onDataChanged);
        }

        private void ValidateServerParameters()
        {
            ValidateIpAddress(_ip);
            ValidateServerName(_serverName);
        }

        public OpcDaFactory WithServerName(string serverName)
        {
            ValidateServerName(serverName);

            _serverName = serverName;

            return this;
        }

        public OpcDaFactory WithIp(string ip)
        {
            ValidateIpAddress(ip);

            _ip = ip;

            return this;
        }

        public OpcDaFactory WithServer(ServerHost server)
        {
            _ip = server.Host;
            _serverName = server.ServerName;

            ValidateServerParameters();

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
            if (action is null)
            {
                throw new ArgumentNullException("Data changed callback must not be null");
            }

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

            var servers = await BrowseOpcDaServers.BrowseServersAsync(_ip, cancellationToken);
            var server = servers.FirstOrDefault(i => i.ServerName == _serverName);

            return BuildAndConnect(server);
        }
    }
}