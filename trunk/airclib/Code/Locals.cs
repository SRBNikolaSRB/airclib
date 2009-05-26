/* Locals.cs
   Advanced IRC Library Project (airclib)
   See LICENSE file for Copyrights
   Website "http://code.google.com/p/airclib/" */

using System;
using System.Collections.Generic;
using System.Text;
using airclib;

namespace airclib.locals
{
    public abstract class Locals
    {
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
            public PrivmsgType Type;
            public string WholeData;
        }
        public struct ActionData
        {
            public string Sender;
            public string Target;
            public string Action;
        }

        public enum PrivmsgType : int
        {
            MSGTYPE_USER = 0,
            MSGTYPE_CHANNEL = 1,
            MSGTYPE_DEFAULT = 2
        }
    }
}

