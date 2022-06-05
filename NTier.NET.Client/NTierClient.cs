using Newtonsoft.Json;
using NTier.NET.Client.Events;
using NTier.NET.Client.Handlers;
using NTier.NET.Client.Models;
using NTier.NET.Core.Models;
using PHS.Networking.Enums;
using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Client;
using Tcp.NET.Core.Events.Args;
using Tcp.NET.Core.Models;

namespace NTier.NET.Client
{
    public class NTierClient : 
        TcpNETClientBase<
            NTierConnectionClientEventArgs,
            NTierMessageClientEventArgs,
            NTierErrorClientEventArgs,
            ParamsNTierClient,
            NTierClientHandler,
            ConnectionTcp>,
        INTierClient
    {
        protected Timer _timer;
        protected bool _isTimerRunning;

        public NTierClient(ParamsNTierClient parameters) : base(parameters)
        {
        }

        public override async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
        {
            var result = await base.ConnectAsync(cancellationToken);

            if (_parameters.ReconnectIntervalSec > 0)
            {
                if (_timer != null)
                {
                    _timer.Dispose();
                }

                _timer = new Timer(OnTimerCallback, null, _parameters.ReconnectIntervalSec * 1000, _parameters.ReconnectIntervalSec * 1000);
            }

            return result;
        }
        protected override NTierClientHandler CreateTcpClientHandler()
        {
            return new NTierClientHandler(_parameters);
        }
        protected override void OnConnectionEvent(object sender, NTierConnectionClientEventArgs args)
        {
            switch (args.ConnectionEventType)
            {
                case ConnectionEventType.Connected:
                    Task.Run(async () =>
                    {
                        await SendAsync(JsonConvert.SerializeObject(new Register
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

            base.OnConnectionEvent(this, args);
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
                        if (!IsRunning)
                        {
                            await ConnectAsync();
                        }
                    }
                    catch
                    { }

                    _isTimerRunning = false;
                });
            }
        }

        public override void Dispose()
        {
            if (_timer != null)
            {
                _timer.Dispose();
            }

            base.Dispose();
        }
    }
}
