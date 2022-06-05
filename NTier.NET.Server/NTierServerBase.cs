using Newtonsoft.Json;
using NTier.NET.Core.Enums;
using NTier.NET.Core.Models;
using NTier.NET.Server.Managers;
using NTier.NET.Server.Models;
using PHS.Networking.Enums;
using System;
using System.Threading.Tasks;
using Tcp.NET.Server;
using Tcp.NET.Server.Events.Args;
using Tcp.NET.Server.Handlers;
using Tcp.NET.Server.Models;

namespace NTier.NET.Server
{
    public abstract class NTierServerBase<T, U, V, W, X, Y, Z, R> : 
        TcpNETServerBase<T, U, V, W, X, Y, Z>
        where T : TcpConnectionServerBaseEventArgs<Z>
        where U : TcpMessageServerBaseEventArgs<Z>
        where V : TcpErrorServerBaseEventArgs<Z>
        where W : ParamsTcpServer
        where X : TcpHandlerServerBase<T, U, V, W, Z>
        where Y : NTierConnectionManagerBase<Z>
        where Z : NTierConnection
        where R : Register
    {
        public NTierServerBase(W parameters) : base(parameters)
        {
        }

        public NTierServerBase(W parameters, byte[] certificate, string certificatePassword) : base(parameters, certificate, certificatePassword)
        {
        }

        protected override void OnMessageEvent(object sender, U args)
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
                            var deserialized = JsonConvert.DeserializeObject<R>(args.Message);
                            connection.RegisterType = deserialized.RegisterType;

                            if (connection.RegisterType == RegisterType.Service)
                            {
                                _connectionManager.QueueServiceConnection(connection);
                            }
                        }
                        else
                        {

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
                                    // When a Provider sends a message to a processing server, round-robin send it to the processing server
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
                    catch (Exception ex)
                    {
                        FireEvent(this, CreateErrorEventArgs(new TcpErrorServerBaseEventArgs<Z>
                        {
                            Connection = args.Connection,
                            Exception = ex,
                            Message = ex.Message
                        }));
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
