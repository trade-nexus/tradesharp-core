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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyNetQ;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.Utility;
using TradeHub.Common.Core.ValueObjects;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.Inquiry;
using TradeHub.OrderExecutionEngine.Configuration.Service;
using TradeHub.OrderExecutionEngine.OrderExecutionProviderGateway.Service;
using Constants = TradeHub.Common.Core.Constants;

namespace TradeHub.OrderExecutionEngine.Server.Service
{
    /// <summary>
    /// Startup class which handles all the "OrderExecutionEngine.Server" activities
    /// </summary>
    public class ApplicationController
    {
        private Type _type = typeof (ApplicationController);

        /// <summary>
        /// Holds reference to the MQ Server for Order Execution Engine
        /// </summary>
        private readonly OrderExecutionMqServer _mqServer;

        /// <summary>
        /// Holds reference of Order Execution Message Processor for handling incoming request to OEE
        /// </summary>
        private readonly OrderExecutionMessageProcessor _messageProcessor;

        /// <summary>
        /// Keeps Track of all the connected strategies routing key information
        /// KEY = Application ID
        /// VALUE = Dictionary containing Routing Info for corresponding messages
        /// </summary>
        private ConcurrentDictionary<string, Dictionary<string, ClientMqParameters>> _strategiesInfoMap;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="mqServer">TradeHub Order Execution MQ Server</param>
        /// <param name="messageProcessor"> </param>
        public ApplicationController(OrderExecutionMqServer mqServer, OrderExecutionMessageProcessor messageProcessor)
        {
            _strategiesInfoMap = new ConcurrentDictionary<string, Dictionary<string, ClientMqParameters>>();
         
            _mqServer = mqServer;
            _messageProcessor = messageProcessor;

            // Hook Order Execution MQ Server Events
            RegisterMqServerEvents();

            // Hook Order Execution Message Processor Events
            RegisterMessageProcessorEvents();
        }

        /// <summary>
        /// Hooks Mq Server Events to get incoming messages from strategies to OEES
        /// </summary>
        private void RegisterMqServerEvents()
        {
            try
            {
                _mqServer.AppInfoReceived += OnAppInfoReceived;
                _mqServer.DisconnectApplication += OnDisconnectApplicationReceived;
                _mqServer.LogonRequestRecieved += OnLogonRequestReceived;
                _mqServer.LogoutRequestRecieved += OnLogoutRequestReceived;
                _mqServer.InquiryRequestReceived += OnInquiryRequestReceived;
                _mqServer.LimitOrderRequestRecieved += OnLimitOrderRequestRecieved;
                _mqServer.MarketOrderRequestRecieved += OnMarketOrderRequestRecieved;
                _mqServer.StopOrderRequestRecieved += OnStopOrderRequestRecieved;
                _mqServer.StopLimitOrderRequestRecieved += OnStopLimitOrderRequestRecieved;
                _mqServer.CancelOrderRequestRecieved += OnOrderCancellationRequestReceived;
                _mqServer.LocateResponseRecieved += OnLocateResponseRecieved;
                
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "RegisterMqServerEvents");
            }
        }

        /// <summary>
        /// Hooks Order Execution Message Processor events to get outgoing messages from OEE to strategies
        /// </summary>
        private void RegisterMessageProcessorEvents()
        {
            _messageProcessor.LogonArrived += OnExecutionProviderLogonArrived;
            _messageProcessor.LogoutArrived += OnExecutionProviderLogoutArrived;
            _messageProcessor.NewArrived += OnExecutionProviderNewArrived;
            _messageProcessor.CancellationArrived += OnExecutionProviderCancellationArrived;
            _messageProcessor.ExecutionArrived += OnExecutionProviderExecutionArrived;
            _messageProcessor.RejectionArrived += OnExecutionProviderRejectionArrived;
            _messageProcessor.LocateMessageArrived += OnExecutionProviderLocateMessageArrived;
            _messageProcessor.PositionMessageArrived += OnPositionMessageArrived;

        }

