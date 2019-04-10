using System.Net.Sockets;
using System.Text;

namespace Networking
{
    public class TH_TcpClient
    {
        private string server_ip = "0.0.0.0";
        private int port_no = -1;

        private TcpClient tcp_client = null;
        NetworkStream network_stream = null;

        public TH_TcpClient(string server_ip, int port)
        {
            this.server_ip = server_ip;
            port_no = port;

            tcp_client = new TcpClient(server_ip, port);
            network_stream = tcp_client.GetStream();
        }

        public void Send(string str)
        {
            if (network_stream != null)
            {
                byte[] data = ASCIIEncoding.ASCII.GetBytes(str);
                network_stream.Write(data, 0, data.Length);
            }
        }

        public string Receive()
        {
            if (network_stream != null)
            {
                byte[] data = new byte[tcp_client.ReceiveBufferSize];
                int byte_count = network_stream.Read(data, 0, tcp_client.ReceiveBufferSize);
                string result = Encoding.ASCII.GetString(data, 0, byte_count);
                return result;
            }
            return "";
        }

        public void Close()
        {
            if (tcp_client != null) tcp_client.Close();
        }
    }
}
