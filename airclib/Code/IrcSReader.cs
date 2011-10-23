using System;
using System.Collections.Generic;
using System.Text;
using airclib.Constants;

namespace airclib.StringReader
{
    /// <summary>
    /// Handles and Analyses irc protocol.
    /// </summary>
    public class IrcSReader
    {
        /// <summary>
        /// Reads channel or user private message. Returns PrivmsgData.
        /// </summary>
        /// <param name="Data">Data to get readed.</param>
        /// <returns>PrivmsgData.</returns>
        public PrivmsgData ReadPrivmsg(string Data)
        {
            PrivmsgData msgData = new PrivmsgData();

            if (!Data.Contains("PRIVMSG") && !Data.Contains("NOTICE"))
            {
                msgData.Type = DataType.MSGTYPE_DEFAULT;
                msgData.StringData = Data;
            }
            else
            {
                string[] sData = Data.Split(new char[] { ' ' }, 4, StringSplitOptions.None);

                msgData.Sender = sData[0];
                msgData.Command = sData[1];
                msgData.Target = sData[2];
                msgData.Message = sData[3];
                msgData.StringData = Data;

                if (msgData.Target.StartsWith("#")) // if it does, that is channel
                    msgData.Type = DataType.MSGTYPE_CHANNEL; // Than message is channel type
                else
                    msgData.Type = DataType.MSGTYPE_USER;
            }

            return msgData;
        }
        /// <summary>
        /// Parses nickname of sender.
        /// </summary>
        /// <param name="Sender">Sender ID, i recommend using ReadPrivmsg's parsed sender.</param>
        /// <returns>Nickname.</returns>
        public string ReadNick(string Sender)
        {
            return Sender.Split(new char[] { ':', '!' }, 2, StringSplitOptions.RemoveEmptyEntries)[0];
        }
        /// <summary>
        /// Removes ":" from wanted message.
        /// </summary>
        /// <param name="Message">Wanted message.</param>
        /// <returns>String.</returns>
        public string ReadOnlyMessage(string Message)
        {
            return Message.Substring(Message.IndexOf(":") + 1);
        }
        /// <summary>
        /// Reads data with server type.
        /// </summary>
        /// <param name="Message">Actual data.</param>
        /// <returns>Returns server data structure.</returns>
        public ServerData ReadServerData(string Message)
        {
            ServerData serverData = new ServerData();

            string[] message = Message.Split(new char[] { ' ' }, 4, StringSplitOptions.None);

            serverData.Sender = message[0];
            serverData.Command = message[1];
            serverData.Target = message[2];
            serverData.Message = ReadOnlyMessage(message[3]);

            return serverData;
        }
        /// <summary>
        /// Reads action, with normal formular, action sender and action it self. Data should be action type.
        /// </summary>
        /// <param name="Data">Data, reccomended action type.</param>
        /// <returns>ActionData structure.</returns>
        public ActionData ParseAction(string Data)
        {
            ActionData actionData = new ActionData();

            string[] splitData = Data.Split(new string[] { " " }, 4, StringSplitOptions.None);

            actionData.Sender = ReadNick(splitData[0]);
            actionData.Target = splitData[2];
            actionData.Action = splitData[3].Replace(":ACTION ", "");

            return actionData;
        }
        /// <summary>
        /// Reads irc data, returning its data type.
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public DataType GetDataType(string Data)
        {
            if (Data.Contains("PRIVMSG") || Data.Contains("NOTICE"))
            {
                if (ReadPrivmsg(Data).Command != "NOTICE" && Data.Split(' ')[2].StartsWith("#")) // must be a channel type
                    return DataType.MSGTYPE_CHANNEL;
                else if (Data.Contains("ACTION "))
                    return DataType.MSGTYPE_ACTION;
                else
                    return DataType.MSGTYPE_USER;
            }
            else
                return DataType.MSGTYPE_SERVER;
        }
    }
}
