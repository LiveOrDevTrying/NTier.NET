using NTier.NET.Core.Events;
using System;
using System.Threading.Tasks;

namespace NTier.NET.Client
{
    public interface INTierClient : IDisposable
    {
        event MessageEventHandler MessageEvent;

        Task StartAsync();
        Task StopAsync();

        Task SendToServerAsync<T>(T instance) where T : class;
        Task SendToServerAsync(string message);
    }
}