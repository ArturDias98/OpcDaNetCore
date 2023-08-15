namespace OpcDaNetCore.Models;

public class SubscriptionModel
{
    public string Name { get; set; } = string.Empty;
    public int UpdateRate { get; set; }
    public bool IsActive { get; set; }
    public IEnumerable<string> Items { get; set; } = Enumerable.Empty<string>();
}
