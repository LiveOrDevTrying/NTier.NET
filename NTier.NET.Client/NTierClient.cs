using Newtonsoft.Json;
using NTier.NET.Client.Models;
using NTier.NET.Core.Enums;
using NTier.NET.Core.Events;
using NTier.NET.Core.Models;
using PHS.Networking.Enums;
using PHS.Networking.Models;
using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Client;
using Tcp.NET.Client.Events.Args;
using Tcp.NET.Client.Models;

namespace NTier.NET.Client
{
    public class NTierClient : INTierClient
    {
        protected readonly ITcpNETClient _client;
        protected readonly IParamsNTierClient _parameters;
        protected Timer _timer;
        protected bool _isTimerRunning;

        private event MessageEventHandler _messageEvent;
            
        public NTierClient(IParamsNTierClient parameters, string oauthToken = "")
        {
            _parameters = parameters;
            _client = string.IsNullOrWhiteSpace(oauthToken)
                ? new TcpNETClient(new ParamsTcpClient
                {
                    EndOfLineCharacters = "\r\n",
                    Port = _parameters.Port,
                    Uri = _parameters.Uri,
                    IsSSL = _parameters.IsSSL
                })
                : new TcpNETClient(new ParamsTcpClient
                {
                    EndOfLineCharacters = "\r\n",
                    Port = _parameters.Port,
                    Uri = _parameters.Uri,
                    IsSSL = _parameters.IsSSL
                }, oauthToken: oauthToken);

            _client.ConnectionEvent += OnConnectionEvent;
            _client.ErrorEvent += OnErrorEvent;
            _client.MessageEvent += OnMessageEvent;
        }
        public virtual async Task StartAsync()
        {
            try
            {
                await _client.ConnectAsync();

                if (_parameters.ReconnectIntervalSec > 0)
                {
                    _timer = new Timer(OnTimerCallback, null, _parameters.ReconnectIntervalSec * 1000, _parameters.ReconnectIntervalSec * 1000);
                }
            }
            catch
            { }
        }
        public virtual async Task StopAsync()
        {
            await _client.DisconnectAsync();
        }
        public virtual async Task SendToServerAsync<T>(T instance) where T : class
        {
            await SendToServerAsync(JsonConvert.SerializeObject(instance));
        }

        public virtual async Task SendToServerAsync(string message)
        {
            if (_client.IsRunning)
            {
                switch (_parameters.RegisterType)
                {
                    case RegisterType.Service:
                        await _client.SendToServerAsync(new Packet
                        {
                            Data = JsonConvert.SerializeObject(new Message
                            {
                                MessageType = MessageType.FromService,
                                Content = message
                            })
                        });
                        break;
                    case RegisterType.Provider:
                        await _client.SendToServerAsync(new Packet
                        {
                            Data = JsonConvert.SerializeObject(new Message
                            {
                                MessageType = MessageType.FromProvider,
                                Content = message
                            })
                        });
                        break;
                    default:
                        break;
                }
            }
        }

        protected virtual async Task OnMessageEvent(object sender, TcpMessageClientEventArgs args)
        {
            switch (args.MessageEventType)
            {
                case MessageEventType.Sent:
                    break;
                case MessageEventType.Receive:
                    await FireMessageEventAsync(sender, new Message
                    {
                        Content = args.Packet.Data,
                        MessageType = MessageType.FromService
                    });
                    break;
                default:
                    break;
            }
        }
        protected virtual Task OnErrorEvent(object sender, TcpErrorClientEventArgs args)
        {
            return Task.CompletedTask;

        }
        protected virtual async Task OnConnectionEvent(object sender, TcpConnectionClientEventArgs args)
        {
            switch (args.ConnectionEventType)
            {
                case ConnectionEventType.Connected:
                    switch (_parameters.RegisterType)
                    {
                        case RegisterType.Service:
                            await _client.SendToServerRawAsync(JsonConvert.SerializeObject(new Register
                            {
                                MessageType = MessageType.FromService,
                                RegisterType = _parameters.RegisterType
                            }));
                            break;
                        case RegisterType.Provider:
                            await _client.SendToServerRawAsync(JsonConvert.SerializeObject(new Register
                            {
                                MessageType = MessageType.FromProvider,
                                RegisterType = _parameters.RegisterType
                            }));
                            break;
                        default:
                            break;
                    }
                    break;
                case ConnectionEventType.Disconnect:
                    break;
                case ConnectionEventType.Connecting:
                    break;
                default:
                    break;
            }
        }
        protected virtual void OnTimerCallback(object state)
        {
            if (!_isTimerRunning)
            {
                _isTimerRunning = true;

                Task.Run(async () =>
                {
                    try
                    {
                        if (!_client.IsRunning)
                        {
                            await _client.ConnectAsync();
                        }
                    }
                    catch
                    { }

                    _isTimerRunning = false;
                });
            }
        }
        protected virtual async Task FireMessageEventAsync(object sender, IMessage message)
        {
            if (_messageEvent != null)
            {
                await _messageEvent?.Invoke(sender, message);
            }
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

            if (_timer != null)
            {
                _timer.Dispose();
            }
        }

        public event MessageEventHandler MessageEvent
        {
            add
            {
                _messageEvent += value;
            }
            remove
            {
                _messageEvent -= value;
            }
        }
    }
}
