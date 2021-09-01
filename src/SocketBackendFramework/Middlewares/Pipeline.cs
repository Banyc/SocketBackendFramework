using SocketBackendFramework.Models.Middlewares;

namespace SocketBackendFramework.Middlewares
{
    public class Pipeline
    {
        public SocketRequestDelegate Entry { get; set; }
    }
}
