using OpcDaNetCore.Contracts;
using OpcDaNetCore.Exceptions;
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
        private string _serverName = string.Empty;
        private string _ip = string.Empty;
        private readonly List<Group> _subscriptions = new List<Group>();
        private Action<IEnumerable<ItemDataValue>> _onDataChanged;

        private IOpcDaService BuildAndConnect(ServerHost host)
        {
            if (host is null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            var create = BrowseOpcDaServers.CreateServerAndConnect(host.Url);

            return new OpcDaService(create, _subscriptions, _onDataChanged);
        }

        private void ValidateServerParameters()
        {
            InvalidIpAddressException.ThrowIfInvalidAddress(_ip);
            InvalidServerException.ThrowIfInvalidServer(_serverName);
        }

        public OpcDaFactory WithServerName(string serverName)
        {
            InvalidServerException.ThrowIfInvalidServer(serverName);

            _serverName = serverName;

            return this;
        }

        public OpcDaFactory WithIp(string ip)
        {
            InvalidIpAddressException.ThrowIfInvalidAddress(ip);

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
            _onDataChanged = action ?? throw new ArgumentNullException(nameof(action));

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