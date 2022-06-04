using NTier.NET.Server.Events;
using NTier.NET.Server.Models;
using PHS.Networking.Server.Services;

namespace NTier.NET.Server
{
    public interface INTierServerAuth<T> : ICoreNetworkingServer<
            NTierConnectionServerAuthEventArgs<T>,
            NTierMessageServerAuthEventArgs<T>,
            NTierErrorServerAuthEventArgs<T>,
            NTierConnectionAuth<T>>
    {
    }
}