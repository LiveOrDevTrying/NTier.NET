using NTier.NET.Client.Events;
using NTier.NET.Client.Models;
using Tcp.NET.Client.Handlers;
using Tcp.NET.Core.Events.Args;
using Tcp.NET.Core.Models;

namespace NTier.NET.Client.Handlers
{
    public class NTierClientHandler : 
        TcpClientHandlerBase<
            NTierConnectionClientEventArgs,
            NTierMessageClientEventArgs,
            NTierErrorClientEventArgs,
            ParamsNTierClient,
            ConnectionTcp>
    {
        public NTierClientHandler(ParamsNTierClient parameters) : base(parameters)
        {
        }

        protected override ConnectionTcp CreateConnection(ConnectionTcp connection)
        {
            return connection;
        }

        protected override NTierConnectionClientEventArgs CreateConnectionEventArgs(TcpConnectionEventArgs<ConnectionTcp> args)
        {
            return new NTierConnectionClientEventArgs
            {
                Connection = args.Connection,
                ConnectionEventType = args.ConnectionEventType
            };
        }

        protected override NTierErrorClientEventArgs CreateErrorEventArgs(TcpErrorEventArgs<ConnectionTcp> args)
        {
            return new NTierErrorClientEventArgs
            {
                Connection = args.Connection,
                Exception = args.Exception,
                Message = args.Message
            };
        }

        protected override NTierMessageClientEventArgs CreateMessageEventArgs(TcpMessageEventArgs<ConnectionTcp> args)
        {
            return new NTierMessageClientEventArgs
            {
                Bytes = args.Bytes,
                Connection = args.Connection,
                Message = args.Message,
                MessageEventType = args.MessageEventType
            };
        }
    }
}
