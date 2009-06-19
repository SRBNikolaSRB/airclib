/* IrcClient.cs
 * Advanced IRC Library Project (airclib)
 * See LICENSE file for Copyright
 * Website "http://code.google.com/p/airclib/" 
 */

#region Using...
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using airclib.locals;
#endregion

namespace airclib
{
    /// <summary>
    /// IrcClient class, main class of airclib.
    /// </summary>
    public class IrcClient : Locals
    {
        private TcpClient irc = new TcpClient();
        private NetworkStream Stream;

        //Vars
        private bool isConnected;
        private int ChannelCount = 0;
        private string Nick = "";

        //Events
        public delegate void OnConnectEventHandler();
        public delegate void OnDataSentEventHandler(string Data);
        public delegate void OnReciveDataEventHandler(string Data);
        public delegate void OnChannelJoinEventHandler(string Channel);
        public delegate void OnUserJoinedChannelEventHandler(string UserNick, string Channel);
        public delegate void OnUserLeftChannelEventHandler(string UserNick, string Channel);
        public event OnConnectEventHandler OnConnect;
        public event OnDataSentEventHandler OnDataSent;
        public event OnReciveDataEventHandler OnReciveData;
        public event OnChannelJoinEventHandler OnChannelJoin;
        public event OnUserJoinedChannelEventHandler OnUserJoinedChannel;
        public event OnUserLeftChannelEventHandler OnUserLeftChannel;

        #region Connection
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
        /// <summary>
        /// Connection event, connects to wanted "IrcServer"
        /// </summary>
        /// <param name="Server">Server structure.</param>
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
                        if (Data.Contains("PING"))
                        {
                            SendData(Data.Replace("PING", "PONG"));
                            Data = "";
                        }
                        else if (Data.Contains(" JOIN #")) // hooks OnUserJoinedChannel(string UserNick, string Channel)
                        {
                            string[] dt = Data.Split(' ');

                            if (OnUserJoinedChannel != null && ReadNick(dt[0]) != GetNick())
                                OnUserJoinedChannel(ReadNick(dt[0]), dt[2]);
                        }
                        else if (Data.Contains(" PART #")) // hooks OnUserLeftChannel(string UserNick, string Channel)
                        {
                            string[] dt = Data.Split(' ');
                            if (OnUserLeftChannel != null && ReadNick(dt[0]) != GetNick())
                                OnUserLeftChannel(ReadNick(dt[0]), dt[2]);
                        }

                        if (OnReciveData != null && Data != null)
                            OnReciveData(Data);

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

        #region Reading
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

            if (!Data.Contains("PRIVMSG") && !Data.Contains("NOTICE"))
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

            try
            {
                cd.Sender = msg[0];
                cd.Command = msg[1];
                cd.Target = msg[2];
                cd.Message = ReadOnlyMessage(msg[3]);
            }
            catch
            {
                return cd;
            }

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
                ad.Target = dt[2];
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

            if (Data.Contains("PRIVMSG") || Data.Contains("NOTICE"))
                if (ReadPrivmsg(Data).Command != "NOTICE" && Data.Split(' ')[2].StartsWith("#") && ChannelCount != 0) // must be a channel type
                    return DataType.MSGTYPE_CHANNEL;
                else if (Data.Contains("ACTION "))
                    return DataType.MSGTYPE_ACTION;
                else
                    return DataType.MSGTYPE_USER;
            else
                return DataType.MSGTYPE_SERVER;
        }
        #endregion

        #region Connection States
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
        /// <summary>
        /// Returns channel count.
        /// </summary>
        /// <returns></returns>
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

