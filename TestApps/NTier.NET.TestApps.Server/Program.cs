using NTier.NET.Server;
using NTier.NET.Server.Models;
using System;
using System.Threading.Tasks;
using Tcp.NET.Server.Models;

namespace NTier.NET.TestApps.Server
{
    class Program
    {
        private static INTierServer _server;

        static void Main(string[] args)
        {
            _server = new NTierServer(new NTierServerParams
            {
                Port = 9345,
                ConnectionSuccessString = "You have connected to NTier.NET successfully."
            });
            _server.Start();

            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
