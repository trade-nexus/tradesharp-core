/***************************************************************************** 
* Copyright 2016 Aurora Solutions 
* 
*    http://www.aurorasolutions.io 
* 
* Aurora Solutions is an innovative services and product company at 
* the forefront of the software industry, with processes and practices 
* involving Domain Driven Design(DDD), Agile methodologies to build 
* scalable, secure, reliable and high performance products.
* 
* TradeSharp is a C# based data feed and broker neutral Algorithmic 
* Trading Platform that lets trading firms or individuals automate 
* any rules based trading strategies in stocks, forex and ETFs. 
* TradeSharp allows users to connect to providers like Tradier Brokerage, 
* IQFeed, FXCM, Blackwood, Forexware, Integral, HotSpot, Currenex, 
* Interactive Brokers and more. 
* Key features: Place and Manage Orders, Risk Management, 
* Generate Customized Reports etc 
* 
* Licensed under the Apache License, Version 2.0 (the "License"); 
* you may not use this file except in compliance with the License. 
* You may obtain a copy of the License at 
* 
*    http://www.apache.org/licenses/LICENSE-2.0 
* 
* Unless required by applicable law or agreed to in writing, software 
* distributed under the License is distributed on an "AS IS" BASIS, 
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
* See the License for the specific language governing permissions and 
* limitations under the License. 
*****************************************************************************/


﻿using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;
using TraceSourceLogger;
using Timer = System.Timers.Timer;

namespace TradeHub.MarketDataProvider.Redi.Utility
{
    public class SocketCommunication
    {
        private Type _type = typeof (SocketCommunication);

        private readonly AsyncClassLogger _asyncClassLogger;

        public delegate void EventHandler(String message);

        public event EventHandler SendMessage;

        public delegate void ConnectionError(bool Connect);
        public event ConnectionError ErrorInTcp;
        private readonly Queue _queue;
        private readonly string _user;
        private readonly string _password;

        private int _apiPort; //8001; //
        private string _apiHost; //"127.0.0.1"; //
        private TcpClient _client = null;
        private NetworkStream _networkStream = null;
        private StreamWriter _streamWriter = null;
        private Thread _readerThread = null;

        private Timer _heartbeatTimer = new Timer();
        private Timer _reconnectTimer = new Timer();

        /// <summary>
        /// temp variable for Logout 
        /// </summary>
        private bool _logoutArrived = false;

        public int TestServerDataCount = 0;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="user"> </param>
        /// <param name="password"> </param>
        /// <param name="ipAddress"> </param>
        /// <param name="port"> </param>
        /// <param name="asyncClassLogger"></param>
        public SocketCommunication(Queue queue, string user, string password, string ipAddress, int port, AsyncClassLogger asyncClassLogger)
        {
            _queue = queue;
            _user = user;
            _password = password;
            _apiPort = port;
            _asyncClassLogger = asyncClassLogger;
            _apiHost = ipAddress;
            _client = null;
            _heartbeatTimer.Interval = 1000;
            _reconnectTimer.Interval = 900000;
        }

        /// <summary>
        /// Gets or Sets the ApiPort
        /// </summary>
        public int ApiPort
        {
            get { return _apiPort; }
            set { _apiPort = value; }
        }

        /// <summary>
        /// Gets or Sets the ApiHost
        /// </summary>
        public string ApiHost
        {
            get { return _apiHost; }
            set { _apiHost = value; }
        }

        /// <summary>
        /// Lock covering stopping and stopped
        /// </summary>
        private readonly object _stopLock = new object();

        /// <summary>
        /// Whether or not the worker thread has been asked to stop
        /// </summary>
        private bool _stopping = false;

        /// <summary>
        /// Whether or not the worker thread has stopped
        /// </summary>
        private bool _stopped = false;

        /// <summary>
        /// Returns whether the worker thread has been asked to stop.
        /// This continues to return true even after the thread has stopped.
        /// </summary>
        public bool Stopping
        {
            get
            {
                lock (_stopLock)
                {
                    return _stopping;
                }
            }
        }

        /// <summary>
        /// Returns whether the worker thread has stopped.
        /// </summary>
        public bool Stopped
        {
            get
            {
                lock (_stopLock)
                {
                    return _stopped;
                }
            }
        }

