namespace OpcDaNetCore.ValueObjects;

public class BrowseItem
{
    public BrowseItem(string name, string id, bool hasChildren)
    {
        Name = name;
        Id = id;
        HasChildren = hasChildren;
    }

    public string Name { get; }
    public string Id { get; }
    public bool HasChildren { get; }
}
