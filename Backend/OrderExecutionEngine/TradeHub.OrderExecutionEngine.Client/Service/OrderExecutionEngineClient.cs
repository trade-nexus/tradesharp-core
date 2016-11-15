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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Spring.Context.Support;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.ValueObjects;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.Inquiry;
using TradeHub.OrderExecutionEngine.Client.Utility;
using TradeHubConstants = TradeHub.Common.Core.Constants;

namespace TradeHub.OrderExecutionEngine.Client.Service
{
    /// <summary>
    /// Provides connectivity with Order Execution Engine
    /// </summary>
    public class OrderExecutionEngineClient
    {
        private Type _type = typeof (OrderExecutionEngineClient);
        private AsyncClassLogger _asyncClassLogger;

        #region Events

        // ReSharper disable InconsistentNaming
        private event Action<string> _logonArrived;
        private event Action<string> _logoutArrived;
        private event Action<InquiryResponse> _inquiryResponseArrived;
        private event Action<Order> _newArrived;
        private event Action<Order> _cancellationArrived;
        private event Action<LimitOrder> _locateMessageArrived;
        private event Action<Execution> _executionArrived;
        private event Action<Rejection> _rejectionArrived;
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

        public event Action<Order> NewArrived
        {
            add
            {
                if (_newArrived == null)
                {
                    _newArrived += value;
                }
            }
            remove { _newArrived -= value; }
        }

        public event Action<Order> CancellationArrived
        {
            add
            {
                if (_cancellationArrived == null)
                {
                    _cancellationArrived += value;
                }
            }
            remove { _cancellationArrived -= value; }
        }

        public event Action<LimitOrder> LocateMessageArrived
        {
            add
            {
                if (_locateMessageArrived == null)
                {
                    _locateMessageArrived += value;
                }
            }
            remove { _locateMessageArrived -= value; }
        }

        public event Action<Execution> ExecutionArrived
        {
            add
            {
                if (_executionArrived == null)
                {
                    _executionArrived += value;
                }
            }
            remove { _executionArrived -= value; }
        }

        public event Action<Rejection> RejectionArrived
        {
            add
            {
                if (_rejectionArrived == null)
                {
                    _rejectionArrived += value;
                }
            }
            remove { _rejectionArrived -= value; }
        }

        public event Action<InquiryResponse> InquiryResponseArrived
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
                if (_serverConnected == null)
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
        private OrderExecutionClientMqServer _mqServer;

        /// <summary>
        /// Reads and holds the Exchange/Queues information to be used with the Client
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
        public OrderExecutionEngineClient(): this("OEEServer.xml", "OEEClientMqConfig.xml")
        {
            
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="server">OEE Server Config file</param>
        /// <param name="client">OEE Client MQ Config File</param>
        public OrderExecutionEngineClient(string server = "OEEServer.xml", string client = "OEEClientMqConfig.xml")
        {
            _asyncClassLogger = new AsyncClassLogger("OrderExecutionClient");
            // Set logging level
            // Set logging path
            _asyncClassLogger.LogDirectory(TradeHubConstants.DirectoryStructure.CLIENT_LOGS_LOCATION);
            _asyncClassLogger.SetLoggingLevel();

            // Get Configuration details
            _configurationReader = new ConfigurationReader(server, client, _asyncClassLogger);
            _configurationReader.ReadParameters();

            // Get MQ Server object
            _mqServer = new OrderExecutionClientMqServer(_configurationReader.OeeMqServerparameters, _configurationReader.ClientMqParameters, _asyncClassLogger);
        }

        /// <summary>
        /// Initializes necessary components after client is disconnected
        /// </summary>
        public void Initialize()
        {
            // Reset configuration values
            _configurationReader.ReadParameters();

            // Initialize MQ-Server
            _mqServer.Initialize();
        }

        /// <summary>
        /// Initializes required parameters for the Connector
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
                    // Notify OEE about application close
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
            _mqServer.NewArrived += OnNewArrived;
            _mqServer.CancellationArrived += OnCancellationArrived;
            _mqServer.ExecutionArrived += OnExecutionArrived;
            _mqServer.RejectionArrived += OnRejectionArrived;
            _mqServer.InquiryResponseArrived += OnInquiryResponseArrived;
            _mqServer.LocateMessageArrived += OnLocateMessageArrived;
        }

        /// <summary>
        /// Unhooks Client MQServer events
        /// </summary>
        private void UnregisterClientMqServerEvents()
        {
            _mqServer.ServerDisconnected -= OnServerDisconnected;
            _mqServer.LogonArrived -= OnLogonArrived;
            _mqServer.LogoutArrived -= OnLogoutArrived;
            _mqServer.NewArrived -= OnNewArrived;
            _mqServer.CancellationArrived -= OnCancellationArrived;
            _mqServer.ExecutionArrived -= OnExecutionArrived;
            _mqServer.RejectionArrived -= OnRejectionArrived;
            _mqServer.InquiryResponseArrived -= OnInquiryResponseArrived;
        }

