using NTier.NET.Core.Interfaces;

namespace NTier.NET.Core.Models
{
    public class NTierMessageTypedDTO<T> : NTierMessageDTO where T : IMessageDTO
    {
        public new T Message { get; set; }
    }
}
