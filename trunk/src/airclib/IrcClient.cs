/* IrcClient.cs
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
using System.IO;
using System.Net.Sockets;
using System.Threading;
using airclib.Base;
using airclib.Constants;
using airclib.StringReader;

namespace airclib
{
    public delegate void ConnectionChangedEventHandler(object sender, IrcConnectionEventArgs e);
    public delegate void OnDataSentEventHandler(object sender, IrcDataEventArgs e);
    public delegate void OnReciveDataEventHandler(object sender, IrcDataEventArgs e);
    public delegate void OnChannelJoinEventHandler(object sender, IrcChannelEventArgs channel);
    public delegate void OnUserJoinedChannelEventHandler(object sender, IrcChannelEventArgs e);
    public delegate void OnUserLeftChannelEventHandler(object sender, IrcChannelEventArgs e);

    /// <summary>
    /// IrcClient class, main class of airclib.
    /// </summary>
    public partial class IrcClient
    {
        private readonly TcpClient _connection = new TcpClient();
        private StreamWriter _writer;
        private StreamReader _reader;
        private NetworkStream _stream;
        private Thread _listenThread;

        private bool _isConnected;
        private string _nick;

        public event ConnectionChangedEventHandler OnConnectionChanged;
        public event OnDataSentEventHandler OnDataSent;
        public event OnReciveDataEventHandler OnReciveData;
        public event OnChannelJoinEventHandler OnChannelJoin;
        public event OnChannelJoinEventHandler OnChannelPart;
        public event OnUserJoinedChannelEventHandler OnUserJoinedChannel;
        public event OnUserLeftChannelEventHandler OnUserLeftChannel;

        /// <summary>
        /// Initializes IrcClient class.
        /// </summary>
        /// <param name="server">Server that IrcClient will connect to.</param>
        /// <param name="port">Port of server.</param>
        public IrcClient(string server, int port)
        {
            ChannelCount = 0;
            ServerAdress = server;
            ServerPort = port;
        }

        /// <summary>
        /// Initializes IrcClient class.
        /// </summary>
        /// <param name="server">Server that IrcClient will connect to.</param>
        public IrcClient(IrcServer server)
            : this(server.Server, server.Port)
        {
        }

        /// <summary>
        /// Gets, sets current connections server port.
        /// </summary>
        public int ServerPort { get; set; }

        /// <summary>
        /// Gets, sets current connections server adress.
        /// </summary>
        public string ServerAdress { get; set; }

        /// <summary>
        /// Gets current connections server count based on how much channels client is in.
        /// </summary>
        public uint ChannelCount { get; set; }

        /// <summary>
        /// Connects to irc server.
        /// </summary>
        public void Connect()
        {
            TryConnecting();

            if (_isConnected && OnConnectionChanged != null)
            {
                var args = new IrcConnectionEventArgs
                               {
                                   ServerAdress = ServerAdress,
                                   Port = ServerPort,
                                   Connected = true
                               };
                OnConnectionChanged(this, args);
            }
        }

        /// <summary>
        /// Connection event, connects to irc server.
        /// </summary>
        /// <param name="server">Server address or ip.</param>
        /// <param name="port">Server port.</param>
        public void Connect(string server, int port)
        {
            ServerAdress = server;
            ServerPort = port;
            Connect();
        }

        /// <summary>
        /// Connection event, connects to wanted "IrcServer"
        /// </summary>
        /// <param name="server">Server structure.</param>
        public void Connect(IrcServer server)
        {
            Connect(server.Server, server.Port);
        }

        /// <summary>
        /// Tries to connect
        /// </summary>
        private void TryConnecting()
        {
            try
            {
                _connection.Connect(ServerAdress, ServerPort);
                NStream = _connection.GetStream();
                _isConnected = true;
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }

        /// <summary>
        /// Sends data to connected server, must be connected to some server.
        /// </summary>
        /// <param name="data">Data to be sent.</param>
        public void SendData(string data)
        {
            _writer.WriteLine(data);
            _writer.Flush();

            if (OnDataSent != null)
            {
                var args = new IrcDataEventArgs
                    {
                        Data = data,
                        Sender = IrcSReader.ReadNick(data)
                    };
                OnDataSent(this, args);
            }
        }

        /// <summary>
        /// Reads all incoming data from socket.
        /// </summary>
        /// <param name="listen">If set true, it will start listening.</param>
        public void Listen(bool listen)
        {
            if (!listen && _listenThread.IsAlive)
            {
                _listenThread.Abort();
            }

            _listenThread = new Thread(KeepListen);
            _listenThread.Start();
        }

        /// <summary>
        /// Disconnects from server.
        /// </summary>
        public void Disconnect()
        {
            Listen(false);
            Dispose();
            _connection.Close();
            _isConnected = false;
            ChannelCount = 0;

            var args = new IrcConnectionEventArgs
                           {
                               ServerAdress = ServerAdress,
                               Port = ServerPort,
                               Connected = false
                           };
            OnConnectionChanged(this, args);
        }

        /// <summary>
        /// Disposes all active streams.
        /// </summary>
        private void Dispose()
        {
            _writer.Dispose();
            _reader.Dispose();
            _stream.Dispose();
        }

        /// <summary>
        /// Bool, returns isConnected.
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            return _isConnected;
        }

        /// <summary>
        /// Returns channel count.
        /// </summary>
        /// <returns></returns>
        public bool IsInChannel()
        {
            return ChannelCount != 0;
        }

        /// <summary>
        /// Returns connections current irc Nick.
        /// </summary>
        /// <returns>String</returns>
        public string GetNick()
        {
            return _nick;
        }

        /// <summary>
        /// Returns clients current stearm.
        /// </summary>
        /// <returns></returns>
        public NetworkStream NStream 
        {
            private set 
            { 
                if(value != null)
                {
                    _writer = new StreamWriter(value);
                    _reader = new StreamReader(value);
                }

                _stream = value; 
            }
            get { return _stream; }
        }

        /// <summary>
        /// Main listener, threaded.
        /// </summary>
        private void KeepListen()
        {
            while (NStream.DataAvailable)
            {
                var data = _reader.ReadLine();
                if (String.IsNullOrEmpty(data))
                    continue;

                if (OnReciveData != null)
                {
                    var args = new IrcDataEventArgs
                    {
                        Data = data,
                        Sender = IrcSReader.ReadSender(data)
                    };
                    OnReciveData(this, args);
                }

                if (data.StartsWith("PING"))
                {
                    SendData(data.Replace("PING", "PONG"));
                }
                else switch (IrcSReader.ReadCommand(data))
                {
                    case "JOIN":
                        {
                            var splitData = data.Split(' ');
                            splitData[0] = IrcSReader.ReadNick(splitData[0]);
                            if (OnUserJoinedChannel != null && splitData[0] != _nick)
                            {
                                var args = new IrcChannelEventArgs
                                               {
                                                   Channel = splitData[2],
                                                   Nick = splitData[0]
                                               };
                                OnUserJoinedChannel(this, args);
                            }
                            if (OnChannelJoin != null && splitData[0] == _nick)
                            {
                                ChannelCount++;
                                var args = new IrcChannelEventArgs
                                               {
                                                   Channel = splitData[2],
                                                   Nick = _nick
                                               };
                                OnChannelJoin(this, args);
                            }
                        }
                        break;
                    case "PART":
                        {
                            var splitData = data.Split(' ');
                            splitData[0] = IrcSReader.ReadNick(splitData[0]);
                            if (OnChannelPart != null && splitData[0] == _nick)
                            {
                                ChannelCount--;
                                var args = new IrcChannelEventArgs
                                {
                                    Channel = splitData[2],
                                    Nick = _nick
                                };
                                OnChannelPart(this, args);
                            }
                            if (OnUserLeftChannel != null && splitData[0] != _nick)
                            {
                                var args = new IrcChannelEventArgs
                                               {
                                                   Channel = splitData[2],
                                                   Nick = splitData[0]
                                               };
                                OnUserLeftChannel(this, args);
                            }
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Gets singe Irc channel.
        /// </summary>
        /// <param name="channel">Chanel name.</param>
        /// <returns>Irc channel.</returns>
        public Channel GetChannel(string channel)
        {
            return new Channel(channel, this);
        }
    }
}


