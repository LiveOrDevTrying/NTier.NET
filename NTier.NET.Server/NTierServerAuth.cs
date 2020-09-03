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
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Tcp.NET.Server;
using Tcp.NET.Server.Events.Args;
using Tcp.NET.Server.Models;

namespace NTier.NET.Server
{
    public class NTierServerAuth<T> : INTierServer
    {
        protected readonly ITcpNETServerAuth<T> _server;
        protected readonly ConcurrentQueue<IConnectionServer> _connectionsToServices =
            new ConcurrentQueue<IConnectionServer>();
        protected readonly ConcurrentBag<IConnectionServer> _connectionsToProviders =
            new ConcurrentBag<IConnectionServer>();
        protected readonly ConcurrentDictionary<int, IConnectionServer> _connectionsUnregistered =
            new ConcurrentDictionary<int, IConnectionServer>();

        public NTierServerAuth(INTierServerParamsAuth parameters, IUserService<T> userService)
        {
            _server = new TcpNETServerAuth<T>(new ParamsTcpServerAuth
            {
                ConnectionSuccessString = parameters.ConnectionSuccessString,
                ConnectionUnauthorizedString = parameters.ConnectionUnauthorizedString,
                EndOfLineCharacters = "\r\n",
                Port = parameters.Port
            }, userService);
            _server.ConnectionEvent += OnConnectionEvent;
            _server.ErrorEvent += OnErrorEvent;
            _server.MessageEvent += OnMessageEvent;
            _server.ServerEvent += OnServerEvent;
        }
        public NTierServerAuth(INTierServerParamsAuth parameters, IUserService<T> userService, byte[] certificate, string certificatePassword)
        {
            _server = new TcpNETServerAuth<T>(new ParamsTcpServerAuth
            {
                ConnectionSuccessString = parameters.ConnectionSuccessString,
                ConnectionUnauthorizedString = parameters.ConnectionUnauthorizedString,
                EndOfLineCharacters = "\r\n",
                Port = parameters.Port
            }, userService, certificate, certificatePassword);
            _server.ConnectionEvent += OnConnectionEvent;
            _server.ErrorEvent += OnErrorEvent;
            _server.MessageEvent += OnMessageEvent;
            _server.ServerEvent += OnServerEvent;
        }

        public virtual async Task StartAsync()
        {
            await _server.StartAsync();
        }

        public virtual async Task StopAsync()
        {
            await _server.StopAsync();
        }

        protected virtual async Task OnMessageEvent(object sender, TcpMessageServerAuthEventArgs<T> args)
        {
            switch (args.MessageEventType)
            {
                case MessageEventType.Sent:
                    break;
                case MessageEventType.Receive:
                    try
                    {
                        if (_connectionsUnregistered.TryRemove(args.Connection.Client.GetHashCode(), out var tcpClient))
                        {
                            // Register the connection if it is new
                            var registerDTO = JsonConvert.DeserializeObject<Register>(args.Packet.Data);

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
                            var deserialized = JsonConvert.DeserializeObject<Message>(args.Packet.Data);

                            switch (deserialized.MessageType)
                            {
                                case MessageType.FromProvider:
                                    if (_connectionsToServices.TryDequeue(out var connectionService))
                                    {
                                        _connectionsToServices.Enqueue(connectionService);
                                        await _server.SendToConnectionAsync(new Packet
                                        {
                                            Data = args.Message,
                                            Timestamp = DateTime.UtcNow
                                        }, connectionService);
                                    }
                                    break;
                                case MessageType.FromService:
                                    foreach (var connectionToProvider in _connectionsToProviders)
                                    {
                                        await _server.SendToConnectionAsync(new Packet
                                        {
                                            Data = args.Message,
                                            Timestamp = DateTime.UtcNow
                                        }, connectionToProvider);
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
        protected virtual Task OnErrorEvent(object sender, TcpErrorServerAuthEventArgs<T> args)
        {
            return Task.CompletedTask;
        }
        protected virtual Task OnConnectionEvent(object sender, TcpConnectionServerAuthEventArgs<T> args)
        {
            switch (args.ConnectionEventType)
            {
                case ConnectionEventType.Connected:
                    _connectionsUnregistered.TryAdd(args.Connection.Client.GetHashCode(), args.Connection);
                    break;
                case ConnectionEventType.Disconnect:
                    _connectionsUnregistered.TryRemove(args.Connection.Client.GetHashCode(), out var _);
                    break;
                case ConnectionEventType.Connecting:
                    break;
                default:
                    break;
            }

            return Task.CompletedTask;
        }
        protected virtual Task OnServerEvent(object sender, ServerEventArgs args)
        {
            switch (args.ServerEventType)
            {
                case ServerEventType.Start:
                    break;
                case ServerEventType.Stop:
                    _connectionsToServices.Clear();
                    break;
                default:
                    break;
            }

            return Task.CompletedTask;
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
