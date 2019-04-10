using System;

namespace TechHacksSoftware
{
    public class Network
    {
        private static Networking.TH_TcpClient tcp_client = null;

        public static void Connect(string server_ip, int server_port, string username)
        {
            tcp_client = new Networking.TH_TcpClient(server_ip, server_port);
            tcp_client.Send(username);
        }

        public static void Disconnect()
        {
            if (tcp_client != null)
            {
                tcp_client.Close();
                tcp_client = null;
            }
            
        }

        public static void Send(string str)
        {
            if (tcp_client != null)
            {
                try
                {
                    tcp_client.Send(str);
                }
                catch (System.IO.IOException)
                { }
            }
        }

        public static void StartMessageReceivingLoop(Action<string> message_processing_callback)
        {
            while (tcp_client != null)
            {
                try
                {
                    string data = tcp_client.Receive();
                    message_processing_callback(data + "\r\n");
                }
                catch (System.IO.IOException)
                {
                    message_processing_callback("----- You have disconnected -----\r\n");
                    break;
                }
            }
        }
    }
}
