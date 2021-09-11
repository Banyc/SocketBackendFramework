using System.Text;
using System.Net.Sockets;

bool isGetEchoFromTheSameTransportAgent;
Console.Write("Get echo from the same transport agent? (Y/n) > ");
isGetEchoFromTheSameTransportAgent = string.Compare(Console.ReadLine(), "n", ignoreCase: true) != 0;
Console.WriteLine($"Your choice: {isGetEchoFromTheSameTransportAgent}");

UdpClient client = new();

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
    Console.Write("Send > ");
    string userMessage = Console.ReadLine();
    if (userMessage != "")
    {
        hello = userMessage;
    }
    byte[] helloBytes = Encoding.UTF8.GetBytes(hello);
    List<byte> packet = new(header);
    packet.AddRange(helloBytes);
    int bytesSent = client.Send(packet.ToArray(), packet.Count, "127.0.0.1", 8080);
    Console.WriteLine($"Sent     \"{hello}\" ({bytesSent} bytes).");
    var result = await client.ReceiveAsync();
    var response = result.Buffer;
    Console.WriteLine($"Received \"{Encoding.UTF8.GetString(response, 1, response.Length - 1)}\" ({response.Length} bytes).");
}
