using Newtonsoft.Json;
using NTier.NET.Core.Enums;
using NTier.NET.Core.Models;
using PHS.Core.Enums;
using PHS.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Tasks;
using Tcp.NET.Core.Events.Args;
using Tcp.NET.Server;
using Tcp.NET.Server.Models;

namespace NTier.NET.Server
{
    public class NTierServer : INTierServer
    {
        protected readonly ITcpNETServer _server;
        protected ConcurrentQueue<ConnectionSocketDTO> _connectionsToServices =
            new ConcurrentQueue<ConnectionSocketDTO>();
        protected ConcurrentBag<ConnectionSocketDTO> _connectionsToProviders =
            new ConcurrentBag<ConnectionSocketDTO>();
        protected ConcurrentDictionary<int, Socket> _connectionsUnregistered =
            new ConcurrentDictionary<int, Socket>();

        public NTierServer(IParamsTcpServer parameters)
        {
            _server = new TcpNETServer(parameters, new TcpConnectionManager());
            _server.ConnectionEvent += OnConnectionEvent;
            _server.ErrorEvent += OnErrorEvent;
            _server.MessageEvent += OnMessageEvent;
        }

        protected virtual Task OnMessageEvent(object sender, TcpMessageEventArgs args)
        {
            switch (args.MessageEventType)
            {
                case MessageEventType.Sent:
                    break;
                case MessageEventType.Receive:
                    try
                    {
                        if (_connectionsUnregistered.TryRemove(args.Socket.GetHashCode(), out var socket))
                        {
                            // Register the connection if it is new
                            var registerDTO = JsonConvert.DeserializeObject<NTierRegisterDTO>(args.Message);

                            switch (registerDTO.NTierRegisterType)
                            {
                                case NTierRegisterType.Service:
                                    _connectionsToServices.Enqueue(new ConnectionSocketDTO
                                    {
                                        Socket = socket
                                    });
                                    break;
                                case NTierRegisterType.Provider:
                                case NTierRegisterType.WebApp:
                                    _connectionsToProviders.Add(new ConnectionSocketDTO
                                    {
                                        Socket = socket
                                    });
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            // When a Provider sends a message to the cache, round-robin send it to the processing server
                            var deserialized = JsonConvert.DeserializeObject<NTierMessageDTO>(args.Message);

                            switch (deserialized.NTierMessageType)
                            {
                                case NTierMessageType.FromProvider:
                                case NTierMessageType.FromWebApp:
                                    if (_connectionsToServices.TryDequeue(out var connectionService))
                                    {
                                        _connectionsToServices.Enqueue(connectionService);
                                        _server.SendToConnection(new PacketDTO
                                        {
                                            Action = (int)ActionType.SendToClient,
                                            Data = args.Message,
                                            Timestamp = DateTime.UtcNow
                                        }, connectionService.Socket);
                                    }
                                    break;
                                case NTierMessageType.FromService:
                                    foreach (var connectionToProvider in _connectionsToProviders)
                                    {
                                        _server.SendToConnection(new PacketDTO
                                        {
                                            Action = (int)ActionType.SendToClient,
                                            Data = args.Message,
                                            Timestamp = DateTime.UtcNow
                                        }, connectionToProvider.Socket);
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

            return Task.CompletedTask;
        }
        protected virtual Task OnErrorEvent(object sender, TcpErrorEventArgs args)
        {
            return Task.CompletedTask;
        }
        protected virtual Task OnConnectionEvent(object sender, TcpConnectionEventArgs args)
        {
            switch (args.ConnectionEventType)
            {
                case ConnectionEventType.Connected:
                    _connectionsUnregistered.TryAdd(args.Socket.GetHashCode(), args.Socket);
                    break;
                case ConnectionEventType.Disconnect:
                    _connectionsUnregistered.TryRemove(args.Socket.GetHashCode(), out var _);
                    break;
                case ConnectionEventType.ServerStart:
                    break;
                case ConnectionEventType.ServerStop:
                    _connectionsToServices.Clear();
                    break;
                case ConnectionEventType.Connecting:
                    break;
                case ConnectionEventType.MaxConnectionsReached:
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
                _server.Dispose();
            }
        }
    }
}
