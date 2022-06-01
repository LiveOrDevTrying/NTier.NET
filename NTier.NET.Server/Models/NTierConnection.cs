using NTier.NET.Core.Enums;
using Tcp.NET.Server.Models;

namespace NTier.NET.Server.Models
{
    public class NTierConnection : ConnectionTcpServer
    {
        public RegisterType? RegisterType { get; set; }
    }
}
