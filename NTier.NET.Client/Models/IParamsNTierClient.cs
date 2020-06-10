using NTier.NET.Core.Enums;
using Tcp.NET.Client.Models;

namespace NTier.NET.Client.Models
{
    public interface IParamsNTierClient
    {
        RegisterType RegisterType { get; set; }
        int ReconnectIntervalSec { get; set; }
        int Port { get; set; }
        string Uri { get; set; }
        bool IsSSL { get; set; }
    }
}