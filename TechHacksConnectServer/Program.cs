using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TechHacksConnectServer
{
    class Program
    {
        

        static void Main(string[] args)
        {
            Console.WriteLine("[+] Starting TechHacksConnect Server [+]\n");

            ConnectionCenter connection_center = new ConnectionCenter(IPAddress.Any.ToString(), 4554);
            connection_center.StartAcceptingConnections();

            while (true)
            {
                Console.WriteLine("Active Connections: " + connection_center.connected_clients.Count);
                Thread.Sleep(5000);
            }
        }
    }
}
