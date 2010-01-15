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
using System.Threading;
using airclib.Constants;
#endregion

namespace airclib
{
    /// <summary>
    /// IrcClient class, main class of airclib.
    /// </summary>
    public class IrcClient
    {
        #region Events and variables

        //Variables
        private TcpClient m_connection = new TcpClient();
        private NetworkStream m_stream;
        private Thread m_listenThread;
        private TextEffects m_effects = new TextEffects();

        private bool m_isConnected = false;
        private int m_channelCount = 0;
        private string m_nick = "";
        private string m_server = "";
        private int m_port = 0;

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

        #endregion

        #region Class Constructors

        /// <summary>
        /// Initializes IrcClient class.
        /// </summary>
        /// <param name="Server">Server that IrcClient will connect to.</param>
        /// <param name="Port">Port of server.</param>
        public IrcClient(string Server, int Port)
        {
            m_server = Server;
            m_port = Port;
        }

        /// <summary>
        /// Initializes IrcClient class.
        /// </summary>
        /// <param name="Server">Server that IrcClient will connect to.</param>
        public IrcClient(IrcServer Server)
            : this(Server.Server, Server.Port)
        {
        }

        #endregion

        #region Get, set private variables

        public int ServerPort
        {
            get
            {
                return m_port;
            }
            set
            {
                m_port = value;
            }
        }

        public string ServerAdress
        {
            get
            {
                return m_server;
            }
            set
            {
                m_server = value;
            }
        }

        public TextEffects Effects
        {
            get { return m_effects; }
        }

        public IrcChannel GetChannel(string Channel)
        {
            return new IrcChannel( m_channelCount, Channel, this );
        }

        #endregion

        #region Connection

