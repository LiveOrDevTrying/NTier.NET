using NTier.NET.Core.Enums;
using NTier.NET.Core.Interfaces;

namespace NTier.NET.Core.Models
{
    public class NTierDTO : INTierDTO
    {
        public NTierMessageType NTierMessageType { get; set; }
    }
}
