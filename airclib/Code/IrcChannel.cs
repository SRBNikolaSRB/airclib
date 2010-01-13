using System;
using System.Collections.Generic;
using System.Text;

namespace airclib
{
    public class IrcChannel
    {
        private bool m_isConnected;
        private int m_channelCount;
        private string m_channel;
        private IrcClient m_client;

        public IrcChannel(int ChannelCount, string Channel, IrcClient Client )
        {
            m_isConnected = Client.Connected();
            m_channelCount = ChannelCount;
            m_channel = Channel;
            m_client = Client;
        }

        /// <summary>
        /// Kicks wanted user from channel.
        /// </summary>
        /// <param name="Channel">Channel to kick from.</param>
        /// <param name="User">Wanted user.</param>
        public void Kick( string User)
        {
            if ( m_client != null && !m_isConnected && m_channelCount <= 0)
                return;

            m_client.SendData(String.Format("KICK {0} {1}", m_channel, User));
        }
        /// <summary>
        /// Kicks wanted user from channel. With message.
        /// </summary>
        /// <param name="Channel">Channel to kick from.</param>
        /// <param name="User">Wanted user.</param>
        /// <param name="Message">Message, reason.</param>
        public void Kick( string User, string Message)
        {
            if (m_client != null && !m_isConnected && m_channelCount <= 0)
                return;

            m_client.SendData(String.Format("KICK {0} {1} {2}", m_channel, User, Message));
        }
        /// <summary>
        /// Bans user from channel.
        /// </summary>
        /// <param name="Channel">From channel.</param>
        /// <param name="User">Wanted user.</param>
        public void Ban( string User)
        {
            if (m_client != null && !m_isConnected && m_channelCount <= 0)
                return;

            m_client.SendData(String.Format("MODE {0} +b {1}", m_channel, User));
        }
        /// <summary>
        /// Unbans user from channel.
        /// </summary>
        /// <param name="Channel">From channel.</param>
        /// <param name="User">Wanted user.</param>
        public void UnBan( string User)
        {
            if (m_client != null && !m_isConnected && m_channelCount <= 0)
                return;

            m_client.SendData(String.Format("MODE {0} -b {1}", m_channel, User));
        }
        /// <summary>
        /// Bans, than kicks wanted user in wanted channel.
        /// </summary>
        /// <param name="Channel">Channel location.</param>
        /// <param name="User">Wanted user.</param>
        public void KickBan( string User)
        {
            if (m_client != null && !m_isConnected && m_channelCount <= 0)
                return;

            m_client.SendData(String.Format("MODE {0} +b {1}", m_channel, User));
            m_client.SendData(String.Format("KICK {0} {1} {2}", m_channel, User));
        }
        /// <summary>
        /// Bans, than kicks wanted user in wanted channel. With message.
        /// </summary>
        /// <param name="Channel">Channel location.</param>
        /// <param name="User">Wanted user.</param>
        /// <param name="Message">Good by message, reason.</param>
        public void KickBan( string User, string Message)
        {
            if (m_client != null && !m_isConnected && m_channelCount <= 0)
                return;

            m_client.SendData(String.Format("MODE {0} +b {1}", m_channel, User));
            m_client.SendData(String.Format("KICK {0} {1} {2}", m_channel, User, Message));
        }
        /// <summary>
        /// Changes, sets topic to wanted channel.
        /// </summary>
        /// <param name="Channel">Wanted Channel.</param>
        /// <param name="Topic">Wanted Topic.</param>
        public void SetTopic( string Topic)
        {
            if (m_client != null && !m_isConnected && m_channelCount <= 0)
                return;

            string Data = String.Format("TOPIC {0} :{1}", m_channel, Topic);
            m_client.SendData(Data);
        }
    }
}
