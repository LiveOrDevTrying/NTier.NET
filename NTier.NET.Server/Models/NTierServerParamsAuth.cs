namespace NTier.NET.Server.Models
{
    public struct NTierServerParamsAuth : INTierServerParamsAuth
    {
        public int Port { get; set; }
        public string ConnectionSuccessString { get; set; }
        public string ConnectionUnauthorizedString { get; set; }
    }
}
