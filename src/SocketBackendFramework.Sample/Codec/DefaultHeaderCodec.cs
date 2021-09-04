using SocketBackendFramework.Middlewares.Codec;
using SocketBackendFramework.Models;

namespace SocketBackendFramework.Sample.Codec
{
    public class DefaultHeaderCodec : IHeaderCodec
    {
        public void DecodeRequest(PacketContext context)
        {
        }

        public void EncodeResponse(PacketContext context)
        {
        }
    }
}
