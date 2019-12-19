using NTier.NET.Core.Enums;

namespace NTier.NET.Client.Models
{
    public struct Parameters : IParameters
    {
        public NTierRegisterType NTierRegisterType { get; set; }
        public string Uri { get; set; }
        public int Port { get; set; }
        public string EndOfLineCharacters { get; set; }
    }
}
