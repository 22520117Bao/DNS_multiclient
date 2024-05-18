using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace TryForBetter
{
    public partial class ServerForm : Form
    {
        private delegate void UpdateStatusCallback(string strMessage);
        public ServerForm()
        {
            InitializeComponent();
        }

        private void btnListen_Click(object sender, EventArgs e)
        {
            IPAddress ipAddr = IPAddress.Parse(txtIp.Text);
            ChatServer mainServer = new ChatServer(ipAddr);
            ChatServer.StatusChanged += new StatusChangedEventHandler(mainServer_StatusChanged);
            mainServer.StartListening();
            txtLog.AppendText("Đang lắng nghe các kết nối...\r\n");
        }
        public void mainServer_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            this.Invoke(new UpdateStatusCallback(this.UpdateStatus), new object[] { e.EventMessage });
        }

        private void UpdateStatus(string strMessage)
        {
            // Updates the log with the message
            txtLog.AppendText(strMessage + "\r\n");
        }
    }
}
