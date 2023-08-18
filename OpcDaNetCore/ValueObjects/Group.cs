namespace OpcDaNetCore.ValueObjects;

public class Group
{
    public Group(string name, int updateRate, IEnumerable<string> items)
    {
        if (items?.Any() != true)
        {
            throw new ArgumentException("Group without items");
        }

        Name = name;
        UpdateRate = updateRate;
        Items = items;

        ValidateFields();
    }

    public Group(string name, int updateRate)
    {
        Name = name;
        UpdateRate = updateRate;

        ValidateFields();
    }

    public Group(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, "The group name is required");

        Name = name;
    }

    private void ValidateFields()
    {
        ArgumentException.ThrowIfNullOrEmpty(Name, "The group name is required");

        if (UpdateRate <= 0)
        {
            throw new ArgumentException("Invalid update rate");
        }
    }

    public string Name { get; private set; }
    public int UpdateRate { get; private set; } = 1000;
    public IEnumerable<string> Items { get; } = Enumerable.Empty<string>();

    internal void Default()
    {
        Name = "Group 1";
        UpdateRate = 1000;
    }
}
