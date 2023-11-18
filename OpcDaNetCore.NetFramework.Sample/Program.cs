using OpcDaNetCore.Factory;
using OpcDaNetCore.Utilities;
using OpcDaNetCore.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpcDaNetCore.NetFramework.Sample
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {
            var servers = await BrowseOpcDaServers.BrowseServersAsync("localhost");

            foreach (var item in servers)
            {
                Console.WriteLine(item.ServerName);
            }

            Group group1 = new Group("Group 1", 1000, new List<string> { "Channel1.Device1.Tag1", "Channel1.Device1.Tag2" });
            Group group2 = new Group("Group 2", 1000, new List<string> { "Simulation Examples.Functions.Ramp1", "Simulation Examples.Functions.Sine1" });

            var server = await new OpcDaFactory()
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

            server.AddItems("Group 1", "Simulation Examples.Functions.Ramp2", "Simulation Examples.Functions.Ramp3");
            await Task.Delay(5000);
            server.RemoveItems("Group 1", "Simulation Examples.Functions.Ramp2", "Simulation Examples.Functions.Ramp3");

            var read = server.Read("Group 1");

            foreach (var readElement in read)
            {
                Console.WriteLine($"{readElement.ItemName} - {readElement.Value}");
            }

            var readAny = server.Read("Group 1", "Simulation Examples.Functions.Ramp1", "Simulation Examples.Functions.Sine1");

            foreach (var readElement in readAny)
            {
                Console.WriteLine($"{readElement.ItemName} - {readElement.Value}");
            }

            var valueToWrite1 = new ItemDataValue("Channel1.Device1.Tag2", 125);
            var valueToWrite2 = new ItemDataValue("Channel1.Device1.Tag3", 126);
            server.Write("Group 1", valueToWrite1, valueToWrite2);

            Console.ReadKey();

            server.Dispose();
        }

        static void PrintValues(IEnumerable<ItemDataValue> items)
        {
            foreach (var item in items)
            {
                Console.WriteLine($"{item.ItemName}: {item.Value}");
            }
        }
    }
}
