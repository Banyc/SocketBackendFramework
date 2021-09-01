using SocketBackendFramework.Models;
using SocketBackendFramework.Models.Listeners;

namespace SocketBackendFramework.Listeners
{
    public class Listener
    {
        private readonly ListenerConfig config;
        private readonly ListenersMapper mapper;

        public Listener(ListenerConfig config, ListenersMapper mapper)
        {
            this.config = config;
            this.mapper = mapper;

            // build system socket
            // don't start listening yet
        }

        public void Start()
        {
            // activate socket
        }

        public void Respond(SocketContext context)
        {
            if (!context.ShouldRespond)
            {
                return;
            }
        }
    }
}
