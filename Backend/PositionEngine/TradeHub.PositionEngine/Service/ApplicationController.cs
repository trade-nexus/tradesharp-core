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
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using EasyNetQ;
using EasyNetQ.Topology;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.Utility;
using TradeHub.Common.Core.ValueObjects;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.Inquiry;
using  TradeHub.PositionEngine.Configuration.Service;
using TradeHub.PositionEngine.ProviderGateway.Service;


namespace TradeHub.PositionEngine.Service
{
    /// <summary>
    /// Startup class which handles all the "Position" activities
    /// </summary>
    public class ApplicationController
    {
        private Type _type = typeof (ApplicationController);
        private PositionEngineMqServer _mqServer;
        private PositionMessageProcessor _messageProcessor;

        /// <summary>
        /// Keeps Track of all the connected apps routing key information
        /// KEY = Application ID
        /// VALUE = Dictionary containing Routing Info for corresponding messages
        /// </summary>
        private ConcurrentDictionary<string, Dictionary<string, ClientMqParameters>> _appsinfo;

        /// <summary>
        /// Keeps Track of all the connected apps provider request information
        /// KEY = Application ID
        /// VALUE = List containing Provider request
        /// </summary>
        private Dictionary<string, List<string>> _providerPositionsRequest; 

        public ApplicationController(PositionEngineMqServer mqServer,PositionMessageProcessor messageProcessor)
        {
            _appsinfo=new ConcurrentDictionary<string, Dictionary<string, ClientMqParameters>>();
            _providerPositionsRequest=new Dictionary<string, List<string>>();
            _mqServer = mqServer;
            _messageProcessor = messageProcessor;
        }

        /// <summary>
        ///start the server
        /// </summary>
        public void StartServer()
        {
            //connect mq server
            _mqServer.Connect();

            //register mq server events
            RegisterMqServerEvents();
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

        /// <summary>
        /// registering MQ Server Events
        /// </summary>
        private void RegisterMqServerEvents()
        {
            try
            {
                _mqServer.AppInfoReceived += _mqServer_AppInfoReceived;
                _mqServer.PositionMessageArrived += _mqServer_PositionMessageArrived;
                _mqServer.ProviderRequestReceived += _mqServer_ProviderRequestReceived;
                _mqServer.InquiryRequestReceived += _mqServer_InquiryRequestReceived;

            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "RegisterMqServerEvents");
            }
        }

        //inquiry request received from Client
        void _mqServer_InquiryRequestReceived(IMessage<InquiryMessage> inquiryMessage)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Inquiry info received from MQ Server: " + inquiryMessage.Properties.AppId, _type.FullName, "_mqServer_InquiryRequestReceived");
                }

                // Create the Inquiry Respons to be sent
                InquiryResponse inquiryResponse = new InquiryResponse();

                if (inquiryMessage.Body.Type.Equals(Common.Core.Constants.InquiryTags.AppID))
                {
                    string id = ApplicationIdGenerator.NextId();
                    inquiryResponse.Type = Common.Core.Constants.InquiryTags.AppID;
                    inquiryResponse.AppId = id;
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
                Logger.Error(exception, _type.FullName, "_mqServer_InquiryRequestReceived");
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

        /// <summary>
        /// Provider request received
        /// </summary>
        private void _mqServer_ProviderRequestReceived(IMessage<string> obj)
        {

            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Provider Request received from MQ Server: " + obj.Properties.AppId + obj.Body,
                    _type.FullName, "_mqServer_ProviderRequestReceived");
            }
            lock (this)
            {
                // _messageProcessor.ProviderRequestReceived(obj.Body,obj.Properties.AppId);
                if (obj.Body.Contains("unsubscribe"))
                {
                    _providerPositionsRequest.Remove(obj.Properties.AppId);
                    Dictionary<string, ClientMqParameters> temp;
                    _appsinfo.TryRemove(obj.Properties.AppId, out temp);
                    return;

                }
                if (_providerPositionsRequest.ContainsKey(obj.Properties.AppId))
                {
                    List<string> list = _providerPositionsRequest[obj.Properties.AppId];
                    if (list.Contains(obj.Body))
                    {
                        Logger.Info(
                            string.Format("This provider {0} is already registered against appID={1}", obj.Body,
                                obj.Properties.AppId), _type.FullName, "_mqServer_ProviderRequestReceived");
                    }
                    else
                    {
                        list.Add(obj.Body);
                    }
                }
                else
                {
                    List<string> list = new List<string>();
                    list.Add(obj.Body);
                    _providerPositionsRequest.Add(obj.Properties.AppId, list);
                }
            }
        }

        /// <summary>
        /// App info received from client
        /// </summary>
        void _mqServer_AppInfoReceived(IMessage<Dictionary<string, string>> responseDetails)
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
                    // Update apps Map
                    _appsinfo.AddOrUpdate(responseDetails.Properties.AppId, clientMqParameters,
                                               (key, value) => clientMqParameters);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnAppInfoReceived");
            }
           
        }
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

                // Read Position Info
                var parameters = new ClientMqParameters()
                {
                    AppId = appId,
                    ReplyTo = responseReply["Position"]
                };
                clientMqParameters.Add("Position", parameters);
               return clientMqParameters;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ReadAppInfoMessage");
                return null;
            }
        }

        /// <summary>
        ///Position Messages Arrived
        /// </summary>
        void _mqServer_PositionMessageArrived(IMessage<Position> obj)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Application Info received from MQ Server: " + obj.Properties.Type, _type.FullName, "_mqServer_PositionMessageArrived");
                }
                _messageProcessor.OnPositionMessageRecieved(obj.Body);
                CheckProviders(obj.Body);
                
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "_mqServer_PositionMessageArrived");
            }
        }

        /// <summary>
        ///Check subscribed apps and providers and publish position messages
        /// </summary>
        void CheckProviders(Position position)
        {
            try
            {
                foreach (var pair in _appsinfo)
                {
                    foreach (var subscribedprovider in _providerPositionsRequest)
                    {
                        if (subscribedprovider.Value.Count > 0)
                        {
                            List<string> list = subscribedprovider.Value;
                            foreach (var provider in list)
                            {
                                if (provider == position.Provider)
                                {
                                    Message<Position> positionMessage = new Message<Position>(position);
                                    PublishPositionMessages(pair.Value["Position"], positionMessage);

                                }
                            }
                        }
                    }

                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "CheckProviders");
            }
        }

        /// <summary>
        /// Publishes Poition messages to the MQ Exchange
        /// </summary>
        private void PublishPositionMessages(ClientMqParameters strategyInfo, Message<Position> message)
        {
            try
            {
                // Send messages to the MQ Server for publishing
                _mqServer.PublishPositionMessages(strategyInfo.ReplyTo, message);

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
    }
}
