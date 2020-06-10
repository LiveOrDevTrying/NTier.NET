using NTier.NET.Core.Enums;

namespace NTier.NET.Client.Models
{
    public struct ParamsNTierClient : IParamsNTierClient
    {
        public RegisterType RegisterType { get; set; }
        public string Uri { get; set; }
        public int Port { get; set; }
        public int ReconnectIntervalSec { get; set; }
        public bool IsSSL { get; set; }
    }
}
