using NTier.NET.Core.Enums;

namespace NTier.NET.Core.Models
{
    public class NTierRegisterDTO : NTierDTO, INTierRegisterDTO
    {
        public NTierRegisterType NTierRegisterType { get; set; }
    }
}
