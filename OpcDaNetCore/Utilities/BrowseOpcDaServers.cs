using Opc;
using OpcDaNetCore.Exceptions;
using OpcDaNetCore.ValueObjects;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpcDaNetCore.Utilities
{
    public static class BrowseOpcDaServers
    {
        public static IEnumerable<ServerHost> BrowseServers(string ip)
        {
            InvalidIpAddressException.ThrowIfInvalidAddress(ip);

            using (IDiscovery discovery = new OpcCom.ServerEnumerator())
            {
                var enumetare = discovery.GetAvailableServers(Specification.COM_DA_20, ip, null);
                return enumetare.Select(i => new ServerHost(i.Name, i.Url.HostName, i.Url.ToString()));
            }
        }

        public static Task<IEnumerable<ServerHost>> BrowseServersAsync(string ip, CancellationToken cancellationToken = default)
        {
            return Task.Factory.StartNew(() => BrowseServers(ip), cancellationToken);
        }

        public static Opc.Da.Server CreateServer(string url)
        {
            var opcFactory = new OpcCom.Factory();
            var opcUrl = new Opc.URL(url);

            return new Opc.Da.Server(opcFactory, opcUrl);
        }

        public static Opc.Da.Server CreateServerAndConnect(string url)
        {
            var opcFactory = new OpcCom.Factory();
            var opcUrl = new URL(url);

            var server = new Opc.Da.Server(opcFactory, opcUrl);

            server.Connect();

            return server;
        }
    }
}