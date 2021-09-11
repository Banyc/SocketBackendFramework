using System.Net;
using System.Text;
using System.Net.Sockets;

bool isGetEchoFromTheSameTransportAgent;
Console.Write("Get echo from the same transport agent? (Y/n) > ");
isGetEchoFromTheSameTransportAgent = string.Compare(Console.ReadLine(), "n", ignoreCase: true) != 0;
Console.WriteLine($"Your choice: {isGetEchoFromTheSameTransportAgent}");

TcpClient client = new();
client.Connect("127.0.0.1", 8081);
int clientLocalPort = ((IPEndPoint)client.Client.LocalEndPoint).Port;

TcpListener listener = new(8082);
TcpClient listenerSession = null;
listener.Start();

byte[] header = new byte[1];
if (isGetEchoFromTheSameTransportAgent)
{
    header[0] = (byte)1;  // Echo
}
else
{
    header[0] = (byte)2;  // EchoByClient
}
string hello = "hello";

Memory<byte> receiver = new(new byte[8046]);

while (true)
{
    Task<TcpClient> listeningTask = listener.AcceptTcpClientAsync();

    Console.Write("Send > ");
    string userMessage = Console.ReadLine();
    if (userMessage != "")
    {
        hello = userMessage;
    }
    byte[] helloBytes = Encoding.UTF8.GetBytes(hello);
    List<byte> packet = new(header);
    packet.AddRange(helloBytes);
    var stream = client.GetStream();
    await stream.WriteAsync(packet.ToArray(), 0, packet.Count);
    Console.WriteLine($"Sent     \"{hello}\" ({packet.Count} bytes).");

    if (!isGetEchoFromTheSameTransportAgent)
    {
        listenerSession?.Dispose();
        listenerSession = await listeningTask;
        stream = listenerSession.GetStream();
    }

    int receivedBytes;
    do
    {
        receivedBytes = await stream.ReadAsync(receiver);
    }
    while (receivedBytes == 0);
    var response = receiver.ToArray();
    Console.WriteLine($"Received \"{Encoding.UTF8.GetString(response, 1, receivedBytes - 1)}\" ({receivedBytes} bytes).");
}
