/* Channel.cs
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

namespace airclib.Base
{
    public class Channel
    {
        private readonly string _channel;
        private readonly IrcClient _client;

        public Channel(string channel, IrcClient client ) 
		{
            _channel = channel;
            _client = client;
        }

        /// <summary>
        /// Kicks wanted user from channel.
        /// </summary>
        /// <param name="user">Wanted user.</param>
        public void Kick(string user) 
        {
            _client.SendData(String.Format("KICK {0} {1}", _channel, user));
        }
        /// <summary>
        /// Kicks wanted user from channel. With message.
        /// </summary>
        /// <param name="Channel">Channel to kick from.</param>
        /// <param name="user">Wanted user.</param>
        /// <param name="message">Message, reason.</param>
        public void Kick(string user, string message) 
        {
            _client.SendData(String.Format("KICK {0} {1} {2}", _channel, user, message));
        }
        /// <summary>
        /// Bans user from channel.
        /// </summary>
        /// <param name="Channel">From channel.</param>
        /// <param name="user">Wanted user.</param>
        public void Ban(string user) 
        {
            _client.SendData(String.Format("MODE {0} +b {1}", _channel, user));
        }
        /// <summary>
        /// Unbans user from channel.
        /// </summary>
        /// <param name="Channel">From channel.</param>
        /// <param name="user">Wanted user.</param>
        public void UnBan(string user) 
        {
            _client.SendData(String.Format("MODE {0} -b {1}", _channel, user));
        }
        /// <summary>
        /// Bans, than kicks wanted user in wanted channel.
        /// </summary>
        /// <param name="user">Wanted user.</param>
        public void KickBan(string user) 
        {
            _client.SendData(String.Format("MODE {0} +b {1}", _channel, user));
            _client.SendData(String.Format("KICK {0} {1} {2}", _channel, user));
        }
        /// <summary>
        /// Bans, than kicks wanted user in wanted channel. With message.
        /// </summary>
        /// <param name="Channel">Channel location.</param>
        /// <param name="user">Wanted user.</param>
        /// <param name="message">Good by message, reason.</param>
        public void KickBan(string user, string message) 
        {
            _client.SendData(String.Format("MODE {0} +b {1}", _channel, user));
            _client.SendData(String.Format("KICK {0} {1} {2}", _channel, user, message));
        }
        /// <summary>
        /// Changes, sets topic to wanted channel.
        /// </summary>
        /// <param name="Channel">Wanted Channel.</param>
        /// <param name="topic">Wanted Topic.</param>
        public void SetTopic(string topic) 
        {
            _client.SendData(String.Format("TOPIC {0} :{1}", _channel, topic));
        }
    }
}
