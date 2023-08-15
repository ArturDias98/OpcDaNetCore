namespace OpcDaNetCore.Models;

public class ItemModel
{
    public ItemModel(string itemName, object value)
    {
        ItemName = itemName;
        Value = value;
    }

    public string ItemName { get; set; }
    public object Value { get; set; }
}
