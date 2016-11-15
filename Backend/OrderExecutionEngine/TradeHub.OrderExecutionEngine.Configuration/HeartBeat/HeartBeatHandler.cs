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
using TraceSourceLogger;
using TradeHub.Common.Core.ValueObjects.Heartbeat;

namespace TradeHub.OrderExecutionEngine.Configuration.HeartBeat
{
    /// <summary>
    /// Keeps tracks incoming Heartbeats and informs the parent upon expected disconnection
    /// </summary>
    internal class HeartBeatHandler
    {
        private Type _type = typeof(HeartBeatHandler);

        private readonly int _heartbeatValidationInterval;
        private readonly int _heartbeatResponseInterval;

        #region Events

        // ReSharper Disable InconsistentNaming
        private event Action<string> _applicationDisconnect;
        private event Action<HeartbeatMessage> _responseHeartbeat;
        // ReSharper Enable InconsistentNaming

        /// <summary>
        /// Raised when HeartBeat threshold expires for the given application
        /// </summary>
        public event Action<string> ApplicationDisconnect
        {
            add
            {
                if (_applicationDisconnect == null)
                {
                    _applicationDisconnect += value;
                }
            }
            remove { _applicationDisconnect -= value; }
        }

        /// <summary>
        /// Raised to send Heartbeat message from MDE to connecting applications
        /// </summary>
        public event Action<HeartbeatMessage> ResponseHeartbeat
        {
            add
            {
                if (_responseHeartbeat == null)
                {
                    _responseHeartbeat += value;
                }
            }
            remove { _responseHeartbeat -= value; }
        }

        #endregion

        /// <summary>
        /// Keeps track of each strategy's HeartBeat Processor
        /// Key = Application ID
        /// Value = <see cref="HeartBeatProcessor"/>
        /// </summary>
        private ConcurrentDictionary<string, HeartBeatProcessor> _heartBeatProcessors;
        
        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="heartbeatValidationInterval">Time after which if heartbeat doesnot arrive connection will be closed</param>
        /// <param name="heartbeatResponseInterval">Timer after which server will send Heartbeat to connecting Applications</param>
        public HeartBeatHandler(int heartbeatValidationInterval = 10000, int heartbeatResponseInterval = 120000)
        {
            _heartbeatValidationInterval = heartbeatValidationInterval;
            _heartbeatResponseInterval = heartbeatResponseInterval;
            _heartBeatProcessors = new ConcurrentDictionary<string, HeartBeatProcessor>();
        }

        /// <summary>
        /// Handles the new incoming heartbeat 
        /// </summary>
        /// <param name="heartbeat">TradeHub Heartbeat Message</param>
        public void Update(HeartbeatMessage heartbeat)
        {
            try
            {
                HeartBeatProcessor processor;
                if (!_heartBeatProcessors.TryGetValue(heartbeat.ApplicationId, out processor))
                {
                    processor = new HeartBeatProcessor(heartbeat, _heartbeatValidationInterval,
                                                       _heartbeatResponseInterval);

                    // Register Heartbeat Processor Events
                    RegisterProcessorEvents(processor);

                    // Update Heartbeat Processors Map
                    _heartBeatProcessors.TryAdd(heartbeat.ApplicationId, processor);

                    // Add MDE-Server Heartbeat Interval
                    heartbeat.HeartbeatInterval = _heartbeatResponseInterval;

                    // Send Heartbeat Response
                    OnProcessorResponse(heartbeat);
                }

                // Update Heartbeat Processor
                processor.Update();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "Update");
            }
        }

        /// <summary>
        /// Raised when Disconnect is received from the <see cref="HeartBeatProcessor"/>
        /// </summary>
        /// <param name="applicationId">Unique Application ID</param>
        private void OnProcessorDisconnect(string applicationId)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Disconnect Event receieved for: " + applicationId, _type.FullName,
                                "OnProcessorDisconnect");
                }

                HeartBeatProcessor processor;
                if (_heartBeatProcessors.TryRemove(applicationId, out processor))
                {
                    // Unhook Heartbeat Processor Events
                    UnregisterProcessorEvents(processor);
                }

                // Notify listeners about given application disconnect
                if (_applicationDisconnect != null)
                {
                    _applicationDisconnect(applicationId);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnProcessorDisconnect");
            }
        }

        /// <summary>
        /// Raised to send Response Heartbeat to connection Application
        /// </summary>
        /// <param name="heartbeat">TradeHub Heartbeat Message</param>
        private void OnProcessorResponse(HeartbeatMessage heartbeat)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Heartbeat Response Event received for: " + heartbeat.ApplicationId, _type.FullName,
                                 "OnProcessorResponse");
                }

                // Notify Listener to Send Server Heartbeat
                if (_responseHeartbeat != null)
                {
                    _responseHeartbeat(heartbeat);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnProcessorResponse");
            }
        }


        /// <summary>
        /// Hooks HeartBeatProcessor Events
        /// </summary>
        private void RegisterProcessorEvents(HeartBeatProcessor processor)
        {
            processor.Disconnect += OnProcessorDisconnect;
            processor.Response += OnProcessorResponse;
        }

        /// <summary>
        /// Unhooks HeartBeat Processor Events
        /// </summary>
        private void UnregisterProcessorEvents(HeartBeatProcessor processor)
        {
            processor.Disconnect -= OnProcessorDisconnect;
            processor.Response -= OnProcessorResponse;
        }
    }
}
