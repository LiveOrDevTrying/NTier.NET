using NTier.NET.Server.Models;
using Tcp.NET.Server.Events.Args;

namespace NTier.NET.Server.Events
{
    public class NTierConnectionServerAuthEventArgs<T> : TcpConnectionServerAuthBaseEventArgs<NTierConnectionAuth<T>, T>
    {
    }
}