        /// <summary>
        /// Connects to irc server.
        /// </summary>
        public void Connect()
        {
            try
            {
                m_connection.Connect(m_server, m_port);
                m_stream = m_connection.GetStream();
                m_isConnected = true;
                if (OnConnect != null)
                    OnConnect();
            }
            catch
            {
                return;
            }
        }
        /// <summary>
        /// Connection event, connects to irc server.
        /// </summary>
        /// <param name="Server">Server address or ip.</param>
        /// <param name="Port">Server port.</param>
        public void Connect(string Server, int Port)
        {
            try
            {
                m_connection.Connect(Server, Port);
                m_stream = m_connection.GetStream();
                m_isConnected = true;
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
                m_connection.Connect(Server.Server, Server.Port);
                m_stream = m_connection.GetStream();
                m_isConnected = true;
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
                StreamWriter Writer = new StreamWriter(m_stream);
                Writer.WriteLine(Data, m_stream);
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
            if (!m_isConnected || m_stream == null)
                return;

            if (!bListen && m_listenThread.IsAlive)
                m_listenThread.Abort();

            try
            {
                m_listenThread = new Thread(KeepListen);
                m_listenThread.Start();
            }
            catch
            { }
        }

        /// <summary>
        /// Disconnects from server.
        /// </summary>
        public void Disconnect()
        {
            m_connection.Close();
            m_isConnected = false;
            m_channelCount = 0;
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

            if (!m_isConnected)
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
                    if (m_channelCount == 0)
                    {
                        string[] sData = Data.Split(new string[] { " " }, 4, StringSplitOptions.None);

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
                        string[] sData = Data.Split(new string[] { " " }, 4, StringSplitOptions.None);

                        pmsg.Sender = sData[0];
                        pmsg.Command = sData[1];
                        pmsg.Target = sData[2];
                        pmsg.Message = sData[3];
                        pmsg.WholeData = Data;

                        if (pmsg.Target.StartsWith("#")) // if it does, that is channel
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
            if (!m_isConnected)
                return null;

            try
            {
                string[] sp1 = Sender.Split(new string[] { ":", "!" }, 2, StringSplitOptions.RemoveEmptyEntries);
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

            if (!m_isConnected)
                return cd;

            string[] msg = Message.Split( new string[] { " " }, 4, StringSplitOptions.None);

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
            if (!m_isConnected)
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

            if (GetDataType(Data) != DataType.MSGTYPE_ACTION && !m_isConnected)
                return ad;

            try
            {
                string[] dt = Data.Split(new string[] { " " }, 4, StringSplitOptions.None);
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
            if (!m_isConnected)
                return DataType.NULL;

            if (Data.Contains("PRIVMSG") || Data.Contains("NOTICE"))
                if (ReadPrivmsg(Data).Command != "NOTICE" && Data.Split(' ')[2].StartsWith("#") && m_channelCount != 0) // must be a channel type
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
            if (m_isConnected == true)
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
            if (m_channelCount != 0)
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
            return m_nick;
        }
        /// <summary>
        /// Returns clients current stearm.
        /// </summary>
        /// <returns></returns>
        public NetworkStream GetStream()
        {
            if (m_stream != null)
                return m_stream;
            else
                return null;
        }
        #endregion

        #region IRC Commands
        /// <summary>
        /// Quits, disconnects from server, with leaving message.
        /// </summary>
        /// <param name="Message">Leaving message.</param>
        public void Quit(string Message)
        {
            SendData("QUIT :" + Message);
            m_connection.Close();
        }
        /// <summary>
        /// Joins channel.
        /// </summary>
        /// <param name="Channel">Channel name.</param>
        public void JoinChannel(string Channel)
        {
            if (!m_isConnected)
                return;

            SendData("JOIN " + Channel);

            if (OnChannelJoin != null)
                OnChannelJoin(Channel);

            m_channelCount++;
            GetTopic(Channel);
            GetNames(Channel);
        }
        /// <summary>
        /// Leaves channel.
        /// </summary>
        /// <param name="Channel">Channel.</param>
        public void LeaveChannel(string Channel)
        {
            if (!m_isConnected || m_channelCount == 0)
                return;

            SendData("PART " + Channel);
            m_channelCount--;
        }
        /// <summary>
        /// Requests topic channel.
        /// </summary>
        /// <param name="Channel">Channel.</param>
        public void GetTopic(string Channel)
        {
            if (!m_isConnected || m_channelCount == 0)
                return;

            SendData("TOPIC " + Channel);
        }
        /// <summary>
        /// Gets name list from channel.
        /// </summary>
        /// <param name="Channel">From channel.</param>
        public void GetNames(string Channel)
        {
            if (!m_isConnected || m_channelCount == 0)
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
            if (!m_isConnected)
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
            if (!m_isConnected)
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
            if (!m_isConnected || m_channelCount == 0)
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
            if (!m_isConnected)
                return;

            try
            {
                SendData("NICK " + Nick);
                this.m_nick = Nick;
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
            if (!m_isConnected)
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
            if (!m_isConnected || m_channelCount == 0)
                return;

            string Data = String.Format("NOTICE {0} :{1}", User, Message);
            SendData(Data);
        }
        /// <summary>
        /// Requests server's current motd, message of the day.
        /// </summary>
        public void ServerMOTD()
        {
            if (!m_isConnected)
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
            if (!m_isConnected)
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
            if (!m_isConnected)
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
            if (!m_isConnected)
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
            if (!m_isConnected)
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
            if (!m_isConnected)
                return;

            string data = String.Format("USER {0} {1} {2} {3}", UserName, HostName, ServerName, RealName);
            SendData(data);
        }
        #endregion

        #region Private Functions

        private void KeepListen()
        {
            StreamReader Reader = new StreamReader(m_connection.GetStream());
            string Data = "";
            while ((Data = Reader.ReadLine()) != "")
            {
                if (Data.Contains("PING"))
                {
                    SendData(Data.Replace("PING", "PONG"));
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

            }
        }

        #endregion
    }
}


