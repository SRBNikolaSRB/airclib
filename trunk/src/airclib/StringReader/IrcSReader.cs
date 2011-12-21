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
    static class IrcSReader
    {
        private const char WhiteSpace = ' ';

        /// <summary>
        /// Reads channel or user private message. Returns PrivmsgData.
        /// </summary>
        /// <param name="data">Data to get readed.</param>
        /// <returns>PrivmsgData.</returns>
        public static PrivmsgData ReadPrivmsg(string data)
        {
            var msgData = new PrivmsgData();

            if (CheckType(ref data))
            {
                msgData.Type = DataType.Default;
                msgData.RawData = data;
            }
            else
            {
                ConstructData(ref msgData,data);
            }

            return msgData;
        }

        /// <summary>
        /// Constructs private message data with provided string.
        /// </summary>
        private static void ConstructData(ref PrivmsgData pmsg, string data)
        {
            var sData = SplitBy(ref data, 4, WhiteSpace); 
            
            pmsg.Sender = sData[0];
            pmsg.Command = sData[1];
            pmsg.Target = sData[2];
            pmsg.Message = sData[3];
            pmsg.RawData = data;

            pmsg.Type = pmsg.Target.StartsWith("#") ? DataType.Channel : DataType.User;
        }

        /// <summary>
        /// Parses sender data.
        /// </summary>
        /// <param name="data">Sender ID, i recommend using ReadPrivmsg's parsed sender.</param>
        /// <returns>Sender string.</returns>
        public static string ReadSender(string data) 
        {
            return ReadBy(ref data, 1, 0, WhiteSpace);
        }
        /// <summary>
        /// Parses nickname of sender.
        /// </summary>
        /// <param name="sender">Sender ID, i recommend using ReadPrivmsg's parsed sender.</param>
        /// <returns>Nickname.</returns>
        public static string ReadNick(string sender)
        {
            return ReadBy(ref sender, 2, 0, ':', '!');
        }

        /// <summary>
        /// Parses nickname of sender from data.
        /// </summary>
        /// <param name="data">Data received from irc server.</param>
        /// <returns>Nickname.</returns>
        public static string ReadNickFromData(string data)
        {
            return ReadNick(ReadSender(data));
        }

        /// <summary>
        /// Removes ":" from wanted message.
        /// </summary>
        /// <param name="message">Wanted message.</param>
        /// <returns>String.</returns>
        public static string ReadOnlyMessage(string message)
        {
            return message.Substring(message.IndexOf(':') + 1);
        }

        /// <summary>
        /// Reads IRC command from given IRC data.
        /// </summary>
        /// <param name="data">Data to read from.</param>
        /// <returns>Command string.</returns>
        public static string ReadCommand(string data)
        {
            return ReadBy(ref data, 3, 1, WhiteSpace);
        }

        /// <summary>
        /// Reads raw message from given IRC data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ReadMessage(string data)
        {
            return ReadOnlyMessage(ReadBy(ref data, 4, 3, WhiteSpace));
        }

        /// <summary>
        /// Reads data with server type.
        /// </summary>
        /// <param name="data">Actual data.</param>
        /// <returns>Returns server data structure.</returns>
        public static ServerData ReadServerData(string data)
        {
            var serverData = new ServerData();
            var splitData = SplitBy(ref data, 4, WhiteSpace);

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
        public static ActionData ParseAction(string data)
        {
            var actionData = new ActionData();
            var splitData = SplitBy(ref data, 4, WhiteSpace);

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
        public static DataType GetDataType(string data)
        {
            var dtype = DataType.Default;
            if (CheckType(ref data))
            {
                dtype = DataType.Server;
            }
            else
            {
                if (ReadPrivmsg(data).Command != "NOTICE" && ReadBy(ref data, 4, 2, WhiteSpace).StartsWith("#")) // must be a channel type 
                {
                    dtype = DataType.Channel;
                }
                else if (data.Contains("ACTION "))
                {
                    dtype = DataType.Action;
                }
                else
                {
                    dtype = DataType.User;
                }
            }

            return dtype;
        }


        private static string[] SplitBy(ref string data, int count, params char[] p)
        {
            return data.Split(p, count, StringSplitOptions.None);
        }

        private static string ReadBy(ref string data, int count, int place, params char[] p)
        {
            return data.Split(p, count, StringSplitOptions.RemoveEmptyEntries)[place];
        }

        /// <summary>
        /// Cheks if data is private message or notice.
        /// </summary>
        private static bool CheckType(ref string data)
        {
            var command = ReadCommand(data);
            return command != "PRIVMSG" || command != "NOTICE";
        }
    }
}
