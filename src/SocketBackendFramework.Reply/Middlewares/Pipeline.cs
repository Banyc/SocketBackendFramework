using SocketBackendFramework.Reply.Models.Middlewares;

namespace SocketBackendFramework.Reply.Middlewares
{
    public class Pipeline<TMiddlewareContext>
    {
        public MiddlewareRequestDelegate<TMiddlewareContext> Entry { get; set; }
    }
}
