namespace OpcDaNetCore.ValueObjects;

public class ItemValue
{
    public ItemValue(string itemName, object value)
    {
        ArgumentException.ThrowIfNullOrEmpty(itemName, "Invalid item name");

        ItemName = itemName;
        Value = value;
    }

    public string ItemName { get; set; }
    public object Value { get; set; }
}
