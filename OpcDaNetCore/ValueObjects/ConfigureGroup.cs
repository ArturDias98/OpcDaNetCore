namespace OpcDaNetCore.ValueObjects;

public class ConfigureGroup
{
    public ConfigureGroup()
    {
    }

    public ConfigureGroup(string name, bool isActive, int updateRate)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, "Invalid group name");

        IsActive = isActive;
        Name = name;

        UpdateRate = updateRate;

        ValidateRate();
    }

    private void ValidateRate()
    {
        if (UpdateRate <= 0)
        {
            throw new ArgumentException("Invalid update rate");
        }
    }

    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public int UpdateRate { get; private set; }

    public ConfigureGroup WithGroupName(string groupName)
    {
        ArgumentException.ThrowIfNullOrEmpty(groupName, "Invalid group name");

        Name = groupName;

        return this;
    }

    public ConfigureGroup WithUpdateRate(int updateRate)
    {
        UpdateRate = updateRate;

        ValidateRate();

        return this;
    }

    public ConfigureGroup WithActivation(bool isActive)
    {
        IsActive = isActive;

        return this;
    }

    public void ChangeActive(bool isActive)
    {
        IsActive = isActive;
    }

    public void ChangeUpdateRate(int updateRate)
    {
        ValidateRate();

        UpdateRate = updateRate;
    }
}
