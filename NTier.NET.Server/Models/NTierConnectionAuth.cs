using NTier.NET.Core.Enums;
using Tcp.NET.Server.Models;

namespace NTier.NET.Server.Models
{
    public class NTierConnectionAuth<T> : IdentityTcpServer<T>
    {
        public RegisterType? RegisterType { get; set; }
    }
}
