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
using Spring.Context.Support;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.MarketDataProvider;
using TradeHub.Common.Core.ValueObjects;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.Inquiry;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataEngine.Client.Utility;
using TradeConstants = TradeHub.Common.Core.Constants;

namespace TradeHub.MarketDataEngine.Client.Service
{
    /// <summary>
    /// Provides Connectivity with the Market Data Engine 
    /// </summary>
    public class MarketDataEngineClient
    {
        private Type _type = typeof (MarketDataEngineClient);
        private AsyncClassLogger _asyncClassLogger;

        #region Events

        // ReSharper disable InconsistentNaming
        private event Action<string> _logonArrived;
        private event Action<string> _logoutArrived;
        private event Action<MarketDataProviderInfo> _inquiryResponseArrived;
        private event Action<Tick> _tickArrived;
        private event Action<HistoricBarData> _historicBarsArrived;
        //private event Action<RabbitMqMessage> _marketDataArrived;
        private event Action<byte[]> _marketDataArrived;
        private event Action<Bar> _liveBarArrived;
        private event Action _serverDisconnected;
        private event Action _serverConnected; 
        // ReSharper restore InconsistentNaming

        public event Action<string> LogonArrived
        {
            add
            {
                if (_logonArrived == null)
                {
                    _logonArrived += value;
                }
            }
            remove { _logonArrived -= value; }
        }

        public event Action<string> LogoutArrived
        {
            add
            {
                if (_logoutArrived == null)
                {
                    _logoutArrived += value;
                }
            }
            remove { _logoutArrived -= value; }
        }

        public event Action<Tick> TickArrived
        {
            add
            {
                if (_tickArrived == null)
                {
                    _tickArrived += value;
                }
            }
            remove { _tickArrived -= value; }
        }

        public event Action<HistoricBarData> HistoricBarsArrived
        {
            add
            {
                if (_historicBarsArrived == null)
                {
                    _historicBarsArrived += value;
                }
            }
            remove { _historicBarsArrived -= value; }
        }

        public event Action<byte[]> MarketDataArrived
        {
            add
            {
                if (_marketDataArrived == null)
                {
                    _marketDataArrived += value;
                }
            }
            remove { _marketDataArrived -= value; }
        }

        public event Action<Bar> LiveBarArrived
        {
            add
            {
                if (_liveBarArrived == null)
                {
                    _liveBarArrived += value;
                }
            }
            remove { _liveBarArrived -= value; }
        }

        public event Action<MarketDataProviderInfo> InquiryResponseArrived
        {
            add
            {
                if (_inquiryResponseArrived == null)
                {
                    _inquiryResponseArrived += value;
                }
            }
            remove { _inquiryResponseArrived -= value; }
        }

        public event Action ServerDisconnected
        {
            add
            {
                if (_serverDisconnected == null)
                {
                    _serverDisconnected += value;
                }
            }
            remove { _serverDisconnected -= value; }
        }

        public event Action ServerConnected
        {
            add
            {
                if (_serverConnected==null)
                {
                    _serverConnected += value;
                }
            }
            remove { _serverConnected -= value; }
        }

        #endregion

        // Application ID to uniquely identify the running instance
        private string _appId;

        /// <summary>
        /// Holds reference to the MQ Server for Rabbit MQ Communication
        /// </summary>
        private ClientMqServer _mqServer;

        /// <summary>
        /// Reads and Holds Exchange/Queue parameters to be used by Client
        /// </summary>
        private ConfigurationReader _configurationReader;

