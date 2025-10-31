# OpcDaNetCore

[![NuGet](https://img.shields.io/nuget/v/OpcDaNetCore.svg)](https://www.nuget.org/packages/OpcDaNetCore/)
[![License](https://img.shields.io/github/license/ArturDias98/OpcDaNetCore)](LICENSE)

A modern .NET library for working with OPC Data Access (OPC DA) servers, providing a simple and intuitive API for reading, writing, and monitoring industrial automation data.

> **⚠️ Platform Requirement:** OPC DA is a Windows-only technology. This library requires Windows and .NET Framework 4.6.1 or .NET Standard 2.0+.

## Features

- 🔍 **Server Discovery** - Automatically discover available OPC DA servers on local or remote machines
- 🏗️ **Fluent API** - Easy-to-use builder pattern for service configuration
- 📊 **Data Operations** - Read and write tag values with full type support
- 🌳 **Node Browsing** - Explore server address space and tag hierarchy
- 👥 **Group Management** - Organize tags into logical groups for efficient operations
- ⚡ **Async/Await** - Modern asynchronous API throughout

## Installation  

Install the package via NuGet Package Manager:

```shell
dotnet add package OpcDaNetCore
```

Or via Package Manager Console:

```powershell
Install-Package OpcDaNetCore
```

## Quick Start

### 1. Discover Available Servers

Find all OPC DA servers available on a specific host:

```cs
// Search on local machine
var servers = await BrowseOpcDaServers.BrowseServersAsync("localhost");

// Or search on a remote machine
var remoteServers = await BrowseOpcDaServers.BrowseServersAsync("192.168.1.100");

foreach (var server in servers)
{
    Console.WriteLine($"Server: {server.ServerName}");
}
```

### 2. Connect to an OPC DA Server

#### Option A: Using a discovered server

```cs
ServerHost host = servers.FirstOrDefault();
using var opcService = await new OpcDaFactory()
    .WithServer(host)
    .BuildAsync();
```

#### Option B: Manual configuration

```cs
using var opcService = await new OpcDaFactory()
    .WithIp("localhost")
    .WithServerName("Kepware.KEPServerEX.V5")
    .BuildAsync();
```

### 3. Browse the Server Address Space

Explore the tag hierarchy and discover available data points:

```cs
// Browse root level
var rootItems = await opcService.BrowseNodeAsync(null);

foreach (var item in rootItems)
{
    Console.WriteLine($"Name: {item.Name}");
    Console.WriteLine($"ID: {item.Id}");
    Console.WriteLine($"Has Children: {item.HasChildren}");
    Console.WriteLine("---");
}

// Browse a specific node
var nodeId = rootItems.FirstOrDefault()?.Id;
if (nodeId != null)
{
    var childItems = await opcService.BrowseNodeAsync(nodeId);
    
    foreach (var item in childItems)
    {
        Console.WriteLine($"Child: {item.Name} (ID: {item.Id})");
    }
}
```

## Usage Guide

### Managing Groups and Items

Groups help organize tags for efficient batch operations.

#### Add Items to a Group

If the group doesn't exist, it will be created automatically:

```cs
opcService.AddItems("Group 1", 
    "Simulation Examples.Functions.Ramp2", 
    "Simulation Examples.Functions.Ramp3");
```

#### Remove Items from a Group

```cs
opcService.RemoveItems("Group 1", 
    "Simulation Examples.Functions.Ramp2", 
    "Simulation Examples.Functions.Ramp3");
```

### Reading Data

#### Read All Items in a Group

```cs
var values = opcService.Read("Group 1");

foreach (var value in values)
{
    Console.WriteLine($"Tag: {value.ItemName}, Value: {value.Value}");
}
```

#### Read Specific Items

You can read any items available on the server by specifying their full path. Note that you must still specify a group name:

```cs
var specificValues = opcService.Read("Group 1", 
    "Simulation Examples.Functions.Ramp1", 
    "Simulation Examples.Functions.Sine1");

foreach (var value in specificValues)
{
    Console.WriteLine($"Tag: {value.ItemName}, Value: {value.Value}, Quality: {value.Quality}");
}
```

### Writing Data

Write values to one or more tags:

```cs
var valueToWrite1 = new ItemDataValue("Channel1.Device1.Tag2", 125);
var valueToWrite2 = new ItemDataValue("Channel1.Device1.Tag3", 126);

opcService.Write("Group 1", valueToWrite1, valueToWrite2);
```

## API Reference

### Core Classes

- **`OpcDaFactory`** - Builder for creating OPC DA service instances
- **`IOpcDaService`** - Main service interface for OPC DA operations
- **`BrowseOpcDaServers`** - Utility for discovering available servers
- **`ServerHost`** - Represents an OPC DA server configuration
- **`BrowseItem`** - Represents a node in the server address space
- **`ItemDataValue`** - Represents a tag value with metadata

## Requirements

- Windows operating system
- .NET Framework 4.6.1 or higher, OR
- .NET Standard 2.0+ (.NET Core 2.0+, .NET 5+, .NET 6+, .NET 7+)
- OPC DA server installed and accessible

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

Built for the industrial automation community to simplify OPC DA integration in modern .NET applications.
