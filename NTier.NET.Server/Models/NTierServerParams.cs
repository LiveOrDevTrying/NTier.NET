namespace NTier.NET.Server.Models
{
    public struct NTierServerParams : INTierServerParams
    {
        public int Port { get; set; }
        public string ConnectionSuccessString { get; set; }
    }
}
