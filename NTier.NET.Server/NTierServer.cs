using NTier.NET.Core.Models;
using NTier.NET.Server.Events;
using NTier.NET.Server.Handlers;
using NTier.NET.Server.Managers;
using NTier.NET.Server.Models;
using System.Linq;
using Tcp.NET.Server.Events.Args;
using Tcp.NET.Server.Models;

namespace NTier.NET.Server
{
    public class NTierServer :
        NTierServerBase<NTierConnectionServerEventArgs,
            NTierMessageServerEventArgs,
            NTierErrorServerEventArgs,
            ParamsTcpServer,
            NTierHandler,
            NTierConnectionManager,
            NTierConnection, 
            Register>, 
        INTierServer
    {
        public NTierServer(ParamsTcpServer parameters) : base(parameters)
        {
        }

        public NTierServer(ParamsTcpServer parameters, byte[] certificate, string certificatePassword) : base(parameters, certificate, certificatePassword)
        {
        }

        protected override NTierConnectionManager CreateConnectionManager()
        {
            return new NTierConnectionManager();
        }

        protected override NTierHandler CreateHandler(byte[] certificate = null, string certificatePassword = null)
        {
            return certificate == null || certificate.Length <= 0 || certificate.All(x => x == 0) ?
                new NTierHandler(_parameters) :
                new NTierHandler(_parameters, certificate, certificatePassword);
        }

        protected override NTierErrorServerEventArgs CreateErrorEventArgs(TcpErrorServerBaseEventArgs<NTierConnection> args)
        {
            return new NTierErrorServerEventArgs
            {
                Connection = args.Connection,
                Exception = args.Exception,
                Message = args.Message
            };
        }
    }
}
