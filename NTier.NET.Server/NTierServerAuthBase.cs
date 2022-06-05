using Newtonsoft.Json;
using NTier.NET.Core.Enums;
using NTier.NET.Core.Models;
using NTier.NET.Server.Managers;
using NTier.NET.Server.Models;
using PHS.Networking.Enums;
using PHS.Networking.Server.Services;
using System;
using System.Threading.Tasks;
using Tcp.NET.Server;
using Tcp.NET.Server.Events.Args;
using Tcp.NET.Server.Handlers;
using Tcp.NET.Server.Models;

namespace NTier.NET.Server
{
    public abstract class NTierServerAuthBase<T, U, V, W, X, Y, Z, A, B, R> : 
        TcpNETServerAuthBase<T, U, V, W, X, Y, Z, A, B>
        where T : TcpConnectionServerAuthBaseEventArgs<Z, A>
        where U : TcpMessageServerAuthBaseEventArgs<Z, A>
        where V : TcpErrorServerAuthBaseEventArgs<Z, A>
        where W : ParamsTcpServerAuth
        where X : TcpHandlerServerAuthBase<T, U, V, W, B, Z, A>
        where Y : NTierConnectionManagerAuthBase<Z, A>
        where Z : NTierConnectionAuth<A>
        where B : TcpAuthorizeBaseEventArgs<Z, A>
        where R : Register
    {
        public NTierServerAuthBase(W parameters, IUserService<A> userService) : base(parameters, userService)
        {
        }

        public NTierServerAuthBase(W parameters, IUserService<A> userService, byte[] certificate, string certificatePassword) : base(parameters, userService, certificate, certificatePassword)
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
