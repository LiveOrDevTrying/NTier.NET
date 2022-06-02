using NTier.NET.Client;
using NTier.NET.Client.Events;
using NTier.NET.Client.Models;
using NTier.NET.Core.Enums;
using System;
using System.Threading.Tasks;

namespace NTier.NET.TestApps.Client
{
    class Program
    {
        private static INTierClient _client;
        private static ParamsNTierClient _parameters;

        static async Task Main(string[] args)
        {
            await MenuAsync();
        }

        static async Task MenuAsync()
        {
            if (_client != null)
            {
                Console.WriteLine("Client created");

                if (_parameters != null)
                {
                    Console.WriteLine(_parameters.RegisterType.ToString());
                }
            }

            Console.WriteLine("1. Create Client");

            if (_client != null)
            {
                Console.WriteLine("2. Send message");
            }

            Console.WriteLine("Q. Quit");

            var line = Console.ReadLine();

            switch (line.Trim().ToLower())
            {
                case "1":
                    await CreateClientAsync();
                    break;
                case "2":
                    if (_client == null)
                    {
                        Console.WriteLine("Not a valid selection");
                        await MenuAsync();
                    }
                    await CreateMessageAsync();
                    break;
                case "q":
                    Environment.Exit(-1);
                    break;
                default:
                    await MenuAsync();
                    break;
            }
        }

        static async Task CreateClientAsync()
        {
            Console.WriteLine("Select service type: ");

            for (int i = 0; i < Enum.GetNames(typeof(RegisterType)).Length; i++)
            {
                Console.WriteLine($"{i + 1}: {((RegisterType)i).ToString()}");
            }

            var line = Console.ReadLine();

            if (int.TryParse(line, out var selection) &&
                --selection >= 0 &&
                selection < Enum.GetNames(typeof(RegisterType)).Length)
            {
                if (_client != null)
                {
                    _client.ConnectionEvent -= OnNTierConnectionEvent;
                    _client.MessageEvent -= OnNTierMessageEvent;
                    _client.ErrorEvent -= OnNTierErrorEvent;
                    _client.Dispose();
                    _client = null;
                }

                _parameters = new ParamsNTierClient("localhost", 9345, (RegisterType)selection, "\r\n", false, "testToken");

                _client = new NTierClient(_parameters);
                _client.ConnectionEvent += OnNTierConnectionEvent;
                _client.MessageEvent += OnNTierMessageEvent;
                _client.ErrorEvent += OnNTierErrorEvent;
                await _client.ConnectAsync();
            }

            await MenuAsync();
        }

        private static void OnNTierConnectionEvent(object sender, NTierConnectionClientEventArgs args)
        {
            Console.WriteLine(args.ConnectionEventType);
        }
        private static void OnNTierMessageEvent(object sender, NTierMessageClientEventArgs args)
        {
            switch (args.MessageEventType)
            {
                case PHS.Networking.Enums.MessageEventType.Sent:
                    break;
                case PHS.Networking.Enums.MessageEventType.Receive:
                    Console.WriteLine(args.Message);

                    if (_parameters.RegisterType == RegisterType.Service)
                    {
                        Task.Run(async () =>
                        {
                            await _client.SendAsync(args.Message);
                        });
                    }
                    break;
                default:
                    break;
            }
        }
        private static void OnNTierErrorEvent(object sender, NTierErrorClientEventArgs args)
        {
            Console.WriteLine(args.Message);
        }

        static async Task CreateMessageAsync()
        {
            Console.WriteLine("Enter message:");

            try
            {
                var line = Console.ReadLine();

                await _client.SendAsync(line);

                Console.WriteLine("Message sent successfully");
            }
            catch
            { }

            await MenuAsync();
        }
    }
}
