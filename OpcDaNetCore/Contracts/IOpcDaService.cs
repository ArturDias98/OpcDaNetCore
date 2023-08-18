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
    void AddItems(string groupName, params string[] items);
    void AddItems(Group group);
    void RemoveItems(string groupName, IEnumerable<string> items);
    void RemoveItems(string groupName, params string[] items);
    void RemoveItems(Group group);
    void RemoveGroup(string groupName);
    void ConfigureGroup(ConfigureGroup configure);
    IEnumerable<BrowseItem> BrowseNode(string? node);
    Task<IEnumerable<BrowseItem>> BrowseNodeAsync(string? node, CancellationToken cancellationToken = default);
    IEnumerable<ItemDataValue> Read(string groupName);
    IEnumerable<ItemDataValue> Read(string groupName, IEnumerable<string> ids);
    IEnumerable<ItemDataValue> Read(string groupName, params string[] ids);
    void Write(string groupName, IEnumerable<ItemDataValue> items);
    void Write(string groupName, params ItemDataValue[] items);
}
