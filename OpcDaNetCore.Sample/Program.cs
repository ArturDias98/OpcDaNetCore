using OpcDaNetCore.Classes;
using OpcDaNetCore.Models;

var server = await new OpcDaFactory()
    .WithIp("localhost")
    .WithServerName("Kepware.KEPServerEX.V5")
    .WithSubscription(new SubscriptionModel()
    {
        IsActive = true,
        Name = "Group 1",
        UpdateRate = 1000,
        Items = new List<string> { "Channel1.Device1.Tag1", "Channel1.Device1.Tag2" }
    })
    .WithDataChangedCallback(PrintValues)
    .BuildAsync();

Console.ReadKey();

static void PrintValues(IEnumerable<ItemModel> items)
{
    foreach (var item in items)
    {
        Console.WriteLine($"{item.ItemName}: {item.Value}");
    }
}