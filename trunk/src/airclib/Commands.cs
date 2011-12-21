using System;
using airclib.Constants;

namespace airclib
{
    public partial class IrcClient
    {

        /// <summary>
        /// Quits, disconnects from server, with leaving message.
        /// </summary>
        /// <param name="message">Leaving message.</param>
        public void Quit(string message)
        {
            SendData("QUIT :" + message);
        }

        /// <summary>
        /// Joins channel.
        /// </summary>
        /// <param name="channel">Channel name.</param>
        public void JoinChannel(string channel)
        {
            SendData("JOIN " + channel);
        }

        /// <summary>
        /// Leaves channel.
        /// </summary>
        /// <param name="channel">Channel.</param>
        public void LeaveChannel(string channel)
        {
            SendData("PART " + channel);
            ChannelCount--;
        }

        /// <summary>
        /// Requests topic channel.
        /// </summary>
        /// <param name="channel">Channel.</param>
        public void GetTopic(string channel)
        {
            SendData("TOPIC " + channel);
        }

        /// <summary>
        /// Gets name list from channel.
        /// </summary>
        /// <param name="channel">From channel.</param>
        public void GetNames(string channel)
        {
            SendData("NAMES " + channel);
        }

        /// <summary>
        /// Query, messages nick.
        /// </summary>
        /// <param name="targetNick">Sends message to this Nick/User.</param>
        /// <param name="message">Message.</param>
        /// <param name="color">Wanted message color.</param>
        public void MessageUser(string targetNick, string message, ColorMessages color = ColorMessages.Black)
        {
            var data = String.Format("PRIVMSG {0} :\u0003{2} {1}", targetNick, message, (int)color);
            SendData(data);
        }

        /// <summary>
        /// Sends message to wanted channel, connection must be connected to channel. With color.
        /// </summary>
        /// <param name="channel">Channel name, connection must be connected to this channel.</param>
        /// <param name="message">Message.</param>
        /// <param name="color">Wanted color.</param>
        public void MessageChannel(string channel, string message, ColorMessages color = ColorMessages.Black)
        {
            var data = String.Format("PRIVMSG {0} :\u0003{1} {2}", channel, (int)color, message);
            SendData(data);
        }

        /// <summary>
        /// Does wanted action.
        /// </summary>
        /// <param name="target">Target channel or user.</param>
        /// <param name="action">Action text.</param>
        public void DoAction(string target, string action)
        {
            var data = String.Format("PRIVMSG {0} :ACTION {1}", target, action);
            SendData(data);
        }

        /// <summary>
        /// Request change of current connection's nick name.
        /// </summary>
        /// <param name="nick">Wanted nick.</param>
        public void SetNick(string nick)
        {
            SendData("NICK " + nick); // TO-DO: Confirm from server...
            _nick = nick;
        }

        /// <summary>
        /// Request who is of user.
        /// </summary>
        /// <param name="nick">Users nick.</param>
        public void WhoIs(string nick)
        {
            SendData("WHOIS " + nick);
        }

        /// <summary>
        /// Sends notice message to user.
        /// </summary>
        /// <param name="user">Users nickname.</param>
        /// <param name="message">Notice.</param>
        public void Notice(string user, string message)
        {
            var data = String.Format("NOTICE {0} :{1}", user, message);
            SendData(data);
        }

        /// <summary>
        /// Requests server's current motd, message of the day.
        /// </summary>
        public void ServerMOTD()
        {
            SendData("MOTD");
        }

        /// <summary>
        /// Invites user with wanted nickname to wanted channel.
        /// </summary>
        /// <param name="nickname">Users nickname.</param>
        /// <param name="channel">Wanted channel.</param>
        public void InviteToChannel(string nickname, string channel)
        {
            var data = String.Format("INVITE {0} {1}", nickname, channel);
            SendData(data);
        }

        /// <summary>
        /// Sets away, and away message.
        /// </summary>
        /// <param name="away">Boolean, true for being away.</param>
        /// <param name="message">Message, only works if Away is true.</param>
        public void SetAway(bool away, string message = null)
        {
            SendData("AWAY " + message);
        }

        /// <summary>
        /// Sets user info. Args talk for them self.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="hostName"></param>
        /// <param name="serverName"></param>
        /// <param name="realName"></param>
        public void SetUserInfo(string userName, string hostName, string serverName, string realName)
        {
            // Command: USER
            // Parameters: <username> <hostname> <servername> <realname>
            string data = String.Format("USER {0} {1} {2} {3}", userName, hostName, serverName, realName);
            SendData(data);
        }

    }
}
