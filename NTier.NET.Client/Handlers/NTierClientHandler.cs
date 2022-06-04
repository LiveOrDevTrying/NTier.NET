using Tcp.NET.Client.Handlers;
using Tcp.NET.Client.Models;
using Tcp.NET.Core.Models;

namespace NTier.NET.Client.Handlers
{
    public class NTierClientHandler : TcpClientHandlerBase<ConnectionTcp>
    {
        public NTierClientHandler(ParamsTcpClient parameters) : base(parameters)
        {
        }

        protected override ConnectionTcp CreateConnection(ConnectionTcp connection)
        {
            return connection;
        }
    }
}
