namespace OpcDaNetCore.ValueObjects;

public class BrowseItem
{
    public BrowseItem(string name, string id, bool hasChildren)
    {
        Name = name;
        Id = id;
        HasChildren = hasChildren;
    }

    public string Name { get; set; }
    public string Id { get; set; }
    public bool HasChildren { get; set; }
}
