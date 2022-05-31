using Newtonsoft.Json;
using NTier.NET.Core.Enums;
using NTier.NET.Core.Models;
using NTier.NET.Server.Models;
using PHS.Networking.Enums;
using PHS.Networking.Models;
using PHS.Networking.Server.Enums;
using PHS.Networking.Server.Events.Args;
using PHS.Networking.Server.Services;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Tcp.NET.Server;
using Tcp.NET.Server.Events.Args;
using Tcp.NET.Server.Models;

namespace NTier.NET.Server
{
    public class NTierServerAuth<T> : INTierServer
    {
        protected readonly ITcpNETServerAuth<T> _server;
        protected readonly ConcurrentQueue<IdentityTcpServer<T>> _connectionsToServices =
            new ConcurrentQueue<IdentityTcpServer<T>>();
        protected readonly ConcurrentBag<IdentityTcpServer<T>> _connectionsToProviders =
            new ConcurrentBag<IdentityTcpServer<T>>();
        protected readonly ConcurrentDictionary<string, IdentityTcpServer<T>> _connectionsUnregistered =
            new ConcurrentDictionary<string, IdentityTcpServer<T>>();

        public NTierServerAuth(INTierServerParamsAuth parameters, IUserService<T> userService)
        {
            _server = new TcpNETServerAuth<T>(new ParamsTcpServerAuth(parameters.Port, "\r\n", parameters.ConnectionSuccessString, parameters.ConnectionUnauthorizedString), userService);
            _server.ConnectionEvent += OnConnectionEvent;
            _server.ErrorEvent += OnErrorEvent;
            _server.MessageEvent += OnMessageEvent;
            _server.ServerEvent += OnServerEvent;
        }
        public NTierServerAuth(INTierServerParamsAuth parameters, IUserService<T> userService, byte[] certificate, string certificatePassword)
        {
            _server = new TcpNETServerAuth<T>(new ParamsTcpServerAuth(parameters.Port, "\r\n", parameters.ConnectionSuccessString, parameters.ConnectionUnauthorizedString), userService, certificate, certificatePassword);
            _server.ConnectionEvent += OnConnectionEvent;
            _server.ErrorEvent += OnErrorEvent;
            _server.MessageEvent += OnMessageEvent;
            _server.ServerEvent += OnServerEvent;
        }

        public virtual void Start()
        {
            _server.Start();
        }

        public virtual void Stop()
        {
            _server.Stop();
        }

        protected virtual void OnMessageEvent(object sender, TcpMessageServerAuthEventArgs<T> args)
        {
            switch (args.MessageEventType)
            {
                case MessageEventType.Sent:
                    break;
                case MessageEventType.Receive:
                    try
                    {
                        if (_connectionsUnregistered.TryRemove(args.Connection.ConnectionId, out var tcpClient))
                        {
                            // Register the connection if it is new
                            var registerDTO = JsonConvert.DeserializeObject<Register>(args.Message);

                            switch (registerDTO.RegisterType)
                            {
                                case RegisterType.Service:
                                    _connectionsToServices.Enqueue(args.Connection);
                                    break;
                                case RegisterType.Provider:
                                    _connectionsToProviders.Add(args.Connection);
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            // When a Provider sends a message to the cache, round-robin send it to the processing server
                            var deserialized = JsonConvert.DeserializeObject<Message>(args.Message);

                            switch (deserialized.MessageType)
                            {
                                case MessageType.FromProvider:
                                    if (_connectionsToServices.TryDequeue(out var connectionService))
                                    {
                                        _connectionsToServices.Enqueue(connectionService);
                                        Task.Run(async () =>
                                        {
                                            await _server.SendToConnectionAsync(args.Message, connectionService);
                                        });
                                    }
                                    break;
                                case MessageType.FromService:
                                    foreach (var connectionToProvider in _connectionsToProviders)
                                    {
                                        Task.Run(async () =>
                                        {
                                            await _server.SendToConnectionAsync(args.Message, connectionToProvider);
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
        protected virtual void OnErrorEvent(object sender, TcpErrorServerAuthEventArgs<T> args)
        {
        }
        protected virtual void OnConnectionEvent(object sender, TcpConnectionServerAuthEventArgs<T> args)
        {
            switch (args.ConnectionEventType)
            {
                case ConnectionEventType.Connected:
                    _connectionsUnregistered.TryAdd(args.Connection.ConnectionId, args.Connection);
                    break;
                case ConnectionEventType.Disconnect:
                    _connectionsUnregistered.TryRemove(args.Connection.ConnectionId, out var _);
                    break;
                default:
                    break;
            }
        }
        protected virtual void OnServerEvent(object sender, ServerEventArgs args)
        {
            switch (args.ServerEventType)
            {
                case ServerEventType.Start:
                    break;
                case ServerEventType.Stop:
                    while (_connectionsToServices.TryDequeue(out var _)) { }
                    break;
                default:
                    break;
            }
        }

        public virtual void Dispose()
        {
            if (_server != null)
            {
                _server.ConnectionEvent -= OnConnectionEvent;
                _server.ErrorEvent -= OnErrorEvent;
                _server.MessageEvent -= OnMessageEvent;
                _server.ServerEvent -= OnServerEvent;
                _server.Dispose();
            }
        }
    }
}
