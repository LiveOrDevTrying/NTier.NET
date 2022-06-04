using System.Collections.Concurrent;
using Tcp.NET.Server.Managers;
using Tcp.NET.Server.Models;

namespace NTier.NET.Server.Managers
{
    public abstract class NTierConnectionManagerBase<T> : TcpConnectionManagerBase<T> where T : ConnectionTcpServer
    {
        protected ConcurrentQueue<T> _serviceConnections = new ConcurrentQueue<T>();

        public virtual void QueueServiceConnection(T connection)
        {
            _serviceConnections.Enqueue(connection);
        }

        public virtual bool DequeueServiceConnection(out T connection)
        {
            return _serviceConnections.TryDequeue(out connection);
        }
    }
}