        #region IRC Commands
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
            SendData("QUIT :" + Message);
            irc.Close();
        }
        /// <summary>
        /// Joins channel.
        /// </summary>
        /// <param name="Channel">Channel name.</param>
        public void JoinChannel(string Channel)
        {
            if (!isConnected)
                return;

            SendData("JOIN " + Channel);

            if (OnChannelJoin != null)
                OnChannelJoin(Channel);

            ChannelCount++;
            GetTopic(Channel);
            GetNames(Channel);
        }
        /// <summary>
        /// Leaves channel.
        /// </summary>
        /// <param name="Channel">Channel.</param>
        public void LeaveChannel(string Channel)
        {
            if (!isConnected || ChannelCount == 0)
                return;

            SendData("PART " + Channel);
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

            SendData("TOPIC " + Channel);
        }
        /// <summary>
        /// Gets name list from channel.
        /// </summary>
        /// <param name="Channel">From channel.</param>
        public void GetNames(string Channel)
        {
            if (!isConnected || ChannelCount == 0)
                return;

            SendData("NAMES " + Channel);
        }
        /// <summary>
        /// Query, messages nick.
        /// </summary>
        /// <param name="Nick">Sends message to this Nick/User.</param>
        /// <param name="Message">Message.</param>
        public void MessageUser(string tNick, string Message)
        {
            if (!isConnected)
                return;

            string Data = String.Format("PRIVMSG {0} :{1}", tNick, Message);
            SendData(Data);
        }
        /// <summary>
        /// Query, messages nick. With color.
        /// </summary>
        /// <param name="Nick">Sends message to this Nick/User.</param>
        /// <param name="Message">Message.</param>
        /// <param name="Color">Wanted message color.</param>
        public void MessageUser(string tNick, string Message, ColorMessages Color)
        {
            if (!isConnected)
                return;

            string Data = String.Format("PRIVMSG {0} :\u0003{2} {1}", tNick, Message, (int)Color);
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


            string Data = String.Format("PRIVMSG {0} :ACTION {1}", Target, Action);
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
            if (!isConnected)
                return;

            string Data = String.Format("PRIVMSG {0} :{1}", Channel, Message);
            SendData(Data);
        }
        /// <summary>
        /// Sends message to wanted channel, connection must be connected to channel. With color.
        /// </summary>
        /// <param name="Channel">Channel name, connection must be connected to this channel.</param>
        /// <param name="Message">Message.</param>
        /// <param name="Color">Wanted color.</param>
        public void MessageChannel(string Channel, string Message, ColorMessages Color)
        {
            if (!isConnected)
                return;

            string Data = String.Format("PRIVMSG {0} :\u0003{1} {2}", Channel, (int)Color, Message);
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

            string Data = String.Format("INVITE {0} {1}", Nickname, Channel);
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
        /// <summary>
        /// Sets user info. Args talk for them self.
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="HostName"></param>
        /// <param name="ServerName"></param>
        /// <param name="RealName"></param>
        public void SetUserInfo(string UserName, string HostName, string ServerName, string RealName)
        {
            // Command: USER
            // Parameters: <username> <hostname> <servername> <realname>
            if (!isConnected)
                return;

            string data = String.Format("USER {0} {1} {2} {3}", UserName, HostName, ServerName, RealName);
            SendData(data);
        }
        #endregion

        #region Text Effects
        /// <summary>
        /// Changes color of wanted text.
        /// </summary>
        /// <param name="Text">Wanted text.</param>
        /// <param name="Color">Wanted color.</param>
        /// <returns>Returns text with changed color.</returns>
        public string ColorText(string Text, ColorMessages Color)
        {
            return ColoredFont + (int)Color + Text + FontEnd;
        }
        /// <summary>
        /// Makes wanted text bold.
        /// </summary>
        /// <param name="Text">Wanted text.</param>
        /// <returns>Text with bold effect.</returns>
        public string BoldText(string Text)
        {
            return BoldFont + Text + FontEnd;
        }
        /// <summary>
        /// Underlines wanted text.
        /// </summary>
        /// <param name="Text">Wanted text.</param>
        /// <returns>Underlined text.</returns>
        public string UnderlineText(string Text)
        {
            return UnderlineFont + Text + FontEnd;
        }
        #endregion

        #region Channel Administration
        /// <summary>
        /// Kicks wanted user from channel.
        /// </summary>
        /// <param name="Channel">Channel to kick from.</param>
        /// <param name="User">Wanted user.</param>
        public void Kick(string Channel, string User)
        {
            if (!isConnected && ChannelCount <= 0)
                return;

            SendData(String.Format("KICK {0} {1}", Channel, User));
        }
        /// <summary>
        /// Kicks wanted user from channel. With message.
        /// </summary>
        /// <param name="Channel">Channel to kick from.</param>
        /// <param name="User">Wanted user.</param>
        /// <param name="Message">Message, reason.</param>
        public void Kick(string Channel, string User, string Message)
        {
            if (!isConnected && ChannelCount <= 0)
                return;

            SendData(String.Format("KICK {0} {1} {2}", Channel, User, Message));
        }
        /// <summary>
        /// Bans user from channel.
        /// </summary>
        /// <param name="Channel">From channel.</param>
        /// <param name="User">Wanted user.</param>
        public void Ban(string Channel, string User)
        {
            if (!isConnected && ChannelCount <= 0)
                return;

            SendData(String.Format("MODE {0} +b {1}", Channel, User));
        }
        /// <summary>
        /// Unbans user from channel.
        /// </summary>
        /// <param name="Channel">From channel.</param>
        /// <param name="User">Wanted user.</param>
        public void UnBan(string Channel, string User)
        {
            if (!isConnected && ChannelCount <= 0)
                return;

            SendData(String.Format("MODE {0} -b {1}", Channel, User));
        }
        /// <summary>
        /// Bans, than kicks wanted user in wanted channel.
        /// </summary>
        /// <param name="Channel">Channel location.</param>
        /// <param name="User">Wanted user.</param>
        public void KickBan(string Channel, string User)
        {
            if (!isConnected && ChannelCount <= 0)
                return;

            SendData(String.Format("MODE {0} +b {1}", Channel, User));
            SendData(String.Format("KICK {0} {1} {2}", Channel, User));
        }
        /// <summary>
        /// Bans, than kicks wanted user in wanted channel. With message.
        /// </summary>
        /// <param name="Channel">Channel location.</param>
        /// <param name="User">Wanted user.</param>
        /// <param name="Message">Good by message, reason.</param>
        public void KickBan(string Channel, string User, string Message)
        {
            if (!isConnected && ChannelCount <= 0)
                return;

            SendData(String.Format("MODE {0} +b {1}", Channel, User));
            SendData(String.Format("KICK {0} {1} {2}", Channel, User, Message));
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



            string Data = String.Format("TOPIC {0} :{1}", Channel, Topic);
            SendData(Data);
        }
        #endregion
    }
}

