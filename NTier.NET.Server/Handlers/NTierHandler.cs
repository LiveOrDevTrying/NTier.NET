using NTier.NET.Server.Events;
using NTier.NET.Server.Models;
using PHS.Networking.Events.Args;
using PHS.Networking.Models;
using System;
using Tcp.NET.Server.Events.Args;
using Tcp.NET.Server.Handlers;
using Tcp.NET.Server.Models;

namespace NTier.NET.Server.Handlers
{
    public class NTierHandler :
        TcpHandlerServerBase<
            NTierServerConnectionEventArgs,
            NTierServerMessageEventArgs,
            NTierServerErrorEventArgs,
            ParamsTcpServer,
            NTierConnection>
    {
        public NTierHandler(ParamsTcpServer parameters) : base(parameters)
        {
        }

        public NTierHandler(ParamsTcpServer parameters, byte[] certificate, string certificatePassword) : base(parameters, certificate, certificatePassword)
        {
        }

        protected override NTierConnection CreateConnection(ConnectionTcpClient connection)
        {
            return new NTierConnection
            {
                ConnectionId = Guid.NewGuid().ToString(),
                TcpClient = connection.TcpClient,
                RegisterType = null
            };
        }

        protected override NTierServerConnectionEventArgs CreateConnectionEventArgs(ConnectionEventArgs<NTierConnection> args)
        {
            return new NTierServerConnectionEventArgs
            {
                Connection = args.Connection,
                ConnectionEventType = args.ConnectionEventType
            };
        }

        protected override NTierServerErrorEventArgs CreateErrorEventArgs(ErrorEventArgs<NTierConnection> args)
        {
            return new NTierServerErrorEventArgs
            {
                Connection = args.Connection,
                Exception = args.Exception,
                Message = args.Message
            };
        }

        protected override NTierServerMessageEventArgs CreateMessageEventArgs(TcpMessageServerBaseEventArgs<NTierConnection> args)
        {
            return new NTierServerMessageEventArgs
            {
                Bytes = args.Bytes,
                Connection = args.Connection,
                Message = args.Message,
                MessageEventType = args.MessageEventType
            };
        }
    }
}
