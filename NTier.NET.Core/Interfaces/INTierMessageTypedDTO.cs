namespace NTier.NET.Core.Interfaces
{
    public interface INTierMessageTypedDTO<T> where T : INTierMessageDTO
    {
        bool IsFromWebapp { get; set; }
        T Message { get; set; }
    }
}