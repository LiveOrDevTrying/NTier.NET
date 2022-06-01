using NTier.NET.Server.Events;
using NTier.NET.Server.Models;
using PHS.Networking.Server.Services;

namespace NTier.NET.Server
{
    public interface INTierServer : ICoreNetworkingServer<
            NTierServerConnectionEventArgs,
            NTierServerMessageEventArgs,
            NTierServerErrorEventArgs,
            NTierConnection>
    {
    }
}