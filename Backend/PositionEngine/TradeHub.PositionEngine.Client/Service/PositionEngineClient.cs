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
using System.Text;
using System.Threading.Tasks;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.ValueObjects.Inquiry;
using TradeHub.PositionEngine.Client.Utility;
using TradeHubConstants = TradeHub.Common.Core.Constants;

namespace TradeHub.PositionEngine.Client.Service
{
    public class PositionEngineClient
    {
        private Type _type = typeof (PositionEngineClient);

        // ReSharper disable InconsistentNaming

        private event Action<InquiryResponse> _inquiryResponseArrived;
        private event Action<Position> _positionArrived;
        private event Action _serverConnected;

        private string _orderExecutionServer;

        // ReSharper restore InconsistentNaming

        #region events properties

        public event Action<Position> PositionArrived
        {
            add
            {
                if (_positionArrived == null)
                    _positionArrived += value;
            }
            remove
            {
                _positionArrived -= value;
            }
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
        private PositionEngineClientMqServer _mqServer;


        /// <summary>
        /// Returns Unique Application ID
        /// </summary>
        public string AppId
        {
            get { return _appId; }
        }

        public PositionEngineClient(string server = "PEServer.xml", string client = "PEClientMqConfig.xml")
        {
            // Get Configuration details
            var configurationReader = new ConfigurationReader(server, client);

            _mqServer = new PositionEngineClientMqServer(configurationReader.PeMqServerparameters,
                configurationReader.ClientMqParameters);
        }

        /// <summary>
        /// Subscribe provider positions
        /// </summary>
        /// <param name="provider"></param>
        public void SubscribeProviderPosition(string provider)
        {
            //if(_serverConnected!=null)
            
            _mqServer.SubscribeProviderPosition(provider);
            
        }

        /// <summary>
        /// Un-Subscribe provider positions
        /// </summary>
        /// <param name="provider"></param>
        public void UnSubscribeProviderPosition(string provider)
        {
            // if(_serverConnected!=null)

            _mqServer.UnSubscribeProviderPosition(provider);

        }

        /// <summary>
        /// Starts Communication link with Position Engine Server
        /// </summary>
        /// <param name="server">Broker to use for Position Information</param>
        public void Initialize(string server)
        {
            try
            {
                // Register Events
                RegisterClientMqServerEvents();

                // Request for Unique App ID
                RequestAppId();

                _orderExecutionServer = server;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "Intialize");
            }
        }

        /// <summary>
        /// Starts Communication link with Position Engine Server
        /// </summary>
        public void Initialize()
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
                Logger.Error(exception, _type.FullName, "Intialize");
            }
        }

        /// <summary>
        /// Requests Position Engine for Unique Application ID
        /// </summary>
        private void RequestAppId()
        {
           try
                {
                    InquiryMessage inquiry = new InquiryMessage();
                    inquiry.Type = Common.Core.Constants.InquiryTags.AppID;

                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Sending inquiry request message for: " + inquiry.Type, _type.FullName,
                                     "RequestAppId");
                    }

                    // Send Message through the MQ Server
                    _mqServer.SendInquiryMessage(inquiry);
                }
                catch (Exception exception)
                {
                    Logger.Error(exception, _type.FullName, "RequestAppId");
                }
           
        }

        /// <summary>
        /// Hooks Client MQServer events
        /// </summary>
        private void RegisterClientMqServerEvents()
        {
            UnregisterClientMqServerEvents();
            _mqServer.InquiryResponseArrived += _mqServer_InquiryResponseArrived;
            _mqServer.PositionArrived += _mqServer_PositionArrived;
        }

        /// <summary>
        /// postion arrived from mq server
        /// </summary>
        /// <param name="obj"></param>
        private void _mqServer_PositionArrived(Position obj)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Position recieved from Position Engine: " + obj,
                                 _type.FullName, "_mqServer_PositionArrived");
                }

                if (_positionArrived != null)
                    _positionArrived(obj);

            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "_mqServer_PositionArrived");
            }
           
        }

        /// <summary>
        /// Inquiry Response Arrived from Position Engine
        /// </summary>
        /// <param name="inquiryResponse"></param>
        private void _mqServer_InquiryResponseArrived(InquiryResponse inquiryResponse)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Inquiry Response recieved from Order Execution Engine: " + inquiryResponse,
                        _type.FullName, "OnInquiryResponseArrived");
                }

                if (inquiryResponse.Type.Equals(TradeHubConstants.InquiryTags.AppID))
                {
                    _appId = inquiryResponse.AppId;

                    // Start MQ Server
                    _mqServer.Connect(_appId);

                    // Send Application Info
                    _mqServer.SendAppInfoMessage(_appId);

                    //subscribe to provider
                    if (!string.IsNullOrEmpty(_orderExecutionServer))
                    {
                        SubscribeProviderPosition(_orderExecutionServer);
                    }

                    // Raise Event to Notify Listeners that PE-Client is ready to entertain request
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
                Logger.Error(exception, _type.FullName, "_mqServer_InquiryResponseArrived");
            }

        }

        /// <summary>
        /// Unhooks Client MQServer events
        /// </summary>
        private void UnregisterClientMqServerEvents()
        {
            _mqServer.InquiryResponseArrived -= _mqServer_InquiryResponseArrived;
            _mqServer.PositionArrived -= _mqServer_PositionArrived;
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
                   
                    // Disconnect MQ Server
                    _mqServer.Disconnect();

                    // Unhook events
                    UnregisterClientMqServerEvents();
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "Shutdown");
            }
        }
     
    }
}
