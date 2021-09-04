using SocketBackendFramework.Models.Middlewares;

namespace SocketBackendFramework.Middlewares
{
    public class Pipeline<TMiddlewareContext>
    {
        public MiddlewareRequestDelegate<TMiddlewareContext> Entry { get; set; }
    }
}
