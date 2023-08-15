using OpcDaNetCore.Classes;

var server = await new OpcDaFactory()
    .WithIp("localhost")
    .WithServerName("Kepware.KEPServerEX.V5")
    .BuildAsync();

server.Connect();

Console.ReadKey();