        /// <summary>
        /// Returns Unique Application ID
        /// </summary>
        public string AppId
        {
            get { return _appId; }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public MarketDataEngineClient() : this("MDEServer.xml", "ClientMqConfig.xml")
        {
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="server">MDE Server Config file</param>
        /// <param name="client">MDE Client MQ Config File</param>
        public MarketDataEngineClient(string server ="MDEServer.xml", string client="ClientMqConfig.xml")
        {
            _asyncClassLogger = new AsyncClassLogger("MarketDataClient");
            if (_asyncClassLogger != null)
            {
                _asyncClassLogger.SetLoggingLevel();
                //set logging path
                _asyncClassLogger.LogDirectory(TradeConstants.DirectoryStructure.CLIENT_LOGS_LOCATION);
            }

            // Get Configuration details
            _configurationReader = new ConfigurationReader(server, client);
            _configurationReader.ReadParameters();

            // Get MQ Server object
            _mqServer = new ClientMqServer(_configurationReader.MdeMqServerparameters, _configurationReader.ClientMqParameters, _asyncClassLogger);
        }

        /// <summary>
        /// Initializes necessary components after client is disconnected
        /// </summary>
        public void Initialize()
        {
            // Reset configuration parameters
            _configurationReader.ReadParameters();

            // Initialize MQ-Server
            _mqServer.Initialize();
        }

        /// <summary>
        /// Starts communication with server
        /// </summary>
        public void Start()
        {
            try
            {
                // Register Events
                RegisterClientMqServerEvents();
                
                // Request for Unique App ID
                RequestAppId();
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "Intialize");
            }
        }

        /// <summary>
        /// Closes necessary connections/events
        /// </summary>
        public void Shutdown()
        {
            try
            {
                if (_mqServer != null)
                {
                    // Notify MDE about application close
                    SendDisconnectRequest();

                    // Disconnect MQ Server
                    _mqServer.Disconnect();

                    // Unhook events
                    UnregisterClientMqServerEvents();

                    // Notify Listeners
                    if (_serverDisconnected != null)
                    {
                        _serverDisconnected();
                    }
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "Shutdown");
            }    
        }

        /// <summary>
        /// Hooks Client MQServer events
        /// </summary>
        private void RegisterClientMqServerEvents()
        {
            UnregisterClientMqServerEvents();
            _mqServer.ServerDisconnected += OnServerDisconnected;
            _mqServer.LogonArrived += OnLogonArrived;
            _mqServer.LogoutArrived += OnLogoutArrived;
            _mqServer.TickArrived += OnTickArrived;
            _mqServer.LiveBarArrived += OnLiveBarArrived;
            _mqServer.HistoricBarsArrived += OnHistoricalBarDataArrived;
            _mqServer.MarketDataArrived += OnMarketDataArrived;
            _mqServer.InquiryResponseArrived += OnInquiryResponseArrived;
        }

        /// <summary>
        /// Unhooks Client MQServer events
        /// </summary>
        private void UnregisterClientMqServerEvents()
        {
            _mqServer.ServerDisconnected -= OnServerDisconnected;
            _mqServer.LogonArrived -= OnLogonArrived;
            _mqServer.LogoutArrived -= OnLogoutArrived;
            _mqServer.TickArrived -= OnTickArrived;
            _mqServer.LiveBarArrived -= OnLiveBarArrived;
            _mqServer.HistoricBarsArrived -= OnHistoricalBarDataArrived;
            _mqServer.MarketDataArrived -= OnMarketDataArrived;
            _mqServer.InquiryResponseArrived -= OnInquiryResponseArrived;
        }
        
        #region Incoming Messages for Market Data Engine

        /// <summary>
        /// Sends Login request to the MarketDataEngine
        /// </summary>
        /// <param name="login">TradeHub Login message to be sent</param>
        public void SendLoginRequest(Login login)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Sending login request message for: " + login.MarketDataProvider, _type.FullName,
                                 "SendLoginRequest");
                }

