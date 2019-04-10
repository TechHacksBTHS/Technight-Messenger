using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace TechHacksConnectServer
{
    using Connection = KeyValuePair<string, TcpClient>;
    public class ConnectionCenter
    {


        private Networking.TH_TcpServer server = null;
        public List<Connection> connected_clients = new List<Connection>();
        private bool accepting_new_connections = false;
        public static uint MAXIMUM_CONNECTIONS = 64;

        public ConnectionCenter(string host_ip, int port)
        {
            server = new Networking.TH_TcpServer(host_ip, port);
        }

        public void StartAcceptingConnections()
        {
            if (!accepting_new_connections)
            {
                accepting_new_connections = true;
                Thread listening_thread = new Thread(ListeningLoop);
                listening_thread.IsBackground = true;
                listening_thread.Start();
            }
        }

        public void StopAcceptingConnections() => accepting_new_connections = false;

        /*===============================================================================================*/
        /*===============================================================================================*/

        private void SendToAllConnectedClients(string data)
        {
            foreach (Connection conn in connected_clients)
            {
                try
                {
                    server.Send(conn.Value, data);
                }
                catch (System.IO.IOException)
                {
                    continue;
                }
            }
        }

        private void ListeningLoop()
        {
            while (accepting_new_connections)
            {
                // checking if maximum number of clients is connected
                if (connected_clients.Count >= MAXIMUM_CONNECTIONS) break;

                TcpClient client = server.Listen();
                string client_name = server.Receive(client);

                RegisterNewConnection(client_name, client);

                Thread.Sleep(10);
            }
        }

        private void RegisterNewConnection(string client_name, TcpClient client)
        {
            Connection conn = new Connection(client_name, client);
            connected_clients.Add(conn);
            Thread connection_thread = new Thread(delegate ()
            {
                ManageConnectedClient(conn);
            });
            connection_thread.IsBackground = true;
            connection_thread.Start();
            Console.WriteLine(client_name + " has joined");
            SendToAllConnectedClients(client_name + " has joined");
        }

        private void ManageConnectedClient(Connection connection)
        {
            bool connected = true;
            while (connected)
            {
                connected = connection.Value.Connected;
                if (!connected) break;

                try
                {
                    string received_data = server.Receive(connection.Value);
                    SendToAllConnectedClients(connection.Key + ": " + received_data);
                }
                catch (System.IO.IOException)
                {
                    connected_clients.Remove(connection);
                    Console.WriteLine(connection.Key + " has disconnected");
                    SendToAllConnectedClients(connection.Key + " has disconnected");
                    break;
                }
            }
        }
    }
}
