using NTier.NET.Core.Events;
using NTier.NET.Core.Models;
using System;

namespace NTier.NET.Client
{
    public interface INTierClient : IDisposable
    {
        event NTierMessageEventHandler NTierMessageEvent;

        void SendMessageToMessageCache<T>(T message, bool isFromWebapp) where T : MessageDTO;
    }
}