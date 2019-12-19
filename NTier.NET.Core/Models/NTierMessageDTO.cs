using NTier.NET.Core.Interfaces;

namespace NTier.NET.Core.Models
{
    public class NTierMessageDTO : NTierDTO, INTierMessageDTO
    {
        public IMessageDTO Message { get; set; }
        public bool IsFromWebapp { get; set; }
    }
}
