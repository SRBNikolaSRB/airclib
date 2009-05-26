/* IrcClient.cs
   Advanced IRC Library Project (airclib)
   See LICENSE file for Copyrights
   Website "http://code.google.com/p/airclib/" */

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using airclib.locals;

namespace airclib
{
    public class IrcClient : Locals
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

        #region "Connection"
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

            try
            {
                StreamReader Reader = new StreamReader(irc.GetStream());
                for (int i = 0; i < 10; i++)
                {
                    string Data = Reader.ReadLine();
                    while (Data != "")
                    {
                        if (OnReciveData != null)
                            OnReciveData(Data);

                        if (Data.Contains("PING"))
                            SendData(Data.Replace("PING", "PONG"));

                        Data = "";
                        Data = Reader.ReadLine();

                        if (i == 10)
                        {
                            Listen(bListen);
                            return;
                        }
                    }
                }
            }
            catch
            {
                Listen(true);
            }
        }
        #endregion

        #region "Reading"
        /// <summary>
        /// Reads channel or user private message. Returns PrivmsgData.
        /// </summary>
        /// <param name="Data">Data to get readed.</param>
        /// <returns>PrivmsgData.</returns>
        public PrivmsgData ReadPrivmsg(string Data)
        {
            PrivmsgData pmsg = new PrivmsgData();

            if (!isConnected)
                return pmsg;

            if (!Data.Contains("PRIVMSG"))
            {
                pmsg.Type = DataType.MSGTYPE_DEFAULT;
                pmsg.WholeData = Data;

                return pmsg;
            }
            else
            {
                try
                {
                    if (ChannelCount == 0)
                    {
                        string[] sp = { " " };
                        string[] sData = Data.Split(sp, 4, StringSplitOptions.None);

                        pmsg.Sender = sData[0];
                        pmsg.Command = sData[1];
                        pmsg.Target = sData[2];
                        pmsg.Message = sData[3];
                        pmsg.Type = DataType.MSGTYPE_USER;
                        pmsg.WholeData = Data;

                        return pmsg;
                    }
                    else
                    {
                        string[] sp = { " " };
                        string[] sData = Data.Split(sp, 4, StringSplitOptions.None);

                        pmsg.Sender = sData[0];
                        pmsg.Command = sData[1];
                        pmsg.Target = sData[2];
                        pmsg.Message = sData[3];
                        pmsg.WholeData = Data;

                        if (pmsg.Target.Contains("#")) // if it does, that is channel
                            pmsg.Type = DataType.MSGTYPE_CHANNEL; // Than message is channel type
                        else
                            pmsg.Type = DataType.MSGTYPE_USER;

                        return pmsg;
                    }
                }
                catch
                {
                    return pmsg;
                }
            }
        }
        /// <summary>
        /// Parses nickname of sender.
        /// </summary>
        /// <param name="Sender">Sender ID, i recommend using ReadPrivmsg's parsed sender.</param>
        /// <returns>Nickname.</returns>
        public string ReadNick(string Sender)
        {
            if (!isConnected)
                return null;

            try
            {
                string[] sp = { ":", "!" };
                string[] sp1 = Sender.Split(sp, 2, StringSplitOptions.RemoveEmptyEntries);
                return sp1[0];
            }
            catch
            {
                return Sender;
            }
        }
        /// <summary>
        /// Reads data with server type.
        /// </summary>
        /// <param name="Message">Actual data.</param>
        /// <returns>Returns server data structure.</returns>
        public ServerData ReadServerData(string Message)
        {
            ServerData cd = new ServerData();

            if (!isConnected)
                return cd;

            string[] sp = { " " };
            string[] msg = Message.Split(sp, 4, StringSplitOptions.None);

            cd.Sender = msg[0];
            cd.Command = msg[1];
            cd.Target = msg[2];
            cd.Message = ReadOnlyMessage(msg[3]);

            return cd;
        }
        /// <summary>
        /// Removes ":" from wanted message.
        /// </summary>
        /// <param name="Message">Wanted message.</param>
        /// <returns>String.</returns>
        public string ReadOnlyMessage(string Message)
        {
            if (!isConnected)
                return null;

            try
            {
                string cData = Message.Replace(":", "");
                return cData;
            }
            catch
            {
                return Message;
            }
        }
        /// <summary>
        /// Reads action, with normal formular, action sender and action it self. Data should be action type.
        /// </summary>
        /// <param name="Data">Data, reccomended action type.</param>
        /// <returns>ActionData structure.</returns>
        public ActionData GetAction(string Data)
        {
            ActionData ad = new ActionData();

            if (GetDataType(Data) != DataType.MSGTYPE_ACTION && !isConnected)
                return ad;

            try
            {
                string[] st = { " " };
                string[] dt = Data.Split(st, 4, StringSplitOptions.None);
                string newData = dt[3].Replace(":ACTION ", "");
                newData = newData.Replace("", "");
                ad.Sender = ReadNick(dt[0]);
                ad.Action = newData;
                return ad;
            }
            catch
            {
                return ad;
            }
        }
        /// <summary>
        /// Reads irc data, returning its data type.
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public DataType GetDataType(string Data)
        {
            if (!isConnected)
                return DataType.NULL;

            if (Data.Contains("PRIVMSG"))
                if (Data.Contains("#") && ChannelCount != 0 && !Data.Contains("INV")) // must be a channel type
                    return DataType.MSGTYPE_CHANNEL;
                else if (Data.Contains("ACTION "))
                    return DataType.MSGTYPE_ACTION;
                else
                    return DataType.MSGTYPE_USER;
            else
                return DataType.MSGTYPE_SERVER;
        }
        #endregion

        #region "Connection States"
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
        #endregion

        #region "IRC Commands"
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

            string sharp = "";

            if (!Channel.Contains("#"))
                sharp = "#";


            string Data = String.Format("TOPIC {0}{1} :{2}", sharp, Channel, Topic);
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
        /// <summary>
        /// Invites user with wanted nickname to wanted channel.
        /// </summary>
        /// <param name="Nickname">Users nickname.</param>
        /// <param name="Channel">Wanted channel.</param>
        public void InviteToChannel(string Nickname, string Channel)
        {
            if (!isConnected)
                return;

            string sharp = "";
            if (!Channel.Contains("#"))
                sharp = "#";

            string Data = String.Format("INVITE {0} {1}{2}", Nickname, sharp, Channel);
            SendData(Data);
        }
        /// <summary>
        /// Sets away, and away message.
        /// </summary>
        /// <param name="Away">Boolean, true for being away.</param>
        /// <param name="Message">Message, only works if Away is true.</param>
        public void SetAway(bool Away, string Message)
        {
            if (!isConnected)
                return;

            if (!Away)
                SendData("AWAY");
            else
                SendData("AWAY " + Message);
        }
        #endregion
    }
}
