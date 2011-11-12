/* IrcSReader.cs
 * 
 * Advanced IRC Library Project
 * Copyright (C) 2011 Nikola Miljkovic <http://code.google.com/p/airclib/>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
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
        /// <param name="data">Data to get readed.</param>
        /// <returns>PrivmsgData.</returns>
        public PrivmsgData ReadPrivmsg(string data)
        {
            var msgData = new PrivmsgData();

            if (!data.Contains("PRIVMSG") && !data.Contains("NOTICE"))
            {
                msgData.Type = DataType.Default;
                msgData.StringData = data;
            }
            else
            {
                var sData = data.Split(new char[] { ' ' }, 4, StringSplitOptions.None);

                msgData.Sender = sData[0];
                msgData.Command = sData[1];
                msgData.Target = sData[2];
                msgData.Message = sData[3];
                msgData.StringData = data;

                msgData.Type = msgData.Target.StartsWith("#") ? DataType.Channel : DataType.User;
            }

            return msgData;
        }
        /// <summary>
        /// Parses nickname of sender.
        /// </summary>
        /// <param name="sender">Sender ID, i recommend using ReadPrivmsg's parsed sender.</param>
        /// <returns>Nickname.</returns>
        public string ReadNick(string sender)
        {
            return sender.Split(new char[] { ':', '!' }, 2, StringSplitOptions.RemoveEmptyEntries)[0];
        }
        /// <summary>
        /// Removes ":" from wanted message.
        /// </summary>
        /// <param name="message">Wanted message.</param>
        /// <returns>String.</returns>
        public string ReadOnlyMessage(string message)
        {
            return message.Substring(message.IndexOf(":") + 1);
        }
        /// <summary>
        /// Reads data with server type.
        /// </summary>
        /// <param name="data">Actual data.</param>
        /// <returns>Returns server data structure.</returns>
        public ServerData ReadServerData(string data)
        {
            var serverData = new ServerData();
            var splitData = data.Split(new char[] { ' ' }, 4, StringSplitOptions.None);

            serverData.Sender = splitData[0];
            serverData.Command = splitData[1];
            serverData.Target = splitData[2];
            serverData.Message = ReadOnlyMessage(splitData[3]);

            return serverData;
        }
        /// <summary>
        /// Reads action, with normal formular, action sender and action it self. Data should be action type.
        /// </summary>
        /// <param name="data">Data, reccomended action type.</param>
        /// <returns>ActionData structure.</returns>
        public ActionData ParseAction(string data)
        {
            var actionData = new ActionData();

            var splitData = data.Split(new string[] { " " }, 4, StringSplitOptions.None);

            actionData.Sender = ReadNick(splitData[0]);
            actionData.Target = splitData[2];
            actionData.Action = splitData[3].Replace(":ACTION ", "");

            return actionData;
        }
        /// <summary>
        /// Reads irc data, returning its data type.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public DataType GetDataType(string data)
        {
            if (!data.Contains("PRIVMSG") && !data.Contains("NOTICE"))
                return DataType.Server;
            else
            {
                if (ReadPrivmsg(data).Command != "NOTICE" && data.Split(' ')[2].StartsWith("#")) // must be a channel type 
                    return DataType.Channel;
                else if (data.Contains("ACTION "))
                    return DataType.Action;
                else
                    return DataType.User;
            }
        }
    }
}
