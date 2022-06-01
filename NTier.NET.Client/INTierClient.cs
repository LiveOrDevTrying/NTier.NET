using NTier.NET.Client.Events;
using PHS.Networking.Services;
using Tcp.NET.Core.Models;

namespace NTier.NET.Client
{
    public interface INTierClient : 
        ICoreNetworkingClient<
            NTierConnectionClientEventArgs,
            NTierMessageClientEventArgs,
            NTierErrorClientEventArgs,
            ConnectionTcp>
    {
    }
}