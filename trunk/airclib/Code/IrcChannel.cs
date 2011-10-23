using System;
using System.Collections.Generic;
using System.Text;

namespace airclib.Base
{
    public class IrcChannel
    {
        private string m_channel;
        private IrcClient m_client;

        public IrcChannel(string Channel, IrcClient Client ) 
		{
            m_channel = Channel;
            m_client = Client;
        }

        /// <summary>
        /// Kicks wanted user from channel.
        /// </summary>
        /// <param name="Channel">Channel to kick from.</param>
        /// <param name="User">Wanted user.</param>
        public void Kick(string User) 
        {
            m_client.SendData(String.Format("KICK {0} {1}", m_channel, User));
        }
        /// <summary>
        /// Kicks wanted user from channel. With message.
        /// </summary>
        /// <param name="Channel">Channel to kick from.</param>
        /// <param name="User">Wanted user.</param>
        /// <param name="Message">Message, reason.</param>
        public void Kick(string User, string Message) 
        {
            m_client.SendData(String.Format("KICK {0} {1} {2}", m_channel, User, Message));
        }
        /// <summary>
        /// Bans user from channel.
        /// </summary>
        /// <param name="Channel">From channel.</param>
        /// <param name="User">Wanted user.</param>
        public void Ban(string User) 
        {
            m_client.SendData(String.Format("MODE {0} +b {1}", m_channel, User));
        }
        /// <summary>
        /// Unbans user from channel.
        /// </summary>
        /// <param name="Channel">From channel.</param>
        /// <param name="User">Wanted user.</param>
        public void UnBan(string User) 
        {
            m_client.SendData(String.Format("MODE {0} -b {1}", m_channel, User));
        }
        /// <summary>
        /// Bans, than kicks wanted user in wanted channel.
        /// </summary>
        /// <param name="Channel">Channel location.</param>
        /// <param name="User">Wanted user.</param>
        public void KickBan(string User) 
        {
            m_client.SendData(String.Format("MODE {0} +b {1}", m_channel, User));
            m_client.SendData(String.Format("KICK {0} {1} {2}", m_channel, User));
        }
        /// <summary>
        /// Bans, than kicks wanted user in wanted channel. With message.
        /// </summary>
        /// <param name="Channel">Channel location.</param>
        /// <param name="User">Wanted user.</param>
        /// <param name="Message">Good by message, reason.</param>
        public void KickBan(string User, string Message) 
        {
            m_client.SendData(String.Format("MODE {0} +b {1}", m_channel, User));
            m_client.SendData(String.Format("KICK {0} {1} {2}", m_channel, User, Message));
        }
        /// <summary>
        /// Changes, sets topic to wanted channel.
        /// </summary>
        /// <param name="Channel">Wanted Channel.</param>
        /// <param name="Topic">Wanted Topic.</param>
        public void SetTopic(string Topic) 
        {
            m_client.SendData(String.Format("TOPIC {0} :{1}", m_channel, Topic));
        }
    }
}
