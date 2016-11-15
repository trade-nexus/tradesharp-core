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
using Spring.Context.Support;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.Utility;
using TradeHub.Common.Core.ValueObjects;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.Inquiry;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataEngine.Configuration.Service;
using TradeHub.MarketDataEngine.MarketDataProviderGateway.Service;
using TradeHub.MarketDataEngine.MarketDataProviderGateway.Utility;
using Constants = TradeHub.Common.Core.Constants;

namespace TradeHub.MarketDataEngine.Server.Service
{
    /// <summary>
    /// Startup class which handles all the "MarketDataEngine.Server" activities
    /// </summary>
    public class ApplicationController
    {
        private Type _type = typeof (ApplicationController);

        // Holds reference to the MQ server Class
        private MqServer _mqServer;
        
        // Contains methods for processing Incoming and Outgoing Messages
        private MessageProcessor _messageProcessor;

        private ConcurrentDictionary<string, Dictionary<string,ClientMqParameters>> _strategiesMap;

        public ApplicationController(MqServer mqServer, MessageProcessor messageProcessor)
        {
            _strategiesMap = new ConcurrentDictionary<string, Dictionary<string, ClientMqParameters>>();

            _mqServer = mqServer;
            _messageProcessor = messageProcessor;

            // Hook MQ Server Events
            RegisterMqServerEvents();

            // Hooks MessageProcessor Events
            RegisterMessageProcessorEvents();
        }

        /// <summary>
        /// Hooks Mq Server Events to get incoming messages from strategies
        /// </summary>
        private void RegisterMqServerEvents()
        {
            try
            {
                _mqServer.AppInfoReceived += OnAppInfoReceived;
                _mqServer.DisconnectApplication += OnDisconnectApplicationReceived;
                _mqServer.LogonRequestReceived += OnLogonRequestReceived;
                _mqServer.LogoutRequestReceived += OnLogoutRequestReceived;
                _mqServer.SubscribeRequestReceived += OnSubscribeRequestReceived;
                _mqServer.UnsubscribeRequestReceived += OnUnsubscribeRequestReceived;
                _mqServer.LiveBarSubscribeRequestReceived += OnLiveBarSubscribeRequestReceived;
                _mqServer.LiveBarUnsubscribeRequestReceived += OnLiveBarUnsubscribeRequestReceived;
                _mqServer.HistoricBarDataRequestReceived += OnHistoricBarDataRequestReceived;
                _mqServer.InquiryRequestReceived += OnInquiryRequestReceived;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "RegisterMqServerEvents");
            }
        }

