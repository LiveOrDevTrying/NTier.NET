using NTier.NET.Core.Events;
using System;
using System.Threading.Tasks;

namespace NTier.NET.Client
{
    public interface INTierClient : IDisposable
    {
        event MessageEventHandler MessageEvent;

        Task SendToServerAsync<T>(T instance) where T : class;
        Task SendToServerAsync(string message);
    }
}