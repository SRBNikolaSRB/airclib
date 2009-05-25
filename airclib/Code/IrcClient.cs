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

        /// <summary>
        /// Connection event, connects to irc server.
        /// </summary>
        /// <param name="Server">Server address or ip.</param>
        /// <param name="Port">Server port.</param>
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
        /// <summary>
        /// Sends data to connected server, must be connected to some server.
        /// </summary>
        /// <param name="Data"></param>
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
        /// <summary>
        /// Reads all incoming data from socket.
        /// </summary>
        /// <param name="bListen">If set true, it will start listening.</param>
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

                Listen(bListen);
                return;
            }

            Listen(bListen);
            return;
        }

        /// <summary>
        /// Bool, returns isConnected.
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// Returns connections current irc Nick.
        /// </summary>
        /// <returns>String</returns>
        public string GetNick()
        {
            return Nick;
        }
        /// <summary>
        /// Returns clients current stearm.
        /// </summary>
        /// <returns></returns>
        public NetworkStream GetStream()
        {
            if (Stream != null)
                return Stream;
            else
                return null;
        }

        /// <summary>
        /// Disconnects from server.
        /// </summary>
        public void Disconnect()
        {
            irc.Close();
        }
        /// <summary>
        /// Quits, disconnects from server, with leaving message.
        /// </summary>
        /// <param name="Message">Leaving message.</param>
        public void Quit(string Message)
        {
            SendData("QUIT #" + Message);
            irc.Close();
        }
        /// <summary>
        /// Joins channel.
        /// </summary>
        /// <param name="Channel">Channel name.</param>
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
        /// <summary>
        /// Leaves channel.
        /// </summary>
        /// <param name="Channel">Channel.</param>
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
        /// <summary>
        /// Requests topic channel.
        /// </summary>
        /// <param name="Channel">Channel.</param>
        public void GetTopic(string Channel)
        {
            if (!isConnected || ChannelCount == 0)
                return;

            if (Channel.Contains("#"))
                SendData("TOPIC " + Channel);
            else
                SendData("TOPIC #" + Channel);
        }
        /// <summary>
        /// Gets name list from channel.
        /// </summary>
        /// <param name="Channel">From channel.</param>
        public void GetNames(string Channel)
        {
            if (!isConnected || ChannelCount == 0)
                return;

            if (Channel.Contains("#"))
                SendData("NAMES " + Channel);
            else
                SendData("NAMES #" + Channel);
        }
        /// <summary>
        /// Changes, sets topic to wanted channel.
        /// </summary>
        /// <param name="Channel">Wanted Channel.</param>
        /// <param name="Topic">Wanted Topic.</param>
        public void SetTopic(string Channel, string Topic)
        {
            if (!isConnected || ChannelCount == 0)
                return;

            string Data = String.Format("TOPIC #{0} :{1}", Channel, Topic);
            SendData(Data);
        }
        /// <summary>
        /// Query, messages nick.
        /// </summary>
        /// <param name="Nick">Sends message to this Nick/User.</param>
        /// <param name="Message">Message.</param>
        public void MessageUser(string Nick, string Message)
        {
            if (!isConnected)
                return;

            string Data = String.Format("PRIVMSG {0} :{1}", Nick, Message);
            SendData(Data);
        }
        /// <summary>
        /// Does wanted action.
        /// </summary>
        /// <param name="Target">To channel or user</param>
        /// <param name="Action">Action text.</param>
        /// <param name="Channel">Target type, channel or user.</param>
        public void DoAction(string Target, string Action, bool Channel)
        {
            if (!isConnected || ChannelCount == 0)
                return;
            
            string sharp = "";
            if (Channel)
            {
                if (!Target.Contains("#"))
                    sharp = "#";
            }

            string Data = String.Format("PRIVMSG {0}{1} :ACTION {2}", sharp, Target, Action);
            SendData(Data);
        }
        /// <summary>
        /// Request change of current connection's nick name.
        /// </summary>
        /// <param name="Nick">Wanted nick.</param>
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
        /// <summary>
        /// Request who is of user.
        /// </summary>
        /// <param name="Nick">Users nick.</param>
        public void WhoIs(string Nick)
        {
            if (!isConnected)
                return;

            SendData("WHOIS " + Nick);
        }
        /// <summary>
        /// Sends notice message to user.
        /// </summary>
        /// <param name="User">Users nickname.</param>
        /// <param name="Message">Notice.</param>
        public void Notice(string User, string Message)
        {
            if (!isConnected || ChannelCount == 0)
                return;

            string Data = String.Format("NOTICE {0} :{1}", User, Message);
            SendData(Data);
        }
        /// <summary>
        /// Requests server's current motd, message of the day.
        /// </summary>
        public void ServerMOTD()
        {
            if (!isConnected)
                return;

            SendData("MOTD");
        }
        /// <summary>
        /// Sends message to wanted channel, connection must be connected to channel.
        /// </summary>
        /// <param name="Channel">Channel name, connection must be connected to this channel.</param>
        /// <param name="Message">Message.</param>
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
