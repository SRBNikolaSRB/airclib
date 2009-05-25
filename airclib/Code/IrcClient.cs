using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using airclib;

namespace airclib
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

        public NetworkStream GetStream()
        {
            if (Stream != null)
                return Stream;
            else
                return null;
        }
    }
}
