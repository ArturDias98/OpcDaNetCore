using Opc.Da;
using OpcDaNetCore.Classes;
using OpcDaNetCore.ValueObjects;

var server = await new OpcDaFactory()
    .WithIp("localhost")
    .WithServerName("Kepware.KEPServerEX.V5")
    .WithGroup(new Group
    {
        Name = "Group 1",
        UpdateRate = 1000,
        Items = new List<string> { "Channel1.Device1.Tag1", "Channel1.Device1.Tag2"
        }
    },
    new Group
    {
        Name = "Group 2",
        UpdateRate = 1000,
        Items = new List<string> { "Simulation Examples.Functions.Ramp1", "Simulation Examples.Functions.Sine1" }
    })
    .WithDataChangedCallback(PrintValues)
    .BuildAsync();

string? node = null;
var itemID = node is null ? new Opc.ItemIdentifier() : new Opc.ItemIdentifier(node);
var filters = new BrowseFilters { BrowseFilter = browseFilter.all };
var browseElements = server.Browse(itemID, filters, out _);

foreach (var browseElement in browseElements)
{
    Console.WriteLine($"{browseElement.Name}; {browseElement.ItemName}; Is item: {browseElement.IsItem}");
}

Console.WriteLine("------");

node = browseElements.FirstOrDefault()?.ItemName;
browseElements = server.Browse(new Opc.ItemIdentifier(node), filters, out _);

foreach (var browseElement in browseElements)
{
    Console.WriteLine($"{browseElement.Name}; {browseElement.ItemName}; Is item: {browseElement.IsItem}");
}

Console.WriteLine("------");

Console.ReadKey();

static void PrintValues(IEnumerable<ItemDataValue> items)
{
    foreach (var item in items)
    {
        Console.WriteLine($"{item.ItemName}: {item.Value}");
    }
}