        /// <summary>
        /// Hooks MessageProcessor Events
        /// </summary>
        private void RegisterMessageProcessorEvents()
        {
            try
            {
                _messageProcessor.LogonArrived += OnGatewayLogonArrived;
                _messageProcessor.LogoutArrived += OnGatewayLogoutArrived;
                _messageProcessor.TickArrived += OnTickArrived;
                _messageProcessor.LiveBarArrived += OnLiveBarArrived;
                _messageProcessor.HistoricalBarsArrived += OnHistoricBarsArrived;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "RegisterMessageProcessorEvents");
            }  
        }

        /// <summary>
        /// Starts the Application and sets up the MQ Server connection
        /// </summary>
        public void StartServer()
        {
            try
            {
                // Configure Logging level
                Logger.SetLoggingLevel();

                if(Logger.IsInfoEnabled)
                {
                    Logger.Info("Starting Market Data Engine Service", _type.FullName, "StartServer");
                }

                // Start Server
                _mqServer.Connect();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "StartServer");
            }
        }

        /// <summary>
        /// Stops the application and closes the MQ Server connection
        /// </summary>
        public void StopServer()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Closing MQ Server connection", _type.FullName, "StopServer");
                }

                // Disconnect MQ Server
                _mqServer.Disconnect();

                // Stop incoming message processing
                _messageProcessor.StopProcessing();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "StopServer");
            }
        }

        #region MQ Server Event Handling

        /// <summary>
        /// Called when App Info message is received from the MQ Server
        /// </summary>
        /// <param name="responseDetails">Dictionary containing app info for reply response</param>
        private void OnAppInfoReceived(IMessage<Dictionary<string, string>> responseDetails)
        {
            try
            {
                lock (_messageProcessor)
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("App Info recieved from the MQ Server: " + responseDetails.Properties.AppId,
                                     _type.FullName,
                                     "OnAppInfoReceived");
                    }
                    
                    // Get Application Routing Key info
                    var clientMqParameters = ReadAppInfoMessage(responseDetails.Properties.AppId, responseDetails.Body);
                    
                    if (clientMqParameters != null)
                    {
                        // Update Strategies Map
                        _strategiesMap.AddOrUpdate(responseDetails.Properties.AppId, clientMqParameters,
                                                   (key, value) => clientMqParameters);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnAppInfoReceived");
            }
        }

        /// <summary>
        /// Called when Logon Message is received from the MQ Server
        /// </summary>
        public void OnLogonRequestReceived(IMessage<Login> msg)
        {
            try
            {
                lock (_messageProcessor)
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Logon message recieved from the MQ Server: " + msg.Properties.AppId,
                                     _type.FullName, "OnLogonRequestRecieved");
                    }

                    // Process incoming messages
                    _messageProcessor.OnLogonMessageRecieved(msg.Body, msg.Properties.AppId);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnLogonRequestRecieved");
            }
        }

        /// <summary>
        /// Called when Logout Message is received from the MQ Server
        /// </summary>
        public void OnLogoutRequestReceived(IMessage<Logout> msg)
        {
            try
            {
                lock (_messageProcessor)
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Logout message recieved from the MQ Server: " + msg.Properties.AppId,
                                     _type.FullName,
                                     "OnLogoutRequestRecieved");
                    }

                    // Process incoming Messages
                    _messageProcessor.OnLogoutMessageRecieved(msg.Body, msg.Properties.AppId);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnLogoutRequestRecieved");
            }
        }

        /// <summary>
        /// Called when Subscribe Message is received from the MQ Server
        /// </summary>
        public void OnSubscribeRequestReceived(IMessage<Subscribe> msg)
        {
            try
            {
                lock (_messageProcessor)
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Subscribe request recieved from the MQ Server: " + msg.Properties.AppId,
                                     _type.FullName, "OnSubscribeRequestRecieved");
                    }

                    // Process incoming message
                    _messageProcessor.OnTickSubscribeRecieved(msg.Body, msg.Properties.AppId);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnSubscribeRequestRecieved");
            }
        }

        /// <summary>
        /// Called when Unsubscribe Message is received from the MQ Server
        /// </summary>
        public void OnUnsubscribeRequestReceived(IMessage<Unsubscribe> msg)
        {
            try
            {
                lock (_messageProcessor)
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Subscribe request recieved from the MQ Server: " + msg.Properties.AppId,
                                     _type.FullName, "OnUnsubscribeRequestRecieved");
                    }

                    // Process incoming message
                    _messageProcessor.OnTickUnsubscribeRecieved(msg.Body, msg.Properties.AppId);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnUnsubscribeRequestRecieved");
            }
        }

        /// <summary>
        /// Called when Live Bar Subscribe Request Message is received from the MQ Server
        /// </summary>
        public void OnLiveBarSubscribeRequestReceived(IMessage<BarDataRequest> msg)
        {
            try
            {
                lock (_messageProcessor)
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Live Bar Data request recieved from the MQ Server: " + msg.Properties.AppId,
                                     _type.FullName, "OnLiveBarSubscribeRequestRecieved");
                    }

                    // Process incoming message
                    _messageProcessor.OnLiveBarSubscribeRequestRecieved(msg.Body, msg.Properties.AppId);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnLiveBarSubscribeRequestRecieved");
            }
        }

        /// <summary>
        /// Called when Live Bar Unsubscribe Request Message is received from the MQ Server
        /// </summary>
        public void OnLiveBarUnsubscribeRequestReceived(IMessage<BarDataRequest> msg)
        {
            try
            {
                lock (_messageProcessor)
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Live Bar Unsubscribe request recieved from the MQ Server: " + msg.Properties.AppId,
                                     _type.FullName, "OnLiveBarUnsubscribeRequestRecieved");
                    }

                    // Process incoming message
                    _messageProcessor.OnLiveBarUnsubscribeRequestRecieved(msg.Body, msg.Properties.AppId);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnLiveBarUnsubscribeRequestRecieved");
            }
        }

        /// <summary>
        /// Called when Historic Bar Data Request Message is received from the MQ Server
        /// </summary>
        public void OnHistoricBarDataRequestReceived(IMessage<HistoricDataRequest> msg)
        {
            try
            {
                lock (_messageProcessor)
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Historic Bar Data request recieved from the MQ Server: " + msg.Properties.AppId,
                                     _type.FullName, "OnHistoricBarDataRequestRecieved");
                    }

                    // Process incoming message
                    _messageProcessor.OnHistoricBarDataRequestRecieved(msg.Body, msg.Properties.AppId);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnHistoricBarDataRequestRecieved");
            }
        }

        /// <summary>
        /// Called when Inquiry Request Message is received from the MQ Server
        /// </summary>
        public void OnInquiryRequestReceived(IMessage<InquiryMessage> msg)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Inquiry request recieved from the MQ Server: " + msg.Properties.AppId + " for: " + msg.Body.Type,
                                 _type.FullName, "OnInquiryRequestReceived");
                }

                if (msg.Body.Type.Equals(Constants.InquiryTags.AppID))
                {
                    // Create and send a new unique Application ID
                    SendApplictionId(msg);
                }
                else if(msg.Body.Type.Equals(Constants.InquiryTags.MarketDataProviderInfo))
                {
                    // Send the required market data provider info to the requesting application
                    SendMarketDataProviderInfo(msg);
                }
                else if (msg.Body.Type.Equals(Constants.InquiryTags.DisconnectClient))
                {
                    // Remove the given applications info from Internal Maps
                    OnDisconnectApplicationReceived(msg.Properties.AppId);
                }

            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnInquiryRequestReceived");
            }
        }

        /// <summary>
        /// Called when Application Disconnect is received from the MQ Server
        /// </summary>
        public void OnDisconnectApplicationReceived(string applicationId)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Applicaiton Disconnect recieved from the MQ Server: " + applicationId,
                                 _type.FullName, "OnDisconnectApplicationReceived");
                }

                Dictionary<string, ClientMqParameters> responseMap;
                _strategiesMap.TryRemove(applicationId, out responseMap);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnDisconnectApplicationReceived");
            }
        }

        #endregion

        #region MessageProcessor Event handling

        /// <summary>
        /// Raised when Logon is recived from MessageProcessor
        /// </summary>
        private void OnGatewayLogonArrived(string strategyId, string marketDataProvider)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Logon Event recieved from MessageProcessor.", _type.FullName, "OnGatewayLogonArrived");
                }

                Dictionary<string, ClientMqParameters> strategyInfo;
                if(_strategiesMap.TryGetValue(strategyId, out strategyInfo))
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Publishing Message for: " + strategyId, _type.FullName, "OnGatewayLogonArrived");
                    }

                    // Create EasyNetQ message to be published
                    Message<string> message = new Message<string>("Logon-" + marketDataProvider);

                    // Publish Messages on the exchange
                    PublishMessages(strategyInfo["Admin"], message);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnGatewayLogonArrived");
            }
        }

        /// <summary>
        /// Raised when Logout is recieved from MessageProcessor
        /// </summary>
        private void OnGatewayLogoutArrived(string strategyId, string marketDataProvider)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    //Logger.Debug("Logout Event recieved from MessageProcessor.", _type.FullName, "OnGatewayLogoutArrived");
                }

                Dictionary<string, ClientMqParameters> strategyInfo;
                if (_strategiesMap.TryGetValue(strategyId, out strategyInfo))
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Publishing Message for: " + strategyId, _type.FullName, "OnGatewayLogoutArrived");
                    }

                    // Create EasyNetQ message to be published
                    Message<string> message = new Message<string>("Logout-" + marketDataProvider);

                    // Publish Messages on the exchange
                    PublishMessages(strategyInfo["Admin"], message);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnGatewayLogoutArrived");
            }
        }

        /// <summary>
        /// Raised when Tick is recieved from MessageProcessor
        /// </summary>
        private void OnTickArrived(Tick tick, string strategyId)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Tick Event recieved from MessageProcessor.", _type.FullName, "OnTickArrived");
                }

              //  Dictionary<string, ClientMqParameters> strategyInfo;
             //   if (_strategiesMap.TryGetValue(strategyId, out strategyInfo))
              //  {
                    //if (Logger.IsDebugEnabled)
                    //{
                    //    Logger.Debug("Publishing Message for: " + strategyId, _type.FullName, "OnTickArrived");
                    //}

                    // Create EasyNetQ message to be published
                //    Message<Tick> message = new Message<Tick>(tick);

                    // Publish Messages on the exchange
                   // PublishMessages(strategyInfo["Tick"], message);
                    _mqServer.PublishTickMessageToZeroMq(tick);
               // }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnTickArrived");
            }
        }

        /// <summary>
        /// Raised when Live Bar is recieved from MessageProcessor
        /// </summary>
        private void OnLiveBarArrived(Bar bar, string strategyId)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Live Bar Event recieved from MessageProcessor.", _type.FullName, "OnLiveBarArrived");
                }

                //Dictionary<string, ClientMqParameters> strategyInfo;
              //  if (_strategiesMap.TryGetValue(strategyId, out strategyInfo))
              //  {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Publishing Message for: " + strategyId, _type.FullName, "OnLiveBarArrived");
                    }

                    // Create EasyNetQ message to be published
                   // Message<Bar> message = new Message<Bar>(bar);

                    // Publish Messages on the exchange
                    //PublishMessages(strategyInfo["LiveBar"], message);
                _mqServer.PublishBarToZeroMq(bar);
                
              //  }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnLiveBarArrived");
            }
        }

        /// <summary>
        /// Raised when Historic Bars are recieved from MessageProcessor
        /// </summary>
        private void OnHistoricBarsArrived(HistoricBarData historicBarData, string strategyId)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Historic Bars Event recieved from MessageProcessor.", _type.FullName, "OnHistoricBarsArrived");
                }

                Dictionary<string, ClientMqParameters> strategyInfo;
                if (_strategiesMap.TryGetValue(strategyId, out strategyInfo))
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Publishing Message for: " + strategyId, _type.FullName, "OnHistoricBarsArrived");
                    }

                    // Create EasyNetQ message to be published
                    Message<HistoricBarData> message = new Message<HistoricBarData>(historicBarData);

                    // Publish Messages on the exchange
                    PublishMessages(strategyInfo["HistoricBar"], message);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnHistoricBarsArrived");
            }
        }

        #endregion

        #region Publish Messages

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
        /// Publishes Tick messages to the MQ Exchange
        /// </summary>
        private void PublishMessages(ClientMqParameters strategyInfo, Message<Tick> message)
        {
            try
            {
                // Send messages to the MQ Server for publishing
                _mqServer.PublishMessages(strategyInfo, message);

                //if (Logger.IsDebugEnabled)
                //{
                //    Logger.Debug("Message sent to MQ Server for publishing: " + message.Body, _type.FullName,
                //                 "PublishMessages");
                //}
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes Live Bars to the MQ Exchange
        /// </summary>
        private void PublishMessages(ClientMqParameters strategyInfo, Message<Bar> message)
        {
            try
            {
                // Send messages to the MQ Server for publishing
                _mqServer.PublishMessages(strategyInfo, message);

                //if (Logger.IsDebugEnabled)
                //{
                //    Logger.Debug("Message sent to MQ Server for publishing: " + message.Body, _type.FullName,
                //                 "PublishMessages");
                //}
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes Historic Bars to the MQ Exchange
        /// </summary>
        private void PublishMessages(ClientMqParameters strategyInfo, Message<HistoricBarData> message)
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
        /// Publishes Inquiry to the MQ Exchange
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
        /// Provides new unique Application ID in response to inquiry request
        /// </summary>
        /// <param name="msg">IMessage containg Inquiry request</param>
        private void SendApplictionId(IMessage<InquiryMessage> msg)
        {
            try
            {
                // Create the Inquiry Respons to be sent
                InquiryResponse inquiryResponse = new InquiryResponse();

                string id = ApplicationIdGenerator.NextId();
                inquiryResponse.Type = Constants.InquiryTags.AppID;
                inquiryResponse.AppId = id;

                // Create EasyNetQ message to be published
                Message<InquiryResponse> message = new Message<InquiryResponse>(inquiryResponse);

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Publishing Inquiry Response for: " + msg.Body.Type, _type.FullName, "OnInquiryRequestReceived");
                }

                // Publish Messages on the exchange
                PublishMessages(msg.Properties.ReplyTo, message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendApplictionId");
            }
        }

        /// <summary>
        /// Provides functional information about the requested market data provider
        /// </summary>
        /// <param name="msg">IMessage containg Inquiry request</param>
        private void SendMarketDataProviderInfo(IMessage<InquiryMessage> msg)
        {
            try
            {
                // Create the Inquiry Respons to be sent
                InquiryResponse inquiryResponse =
                    _messageProcessor.GetMarketDataProviderInfo(msg.Body.MarketDataProvider);

                if (inquiryResponse==null)
                {
                    Logger.Error("No information available for: " + msg.Body.MarketDataProvider, _type.FullName, "SendMarketDataProviderInfo");
                    return;
                }

                inquiryResponse.Type = Constants.InquiryTags.MarketDataProviderInfo;

                // Create EasyNetQ message to be published
                Message<InquiryResponse> message = new Message<InquiryResponse>(inquiryResponse);

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Publishing Inquiry Response for: " + msg.Body.Type, _type.FullName, "OnInquiryRequestReceived");
                }

                // Publish Messages on the exchange
                PublishMessages(msg.Properties.ReplyTo, message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendMarketDataProviderInfo");
            }
        }

        /// <summary>
        /// Parses the Application Info details to extract info
        /// </summary>
        /// <param name="appId">Unique App ID</param>
        /// <param name="responseReply">Dictionary containg application reply routing keys info</param>
        /// <returns></returns>
        private Dictionary<string, ClientMqParameters> ReadAppInfoMessage(string appId, Dictionary<string,string> responseReply)
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

                // Read Tick Info
                parameters = new ClientMqParameters()
                {
                    AppId = appId,
                    ReplyTo = responseReply["Tick"]
                };
                clientMqParameters.Add("Tick", parameters);

                // Read LiveBar Info
                parameters = new ClientMqParameters()
                {
                    AppId = appId,
                    ReplyTo = responseReply["LiveBar"]
                };
                clientMqParameters.Add("LiveBar", parameters);

                // Read HistoricBar Info
                parameters = new ClientMqParameters()
                {
                    AppId = appId,
                    ReplyTo = responseReply["HistoricBar"]
                };
                clientMqParameters.Add("HistoricBar", parameters);

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