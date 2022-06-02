using NTier.NET.Core.Enums;
using Tcp.NET.Client.Models;

namespace NTier.NET.Client.Models
{
    public class ParamsNTierClient : ParamsTcpClient
    {
        public ParamsNTierClient(string host, int port, RegisterType registerType, string endOfLineCharacters, bool isSSL, string token = "", int reconnectIntervalSec = 30, bool onlyEmitBytes = false, string pingCharacters = "ping", string pongCharacters = "pong") : base(host, port, endOfLineCharacters, isSSL, token, onlyEmitBytes, pingCharacters, pongCharacters)
        {
            RegisterType = registerType;
            ReconnectIntervalSec = reconnectIntervalSec;
        }

        public ParamsNTierClient(string host, int port, RegisterType registerType, byte[] endOfLineBytes, bool isSSL, byte[] token = null, int reconnectIntervalSec = 30, bool onlyEmitBytes = true, byte[] pingBytes = null, byte[] pongBytes = null) : base(host, port, endOfLineBytes, isSSL, token, onlyEmitBytes, pingBytes, pongBytes)
        {
            RegisterType = registerType;
            ReconnectIntervalSec = reconnectIntervalSec;
        }

        public RegisterType RegisterType { get; protected set; }
        public int ReconnectIntervalSec { get; protected set; }
    }
}
