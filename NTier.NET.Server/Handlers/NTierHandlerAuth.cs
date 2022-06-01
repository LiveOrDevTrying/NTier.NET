using NTier.NET.Server.Events;
using NTier.NET.Server.Models;
using PHS.Networking.Enums;
using PHS.Networking.Events.Args;
using PHS.Networking.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Server.Events.Args;
using Tcp.NET.Server.Handlers;
using Tcp.NET.Server.Models;

namespace NTier.NET.Server.Handlers
{
    public class NTierHandlerAuth<T> :
        TcpHandlerServerAuthBase<
            NTierConnectionServerAuthEventArgs<T>,
            NTierMessageServerAuthEventArgs<T>,
            NTierErrorServerAuthEventArgs<T>,
            ParamsTcpServerAuth,
            NTierAuthorizeEventArgs<T>,
            NTierConnectionAuth<T>,
            T>
    {
        public NTierHandlerAuth(ParamsTcpServerAuth parameters) : base(parameters)
        {
        }

        public NTierHandlerAuth(ParamsTcpServerAuth parameters, byte[] certificate, string certificatePassword) : base(parameters, certificate, certificatePassword)
        {
        }

        public override Task AuthorizeCallbackAsync(TcpAuthorizeBaseEventArgs<NTierConnectionAuth<T>, T> args, CancellationToken cancellationToken)
        {
            FireEvent(this, new NTierConnectionServerAuthEventArgs<T>
            {
                ConnectionEventType = ConnectionEventType.Connected,
                Connection = args.Connection
            });

            return Task.CompletedTask;
        }

        protected override NTierAuthorizeEventArgs<T> CreateAuthorizeEventArgs(TcpAuthorizeBaseEventArgs<NTierConnectionAuth<T>, T> args)
        {
            return new NTierAuthorizeEventArgs<T>
            {
                Connection = args.Connection,
                Token = args.Token
            };
        }

        protected override NTierConnectionAuth<T> CreateConnection(ConnectionTcpClient connection)
        {
            return new NTierConnectionAuth<T>
            {
                ConnectionId = Guid.NewGuid().ToString(),
                TcpClient = connection.TcpClient,
                RegisterType = null
            };
        }

        protected override NTierConnectionServerAuthEventArgs<T> CreateConnectionEventArgs(ConnectionEventArgs<NTierConnectionAuth<T>> args)
        {
            return new NTierConnectionServerAuthEventArgs<T>
            {
                Connection = args.Connection,
                ConnectionEventType = args.ConnectionEventType
            };
        }

        protected override NTierErrorServerAuthEventArgs<T> CreateErrorEventArgs(ErrorEventArgs<NTierConnectionAuth<T>> args)
        {
            return new NTierErrorServerAuthEventArgs<T>
            {
                Connection = args.Connection,
                Exception = args.Exception,
                Message = args.Message
            };
        }

        protected override NTierMessageServerAuthEventArgs<T> CreateMessageEventArgs(TcpMessageServerBaseEventArgs<NTierConnectionAuth<T>> args)
        {
            return new NTierMessageServerAuthEventArgs<T>
            {
                Bytes = args.Bytes,
                Connection = args.Connection,
                Message = args.Message,
                MessageEventType = args.MessageEventType
            };
        }
    }
}
