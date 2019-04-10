using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Networking
{
    public class TH_TcpServer
    {
        private string server_ip = "0.0.0.0";
        private int port_no = -1;

        TcpListener tcp_listener = null;

        public TH_TcpServer(string ip, int port)
        {
            server_ip = ip;
            port_no = port;

            //---listen at the specified IP and port no.---
            IPAddress ipaddr = IPAddress.Parse(server_ip);
            tcp_listener = new TcpListener(ipaddr, port_no);
        }

        public TcpClient Listen()
        {
            if (tcp_listener != null)
            {
                tcp_listener.Start();
                TcpClient client = tcp_listener.AcceptTcpClient();
                return client;
            }
            return null;
        }

        public void Send(TcpClient client, string str)
        {
            if (client != null)
            {
                byte[] data = Encoding.ASCII.GetBytes(str);
                NetworkStream network_stream = client.GetStream();
                network_stream.Write(data, 0, data.Length);
            }
        }

        public string Receive(TcpClient client)
        {
            if (client != null)
            {
                NetworkStream network_stream = client.GetStream();
                byte[] buffer = new byte[client.ReceiveBufferSize];

                int bytes = network_stream.Read(buffer, 0, client.ReceiveBufferSize);

                string result = Encoding.ASCII.GetString(buffer, 0, bytes);
                return result;
            }
            return "";
        }

        public void Close()
        {
            if (tcp_listener != null) tcp_listener.Stop();
        }
    }
}
