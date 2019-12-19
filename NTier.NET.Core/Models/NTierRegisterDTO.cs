using NTier.NET.Core.Enums;
using NTier.NET.Core.Interfaces;

namespace NTier.NET.Core.Models
{
    public class NTierRegisterDTO : NTierDTO, INTierRegisterDTO
    {
        public NTierRegisterType NTierRegisterType { get; set; }
    }
}
