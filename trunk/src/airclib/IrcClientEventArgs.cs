using System;

namespace airclib
{
    public class IrcDataEventArgs : EventArgs
    {
        public string Data
        {
            get; 
            set;
        }

        public string Sender
        {
            get;
            set;
        }
    }

    public class IrcChannelEventArgs : EventArgs
    {
        public string Nick
        {
            get;
            set;
        }

        public string Channel
        {
            get;
            set;
        }
    }

    public class IrcConnectionEventArgs : EventArgs
    {
        public string ServerAdress
        {
            get;
            set;
        }

        public int Port
        {
            get;
            set;
        }

        public bool Connected
        {
            get; 
            set;
        }
    }


}