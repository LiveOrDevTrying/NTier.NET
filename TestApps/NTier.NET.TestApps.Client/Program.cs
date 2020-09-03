using Newtonsoft.Json;
using NTier.NET.Client;
using NTier.NET.Client.Models;
using NTier.NET.Core.Enums;
using NTier.NET.Core.Models;
using System;
using System.Threading.Tasks;

namespace NTier.NET.TestApps.Client
{
    class Program
    {
        private static INTierClient _client;
        private static IParamsNTierClient _parameters;

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
                    _client.MessageEvent -= OnNTierMessageEventAsync;
                    _client.Dispose();
                    _client = null;
                }

                _parameters = new ParamsNTierClient
                {
                    Port = 9345,
                    ReconnectIntervalSec = 12,
                    Uri = "localhost",
                    RegisterType = (RegisterType)selection
                };

                _client = new NTierClient(_parameters);
                _client.MessageEvent += OnNTierMessageEventAsync;
                await _client.StartAsync();
            }

            await MenuAsync();
        }
        static async Task CreateMessageAsync()
        {
            Console.WriteLine("Enter message:");

            try
            {
                var line = Console.ReadLine();

                await _client.SendToServerAsync(line);

                Console.WriteLine("Message sent successfully");
            }
            catch
            { }

            await MenuAsync();
        }

        private static async Task OnNTierMessageEventAsync(object sender, IMessage message)
        {
            Console.WriteLine(JsonConvert.SerializeObject(message));

            if (_parameters.RegisterType == RegisterType.Service) 
            {
                await _client.SendToServerAsync(message.Content);
            }
        }
    }
}
