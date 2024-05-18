using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TryForBetter
{
    public partial class Client4Form : Form
    {
        private string UserName = "Unknown";
        private StreamWriter swSender;
        private StreamReader srReceiver;
        private TcpClient tcpServer;
        private delegate void UpdateLogCallback(string strMessage);
        private delegate void CloseConnectionCallback(string strReason);
        private Thread thrMessaging;
        private IPAddress ipAddr;
        private bool Connected;
        public Client4Form()
        {
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            InitializeComponent();
        }
        public void OnApplicationExit(object sender, EventArgs e)
        {
            if (Connected == true)
            {
                Connected = false;
                swSender.Close();
                srReceiver.Close();
                tcpServer.Close();
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (Connected == false)
            {
                InitializeConnection();
            }
            else
            {
                CloseConnection("Hủy kết nối với user.");
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMessage();
        }
        private void InitializeConnection()
        {
            ipAddr = IPAddress.Parse(txtIp.Text);
            tcpServer = new TcpClient();
            tcpServer.Connect(ipAddr, 1986);

            Connected = true;
            UserName = txtUser.Text;

            txtIp.Enabled = false;
            txtUser.Enabled = false;
            txtMessage.Enabled = true;
            btnSend.Enabled = true;
            btnConnect.Text = "Hủy kết nối";

            swSender = new StreamWriter(tcpServer.GetStream());
            swSender.WriteLine(txtUser.Text);
            swSender.Flush();

            thrMessaging = new Thread(new ThreadStart(ReceiveMessages));
            thrMessaging.Start();
        }

        private void ReceiveMessages()
        {
            srReceiver = new StreamReader(tcpServer.GetStream());
            string ConResponse = srReceiver.ReadLine();
            if (ConResponse[0] == '1')
            {
                this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { "Kết nối thành công!" });
            }
            else
            {
                string Reason = "Chưa kết nối: ";
                Reason += ConResponse.Substring(2, ConResponse.Length - 2);
                this.Invoke(new CloseConnectionCallback(this.CloseConnection), new object[] { Reason });
                return;
            }
            while (Connected)
            {
                this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { srReceiver.ReadLine() });
            }
        }

        private void UpdateLog(string strMessage)
        {
            txtLog.AppendText(strMessage + "\r\n");
        }


        private void CloseConnection(string Reason)
        {
            txtLog.AppendText(Reason + "\r\n");
            txtIp.Enabled = true;
            txtUser.Enabled = true;
            txtMessage.Enabled = false;
            btnSend.Enabled = false;
            btnConnect.Text = "Kết nối";
            Connected = false;
            swSender.Close();
            srReceiver.Close();
            tcpServer.Close();
        }

        private void SendMessage()
        {
            if (txtMessage.Lines.Length >= 1)
            {
                swSender.WriteLine(txtMessage.Text);
                swSender.Flush();
                txtMessage.Lines = null;
            }
            txtMessage.Text = "";
        }

        private void txtMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                SendMessage();
            }
        }
    }
}