                // Send Message through the MQ Server
                _mqServer.SendLoginMessage(login);
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendLoginRequest");
            }
        }

        /// <summary>
        /// Sends Logout request to the MarketDataEngine
        /// </summary>
        /// <param name="logout">TradeHub Logout message to be sent</param>
        public void SendLogoutRequest(Logout logout)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Sending logout request message for: " + logout.MarketDataProvider, _type.FullName,
                                 "SendLogoutRequest");
                }

                // Send Message through the MQ Server
                _mqServer.SendLogoutMessage(logout);
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendLogoutRequest");
            }
        }

        /// <summary>
        /// Sends Tick Subscription Request to the Market Data Engine
        /// </summary>
        /// <param name="subscribe">TradeHub Subscribe message to be sent</param>
        public void SendTickSubscriptionRequest(Subscribe subscribe)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug(
                        "Sending Tick subscription request message for: " + subscribe.Security.Symbol + " on: " +
                        subscribe.MarketDataProvider, _type.FullName,
                        "SendTickSubscriptionRequest");
                }

                // Send message though the Mq Server
                _mqServer.SendTickSubscriptionMessage(subscribe);
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendTickSubscriptionRequest");
            }
        }

        /// <summary>
        /// Sends Tick Unsubscription Request to the Market Data Engine
        /// </summary>
        /// <param name="unsubscribe">TradeHub Unsubscribe message to be sent</param>
        public void SendTickUnsubscriptionRequest(Unsubscribe unsubscribe)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug(
                        "Sending Tick unsubscription request message for: " + unsubscribe.Security.Symbol + " on: " +
                        unsubscribe.MarketDataProvider, _type.FullName,
                        "SendTickUnsubscriptionRequest");
                }

                // Send message though the Mq Server
                _mqServer.SendTickUnsubscriptionMessage(unsubscribe);
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendTickUnsubscriptionRequest");
            }
        }

        /// <summary>
        /// Sends Live Bar Subscription Request to the Market Data Engine
        /// </summary>
        /// <param name="barDataRequest">TradeHub Bar Data Request Message to be sent</param>
        public void SendLiveBarSubscriptionRequest(BarDataRequest barDataRequest)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug(
                        "Sending Live Bar subscription request message for: " + barDataRequest.Security.Symbol + " on: " +
                        barDataRequest.MarketDataProvider, _type.FullName,
                        "SendLiveBarSubscriptionRequest");
                }

                // Send message though the Mq Server
                _mqServer.SendLiveBarSubscriptionMessage(barDataRequest);
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendLiveBarSubscriptionRequest");
            }
        }

        /// <summary>
        /// Sends Live Bar Unsubscription Request to the Market Data Engine
        /// </summary>
        /// <param name="barDataRequest">TradeHub Bar Data Request Message to be sent</param>
        public void SendLiveBarUnsubscriptionRequest(BarDataRequest barDataRequest)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug(
                        "Sending Live Bar unsubscription request message for: " + barDataRequest.Security.Symbol + " on: " +
                        barDataRequest.MarketDataProvider, _type.FullName,
                        "SendLiveBarUnsubscriptionRequest");
                }

                // Send message though the Mq Server
                _mqServer.SendLiveBarUnsubscriptionMessage(barDataRequest);
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendLiveBarUnsubscriptionRequest");
            }
        }

        /// <summary>
        /// Sends Historic Bar Data Request to the Market Data Engine
        /// </summary>
        /// <param name="historicDataRequest">TradeHub Historic Bar Data Request message to be sent</param>
        public void SendHistoricBarDataRequest(HistoricDataRequest historicDataRequest)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug(
                        "Sending Historical Bar data request message for: " + historicDataRequest.Security.Symbol + " on: " +
                        historicDataRequest.MarketDataProvider, _type.FullName,
                        "SendHistoricBarDataRequest");
                }

                // Send message though the Mq Server
                _mqServer.SendHistoricalBarDataRequestMessage(historicDataRequest);
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendHistoricBarDataRequest");
            }
        }

        /// <summary>
        /// Sends Inquiry Request to Market Data Engine to get given market data provider information
        /// </summary>
        /// <param name="marketDataProvider">Name of the market data provider for which to get details</param>
        public void SendInquiryRequest(string marketDataProvider)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug( "Sending Inquiry request message for: " + marketDataProvider, _type.FullName,
                        "SendInquiryRequest");
                }

                InquiryMessage inquiryMessage= new InquiryMessage();
                inquiryMessage.Type = TradeConstants.InquiryTags.MarketDataProviderInfo;
                inquiryMessage.MarketDataProvider = marketDataProvider;

                // Send message though the Mq Server
                _mqServer.SendInquiryMessage(inquiryMessage);
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendInquiryRequest");
            }
        }

        #endregion

        #region Incoming Messages from the Market Data Engine

        /// <summary>
        /// Called when Logon message is recieved from the Market Data Engine
        /// </summary>
        /// <param name="message">Incoming string message</param>
        void OnLogonArrived(string message)
        {
            try
            {
                if(_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Logon message recieved from Market Data Engine: " + message, _type.FullName, "OnLogonArrived");
                }

                if(_logonArrived!=null)
                {
                    _logonArrived(message);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnLogonArrived");
            }
        }

        /// <summary>
        /// Called when Logout message is recieved from the Market Data Engine
        /// </summary>
        /// <param name="message">Incoming string message</param>
        void OnLogoutArrived(string message)
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Logout message recieved from Market Data Engine: " + message, _type.FullName, "OnLogoutArrived");
                }

                if (_logoutArrived != null)
                {
                    _logoutArrived(message);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnLogoutArrived");
            }
        }

        /// <summary>
        /// Called when a new Tick is recieved from the Market Data Engine
        /// </summary>
        /// <param name="tick">Incoming TradeHub Tick message</param>
        private void OnTickArrived(Tick tick)
        {
            try
            {
                //if (_asyncClassLogger.IsDebugEnabled)
                //{
                //    _asyncClassLogger.Debug("Tick recieved from Market Data Engine: " + tick, _type.FullName, "OnTickArrived");
               
                //}

                if (_tickArrived != null)
                {
                    _tickArrived(tick);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnTickArrived");
            }
        }

        /// <summary>
        /// Called when a new Live Bar is recived from the Market Data Engine
        /// </summary>
        /// <param name="bar">Incoming TradeHub Bar Message</param>
        private void OnLiveBarArrived(Bar bar)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Bar recieved from Market Data Engine: " + bar, _type.FullName, "OnLiveBarArrived");
                }

                if (_liveBarArrived != null)
                {
                    _liveBarArrived(bar);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnLiveBarArrived");
            }
        }

        /// <summary>
        /// Called when Historical Bars are recieved
        /// </summary>
        /// <param name="historicBarData">Incoming TradeHub HistoricalBarData</param>
        private void OnHistoricalBarDataArrived(HistoricBarData historicBarData)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Historical Bar Data recieved from Market Data Engine: " + historicBarData, _type.FullName, "OnHistoricalBarDataArrived");
                }

                if (_historicBarsArrived != null)
                {
                    _historicBarsArrived(historicBarData);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnHistoricalBarDataArrived");
            }
        }

        /// <summary>
        /// Called whenn new market data message is received from Market Data Engine
        /// </summary>
        /// <param name="rabbitMqMessage">Contains message consumed from messaging queue</param>
        private void OnMarketDataArrived(byte[] bytes)
        {
            if (_asyncClassLogger.IsDebugEnabled)
            {
                _asyncClassLogger.Debug("Market Data received from Market Data Engine", _type.FullName, "OnMarketDataArrived");
            }

            if (_marketDataArrived != null)
            {
                _marketDataArrived(bytes);
            }
        }
        /// <summary>
        /// Called when a Inquiry Response is recieved
        /// </summary>
        /// <param name="inquiryResponse">Incoming TradeHub InquiryResponse</param>
        private void OnInquiryResponseArrived(InquiryResponse inquiryResponse)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Inquiry Response recieved from Market Data Engine: " + inquiryResponse,
                                 _type.FullName, "OnInquiryResponseArrived");
                }

                if (inquiryResponse.Type.Equals(TradeConstants.InquiryTags.AppID))
                {
                    _appId = inquiryResponse.AppId;

                    // Start MQ Server
                    _mqServer.Connect(_appId);

                    // Send Application Info
                    _mqServer.SendAppInfoMessage(_appId);

                    // Start Heartbeat Sequence
                    _mqServer.StartHeartbeat();

                    // Raise Event to Notify Listeners that MDE-Client is ready to entertain request
                    if (_serverConnected != null)
                    {
                        _serverConnected();
                    }
                }
                else if (inquiryResponse.Type.Equals(TradeConstants.InquiryTags.MarketDataProviderInfo))
                {
                    OnMarketDataInfoReceived(inquiryResponse);
                }
                else
                {
                    _asyncClassLogger.Info("Invalid Response received from MDE", _type.FullName, "OnInquiryResponseArrived");
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnInquiryResponseArrived");
            }
        }

        #endregion
        
        /// <summary>
        /// Requests Market Data Engine for Unique Application ID
        /// </summary>
        private void RequestAppId()
        {
            try
            {
                InquiryMessage inquiry = new InquiryMessage();
                inquiry.Type = TradeConstants.InquiryTags.AppID;

                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Sending inquiry request message for: " + inquiry.Type, _type.FullName,
                                 "RequestAppId");
                }

                // Send Message through the MQ Server
                _mqServer.SendInquiryMessage(inquiry);
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "RequestAppId");
            }
        }

        /// <summary>
        /// Extracts Market Data Providers Info from received response from MDE
        /// </summary>
        /// <param name="inquiryResponse">TradeHub InquiryResponse object containing Market Data Provivder Info</param>
        private void OnMarketDataInfoReceived(InquiryResponse inquiryResponse)
        {
            try
            {
                MarketDataProviderInfo marketDataProviderInfo = new MarketDataProviderInfo();

                marketDataProviderInfo.DataProviderName = inquiryResponse.MarketDataProvider;

                // Set Market Data Provider information received
                if (inquiryResponse.MarketDataProviderInfo.Contains(typeof(ILiveTickDataProvider)))
                {
                    marketDataProviderInfo.ProvidesTickData = true;
                }
                if (inquiryResponse.MarketDataProviderInfo.Contains(typeof(ILiveBarDataProvider)))
                {
                    marketDataProviderInfo.ProvidesLiveBarData = true;
                }
                if (inquiryResponse.MarketDataProviderInfo.Contains(typeof(IHistoricBarDataProvider)))
                {
                    marketDataProviderInfo.ProvidesHistoricalBarData = true;
                }

                // Raise Event to notify listeners
                if (_inquiryResponseArrived != null)
                {
                    _inquiryResponseArrived(marketDataProviderInfo);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnMarketDataInfoReceived");
            }
        }

        /// <summary>
        /// Notifies Market Data Engine that the application is closing
        /// </summary>
        private void SendDisconnectRequest()
        {
            try
            {
                InquiryMessage inquiry = new InquiryMessage();
                inquiry.Type = TradeConstants.InquiryTags.DisconnectClient;

                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Sending inquiry request message for: " + inquiry.Type, _type.FullName,
                                 "SendDisconnectRequest");
                }

                // Send Message through the MQ Server
                _mqServer.SendInquiryMessage(inquiry);
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendDisconnectRequest");
            }
        }

        /// <summary>
        /// Raised when MDE Server is disconnected
        /// </summary>
        private void OnServerDisconnected()
        {
            if (_asyncClassLogger.IsDebugEnabled)
            {
                _asyncClassLogger.Debug("Notifying Listeners about MDE Server Disconnection", _type.FullName, "OnServerDisconnected");
            }

            if (_serverDisconnected!=null)
            {
                _serverDisconnected();
            }
        }

    }
}
