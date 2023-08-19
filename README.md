# Opc Da for .NET
This package allows you to use the OPC DA protocol in .Net applications.
> **_NOTE:_**  The OPC DA is only supported in Windows platforms.
## Installation  
You can install this package with command:
```shell
dotnet add package OpcDaNetCore
```
## Use
### Find servers
You can find available servers by specifying the ip address:
```cs
var servers = await BrowseOpcDaServers.BrowseServersAsync("localhost");
```
### Build a Service
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
You can add items to a group. If the group does not exists, then will be created:
```cs
server.AddItems("Group 1", "Simulation Examples.Functions.Ramp2", "Simulation Examples.Functions.Ramp3");
```
You can remove items from a group:
```cs
server.RemoveItems("Group 1", "Simulation Examples.Functions.Ramp2", "Simulation Examples.Functions.Ramp3");
```
You can read all items in a group:
```cs
var read = server.Read("Group 1");

foreach (var readElement in read)
{
    Console.WriteLine($"{readElement.ItemName} - {readElement.Value}");
}
```
Or read specific items. In this method you can read any item available in the server, not just items from a specific group, but must specify a group:
```cs
var readAny = server.Read("Group 1", "Simulation Examples.Functions.Ramp1", "Simulation Examples.Functions.Sine1");

foreach (var readElement in readAny)
{
    Console.WriteLine($"{readElement.ItemName} - {readElement.Value}");
}
```
You can write into tags:
```cs
var valueToWrite1 = new ItemDataValue("Channel1.Device1.Tag2", 125);
var valueToWrite2 = new ItemDataValue("Channel1.Device1.Tag3", 126);
server.Write("Group 1",valueToWrite1, valueToWrite2);
```