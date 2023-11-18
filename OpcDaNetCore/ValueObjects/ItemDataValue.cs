using System;

namespace OpcDaNetCore.ValueObjects
{
    public class ItemDataValue
    {
        public ItemDataValue(string itemName, object value)
        {
            if (string.IsNullOrWhiteSpace(itemName))
            {
                throw new ArgumentException("Invalid item name");
            }

            ItemName = itemName;
            Value = value;
        }

        public string ItemName { get; }
        public object Value { get; }
    }
}