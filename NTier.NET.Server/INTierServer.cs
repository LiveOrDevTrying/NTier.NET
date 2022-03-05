using System;
using System.Threading.Tasks;

namespace NTier.NET.Server
{
    public interface INTierServer : IDisposable
    {
        void Start();
        void Stop();
    }
}