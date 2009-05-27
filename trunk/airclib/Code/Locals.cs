/* Locals.cs
 * Advanced IRC Library Project (airclib)
 * See LICENSE file for Copyright
 * Website "http://code.google.com/p/airclib/" 
 */

using System;
using System.Collections.Generic;
using System.Text;
using airclib;

namespace airclib.locals
{
    public abstract class Locals
    {
        #region Structures
        public struct IrcServer
        {
            public string Server;
            public int Port;
        }
        public struct PrivmsgData
        {
            public string Sender;
            public string Command;
            public string Target;
            public string Message;
            public DataType Type;
            public string WholeData;
        }
        public struct ServerData
        {
            public string Sender;
            public string Command;
            public string Target;
            public string Message;
        }
        public struct ActionData
        {
            public string Sender;
            public string Target;
            public string Action;
        }
        #endregion

        #region Enums
        public enum DataType : int
        {
            MSGTYPE_USER = 0,
            MSGTYPE_CHANNEL = 1,
            MSGTYPE_DEFAULT = 2,
            MSGTYPE_SERVER = 3,
            MSGTYPE_ACTION = 4,
            NULL = 5
        }
        public enum ColorMessages : int
        {
            Black = 01,
            Navy_Blue = 02,
            Green = 03,
            Red = 04,
            Brown = 05,
            Purple = 06,
            Olive = 07,
            Yellow = 08,
            Lime_Green = 09,
            Teal = 10,
            Aqua_Light = 11,
            Royal_Blue = 12,
            Hot_Pink = 13,
            Dark_Gray = 14,
            Light_Gray = 15,
            White = 16
        }
        #endregion

        #region Text Effects
        public const char BoldFont = '\u0002';
        public const char ColoredFont = '\u0003';
        public const char UnderlineFont = '\u001F';
        #endregion
    }
}

