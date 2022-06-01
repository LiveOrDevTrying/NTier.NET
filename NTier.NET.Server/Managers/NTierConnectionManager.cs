using NTier.NET.Server.Models;
using System.Collections.Concurrent;
using Tcp.NET.Server.Managers;

namespace NTier.NET.Server.Managers
{
    public class NTierConnectionManager : TcpConnectionManager<NTierConnection>
    {
        protected ConcurrentQueue<NTierConnection> _serviceConnections = new ConcurrentQueue<NTierConnection>();

        public virtual void QueueServiceConnection(NTierConnection connection)
        {
            _serviceConnections.Enqueue(connection);
        }

        public virtual bool DequeueServiceConnection(out NTierConnection connection)
        {
            return _serviceConnections.TryDequeue(out connection);
        }
    }
}
