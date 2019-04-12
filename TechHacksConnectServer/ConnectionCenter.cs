using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace TechHacksConnectServer
{
    using Connection = KeyValuePair<string, TcpClient>;

    public class Client
    {
        public Connection connection;
        public uint room;

        public Client(Connection connection, uint room)
        {
            this.connection = connection;
            this.room = room;
        }
    }

    public class ConnectionCenter
    {
        private Networking.TH_TcpServer server = null;
        public List<Client> connected_clients = new List<Client>();
        private bool accepting_new_connections = false;
        public static uint MAXIMUM_CONNECTIONS = 250;

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
            foreach (Client client in connected_clients)
            {
                try
                {
                    Connection conn = client.connection;
                    server.Send(conn.Value, data);
                }
                catch (System.IO.IOException)
                {
                    continue;
                }
            }
        }

        private void SendToIndividualClient(Client client, string data)
        {
            try
            {
                Connection conn = client.connection;
                server.Send(conn.Value, data);
            }
            catch (System.IO.IOException) { }
        }

        private void SendToClientRoom(uint room, string data)
        {
            foreach (Client client in connected_clients)
            {
                if (room == client.room)
                {
                    try
                    {
                        Connection conn = client.connection;
                        string msg = (room != 1) ? ("[Room " + room + "] " + data) : data;
                        server.Send(conn.Value, msg);
                    }
                    catch (System.IO.IOException)
                    {
                        continue;
                    }
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

        private void RegisterNewConnection(string client_name, TcpClient tcp_client)
        {
            Connection conn = new Connection(client_name, tcp_client);

            Client client = new Client(conn, 1);
            connected_clients.Add(client);

            string help_msg = GetOptionsMessageString();
            SendToIndividualClient(client, help_msg);

            Thread connection_thread = new Thread(delegate ()
            {
                ManageConnectedClient(client);
            });
            connection_thread.IsBackground = true;
            connection_thread.Start();

            Console.WriteLine(client_name + " has joined");
            SendToClientRoom(client.room, client_name + " has joined");
        }

        private void ManageConnectedClient(Client client)
        {
            Connection connection = client.connection;
            bool connected = true;
            while (connected)
            {
                connected = connection.Value.Connected;
                if (!connected) break;

                try
                {
                    string received_data = server.Receive(connection.Value);
                    int parsing_results = ParseReceivedMessageForCommands(client, received_data);

                    if (parsing_results == 0) SendToClientRoom(client.room, connection.Key + ": " + received_data);
                }
                catch (System.IO.IOException)
                {
                    lock(connected_clients)
                    {
                        connected_clients.Remove(client);
                    }

                    Console.WriteLine(connection.Key + " has disconnected");
                    SendToClientRoom(client.room, connection.Key + " has disconnected");
                    break;
                }
            }
        }

        private int ParseReceivedMessageForCommands(Client client, string msg)
        {
            if (msg.StartsWith("!room "))
            {
                string[] args = msg.Split();
                string cmd = args[1];

                if (cmd.Equals("join"))
                {
                    if (args.Length != 3) return 0;
                    uint room_number;
                    uint.TryParse(args[2], out room_number);

                    if (room_number == 0) return 0; // invalid chat room number

                    // changing the room
                    uint old_room_number = client.room;
                    client.room = room_number;

                    // notifying existing room
                    SendToClientRoom(old_room_number, "----- " + client.connection.Key + " has moved to Room " + room_number + " -----");
                    
                    // notifying new room
                    SendToClientRoom(client.room, "----- " + client.connection.Key + " has moved to Room " + room_number + " -----");

                    // outputting data to the server
                    Console.WriteLine("----- " + client.connection.Key + " has moved to Room " + room_number + " -----");
                }
                else if (cmd.Equals("list"))
                {
                    // list all active chat rooms
                    Dictionary<uint, uint> room_list = GetActiveRooms();
                    string room_list_msg = "";

                    foreach (KeyValuePair<uint, uint> room in room_list)
                    {
                        room_list_msg += "Room " + room.Key + ": [" + room.Value + "/250]\r\n";
                    }

                    SendToIndividualClient(client, room_list_msg);
                }
                else if (cmd.Equals("leave") && client.room != 1)
                {
                    // leave the current room and join the global chat room
                    uint client_old_room = client.room;
                    client.room = 1;
                    SendToClientRoom(client_old_room, "----- " + client.connection.Key + " has moved to Room 1 -----");
                    SendToClientRoom(client.room, "----- " + client.connection.Key + " has been moved to Room 1 -----");
                    Console.WriteLine("----- " + client.connection.Key + " has been moved to Room 1 -----");
                }

                return 1;
            }
            else if (msg.StartsWith("!help") || msg.StartsWith("!options"))
            {
                string help_msg = GetOptionsMessageString();
                SendToIndividualClient(client, help_msg);
                return 1;
            }

            return 0;
        }

        private Dictionary<uint, uint> GetActiveRooms()
        {
            Dictionary<uint, uint> room_list = new Dictionary<uint, uint>();
            room_list.Add(1, 0);
            
            foreach (Client client in connected_clients)
            {
                if (room_list.ContainsKey(client.room))
                {
                    room_list[client.room]++;
                }
                else
                {
                    room_list.Add(client.room, 1);
                }
            }

            return room_list;
        }

        private string GetOptionsMessageString()
        {
            // Don't mind the weird spacing, textbox is displaying text weirdly 
            // and this has to be done in order for it to appear alligned

            return "\r\n-----------------------------------------------------------\r\n" +
                       "    Command                                 Description    \r\n" +
                       "-----------------------------------------------------------\r\n" +
                       "1) !help or !options                       Shows possible commands\r\n" +
                       "2) !room join [room number]      Joins a separate private chat room or creates one if it doesn't exist already\r\n" +
                       "3) !room list                                 Lists existing rooms and number of active users in them\r\n" +
                       "4) !room leave                              Exits current chat room and returns to the global room (Room 1)\r\n" +
                       "5) !clear                                        Erases all current messages\r\n\r\n";
        }
    }
}
