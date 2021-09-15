using System.Text;
using System.Net.Sockets;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

Memory<byte> receiver = new(new byte[8046]);

async Task ReadFromClientAsync(NetworkStream stream)
{
    int receivedBytes;
    do
    {
        receivedBytes = await stream.ReadAsync(receiver);
    }
    while (receivedBytes == 0);
    var response = receiver.ToArray();
    Console.WriteLine($"Received \"{Encoding.UTF8.GetString(response, 1, receivedBytes - 1)}\" ({receivedBytes} bytes).");
}

async Task<TcpClient> GetConnectedClientAsync()
{
    TcpClient client = new();
    client.Connect("127.0.0.1", 8081);
    await ReadFromClientAsync(client.GetStream());
    return client;
}

bool isGetEchoFromTheSameTransportAgent;
Console.Write("Get echo from the same transport agent? (Y/n) > ");
isGetEchoFromTheSameTransportAgent = string.Compare(Console.ReadLine(), "n", ignoreCase: true) != 0;
Console.WriteLine($"Your choice: {isGetEchoFromTheSameTransportAgent}");

TcpClient client = await GetConnectedClientAsync();

TcpListener listener = null;
TcpClient listenerSession = null;
if (!isGetEchoFromTheSameTransportAgent)
{
    listener = new(8082);
    listener.Start();
}

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


while (true)
{
    Task<TcpClient> listeningTask = null;
    if (!isGetEchoFromTheSameTransportAgent)
    {
        listeningTask = listener.AcceptTcpClientAsync();
    }

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
        // if the transmission to the server failed due to disconnection, the following receiving will block forever.
        stream = listenerSession.GetStream();
    }

    try
    {
        await ReadFromClientAsync(stream);
    }
    catch (System.IO.IOException ex)
    {
        Console.WriteLine($"Transmission failed: {ex.Message}");
        client.Dispose();
        client = await GetConnectedClientAsync();
        Console.WriteLine("Reconnected.");
    }
}
