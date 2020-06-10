namespace NTier.NET.Core.Models
{
    public class Message : NTierBase, IMessage
    {
        public string Content { get; set; }
    }
}