        /// <summary>
        /// Tells the worker thread to stop, typically after completing its 
        /// current work item. (The thread is *not* guaranteed to have stopped
        /// by the time method returns.)
        /// </summary>
        public void Stop()
        {
            lock (_stopLock)
            {
                _stopping = true;
            }
        }

        /// <summary>
        /// Connects to Server.
        /// </summary>
        public void Connect()
        {
            _stopping = false;
            if (_client != null)
            {
                throw new Exception("Connection already open!");
            }
            try
            {
                if (InitializeTcpClient())
                {
                    var loginMessage = LogonMessage();
                    StartReaderThread();
                    _networkStream.Write(loginMessage, 0, loginMessage.Length);

                    _streamWriter.Flush();
                }

            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "Connect");
            }
        }

        /// <summary>
        /// Initialize TCp Client
        /// </summary>
        private bool InitializeTcpClient()
        {
            try
            {
                _client = new TcpClient();
                _client.Connect(_apiHost, _apiPort);
                _networkStream = _client.GetStream();
                _streamWriter = new StreamWriter(_networkStream);
                return true;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "InitializeTcpClient");
                return false;
            }
        }

        /// <summary>
        /// Start Reader Thread to read data on TCP port
        /// </summary>
        private void StartReaderThread()
        {
            try
            {
                _readerThread = new Thread(ReaderThreadStarter);
                _readerThread.Start();
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "StartReaderThread");
            }
        }

        /// <summary>
        /// End communication with the Server.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (_client != null)
                {
                    _stopped = true;
                    //_readerThread.Abort();
                    _heartbeatTimer.Stop();
                    byte[] message = Encoding.UTF8.GetBytes("<0x04><0x20><0x00><0x00><0x00><0x00><0x03>");
                    _networkStream.Write(message, 0, message.Length);
                    Stop();
                    _client.Close();
                    _client = null;
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "Disconnect");
            }
        }

        /// <summary>
        /// Starts a reader thread
        /// </summary>
        private void ReaderThreadStarter()
        { 
           // var streamReader = new StreamReader(_networkStream);
            var builder = new StringBuilder();
            while (!_stopping)
            {
                try
                {
                    int i = _networkStream.ReadByte();
                    builder.Append((char)i);
                    if (i==3)
                    {
                        if (SendMessage != null)
                            SendMessage.Invoke(builder.ToString());
                        builder.Length = 0;
                    }
                    _networkStream.Flush();
/*                    var size = new byte[250];
                    var filesize = _networkStream.Read(size, 0, 100);
                    var data = new byte[size[0]];
                    string newMessage = Encoding.ASCII.GetString(size);
                    if (SendMessage != null)
                        SendMessage.Invoke(newMessage);
                    _networkStream.Flush();*/
                }
                catch (Exception exception)
                {
                    _asyncClassLogger.Error(exception, _type.FullName, "ReaderThreadStarter");
                    if (ErrorInTcp != null)
                        ErrorInTcp.Invoke(false);
                    Stop();
                }
            }
        }

        /// <summary>
        /// Subscribe Market Data for requested Currency Pair
        /// </summary>
        /// <param name="symbol"> </param>
        public void SubscribeMarketData(string symbol)
        {
            try
            {
                if(_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Subscribe Market Data for" + symbol, _type.FullName, "SubscribeMarketData");
                }
                var json =new WebClient().DownloadString("http://autoc.finance.yahoo.com/autoc?query="+symbol+"&callback=YAHOO.Finance.SymbolSuggest.ssCallback");
                var type = GetTypeDisplay(json);
                var exchange = GetExchange(json);
                string exchangeNumber = null;
                if (type == "Equity")
                {
                    if (exchange == "NYSE")
                    {
                        exchangeNumber = "558";
                    }
                    else if (exchange == "NASDAQ")
                    {
                        exchangeNumber = "564";
                    }
                }
                else if (type == "ETF")
                {
                    exchangeNumber = "545";
                }
                else
                {
                    return;
                }
                byte[] messageSnapshot =
                    Encoding.UTF8.GetBytes("5022=Subscribe|4=" + exchangeNumber + "|5=" + symbol + "|5026=2");
                var messageStart = new byte[] {0x04, 0x20, 0x00, 0x00, 0x00, (byte) messageSnapshot.Length};
                var messageEnd = new byte[] {0x03};
                var dataMessage = new byte[messageSnapshot.Length + 7];

                for (int i = 0; i < messageStart.Length; i++)
                {
                    dataMessage[i] = messageStart[i];
                }

                for (int i = messageStart.Length; i < messageSnapshot.Length + messageStart.Length; i++)
                {
                    dataMessage[i] = messageSnapshot[i - messageStart.Length];
                }
                dataMessage[dataMessage.Length - 1] = messageEnd[0];
                _networkStream.Write(dataMessage, 0, dataMessage.Length);
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SubscribeMarketData");
            }
        }
        /// <summary>
        /// Returns Exchange Type
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private string GetExchange(string json)
        {
            try
            {
                json = json.Remove(0, 39).Trim();
                json = json.Remove(json.Length - 1);
                JObject o = JObject.Parse(json);
                var exchange = o["ResultSet"]["Result"][0]["exchDisp"];
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Symbol Exchange" + exchange, _type.FullName, "GetExchange");
                }
                return exchange.ToString().Trim();
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "GetExchange");
                return null;
            }
        }
        private string GetTypeDisplay(string json)
        {
            try
            {
                json = json.Remove(0, 39).Trim();
                json = json.Remove(json.Length - 1);
                JObject o = JObject.Parse(json);
                var type = o["ResultSet"]["Result"][0]["typeDisp"];
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Symbol Type" + type, _type.FullName, "GetExchange");
                }
                return type.ToString().Trim();
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "GetExchange");
                return null;
            }
        }
        /// <summary>
        /// UnSubscribe Market Data.
        /// </summary>
        /// <param name="symbol"></param>
        public void UnsubscribeMarketData(string symbol)
        {
            try
            {
                var json =
                new WebClient().DownloadString("http://autoc.finance.yahoo.com/autoc?query="+symbol+"&callback=YAHOO.Finance.SymbolSuggest.ssCallback");
                var type = GetTypeDisplay(json);
                var exchange = GetExchange(json);
                string exchangeNumber = null;
                if (type == "Equity")
                {
                    if (exchange == "NYSE")
                    {
                        exchangeNumber = "558";
                    }
                    else if (exchange == "NASDAQ")
                    {
                        exchangeNumber = "564";
                    }
                }
                else if (type == "ETF")
                {
                    exchangeNumber = "545";
                }
                byte[] messageSnapshot =
                    Encoding.UTF8.GetBytes("5022=Unsubscribe|4=" + exchangeNumber + "|5=" + symbol + "|5026=2");
                var messageStart = new byte[] {0x04, 0x20, 0x00, 0x00, 0x00, (byte) messageSnapshot.Length};
                var messageEnd = new byte[] {0x03};
                var dataMessage = new byte[messageSnapshot.Length + 7];
                for (int i = 0; i < messageStart.Length; i++)
                {
                    dataMessage[i] = messageStart[i];
                }
                for (int i = messageStart.Length; i < messageSnapshot.Length + messageStart.Length; i++)
                {
                    dataMessage[i] = messageSnapshot[i - messageStart.Length];
                }
                dataMessage[dataMessage.Length - 1] = messageEnd[0];
                _networkStream.Write(dataMessage, 0, dataMessage.Length);
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "UnSubscribeMarketData");
            }
        }

        /// <summary>
        /// Login Message
        /// </summary>
        /// <returns></returns>
        public byte[] LogonMessage()
        {
            try
            {
                var loginString =
                    Encoding.ASCII.GetBytes("5022=LoginUser|5026=1|5028=" + _user + "|5029=" + _password + "");
                var messageStart = new byte[] {0x04, 0x20, 0x00, 0x00, 0x00, (byte) loginString.Length};
                var messageEnd = new byte[] {0x03};
                var loginMessage = new byte[56];
                for (int i = 0; i < messageStart.Length; i++)
                {
                    loginMessage[i] = messageStart[i];
                }
                for (int i = messageStart.Length; i < loginString.Length + messageStart.Length; i++)
                {
                    loginMessage[i] = loginString[i - messageStart.Length];
                }
                loginMessage[55] = messageEnd[0];
                return loginMessage;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "LogonMessage");
                return null;
            }
        }
    }
}
