using System.Text;
using SocketBackendFramework.Reply.Middlewares.Codec;
using SocketBackendFramework.Reply.Sample.Models;

namespace SocketBackendFramework.Reply.Sample.Codec
{
    public class DefaultHeaderCodec : IHeaderCodec<MiddlewareContext>
    {
        public void DecodeRequest(MiddlewareContext context)
        {
            int typeFieldOffset = 0;
            int typeFieldSize = 1;
            int typeField = context.PacketContext.RequestPacketRawBuffer[typeFieldOffset];
            context.RequestHeader.Type = (PacketType)typeField;

            int bodyOffset = (int)context.PacketContext.RequestPacketRawOffset + typeFieldSize;
            int bodySize = (int)context.PacketContext.RequestPacketRawSize - typeFieldSize;
            context.RequestBody.Message = Encoding.UTF8.GetString(context.PacketContext.RequestPacketRawBuffer,
                                                          bodyOffset,
                                                          bodySize);
        }

        public void EncodeResponse(MiddlewareContext context)
        {
            context.PacketContext.ResponsePacketRaw.Add(
                (byte)context.ResponseHeader.Type
            );
            context.PacketContext.ResponsePacketRaw.AddRange(
                Encoding.UTF8.GetBytes(context.ResponseBody.Message)
            );
        }
    }
}
