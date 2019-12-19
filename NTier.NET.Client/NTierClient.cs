using NTier.NET.Client.Models;
using Newtonsoft.Json;
using PHS.Core.Enums;
using PHS.Core.Models;
using System.Threading.Tasks;
using Tcp.NET.Client;
using Tcp.NET.Core.Events.Args;
using NTier.NET.Core.Events;
using NTier.NET.Core.Enums;
using NTier.NET.Core.Models;

namespace NTier.NET.Client
{
    public class NTierClient : INTierClient
    {
        // R8D: ToDo Code in auto reconnect
        protected readonly ITcpNETClient _client;
        protected readonly IParameters _parameters;

        private event NTierMessageEventHandler _nTierMessageEvent;
            
        public NTierClient(IParameters parameters)
        {
            _parameters = parameters;
            _client = new TcpNETClient();
            _client.ConnectionEvent += OnConnectionEvent;
            _client.ErrorEvent += OnErrorEvent;
            _client.MessageEvent += OnMessageEvent;
            _client.Connect(_parameters.Uri, _parameters.Port, _parameters.EndOfLineCharacters);
        }

        public virtual void SendMessageToMessageCache<T>(T message, bool isFromWebapp) where T : MessageDTO
        {
            if (_client.IsConnected)
            {
                switch (_parameters.NTierRegisterType)
                {
                    case NTierRegisterType.Service:
                        _client.SendToServer(new PacketDTO
                        {
                            Action = (int)ActionType.SendToServer,
                            Data = JsonConvert.SerializeObject(new NTierMessageDTO
                            {
                                NTierMessageType = NTierMessageType.FromService,
                                Message = message,
                                IsFromWebapp = isFromWebapp
                            })
                        });
                        break;
                    case NTierRegisterType.Provider:
                        _client.SendToServer(new PacketDTO
                        {
                            Action = (int)ActionType.SendToServer,
                            Data = JsonConvert.SerializeObject(new NTierMessageDTO
                            {
                                NTierMessageType = NTierMessageType.FromProvider,
                                IsFromWebapp = isFromWebapp,
                                Message = message
                            })
                        });
                        break;
                    default:
                        break;
                }
            }
        }

        protected virtual Task OnMessageEvent(object sender, TcpMessageEventArgs args)
        {
            switch (args.MessageEventType)
            {
                case MessageEventType.Sent:
                    break;
                case MessageEventType.Receive:
                    if (args.Message.Trim().ToLower() == "ping")
                    {
                        _client.SendToServer("pong");
                    }
                    else
                    {
                        FireNTierMessageEvent(sender, new NTierMessageDTO
                        {
                            IsFromWebapp = false,
                            Message = new MessageDTO
                            {
                                Message = args.Message
                            },
                            NTierMessageType = NTierMessageType.FromService
                        });
                    }
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
                    switch (_parameters.NTierRegisterType)
                    {
                        case NTierRegisterType.Service:
                            _client.SendToServer(new PacketDTO
                            {
                                Action = (int)ActionType.SendToServer,
                                Data = JsonConvert.SerializeObject(new NTierRegisterDTO
                                {
                                    NTierMessageType = NTierMessageType.FromService,
                                    NTierRegisterType = _parameters.NTierRegisterType
                                })
                            });
                            break;
                        case NTierRegisterType.Provider:
                            _client.SendToServer(new PacketDTO
                            {
                                Action = (int)ActionType.SendToServer,
                                Data = JsonConvert.SerializeObject(new NTierRegisterDTO
                                {
                                    NTierMessageType = NTierMessageType.FromProvider,
                                    NTierRegisterType = _parameters.NTierRegisterType
                                })
                            });
                            break;
                        default:
                            break;
                    }
                    break;
                case ConnectionEventType.Disconnect:
                    break;
                case ConnectionEventType.ServerStart:
                    break;
                case ConnectionEventType.ServerStop:
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

        protected virtual void FireNTierMessageEvent(object sender, INTierMessageDTO message)
        {
            _nTierMessageEvent?.Invoke(sender, message);
        }

        public virtual void Dispose()
        {
            if (_client != null)
            {
                _client.ConnectionEvent -= OnConnectionEvent;
                _client.ErrorEvent -= OnErrorEvent;
                _client.MessageEvent -= OnMessageEvent;
                _client.Dispose();
            }
        }

        public event NTierMessageEventHandler NTierMessageEvent
        {
            add
            {
                _nTierMessageEvent += value;
            }
            remove
            {
                _nTierMessageEvent -= value;
            }
        }
    }
}
