using OpcDaNetCore.ValueObjects;

namespace OpcDaNetCore.Contracts;

public interface IOpcDaService : IDisposable
{
    event EventHandler<IEnumerable<ItemDataValue>>? DataChanged;

    bool IsConnected { get; }
    string ServerName { get; }
    string Host { get; }

    bool Connect();
    Task<bool> ConnectAsync(CancellationToken cancellationToken = default);
    bool Disconnect();
    void AddItems(string groupName, IEnumerable<string> items);
    void RemoveGroup(string groupName);
    void AddItems(Group group);
    IEnumerable<BrowseItem> BrowseNode(string? node);
    Task<IEnumerable<BrowseItem>> BrowseNodeAsync(string? node, CancellationToken cancellationToken = default);
}
