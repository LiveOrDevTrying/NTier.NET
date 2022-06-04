using NTier.NET.Server.Models;
using Tcp.NET.Server.Events.Args;

namespace NTier.NET.Server.Events
{
    public class NTierAuthorizeEventArgs<U> : TcpAuthorizeBaseEventArgs<NTierConnectionAuth<U>, U>
    {
    }
}
