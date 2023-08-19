# Opc Da for .NET
This package allows you to use the OPC DA protocol in .net applications.  
You can find available servers by specifying the ip address:
```cs
var servers = await BrowseOpcDaServers.BrowseServersAsync("localhost");
```
You can build an OPC DA service using the selected server from previous method:
```cs
ServerHost host = servers.FirstOrDefault();
using var server = await new OpcDaFactory()
    .WithServer(host)
    .BuildAsync();
```
Or you can add server specifications manually:
```cs
using var server = await new OpcDaFactory()
    .WithIp("localhost")
    .WithServerName("Kepware.KEPServerEX.V5")
    .BuildAsync();
```
You can browse nodes:
```cs
var browse = await server.BrowseNodeAsync(null);

foreach (var browseElement in browse)
{
    Console.WriteLine($"{browseElement.Name}; {browseElement.Id}; Has children: {browseElement.HasChildren}");
}

var node = browse.FirstOrDefault()?.Id;

browse = await server.BrowseNodeAsync(node);

foreach (var browseElement in browse)
{
    Console.WriteLine($"{browseElement.Name}; {browseElement.Id}; Has children: {browseElement.HasChildren}");
}
```
See full implementation:
```cs
var servers = await BrowseOpcDaServers.BrowseServersAsync("localhost");

foreach (var item in servers)
{
    Console.WriteLine(item.ServerName);
}

Group group1 = new("Group 1", 1000, new List<string> { "Channel1.Device1.Tag1", "Channel1.Device1.Tag2" });
Group group2 = new("Group 2", 1000, new List<string> { "Simulation Examples.Functions.Ramp1", "Simulation Examples.Functions.Sine1" });

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
```