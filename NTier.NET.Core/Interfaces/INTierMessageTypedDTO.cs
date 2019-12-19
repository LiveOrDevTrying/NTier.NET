namespace NTier.NET.Core.Models
{
    public interface INTierMessageTypedDTO<T> where T : INTierMessageDTO
    {
        bool IsFromWebapp { get; set; }
        T Message { get; set; }
    }
}