/* Locals.cs
   Advanced IRC Library Project (airclib)
   See LICENSE file for Copyrights
   Website "http://code.google.com/p/airclib/" */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using airclib;

namespace airclib
{
    public abstract class Locals
    {
        public struct IrcServer
        {
            public string Server;
            public int Port;
        }
        public struct ChannelMessageData
        {
            public string Server;
            public string Command;
            public string Channel;
            public string Message;
        }
    }
}
