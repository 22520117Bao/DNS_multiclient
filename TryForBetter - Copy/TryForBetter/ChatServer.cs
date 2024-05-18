using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TryForBetter
{
    public class StatusChangedEventArgs : EventArgs
    {
        private string EventMsg;

        public string EventMessage
        {
            get
            {
                return EventMsg;
            }
            set
            {
                EventMsg = value;
            }
        }

        public StatusChangedEventArgs(string strEventMsg)
        {
            EventMsg = strEventMsg;
        }
    }

    public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs e);

    class ChatServer
    {
        public static Hashtable htUsers = new Hashtable(30);
        public static Hashtable htConnections = new Hashtable(30);
        private IPAddress ipAddress;
        private TcpClient tcpClient;
        public static event StatusChangedEventHandler StatusChanged;
        private static StatusChangedEventArgs e;

        public ChatServer(IPAddress address)
        {
            ipAddress = address;
        }

        private Thread thrListener;

        private TcpListener tlsClient;

        bool ServRunning = false;

        public static void AddUser(TcpClient tcpUser, string strUsername)
        {
            ChatServer.htUsers.Add(strUsername, tcpUser);
            ChatServer.htConnections.Add(tcpUser, strUsername);
            SendAdminMessage(htConnections[tcpUser] + " đã đăng nhập!");
        }

        public static void RemoveUser(TcpClient tcpUser)
        {
            if (htConnections[tcpUser] != null)
            {
                SendAdminMessage(htConnections[tcpUser] + " đã đăng xuất!");
                ChatServer.htUsers.Remove(ChatServer.htConnections[tcpUser]);
                ChatServer.htConnections.Remove(tcpUser);
            }
        }

        public static void OnStatusChanged(StatusChangedEventArgs e)
        {
            StatusChangedEventHandler statusHandler = StatusChanged;
            if (statusHandler != null)
            {
                statusHandler(null, e);
            }
        }

        public static void SendAdminMessage(string Message)
        {
            StreamWriter swSenderSender;
            e = new StatusChangedEventArgs("Administrator: " + Message);
            OnStatusChanged(e);
            TcpClient[] tcpClients = new TcpClient[ChatServer.htUsers.Count];
            ChatServer.htUsers.Values.CopyTo(tcpClients, 0);
            for (int i = 0; i < tcpClients.Length; i++)
            {
                try
                {
                    if (Message.Trim() == "" || tcpClients[i] == null)
                    {
                        continue;
                    }
                    swSenderSender = new StreamWriter(tcpClients[i].GetStream());
                    swSenderSender.WriteLine("Administrator: " + Message);
                    swSenderSender.Flush();
                    swSenderSender = null;
                }
                catch
                {
                    RemoveUser(tcpClients[i]);
                }
            }
        }
        static string ResolveDns(string domainName)
        {
            
            int b = 0;
            string c = "";
            string d = "";
            string a = "";
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(domainName);

                //Console.WriteLine($"Host Name: {hostEntry.HostName}");

                //Console.WriteLine("IP Addresses:");
                //foreach (IPAddress aa in hostEntry.AddressList)
                //{
                //    b++;
                //}


                    foreach (IPAddress ip in hostEntry.AddressList)
                {
                    b++;
                    if (b == 1)
                    {
                        c = ip.ToString();
                        
                    }
                    
                     if (b == 2)
                     {
                            d = "   IP address:  "+ip.ToString();
                            
                     }
                             
                }
                if (b == 1)
                {
                    a = "IP address:  " +c ;
                }
                else
                {
                    a = c + d;
                }
               
            }
             
            catch (Exception ex)
            {
                a = "";
                MessageBox.Show("DNS name ko phu hop");
            }
            return a;
        }
        public static void SendDNSMessage(string Message)
        {
            string a = ResolveDns(Message);
            StreamWriter swSenderSender;
            e = new StatusChangedEventArgs("Administrator: " + a);
            OnStatusChanged(e);
            TcpClient[] tcpClients = new TcpClient[ChatServer.htUsers.Count];
            ChatServer.htUsers.Values.CopyTo(tcpClients, 0);
            for (int i = 0; i < tcpClients.Length; i++)
            {
                try
                {
                    if (a.Trim() == "" || tcpClients[i] == null)
                    {
                        continue;
                    }
                    swSenderSender = new StreamWriter(tcpClients[i].GetStream());
                    swSenderSender.WriteLine("Administrator: " + a);
                    swSenderSender.Flush();
                    swSenderSender = null;
                }
                catch
                {
                    RemoveUser(tcpClients[i]);
                }
            }
        }



        public static void SendMessage(string From, string Message)
        {
            StreamWriter swSenderSender;

            e = new StatusChangedEventArgs(From + " gửi: " + Message);
            OnStatusChanged(e);

            TcpClient[] tcpClients = new TcpClient[ChatServer.htUsers.Count];
            ChatServer.htUsers.Values.CopyTo(tcpClients, 0);
            for (int i = 0; i < tcpClients.Length; i++)
            {
                try
                {
                    if (Message.Trim() == "" || tcpClients[i] == null)
                    {
                        continue;
                    }
                    swSenderSender = new StreamWriter(tcpClients[i].GetStream());
                    swSenderSender.WriteLine(From + " gửi: " + Message);
                    swSenderSender.Flush();
                    swSenderSender = null;
                }
                catch
                {
                    RemoveUser(tcpClients[i]);
                }
            }
        }

        public void StartListening()
        {

            IPAddress ipaLocal = ipAddress;

            tlsClient = new TcpListener(ipaLocal, 1986);

            tlsClient.Start();

            ServRunning = true;

            thrListener = new Thread(KeepListening);
            thrListener.Start();
        }

        private void KeepListening()
        {
            while (ServRunning == true)
            {
                tcpClient = tlsClient.AcceptTcpClient();
                Connection newConnection = new Connection(tcpClient);
            }
        }
    }

    class Connection
    {
        TcpClient tcpClient;
        private Thread thrSender;
        private StreamReader srReceiver;
        private StreamWriter swSender;
        private string currUser;
        private string strResponse;

        public Connection(TcpClient tcpCon)
        {
            tcpClient = tcpCon;
            thrSender = new Thread(AcceptClient);
            thrSender.Start();
        }

        private void CloseConnection()
        {
            tcpClient.Close();
            srReceiver.Close();
            swSender.Close();
        }

        private void AcceptClient()
        {
            srReceiver = new System.IO.StreamReader(tcpClient.GetStream());
            swSender = new System.IO.StreamWriter(tcpClient.GetStream());

            currUser = srReceiver.ReadLine();

            if (currUser != "")
            {
                if (ChatServer.htUsers.Contains(currUser) == true)
                {
                    swSender.WriteLine("0|This username already exists.");
                    swSender.Flush();
                    CloseConnection();
                    return;
                }
                else if (currUser == "Administrator")
                {
                    swSender.WriteLine("0|This username is reserved.");
                    swSender.Flush();
                    CloseConnection();
                    return;
                }
                else
                {
                    swSender.WriteLine("1");
                    swSender.Flush();

                    ChatServer.AddUser(tcpClient, currUser);
                }
            }
            else
            {
                CloseConnection();
                return;
            }
            //currUser = srReceiver.ReadLine();
            try
            {
                while ((strResponse = srReceiver.ReadLine()) != "")
                {
                    if (strResponse == null)
                    {
                        ChatServer.RemoveUser(tcpClient);
                    }
                    else
                    {
                        if (IsValidDomain(strResponse))
                        {
                            ChatServer.SendDNSMessage(strResponse);
                        }
                        else
                        {
                            ChatServer.SendMessage(currUser, strResponse);
                        }
                    }

                }
            }
            catch
            {
                ChatServer.RemoveUser(tcpClient);
            }
        }

        static bool IsValidDomain(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            // Define the regular expression for a valid domain name
            string domainPattern = @"^(?!-)[A-Za-z0-9-]{1,63}(?<!-)\.[A-Za-z]{2,6}$";

            // Use regular expression to check if the input matches the domain pattern
            return Regex.IsMatch(input, domainPattern);
        }
    }
}
