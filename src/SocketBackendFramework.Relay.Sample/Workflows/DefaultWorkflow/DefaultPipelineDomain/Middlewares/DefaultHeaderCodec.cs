using System.Text;
using SocketBackendFramework.Relay.Models;
using SocketBackendFramework.Relay.Pipeline.Middlewares.Codec;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Models;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Middlewares
{
    public class DefaultHeaderCodec : IHeaderCodec<DefaultMiddlewareContext>
    {
        public void DecodeRequest(DefaultMiddlewareContext context)
        {
            if (context.PacketContext.PacketContextType != PacketContextType.ApplicationMessage)
            {
                return;
            }
            int typeFieldOffset = 0;
            int typeFieldSize = 1;
            int typeField = context.PacketContext.RequestPacketRawBuffer[typeFieldOffset];
            context.RequestHeader.Type = (DefaultPacketHeaderType)typeField;

            int bodyOffset = (int)context.PacketContext.RequestPacketRawOffset + typeFieldSize;
            int bodySize = (int)context.PacketContext.RequestPacketRawSize - typeFieldSize;
            context.RequestBody.Message = Encoding.UTF8.GetString(context.PacketContext.RequestPacketRawBuffer,
                                                          bodyOffset,
                                                          bodySize);
        }

        public void EncodeResponse(DefaultMiddlewareContext context)
        {
            if (context.PacketContext.PacketContextType != PacketContextType.ApplicationMessage)
            {
                return;
            }
            context.PacketContext.ResponsePacketRaw.Add(
                (byte)context.ResponseHeader.Type
            );
            context.PacketContext.ResponsePacketRaw.AddRange(
                Encoding.UTF8.GetBytes(context.ResponseBody.Message)
            );
        }
    }
}
