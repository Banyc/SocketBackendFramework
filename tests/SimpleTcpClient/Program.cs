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

async Task<TcpClient> ReconnectToTheServerAsync(TcpClient client)
{
    client.Dispose();
    var newClient = await GetConnectedClientAsync();
    Console.WriteLine("Reconnected.");
    return newClient;
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

Task<TcpClient> listeningTask = null;
if (!isGetEchoFromTheSameTransportAgent)
{
    listeningTask = listener.AcceptTcpClientAsync();
}

while (true)
{

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

    // TODO: check if the connection is been disconnected

    if (!isGetEchoFromTheSameTransportAgent)
    {
        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(1));
        Task anyTask = await Task.WhenAny(listeningTask, timeoutTask);
        if (anyTask == listeningTask)
        {
            // get message from listenerSession
            listenerSession = await listeningTask;
            await ReadFromClientAsync(listenerSession.GetStream());
            // disconnect listenerSession and start listening a new one
            listenerSession?.Dispose();
            listeningTask = listener.AcceptTcpClientAsync();
        }
        else
        {
            Console.WriteLine("No new connection initiated by the server. The disconnection from the remote session might be the cause.");
            client = await ReconnectToTheServerAsync(client);
        }
    }
    else
    {
        try
        {
            await ReadFromClientAsync(client.GetStream());
        }
        catch (System.IO.IOException ex)
        {
            Console.WriteLine($"Transmission failed: {ex.Message}");
            client = await ReconnectToTheServerAsync(client);
        }
    }
}
