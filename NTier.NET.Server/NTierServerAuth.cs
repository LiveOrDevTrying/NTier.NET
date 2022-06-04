using NTier.NET.Core.Models;
using NTier.NET.Server.Events;
using NTier.NET.Server.Handlers;
using NTier.NET.Server.Managers;
using NTier.NET.Server.Models;
using PHS.Networking.Server.Services;
using System.Linq;
using Tcp.NET.Server.Events.Args;
using Tcp.NET.Server.Models;

namespace NTier.NET.Server
{
    public class NTierServerAuth<T> :
        NTierServerAuthBase<
            NTierConnectionServerAuthEventArgs<T>,
            NTierMessageServerAuthEventArgs<T>,
            NTierErrorServerAuthEventArgs<T>,
            ParamsTcpServerAuth,
            NTierHandlerAuth<T>,
            NTierConnectionManagerAuth<T>,
            NTierConnectionAuth<T>,
            T,
            NTierAuthorizeEventArgs<T>,
            Register>,
        INTierServerAuth<T>
    {
        public NTierServerAuth(ParamsTcpServerAuth parameters, IUserService<T> userService) : base(parameters, userService)
        {
        }

        public NTierServerAuth(ParamsTcpServerAuth parameters, IUserService<T> userService, byte[] certificate, string certificatePassword) : base(parameters, userService, certificate, certificatePassword)
        {
        }

        protected override NTierHandlerAuth<T> CreateHandler(byte[] certificate = null, string certificatePassword = null)
        {
            return certificate == null || certificate.Length <= 0 || certificate.All(x => x == 0) ?
                new NTierHandlerAuth<T>(_parameters) :
                new NTierHandlerAuth<T>(_parameters, certificate, certificatePassword);
        }

        protected override NTierConnectionManagerAuth<T> CreateConnectionManager()
        {
            return new NTierConnectionManagerAuth<T>();
        }

        protected override NTierErrorServerAuthEventArgs<T> CreateErrorEventArgs(TcpErrorServerBaseEventArgs<NTierConnectionAuth<T>> args)
        {
            return new NTierErrorServerAuthEventArgs<T>
            {
                Connection = args.Connection,
                Exception = args.Exception,
                Message = args.Message
            };
        }

        protected override NTierConnectionServerAuthEventArgs<T> CreateConnectionEventArgs(TcpConnectionServerBaseEventArgs<NTierConnectionAuth<T>> args)
        {
            return new NTierConnectionServerAuthEventArgs<T>
            {
                Connection = args.Connection,
                ConnectionEventType = args.ConnectionEventType
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
