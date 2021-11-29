using System;
using System.Text;
using SocketBackendFramework.Relay.Models.Transport.PacketContexts;
using SocketBackendFramework.Relay.Pipeline.Middlewares.Codec;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Models;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Middlewares
{
    public class DefaultHeaderCodec : IHeaderCodec<DefaultMiddlewareContext>
    {
        public void DecodeRequest(DefaultMiddlewareContext context)
        {
            if (context.Request!.PacketContext!.EventType !=
                DownwardEventType.ApplicationMessageReceived)
            {
                return;
            }
            int typeFieldOffset = 0;
            int typeFieldSize = 1;
            int typeField = context.Request!.PacketContext!.PacketRawBuffer![typeFieldOffset];
            context.Request.Header.Type = (DefaultPacketHeaderType)typeField;

            int bodyOffset = (int)context.Request.PacketContext.PacketRawOffset + typeFieldSize;
            int bodySize = (int)context.Request.PacketContext.PacketRawSize - typeFieldSize;
            context.Request.Body.Message = Encoding.UTF8.GetString(
                context.Request.PacketContext.PacketRawBuffer,
                bodyOffset,
                bodySize);
        }

        public void EncodeResponse(DefaultMiddlewareContext context)
        {
            if (context.Response!.PacketContext.ActionType != UpwardActionType.SendApplicationMessage)
            {
                return;
            }
            context.Response.PacketContext.PacketRawOffset = 0;
            int typeFieldOffset = 0;
            int typeFieldSize = 1;
            context.Response.PacketContext.PacketRawSize = typeFieldSize + context.Response.Body.Message.Length;
            context.Response.PacketContext.PacketRawBuffer = new byte[context.Response.PacketContext.PacketRawSize];
            context.Response.PacketContext.PacketRawBuffer[typeFieldOffset] =
                (byte)context.Response.Header.Type;
            Span<byte> bodyBuffer = context.Response.PacketContext.PacketRawBuffer.AsSpan(
                typeFieldOffset + typeFieldSize);
            Encoding.UTF8.GetBytes(context.Response.Body.Message, bodyBuffer);
        }
    }
}
