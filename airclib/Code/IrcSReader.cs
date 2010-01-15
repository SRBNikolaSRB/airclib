using System;
using System.Collections.Generic;
using System.Text;
using airclib.Constants;

namespace airclib
{
    /// <summary>
    /// Handles and Analyses irc protocol.
    /// </summary>
    public class IrcSReader 
    {
        private IrcClient m_client;

        public IrcSReader(IrcClient Client)
        {
            m_client = Client;
        }

        /// <summary>
        /// Reads channel or user private message. Returns PrivmsgData.
        /// </summary>
        /// <param name="Data">Data to get readed.</param>
        /// <returns>PrivmsgData.</returns>
        public PrivmsgData ReadPrivmsg(string Data)
        {
            PrivmsgData pmsg = new PrivmsgData();

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
                    if (m_client.ChannelCount == 0)
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

            string[] msg = Message.Split(new string[] { " " }, 4, StringSplitOptions.None);

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

            if (GetDataType(Data) != DataType.MSGTYPE_ACTION)
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
            if (m_client.Connected())
                return DataType.NULL;

            if (Data.Contains("PRIVMSG") || Data.Contains("NOTICE"))
                if (ReadPrivmsg(Data).Command != "NOTICE" && Data.Split(' ')[2].StartsWith("#") && m_client.ChannelCount != 0) // must be a channel type
                    return DataType.MSGTYPE_CHANNEL;
                else if (Data.Contains("ACTION "))
                    return DataType.MSGTYPE_ACTION;
                else
                    return DataType.MSGTYPE_USER;
            else
                return DataType.MSGTYPE_SERVER;
        }
    }
}
