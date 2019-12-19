using NTier.NET.Core.Enums;

namespace NTier.NET.Client.Models
{
    public interface IParameters
    {
        string EndOfLineCharacters { get; set; }
        int Port { get; set; }
        NTierRegisterType NTierRegisterType { get; set; }
        string Uri { get; set; }
    }
}