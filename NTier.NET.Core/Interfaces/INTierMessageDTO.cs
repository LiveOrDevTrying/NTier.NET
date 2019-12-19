using NTier.NET.Core.Interfaces;

namespace NTier.NET.Core.Models
{
    public interface INTierMessageDTO
    {
        bool IsFromWebapp { get; set; }
        IMessageDTO Message { get; set; }
    }
}