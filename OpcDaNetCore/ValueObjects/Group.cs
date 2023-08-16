namespace OpcDaNetCore.ValueObjects;

public class Group
{
    public Group(string name, int updateRate, IEnumerable<string> items)
    {
        Name = name;
        UpdateRate = updateRate;
        Items = items;

        ValidateFields();
    }

    private void ValidateFields()
    {
        ArgumentException.ThrowIfNullOrEmpty(Name, "The group name is required");

        if (UpdateRate <= 0)
        {
            throw new ArgumentException("Invalid update rate");
        }

        if (Items?.Any() == false)
        {
            throw new ArgumentException("Group without items");
        }
    }

    public string Name { get; set; }
    public int UpdateRate { get; set; }
    public IEnumerable<string> Items { get; set; } = Enumerable.Empty<string>();

    internal void Default()
    {
        Name = "Group 1";
        UpdateRate = 1000;
    }
}