        /// <summary>
        /// Starts required connections
        /// </summary>
        public void StartServer()
        {
            try
            {
                // Configure Logging level
                Logger.SetLoggingLevel();

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Starting Order Execution Engine Service", _type.FullName, "StartServer");
                }

                // Connect Order Execution MQ Server
                _mqServer.Connect();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "StartServer");
            }
        }

        /// <summary>
        /// Closes open connections
        /// </summary>
        public void StopServer()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Disconnecting Order Execution Engine Service", _type.FullName, "StopServer");
                }

                // Disconnect Order Execution MQ Server
                _mqServer.Disconnect();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "StopServer");
            }
        }

        #region MQ Server Event Handling

        /// <summary>
        /// Called when Login message is received from MQ Server
        /// </summary>
        private void OnLogonRequestReceived(IMessage<Login> loginMessage)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Logon message received from MQ Server: " + loginMessage.Properties.AppId, _type.FullName, "OnLogonRequestReceived");
                }

                // Send incoming Login message to Message Processor 
                _messageProcessor.OnLogonMessageRecieved(loginMessage.Body, loginMessage.Properties.AppId);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnLogonRequestReceived");
            }
        }

        /// <summary>
        /// Called when Logout message is received from MQ Server
        /// </summary>
        private void OnLogoutRequestReceived(IMessage<Logout> logoutMessage)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Logout message received from MQ Server: " + logoutMessage.Properties.AppId, _type.FullName, "OnLogoutRequestReceived");
                }

                // Send incoming Logout message to Message Processor
                _messageProcessor.OnLogoutMessageRecieved(logoutMessage.Body, logoutMessage.Properties.AppId);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnLogoutRequestReceived");
            }
        }

        /// <summary>
        /// Called when Application disconnect request is receied from the MQ Server
        /// </summary>
        private void OnDisconnectApplicationReceived(string applicationId)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Application disconnect received from MQ Server: " + applicationId, _type.FullName, "OnDisconnectApplicationReceived");
                }

                Dictionary<string, ClientMqParameters> responseMap;
                _strategiesInfoMap.TryRemove(applicationId, out responseMap);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnDisconnectApplicationReceived");
            }
        }

        /// <summary>
        /// Called when Application Info is received from MQ Server
        /// </summary>
        /// <param name="responseDetails">Dictionary containing app info for reply response</param>
        private void OnAppInfoReceived(IMessage<Dictionary<string, string>> responseDetails)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Application Info received from MQ Server: " + responseDetails.Properties.AppId, _type.FullName, "OnAppInfoReceived");
                }

                // Get Application Routing Key info
                var clientMqParameters = ReadAppInfoMessage(responseDetails.Properties.AppId, responseDetails.Body);

                if (clientMqParameters != null)
                {
                    // Update Strategies Map
                    _strategiesInfoMap.AddOrUpdate(responseDetails.Properties.AppId, clientMqParameters,
                                               (key, value) => clientMqParameters);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnAppInfoReceived");
            }
        }

        /// <summary>
        /// Called when Inquiry request is received from MQ Server
        /// </summary>
        private void OnInquiryRequestReceived(IMessage<InquiryMessage> inquiryMessage)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Application Info received from MQ Server: " + inquiryMessage.Properties.AppId, _type.FullName, "OnInquiryRequestReceived");
                }

                // Create the Inquiry Respons to be sent
                InquiryResponse inquiryResponse = new InquiryResponse();

                if (inquiryMessage.Body.Type.Equals(Constants.InquiryTags.AppID))
                {
                    string id = ApplicationIdGenerator.NextId();
                    inquiryResponse.Type = Constants.InquiryTags.AppID;
                    inquiryResponse.AppId = id;
                }
                else if (inquiryMessage.Body.Type.Equals(Constants.InquiryTags.DisconnectClient))
                {
                    OnDisconnectApplicationReceived(inquiryMessage.Properties.AppId);
                    return;
                }

                // Create EasyNetQ message to be published
                Message<InquiryResponse> message = new Message<InquiryResponse>(inquiryResponse);

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Publishing Inquiry Response for: " + inquiryMessage.Body.Type, _type.FullName, "OnInquiryRequestReceived");
                }
                
                // Publish Messages on the exchange
                PublishMessages(inquiryMessage.Properties.ReplyTo, message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnInquiryRequestReceived");
            }
        }

        /// <summary>
        /// Called when Market Order Request is received from MQ Server
        /// </summary>
        private void OnMarketOrderRequestRecieved(MarketOrder marketOrder, string appId)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug(
                        "Market Order request received from MQ Server: " + appId + marketOrder.OrderID,
                        _type.FullName, "OnMarketOrderRequestRecieved");
                }

                // Send Message to Order Execution Processor for further handling
                _messageProcessor.MarketOrderRequestReceived(marketOrder, appId);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnMarketOrderRequestRecieved");
            }
        }

        /// <summary>
        /// Called when Limit Order Request is received from MQ Server
        /// </summary>
        private void OnLimitOrderRequestRecieved(LimitOrder limitOrder, string appId)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Limit Order request received from MQ Server: " + appId + limitOrder.OrderID,
                                 _type.FullName, "OnLimitOrderRequestRecieved");
                }

                // Send Message to Order Execution Processor for further handling
                _messageProcessor.LimitOrderRequestReceived(limitOrder, appId);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnLimitOrderRequestRecieved");
            }
        }

        /// <summary>
        /// Called when Stop Limit Order Request is received from MQ Server
        /// </summary>
        private void OnStopLimitOrderRequestRecieved(IMessage<StopLimitOrder> stopLimitOrderMessage)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Stop Limit Order request received from MQ Server: " + stopLimitOrderMessage.Properties.AppId, _type.FullName, "OnStopLimitOrderRequestRecieved");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnStopLimitOrderRequestRecieved");
            }
        }

        /// <summary>
        /// Called when Stop Order Request is received from MQ Server
        /// </summary>
        private void OnStopOrderRequestRecieved(IMessage<StopOrder> stopOrderMessage)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Stop Order request received from MQ Server: " + stopOrderMessage.Properties.AppId, _type.FullName, "OnStopOrderRequestRecieved");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnStopOrderRequestRecieved");
            }
        }

        /// <summary>
        /// Called when Order Cancellation Request is received from MQ Server
        /// </summary>
        private void OnOrderCancellationRequestReceived(Order order, string appId)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug(
                        "Order Cancellation request received from: " + appId + order.OrderID,
                        _type.FullName, "OnOrderCancellationRequestReceived");
                }

                // Send Message to Order Execution Processor for further handling
                _messageProcessor.CancelOrderRequestReceived(order, appId);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnOrderCancellationRequestReceived");
            }
        }

        /// <summary>
        /// Called when Locate Response is received from MQ Server
        /// </summary>
        private void OnLocateResponseRecieved(IMessage<LocateResponse> locateResponseMessage)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Locate response received from: " + locateResponseMessage.Properties.AppId,
                                 _type.FullName, "OnLocateResponseRecieved");
                }

                // Send Message to Order Execution Processor for further handling
                _messageProcessor.LocateResponseReceived(locateResponseMessage.Body,
                                                             locateResponseMessage.Properties.AppId);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnLocateResponseRecieved");
            }
        }

       

        #endregion

        #region Message Processor Event Handling

        /// <summary>
        /// Called when Logon is received from Order Execution Provider
        /// </summary>
        /// <param name="applicationId">Unique Application ID</param>
        /// <param name="orderExecutionProvider">Name of Order Execution Provider</param>
        private void OnExecutionProviderLogonArrived(string applicationId, string orderExecutionProvider)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Logon message received from: " + orderExecutionProvider + " for: " + applicationId,
                                _type.FullName, "OnExecutionProviderLogonArrived");
                }

                Dictionary<string, ClientMqParameters> strategyInfo;
                if (_strategiesInfoMap.TryGetValue(applicationId, out strategyInfo))
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Publishing Message for: " + applicationId, _type.FullName, "OnExecutionProviderLogonArrived");
                    }

                    // Create EasyNetQ message to be published
                    Message<string> message = new Message<string>("Logon-" + orderExecutionProvider);

                    // Publish Messages on the exchange
                    PublishMessages(strategyInfo["Admin"], message);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnExecutionProviderLogonArrived");
            }
        }

        /// <summary>
        /// Called when Logout is received from Order Execution Provider
        /// </summary>
        /// <param name="applicationId">Unique Application ID</param>
        /// <param name="orderExecutionProvider">Name of Order Execution Provider</param>
        private void OnExecutionProviderLogoutArrived(string applicationId, string orderExecutionProvider)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Logout message received from: " + orderExecutionProvider + " for: " + applicationId,
                                _type.FullName, "OnExecutionProviderLogoutArrived");
                }

                Dictionary<string, ClientMqParameters> strategyInfo;
                if (_strategiesInfoMap.TryGetValue(applicationId, out strategyInfo))
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Publishing Message for: " + applicationId, _type.FullName, "OnExecutionProviderLogoutArrived");
                    }

                    // Create EasyNetQ message to be published
                    Message<string> message = new Message<string>("Logout-" + orderExecutionProvider);

                    // Publish Messages on the exchange
                    PublishMessages(strategyInfo["Admin"], message);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnExecutionProviderLogoutArrived");
            }
        }

        /// <summary>
        /// Called when an Order with status New/Submitted is received from Order Execution Provider
        /// </summary>
        /// <param name="order">TradeHub Order object containing New Order Info</param>
        /// <param name="applicationId">Unique Application ID</param>
        private void OnExecutionProviderNewArrived(Order order, string applicationId)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(
                        "Order status New/Submitted received from: " + order.OrderExecutionProvider + " for: " +
                        applicationId, _type.FullName, "OnExecutionProviderNewArrived");
                }

                Dictionary<string, ClientMqParameters> strategyInfo;
                if (_strategiesInfoMap.TryGetValue(applicationId, out strategyInfo))
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Publishing Message for: " + applicationId, _type.FullName, "OnExecutionProviderNewArrived");
                    }

                    // Create EasyNetQ message to be published
                    Message<Order> message = new Message<Order>(order);

                    // Set Order Status
                    order.OrderStatus = Constants.OrderStatus.SUBMITTED;

                    // Publish Messages on the exchange
                    PublishMessages(strategyInfo["Order"], message);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnExecutionProviderNewArrived");
            }
        }

        /// <summary>
        /// Callen when an Order cancellation is received from Order Execution Provider
        /// </summary>
        /// <param name="order">TradeHub Order object containing Order Cancellation Info</param>
        /// <param name="applicationId">Unique Application ID</param>
        private void OnExecutionProviderCancellationArrived(Order order, string applicationId)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(
                        "Order cancellation received from: " + order.OrderExecutionProvider + " for: " +
                        applicationId, _type.FullName, "OnExecutionProviderCancellationArrived");
                }

                Dictionary<string, ClientMqParameters> strategyInfo;
                if (_strategiesInfoMap.TryGetValue(applicationId, out strategyInfo))
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Publishing Message for: " + applicationId, _type.FullName, "OnExecutionProviderCancellationArrived");
                    }

                    // Create EasyNetQ message to be published
                    Message<Order> message = new Message<Order>(order);

                    // Set Order Status
                    order.OrderStatus = Constants.OrderStatus.CANCELLED;

                    // Publish Messages on the exchange
                    PublishMessages(strategyInfo["Order"], message);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnExecutionProviderCancellationArrived");
            }
        }

        /// <summary>
        /// Called when an Order Execution is received from Order Execution Provider
        /// </summary>
        /// <param name="executionInfo">TradeHub Execution Info object</param>
        /// <param name="applicationId">Unique Application ID</param>
        private void OnExecutionProviderExecutionArrived(Execution executionInfo, string applicationId)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(
                        "Order execution received from: " + executionInfo.OrderExecutionProvider + " for: " +
                        applicationId, _type.FullName, "OnExecutionProviderExecutionArrived");
                }

                Dictionary<string, ClientMqParameters> strategyInfo;
                if (_strategiesInfoMap.TryGetValue(applicationId, out strategyInfo))
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Publishing Message for: " + applicationId, _type.FullName, "OnExecutionProviderExecutionArrived");
                    }

                    // Create EasyNetQ message to be published
                    Message<Execution> message = new Message<Execution>(executionInfo);

                    // Publish Messages on the exchange
                    PublishMessages(strategyInfo["Execution"], message);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnExecutionProviderExecutionArrived");
            }
        }

        /// <summary>
        /// Called when Rejection event is received from Order Execution Provider
        /// </summary>
        /// <param name="rejection">TradeHub Rejection object containing Rejection details</param>
        /// <param name="applicationId">Unique Application ID</param>
        private void OnExecutionProviderRejectionArrived(Rejection rejection, string applicationId)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(
                        "Order rejection received from: " + rejection.OrderExecutionProvider + " for: " +
                        applicationId, _type.FullName, "OnExecutionProviderRejectionArrived");
                }

                Dictionary<string, ClientMqParameters> strategyInfo;
                if (_strategiesInfoMap.TryGetValue(applicationId, out strategyInfo))
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Publishing Message for: " + applicationId, _type.FullName, "OnExecutionProviderRejectionArrived");
                    }

                    // Create EasyNetQ message to be published
                    Message<Rejection> message = new Message<Rejection>(rejection);

                    // Publish Messages on the exchange
                    PublishMessages(strategyInfo["Rejection"], message);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnExecutionProviderRejectionArrived");
            }
        }

        /// <summary>
        /// Called when Locate Message is received from Order Execution Provider
        /// </summary>
        /// <param name="locateOrder">TradeHub LimitOrder containing Locate Message Info</param>
        /// <param name="applicationId">Unique Application ID</param>
        private void OnExecutionProviderLocateMessageArrived(LimitOrder locateOrder, string applicationId)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(
                        "Locate message received from: " + locateOrder.OrderExecutionProvider + " for: " +
                        applicationId, _type.FullName, "OnExecutionProviderLocateMessageArrived");
                }

                Dictionary<string, ClientMqParameters> strategyInfo;
                if (_strategiesInfoMap.TryGetValue(applicationId, out strategyInfo))
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Publishing Message for: " + applicationId, _type.FullName, "OnExecutionProviderLocateMessageArrived");
                    }

                    // Create EasyNetQ message to be published
                    Message<LimitOrder> message = new Message<LimitOrder>(locateOrder);

                    // Publish Messages on the exchange
                    PublishMessages(strategyInfo["Locate"], message);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnExecutionProviderLocateMessageArrived");
            }
        }

        /// <summary>
        /// Called when Position Message is received from Order Execution Provider
        /// </summary>
        /// <param name="position">TradeHub Position Message Info</param>
        private void OnPositionMessageArrived(Position position)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Position message received from: " + position.Provider, _type.FullName, "OnPositionMessageArrived");
                }
                Message<Position> message = new Message<Position>(position);

                //publish message to exchange
                PublishMessages(message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnPositionMessageArrived");
            }


        }

        #endregion

        #region Publish Message through MQ Server

        private void PublishMessages(Message<Position> message)
        {
            try
            {
                // Send messages to the MQ Server for publishing
                _mqServer.PublishMessages(message);

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Message sent to MQ Server for publishing: " + message.Body, _type.FullName,
                                 "PublishMessages");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes string messages to the MQ Exchange
        /// </summary>
        private void PublishMessages(ClientMqParameters strategyInfo, Message<string> message)
        {
            try
            {
                // Send messages to the MQ Server for publishing
                _mqServer.PublishMessages(strategyInfo, message);

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Message sent to MQ Server for publishing: " + message.Body, _type.FullName,
                                 "PublishMessages");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes Order messages to the MQ Exchange
        /// </summary>
        private void PublishMessages(ClientMqParameters strategyInfo, Message<Order> message)
        {
            try
            {
                // Send messages to the MQ Server for publishing
                _mqServer.PublishMessages(strategyInfo, message);

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Message sent to MQ Server for publishing: " + message.Body, _type.FullName,
                                 "PublishMessages");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes LimitOrder messages to the MQ Exchange
        /// </summary>
        private void PublishMessages(ClientMqParameters strategyInfo, Message<LimitOrder> message)
        {
            try
            {
                // Send messages to the MQ Server for publishing
                _mqServer.PublishMessages(strategyInfo, message);

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Message sent to MQ Server for publishing: " + message.Body, _type.FullName,
                                 "PublishMessages");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes Rejection messages to the MQ Exchange
        /// </summary>
        private void PublishMessages(ClientMqParameters strategyInfo, Message<Rejection> message)
        {
            try
            {
                // Send messages to the MQ Server for publishing
                _mqServer.PublishMessages(strategyInfo, message);

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Message sent to MQ Server for publishing: " + message.Body, _type.FullName,
                                 "PublishMessages");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes Execution Info messages to the MQ Exchange
        /// </summary>
        private void PublishMessages(ClientMqParameters strategyInfo, Message<Execution> message)
        {
            try
            {
                // Send messages to the MQ Server for publishing
                _mqServer.PublishMessages(strategyInfo, message);

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Message sent to MQ Server for publishing: " + message.Body, _type.FullName,
                                 "PublishMessages");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes Inquiry Response to the MQ Exchange
        /// </summary>
        private void PublishMessages(string strategyInfo, Message<InquiryResponse> message)
        {
            try
            {
                // Send messages to the MQ Server for publishing
                _mqServer.PublishMessages(strategyInfo, message);

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Message sent to MQ Server for publishing: " + message.Body, _type.FullName,
                                 "PublishMessages");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        #endregion

        /// <summary>
        /// Parses the Application Info details to extract info
        /// </summary>
        /// <param name="appId">Unique App ID</param>
        /// <param name="responseReply">Dictionary containg application reply routing keys info</param>
        /// <returns></returns>
        private Dictionary<string, ClientMqParameters> ReadAppInfoMessage(string appId, Dictionary<string, string> responseReply)
        {
            try
            {
                var clientMqParameters = new Dictionary<string, ClientMqParameters>();

                // Read Admin Info
                var parameters = new ClientMqParameters()
                {
                    AppId = appId,
                    ReplyTo = responseReply["Admin"]
                };
                clientMqParameters.Add("Admin", parameters);

                // Read Order Info
                parameters = new ClientMqParameters()
                {
                    AppId = appId,
                    ReplyTo = responseReply["Order"]
                };
                clientMqParameters.Add("Order", parameters);

                // Read Execution Info
                parameters = new ClientMqParameters()
                {
                    AppId = appId,
                    ReplyTo = responseReply["Execution"]
                };
                clientMqParameters.Add("Execution", parameters);

                // Read Rejection Info
                parameters = new ClientMqParameters()
                {
                    AppId = appId,
                    ReplyTo = responseReply["Rejection"]
                };
                clientMqParameters.Add("Rejection", parameters);

                // Read Locate Response Info
                parameters = new ClientMqParameters()
                {
                    AppId = appId,
                    ReplyTo = responseReply["Locate"]
                };
                clientMqParameters.Add("Locate", parameters);

                return clientMqParameters;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ReadAppInfoMessage");
                return null;
            }
        }

    }
}
