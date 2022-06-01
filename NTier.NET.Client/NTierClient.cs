using Newtonsoft.Json;
using NTier.NET.Client.Models;
using NTier.NET.Core.Enums;
using NTier.NET.Core.Events;
using NTier.NET.Core.Models;
using PHS.Networking.Enums;
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
                ? new TcpNETClient(new ParamsTcpClient(_parameters.Uri, _parameters.Port, "\r\n", _parameters.IsSSL))
                : new TcpNETClient(new ParamsTcpClient(_parameters.Uri, _parameters.Port, "\r\n", _parameters.IsSSL, token: oauthToken));

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
                await _client.SendAsync(message);
            }
        }

        protected virtual void OnMessageEvent(object sender, TcpMessageClientEventArgs args)
        {
            switch (args.MessageEventType)
            {
                case MessageEventType.Sent:
                    break;
                case MessageEventType.Receive:
                    FireMessageEvent(sender, args.Message);
                    break;
                default:
                    break;
            }
        }
        protected virtual void OnErrorEvent(object sender, TcpErrorClientEventArgs args)
        {
        }
        protected virtual void OnConnectionEvent(object sender, TcpConnectionClientEventArgs args)
        {
            switch (args.ConnectionEventType)
            {
                case ConnectionEventType.Connected:
                    Task.Run(async () =>
                    {
                        await _client.SendAsync(JsonConvert.SerializeObject(new Register
                        {
                            RegisterType = _parameters.RegisterType
                        }));
                    });
                    break;
                case ConnectionEventType.Disconnect:
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
        protected virtual void FireMessageEvent(object sender, string message)
        {
            _messageEvent?.Invoke(sender, message);
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
