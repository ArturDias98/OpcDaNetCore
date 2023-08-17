using OpcDaNetCore.Factory;
using OpcDaNetCore.ValueObjects;

Group group1 = new("Group 1", 1000, new List<string> { "Channel1.Device1.Tag1", "Channel1.Device1.Tag2" });
Group group2 = new("Group 1", 1000, new List<string> { "Simulation Examples.Functions.Ramp1", "Simulation Examples.Functions.Sine1" });

using var server = await new OpcDaFactory()
    .WithIp("localhost")
    .WithServerName("Kepware.KEPServerEX.V5")
    .WithGroup(group1, group2)
    .WithDataChangedCallback(PrintValues)
    .BuildAsync();

var browse = await server.BrowseNodeAsync(null);

foreach (var browseElement in browse)
{
    Console.WriteLine($"{browseElement.Name}; {browseElement.Id}; Has children: {browseElement.HasChildren}");
}

Console.WriteLine("------");

var node = browse.FirstOrDefault()?.Id;

browse = await server.BrowseNodeAsync(node);

foreach (var browseElement in browse)
{
    Console.WriteLine($"{browseElement.Name}; {browseElement.Id}; Has children: {browseElement.HasChildren}");
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