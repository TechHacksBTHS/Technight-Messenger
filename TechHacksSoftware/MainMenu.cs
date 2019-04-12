using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TechHacksSoftware
{
    public partial class TechHacksMainMenu : Form
    {
        private const string ServerIP = "127.0.0.1";// "158.222.244.80";
        private string Username = "Anonymous";
        private bool LoggedIn = false;
        private bool ConnectedStatus = false;

        public TechHacksMainMenu()
        {
            InitializeComponent();
        }

        private void AppendMessageArea(string value)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AppendMessageArea), new object[] { value });
                return;
            }
            MessageArea.Text += value;
            return;
        }

        private void MainMenu_Load(object sender, EventArgs e)
        {
            AppendMessageArea("Welcome to TechHacksConnect!\r\n[*] Press \"Connect\" to join a chat room [*]\r\n\r\n");
        }

        private void ConnectBtn_Click(object sender, EventArgs e)
        {
            if (!ConnectedStatus)
            {
                // ============= Connect ============= //
                ConnectClient();
            }
            else
            {
                // ============= Disconnect ============= //
                DisconnectClient();
            }
        }

        private void UserMessageBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (UserMessageBox.Text.Equals("!clear"))
                    MessageArea.Clear();
                else
                    Network.Send(UserMessageBox.Text);

                UserMessageBox.Clear();
            }
        }

        private void LoginBtn_Click(object sender, EventArgs e)
        {
            if (!LoggedIn)
            {
                LoginClient();
            }
            else
            {
                LogoutClient();
            }
        }

        private void ConnectClient()
        {
            ConnectBtn.Enabled = false;

            try
            {
                Network.Connect(ServerIP, 4554, Username);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                MessageBox.Show(ex.ToString());
                ConnectBtn.Enabled = true;
                return;
            }

            // Start message receiving loop
            Thread receiving_thread = new Thread(delegate () {
                Network.StartMessageReceivingLoop(AppendMessageArea);
            });
            receiving_thread.IsBackground = true;
            receiving_thread.Start();

            ConnectedStatusLbl.ForeColor = Color.LawnGreen;
            ConnectedStatusLbl.Text = "Connected";

            ConnectBtn.Enabled = true;
            ConnectBtn.Text = "Disconnect";

            ConnectedStatus = true;
        }

        private void DisconnectClient()
        {
            ConnectBtn.Enabled = false;

            Network.Disconnect();

            ConnectedStatusLbl.ForeColor = Color.FromArgb(192, 0, 0);
            ConnectedStatusLbl.Text = "Disconnected";

            ConnectBtn.Enabled = true;
            ConnectBtn.Text = "Connect";

            ConnectedStatus = false;
        }

        private void LoginClient()
        {
            string username = UsernameInputTextbox.Text;

            if (string.IsNullOrWhiteSpace(username)) return;

            UsernameInputTextbox.Clear();
            Username = username;

            LoginStatusLbl.Text = Username;
            LoginStatusLbl.ForeColor = Color.LawnGreen;

            UsernameInputTextbox.Visible = false;
            UsernameLbl.Visible = false;

            ConnectBtn.Enabled = true;

            LoginBtn.Text = "Logout";
            LoggedIn = true;
        }

        private void LogoutClient()
        {
            if (ConnectedStatus)
            {
                DialogResult result = MessageBox.Show("Are you sure you want to disconnect and log out?", "Logout", MessageBoxButtons.YesNo);
                if (result.ToString().Equals("No")) return;

                DisconnectClient();
            }

            LoginStatusLbl.Text = "Logged Out";
            LoginStatusLbl.ForeColor = Color.FromArgb(192, 0, 0);

            UsernameInputTextbox.Visible = true;
            UsernameLbl.Visible = true;

            ConnectBtn.Enabled = false;

            LoginBtn.Text = "Login";
            LoggedIn = false;
        }

        private void UserMessageBox_Click(object sender, EventArgs e)
        {
            if (UserMessageBox.Text.Equals("Enter your message . . ."))
            {
                UserMessageBox.Clear();
            }
        }

        private void MessageArea_TextChanged(object sender, EventArgs e)
        {
            MessageArea.SelectionStart = MessageArea.Text.Length;
            MessageArea.ScrollToCaret();
        }
    }
}