        #region Incoming Messages for Order Execution Engine

        /// <summary>
        /// Sends Login request to the Order Execution Engine
        /// </summary>
        /// <param name="login">TradeHub Login message to be sent</param>
        public void SendLoginRequest(Login login)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Sending login request message for: " + login.OrderExecutionProvider, _type.FullName,
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
        /// Sends Logout request to the Order Execution Engine
        /// </summary>
        /// <param name="logout">TradeHub Logout message to be sent</param>
        public void SendLogoutRequest(Logout logout)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Sending logout request message for: " + logout.OrderExecutionProvider, _type.FullName,
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
        /// Sends Market Order Request to the Order Execution Engine
        /// </summary>
        /// <param name="marketOrder">TradeHub Market Order object to be sent</param>
        public void SendMarketOrderRequest(MarketOrder marketOrder)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug(
                        "Sending Market Order request message for: " + marketOrder.Security.Symbol + " on: " +
                        marketOrder.OrderExecutionProvider, _type.FullName,
                        "SendMarketOrderRequest");
                }

                // Send message though the Mq Server
                _mqServer.SendMarketOrderRequestMessage(marketOrder);
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendMarketOrderRequest");
            }
        }

        /// <summary>
        /// Sends Limit Order Request to the Order Execution Engine
        /// </summary>
        /// <param name="limitOrder">TradeHub Limit Order object to be sent</param>
        public void SendLimitOrderRequest(LimitOrder limitOrder)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug(
                        "Sending Tick unsubscription request message for: " + limitOrder.Security.Symbol + " on: " +
                        limitOrder.OrderExecutionProvider, _type.FullName,
                        "SendLimitOrderRequest");
                }

                // Send message though the Mq Server
                _mqServer.SendLimitOrderRequestMessage(limitOrder);
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendLimitOrderRequest");
            }
        }

        /// <summary>
        /// Sends Cancel Order Request to the Order Execution Engine
        /// </summary>
        /// <param name="cancelOrder">TradeHub Order object to be sent</param>
        public void SendCancelOrderRequest(Order cancelOrder)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug(
                        "Sending cancel order request message for: " + cancelOrder.Security.Symbol + " on: " +
                        cancelOrder.OrderExecutionProvider, _type.FullName,
                        "SendCancelOrderRequest");
                }

                // Send message though the Mq Server
                _mqServer.SendCancelOrderRequestMessage(cancelOrder);
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendCancelOrderRequest");
            }
        }

        /// <summary>
        /// Sends Locate Response to the Order Execution Engine
        /// </summary>
        /// <param name="locateResponse">TradeHub Locate Response object</param>
        public void SendLocateResponse(LocateResponse locateResponse)
        {
            try
            {
                // Add Unique Application level ID to help identify the application from which the LocateResponse is generated
                locateResponse.StrategyId = _appId + "|" + locateResponse.OrderId; 
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug( "Sending locate response: " + locateResponse, _type.FullName, "SendLocateResponse");
                }

                // Send message though the Mq Server
                _mqServer.SendLocateResponse(locateResponse);
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendCancelOrderRequest");
            }
        }

        /// <summary>
        /// Send Order requests to OEE as Byte Streams
        /// </summary>
        /// <param name="rabbitMqMessage">Contains Order Byte Stream</param>
        public void SendOrderRequests(RabbitMqRequestMessage rabbitMqMessage)
        {
            try
            {
                // Convert AppID into Bytes
                byte[] appIdBytes = Encoding.UTF8.GetBytes(_appId + ",");
                // Create a new byte array to hold complete info
                byte[] completeByteMessage = new byte[rabbitMqMessage.Message.Length + appIdBytes.Length];

                // Add AppID Bytes to new byte array
                appIdBytes.CopyTo(completeByteMessage, 0);
                // Add order request bytes to new byte array
                rabbitMqMessage.Message.CopyTo(completeByteMessage, appIdBytes.Length);

                // Swap values
                rabbitMqMessage.Message = completeByteMessage;

                // Forward requests to Client MQ Server
                _mqServer.SendOrderRequests(rabbitMqMessage);
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendOrderRequests");
            }
        }

        #endregion

        #region Incoming Messages from the Order Execution Engine

        /// <summary>
        /// Called when Logon message is recieved from the Order Execution Engine
        /// </summary>
        /// <param name="message">Incoming string message</param>
        void OnLogonArrived(string message)
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Logon message recieved from Order Exectuion Engine: " + message, _type.FullName, "OnLogonArrived");
                }

                if (_logonArrived != null)
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
        /// Called when Logout message is recieved from the Order Execution Engine
        /// </summary>
        /// <param name="message">Incoming string message</param>
        void OnLogoutArrived(string message)
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Logout message recieved from Order Execution Engine: " + message, _type.FullName, "OnLogoutArrived");
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
        /// Called when order status New/Submitted is recieved from the Order Execution Engine
        /// </summary>
        /// <param name="order">Incoming TradeHub Order message</param>
        private void OnNewArrived(Order order)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Order status New/Submitted recieved from Order Execution Engine: " + order, _type.FullName, "OnNewArrived");
                }

                // Raise Event to notify listeners
                if (_newArrived != null)
                {
                    _newArrived(order);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnNewArrived");
            }
        }

        /// <summary>
        /// Called when order cancellation is recived from the Order Execution Engine
        /// </summary>
        /// <param name="order">Incoming TradeHub Order Message</param>
        private void OnCancellationArrived(Order order)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Order cancellation recieved from Order Execution Engine: " + order, _type.FullName, "OnCancellationArrived");
                }

                // Raise event to notify listeners
                if (_cancellationArrived != null)
                {
                    _cancellationArrived(order);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnCancellationArrived");
            }
        }

        /// <summary>
        /// Called when order execution is recieved from the Order Execution Engine
        /// </summary>
        /// <param name="executionInfo">Incoming TradeHub Execution Info message</param>
        private void OnExecutionArrived(Execution executionInfo)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Order Execution recieved from Order Execution Engine: " + executionInfo, _type.FullName, "OnExecutionArrived");
                }

                // Raise event to notify listeners
                if (_executionArrived != null)
                {
                    _executionArrived(executionInfo);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnExecutionArrived");
            }
        }

        /// <summary>
        /// Called when rejection is recieved from the Order Execution Engine
        /// </summary>
        /// <param name="rejection">Incoming TradeHub Rejection message</param>
        private void OnRejectionArrived(Rejection rejection)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Rejection recieved from Order Execution Engine: " + rejection, _type.FullName, "OnRejectionArrived");
                }

                // Raise event to notify listeners
                if (_rejectionArrived != null)
                {
                    _rejectionArrived(rejection);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnRejectionArrived");
            }
        }

        /// <summary>
        /// Called when a Inquiry Response is recieved from the Order Execution Engine
        /// </summary>
        /// <param name="inquiryResponse">Incoming TradeHub InquiryResponse</param>
        private void OnInquiryResponseArrived(InquiryResponse inquiryResponse)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Inquiry Response recieved from Order Execution Engine: " + inquiryResponse,
                                 _type.FullName, "OnInquiryResponseArrived");
                }

                if (inquiryResponse.Type.Equals(TradeHubConstants.InquiryTags.AppID))
                {
                    _appId = inquiryResponse.AppId;

                    // Start MQ Server
                    _mqServer.Connect(_appId);

                    // Send Application Info
                    _mqServer.SendAppInfoMessage(_appId);

                    // Start Heartbeat Sequence
                    _mqServer.StartHeartbeat();

                    // Raise Event to Notify Listeners that OEE-Client is ready to entertain request
                    if (_serverConnected != null)
                    {
                        _serverConnected();
                    }
                }
                else
                {
                    if (_inquiryResponseArrived != null)
                    {
                        _inquiryResponseArrived(inquiryResponse);
                    }
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnInquiryResponseArrived");
            }
        }

        /// <summary>
        /// Called when Locate Message is received from the Order Execution Engine
        /// </summary>
        /// <param name="locateMessage">TradeHub LimitOrder containing Locate Message info</param>
        private void OnLocateMessageArrived(LimitOrder locateMessage)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Locate message recieved from Order Execution Engine: " + locateMessage, _type.FullName, "OnLocateMessageArrived");
                }

                // Raise event to notify listeners
                if (_locateMessageArrived != null)
                {
                    _locateMessageArrived(locateMessage);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnLocateMessageArrived");
            }
        }

        #endregion

        /// <summary>
        /// Requests Order Execution Engine for Unique Application ID
        /// </summary>
        private void RequestAppId()
        {
            try
            {
                try
                {
                    InquiryMessage inquiry = new InquiryMessage();
                    inquiry.Type = TradeHubConstants.InquiryTags.AppID;

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
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "RequestAppId");
            }
        }

        /// <summary>
        /// Notifies Order Execution Engine that the application is closing
        /// </summary>
        private void SendDisconnectRequest()
        {
            try
            {
                InquiryMessage inquiry = new InquiryMessage();
                inquiry.Type = TradeHubConstants.InquiryTags.DisconnectClient;

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
                _asyncClassLogger.Debug("Notifying Listeners about OEE Server Disconnection", _type.FullName, "OnServerDisconnected");
            }

            if (_serverDisconnected != null)
            {
                _serverDisconnected();
            }
        }

    }
}
