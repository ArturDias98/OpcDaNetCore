using Opc;
using OpcDaNetCore.Models;

namespace OpcDaNetCore.Utilities;

public static class BrowseOpcDaServers
{
    public static IEnumerable<OpcDaServer> BrowseServers(string ip)
    {
        ArgumentNullException.ThrowIfNull(ip, "Invalid server ip");

        using IDiscovery discovery = new OpcCom.ServerEnumerator();
        var enumetare = discovery.GetAvailableServers(Specification.COM_DA_20, ip, null);

        return enumetare.Select(i => new OpcDaServer(i.Name, i.Url.HostName, i.Url.ToString()));
    }
}
