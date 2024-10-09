using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using SecurityProject;
using System.IO;

namespace SecurityProject
{
    public partial class Form1 : Form
    {
        Socket sck;
        EndPoint epLocal, epRemote;
        int count = 0, packSize = 2;
        List<packageBlock> packs = new List<packageBlock>();
        public Form1()
        {
            InitializeComponent();
            sck = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sck.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            textLocalIp.Text = GetLocalIP();
            textFriendsIp.Text = GetLocalIP();
        }

        private string GetLocalIP()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }
        private void MessageCallBack(IAsyncResult aResult)
        {
            try
            {
                int size = sck.EndReceiveFrom(aResult, ref epRemote);
                if (size > 0)
                {
                    byte[] receivedData = new byte[1464];
                    receivedData = (byte[])aResult.AsyncState;
                    string receivedMessage = "";
                    packageBlock package = new packageBlock();
                    package = package.Deserialize(receivedData);
                    if (package.encryption == "RSA")//RSA
                    {
                        RSA rsa = new RSA();
                        receivedMessage = rsa.Decrypt(rsa, package.package);
                    }

                    if (package.type != "file" && package.type != "image")
                        listMessage.Items.Add("(" + package.encryption + ")" + "Friend: " + receivedMessage);
                    else if (package.type == "file")//SAVE FILE
                    {

                        using (StreamWriter sw = File.CreateText("test.txt"))
                        {
                            sw.WriteLine(receivedMessage);
                        }
                        listMessage.Items.Add("(" + package.encryption + ")" + "Friend: sent you a file.");

                    }
                    if (count==0)//auto scroll down chat
                    {
                        listMessage.SelectedIndex = listMessage.Items.Count - 1;
                        listMessage.SelectedIndex = -1;
                    }
                    
                }
                byte[] buffer = new byte[1500];
                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);
                
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textLocalPort.Text != "" && textFriendsPort.Text != "")
            {
                try
                {
                    // binding socket
                    epLocal = new IPEndPoint(IPAddress.Parse(textLocalIp.Text),
                    Convert.ToInt32(textLocalPort.Text));
                    sck.Bind(epLocal);
                    // connect to remote IP and port
                    epRemote = new IPEndPoint(IPAddress.Parse(textFriendsIp.Text),
                    Convert.ToInt32(textFriendsPort.Text));
                    sck.Connect(epRemote);
                    // starts to listen to an specific port
                    byte[] buffer = new byte[1500];
                    sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new
                    AsyncCallback(MessageCallBack), buffer);
                    // release button to send message
                    button2.Enabled = true;//SEND
                  
                    button1.Text = "Connected";
                    button1.Enabled = false;
                    textMessage.Focus();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            else
            {
                MessageBox.Show("missing ports");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            RSA_manual rsa = new RSA_manual();
            rsa.Show();
        }

 

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
        }

        private void listMessage_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

  

        // SENDING PLAINTEXT  
        private void button2_Click(object sender, EventArgs e)
        {
            if(textMessage.Text!="")
            {
                try
                {
                    byte[] msg;
                    packageBlock package = new packageBlock();

               
                 

                    if (comboBox1.SelectedIndex == 0)
                    {
                        RSA rsa = new RSA();
                        msg = rsa.Encrypt(rsa, textMessage.Text);
                        package.encryption = "RSA";
                        package.size = 1;
                        package.id = 1;
                        package.package = msg;
                    }

                 

                    byte[] msgPack = package.Serialize(package);//packing the msg
                    sck.Send(msgPack);// sending the message


                    // add to listbox
                    listMessage.Items.Add("(" + package.encryption + ")" + "You: " + textMessage.Text);
                    // clear txtMessage
                    textMessage.Clear();
                    listMessage.SelectedIndex = listMessage.Items.Count - 1;
                    listMessage.SelectedIndex = -1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            
        }
    }
}
