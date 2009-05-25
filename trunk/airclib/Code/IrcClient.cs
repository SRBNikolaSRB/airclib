/* IrcClient.cs
   Advanced IRC Library Project (airclib)
   See LICENSE file for Copyrights
   Website "http://code.google.com/p/airclib/" */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using airc;

namespace airc
{
    class IrcClient : Locals
    {
        private TcpClient irc = new TcpClient();
        private NetworkStream Stream;

        //Vars
        private bool isConnected;
        private int ChannelCount = 0;
        private string Nick = "";

        //Events
        public event OnConnectEventHandler OnConnect;
        public delegate void OnConnectEventHandler();
        public event OnDataSentEventHandler OnDataSent;
        public delegate void OnDataSentEventHandler(string Data);
        public event OnReciveDataEventHandler OnReciveData;
        public delegate void OnReciveDataEventHandler(string Data);

        public void Connect(string Server, int Port)
        {
            try
            {
                irc.Connect(Server, Port);
                Stream = irc.GetStream();
                isConnected = true;
                if (OnConnect != null)
                    OnConnect();
            }
            catch
            {
                return;
            }
        }
        public void Connect(IrcServer Server)
        {
            try
            {
                irc.Connect(Server.Server, Server.Port);
                Stream = irc.GetStream();
                isConnected = true;
                if (OnConnect != null)
                    OnConnect();
            }
            catch
            {
                return;
            }
        }
        public void SendData(string Data)
        {
            try
            {
                StreamWriter Writer = new StreamWriter(Stream);
                Writer.WriteLine(Data, Stream);
                Writer.Flush();
                if (OnDataSent != null)
                    OnDataSent(Data);
            }
            catch
            {
                return;
            }
        }
        public void Listen(bool bListen)
        {
            if (!bListen || !isConnected || Stream == null)
                return;

            StreamReader Reader = new StreamReader(Stream);
            string Data = Reader.ReadLine();
            while (isConnected == true && Data != "")
            {
                if (OnReciveData != null)
                    OnReciveData(Data);

                Listen(true);
                return;
            }

            Listen(true);
            return;
        }

        public bool Connected()
        {
            if (isConnected == true)
                return true;
            else
                return false;
        }
        public bool IsInChannel()
        {
            if (ChannelCount != 0)
                return true;
            else
                return false;
        }
        public string GetNick()
        {
            return Nick;
        }
        public NetworkStream GetStream()
        {
            if (Stream != null)
                return Stream;
            else
                return null;
        }

        public void Disconnect()
        {
            irc.Close();
        }
        public void Quit(string Reason)
        {
            SendData("QUIT #" + Reason);
            irc.Close();
        }
        public void JoinChannel(string Channel)
        {
            if (!isConnected || ChannelCount == 0)
                return;

            if (Channel.Contains("#"))
                SendData("JOIN " + Channel);
            else
                SendData("JOIN #" + Channel);

            ChannelCount++;
            GetTopic(Channel);
        }
        public void LeaveChannel(string Channel)
        {
            if (!isConnected || ChannelCount == 0)
                return;

            string sharp = "";
            if (!Channel.Contains("#"))
                sharp = "#";

            SendData("PART " + sharp + Channel);
            ChannelCount--;
        }
        public void GetTopic(string Channel)
        {
            if (!isConnected || ChannelCount == 0)
                return;

            if (Channel.Contains("#"))
                SendData("TOPIC " + Channel);
            else
                SendData("TOPIC #" + Channel);
        }
        public void GetNames(string Channel)
        {
            if (!isConnected || ChannelCount == 0)
                return;

            if (Channel.Contains("#"))
                SendData("NAMES " + Channel);
            else
                SendData("NAMES #" + Channel);
        }
        public void SetTopic(string Channel, string Topic)
        {
            if (!isConnected || ChannelCount == 0)
                return;

            string Data = String.Format("TOPIC #{0} :{1}", Channel, Topic);
            SendData(Data);
        }
        public void MessageUser(string User, string Message)
        {
            if (!isConnected)
                return;

            string Data = String.Format("PRIVMSG {0} :{1}", User, Message);
            SendData(Data);
        }
        public void DoAction(string Channel, string Action)
        {
            if (!isConnected || ChannelCount == 0)
                return;

            string sharp = "";
            if (!Channel.Contains("#"))
                sharp = "#";

            string Data = String.Format("PRIVMSG {0}{1} :ACTION {2}", sharp, Channel, Action);
            SendData(Data);
        }
        public void SetNick(string Nick)
        {
            if (!isConnected)
                return;

            try
            {
                SendData("NICK " + Nick);
                this.Nick = Nick;
            }
            catch
            {
                return;
            }
        }
        public void WhoIs(string Nick)
        {
            if (!isConnected)
                return;

            SendData("WHOIS " + Nick);
        }
        public void Notice(string User, string Message)
        {
            if (!isConnected || ChannelCount == 0)
                return;

            string Data = String.Format("NOTICE {0} :{1}", User, Message);
            SendData(Data);
        }
        public void ServerMOTD()
        {
            if (!isConnected)
                return;

            SendData("MOTD");
        }
        public void MessageChannel(string Channel, string Message)
        {
            if (!isConnected || ChannelCount == 0)
                return;

            string sharp = "";
            if (!Channel.Contains("#"))
                sharp = "#";

            string Data = String.Format("PRIVMSG {0}{1} :{2}", sharp, Channel, Message);
            SendData(Data);
        }
    }
}
