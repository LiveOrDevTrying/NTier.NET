using System;
using System.Threading.Tasks;

namespace NTier.NET.Server
{
    public interface INTierServer : IDisposable
    {
        Task StartAsync();
        Task StopAsync();
    }
}