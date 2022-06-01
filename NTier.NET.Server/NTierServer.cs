using Newtonsoft.Json;
using NTier.NET.Core.Enums;
using NTier.NET.Core.Models;
using NTier.NET.Server.Events;
using NTier.NET.Server.Handlers;
using NTier.NET.Server.Managers;
using NTier.NET.Server.Models;
using PHS.Networking.Enums;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Tcp.NET.Server;
using Tcp.NET.Server.Models;

namespace NTier.NET.Server
{
    public class NTierServer : 
        TcpNETServerBase<
            NTierServerConnectionEventArgs, 
            NTierServerMessageEventArgs, 
            NTierServerErrorEventArgs,
            ParamsTcpServer,
            NTierHandler,
            NTierConnectionManager,
            NTierConnection>, 
        INTierServer
    {
        protected readonly ConcurrentBag<NTierConnection> _connections =
            new ConcurrentBag<NTierConnection>();

        public NTierServer(ParamsTcpServer parameters) : base(parameters)
        {
        }

        public NTierServer(ParamsTcpServer parameters, byte[] certificate, string certificatePassword) : base(parameters, certificate, certificatePassword)
        {
        }

        protected override NTierConnectionManager CreateConnectionManager()
        {
            return new NTierConnectionManager();
        }

        protected override NTierHandler CreateHandler(byte[] certificate = null, string certificatePassword = null)
        {
            return certificate == null || certificate.Length <= 0 || certificate.All(x => x == 0) ?
                new NTierHandler(_parameters) :
                new NTierHandler(_parameters, certificate, certificatePassword);
        }

        protected override void OnConnectionEvent(object sender, NTierServerConnectionEventArgs args)
        {
            switch (args.ConnectionEventType)
            {
                case ConnectionEventType.Connected:
                    _connectionManager.Add(args.Connection.ConnectionId, args.Connection);
                    break;
                case ConnectionEventType.Disconnect:
                    _connectionManager.Remove(args.Connection.ConnectionId);
                    break;
                default:
                    break;
            }

            FireEvent(this, args);
        }

        protected override void OnErrorEvent(object sender, NTierServerErrorEventArgs args)
        {
            FireEvent(this, args);
        }

        protected override void OnMessageEvent(object sender, NTierServerMessageEventArgs args)
        {
            switch (args.MessageEventType)
            {
                case MessageEventType.Sent:
                    break;
                case MessageEventType.Receive:
                    try
                    {
                        if (!_connectionManager.Get(args.Connection.ConnectionId, out var connection))
                        {
                            return;
                        }

                        if (connection.RegisterType == null)
                        {
                            // Register the connection if it is new
                            var deserialized = JsonConvert.DeserializeObject<Register>(args.Message);
                            connection.RegisterType = deserialized.RegisterType;

                            if (connection.RegisterType == RegisterType.Service)
                            {
                                _connectionManager.QueueServiceConnection(connection);
                            }
                        }
                        else
                        {
                            // When a Provider sends a message to the cache, round-robin send it to the processing server

                            switch (args.Connection.RegisterType.Value)
                            {
                                case RegisterType.Service:
                                    foreach (var connectionProvider in _connectionManager.GetAll())
                                    {
                                        if (connectionProvider.RegisterType == RegisterType.Provider)
                                        {
                                            Task.Run(async () =>
                                            {
                                                await SendToConnectionAsync(args.Message, connectionProvider);
                                            });
                                        }
                                    }
                                    break;
                                case RegisterType.Provider:
                                    if (_connectionManager.DequeueServiceConnection(out var connectionService))
                                    {
                                        _connectionManager.QueueServiceConnection(connectionService);
                                        Task.Run(async () =>
                                        {
                                            await SendToConnectionAsync(args.Message, connectionService);
                                        });
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    catch
                    { }
                    break;
                default:
                    break;
            }
        }
    }
}
