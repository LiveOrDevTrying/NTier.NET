using System.Collections.Concurrent;
using Tcp.NET.Server.Managers;
using Tcp.NET.Server.Models;

namespace NTier.NET.Server.Managers
{
    public abstract class NTierConnectionManagerAuthBase<Z, A> : TcpConnectionManagerAuthBase<Z, A> where Z : IdentityTcpServer<A>
    {
        protected ConcurrentQueue<Z> _serviceConnections = new ConcurrentQueue<Z>();

        public virtual void QueueServiceConnection(Z connection)
        {
            _serviceConnections.Enqueue(connection);
        }

        public virtual bool DequeueServiceConnection(out Z connection)
        {
            return _serviceConnections.TryDequeue(out connection);
        }
    }
}
