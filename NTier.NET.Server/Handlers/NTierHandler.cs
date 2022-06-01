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
            NTierConnectionServerEventArgs,
            NTierMessageServerEventArgs,
            NTierErrorServerEventArgs,
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

        protected override NTierConnectionServerEventArgs CreateConnectionEventArgs(ConnectionEventArgs<NTierConnection> args)
        {
            return new NTierConnectionServerEventArgs
            {
                Connection = args.Connection,
                ConnectionEventType = args.ConnectionEventType
            };
        }

        protected override NTierErrorServerEventArgs CreateErrorEventArgs(ErrorEventArgs<NTierConnection> args)
        {
            return new NTierErrorServerEventArgs
            {
                Connection = args.Connection,
                Exception = args.Exception,
                Message = args.Message
            };
        }

        protected override NTierMessageServerEventArgs CreateMessageEventArgs(TcpMessageServerBaseEventArgs<NTierConnection> args)
        {
            return new NTierMessageServerEventArgs
            {
                Bytes = args.Bytes,
                Connection = args.Connection,
                Message = args.Message,
                MessageEventType = args.MessageEventType
            };
        }
    }
}
