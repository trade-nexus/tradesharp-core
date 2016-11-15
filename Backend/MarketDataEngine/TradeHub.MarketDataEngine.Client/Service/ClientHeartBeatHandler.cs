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
using System.Timers;
using Spring.Context.Support;
using TraceSourceLogger;
using TradeHub.Common.Core.ValueObjects.Heartbeat;

namespace TradeHub.MarketDataEngine.Client.Service
{
    /// <summary>
    /// Responsible for creating heartbeat to keep the connection alive
    /// </summary>
    internal class ClientHeartBeatHandler
    {
        private Type _type = typeof (ClientHeartBeatHandler);
        private AsyncClassLogger _asyncClassLogger;

        /// <summary>
        /// Duration between successive Heartbeats in milliseconds
        /// </summary>
        private int _heartbeatInterval;

        /// <summary>
        /// Timer responsible for generating periodic heartbeat messages
        /// </summary>
        private readonly Timer _heartbeatTimer;

        /// <summary>
        /// Timer responsible for keeping Track of Server Heartbeat Response
        /// </summary>
        private readonly Timer _validationTimer;

        /// <summary>
        /// Heartbeat Message to be Published
        /// </summary>
        private HeartbeatMessage _heartbeatMessage;

        private int _heartbeatValidationInterval = 10000;

        /// <summary>
        /// Notifies listeners to send new Heartbeat message
        /// </summary>
        private event Action<HeartbeatMessage> _sendHeartbeat;

        /// <summary>
        /// Notifies listeners about MDE disconnection
        /// </summary>
        private event Action _serverDisconnected; 

        /// <summary>
        /// Notifies listener to send new Heartbeat message
        /// </summary>
        public event Action<HeartbeatMessage> SendHeartbeat
        {
            add
            {
                if (_sendHeartbeat == null)
                {
                    _sendHeartbeat += value;
                }
            }
            remove { _sendHeartbeat -= value; }
        }

        /// <summary>
        /// Notifies listener about MDE disconnection
        /// </summary>
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

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="asyncClassLogger">Class Logger Instance to be used</param>
        /// <param name="applicationId">Unique Application ID</param>
        /// <param name="heartbeatInterval">Duration between successive Heartbeats in milliseconds</param>
        public ClientHeartBeatHandler(AsyncClassLogger asyncClassLogger, string applicationId, int heartbeatInterval = 60000)
        {
            _asyncClassLogger = asyncClassLogger;

            _heartbeatInterval = heartbeatInterval;

            // Initialize Heartbeat Message
            _heartbeatMessage= new HeartbeatMessage();
            // Set Applicaiton ID
            _heartbeatMessage.ApplicationId = applicationId;
            // Add/Update Heartbeat Message Info
            _heartbeatMessage.HeartbeatInterval = _heartbeatInterval;

            // Inialize Timers
            _heartbeatTimer = new Timer();
            _validationTimer = new Timer();

            // Set Heartbeat Interval
            _heartbeatTimer.Interval = _heartbeatInterval;

            _heartbeatTimer.Elapsed += OnHeartbeatTimerElapsed;
            _validationTimer.Elapsed += OnServerValidationTimerElapsed;
        }

        /// <summary>
        /// Starts generating Heartbeat requests after specified intervals
        /// </summary>
        public void StartHandler()
        {
            // Start Heartbeat Timer
            _heartbeatTimer.Start();
        }

        /// <summary>
        /// Stops generating Heartbeat requests 
        /// </summary>
        public void StopHandler()
        {
            // Stop Heartbeat Timer
            _heartbeatTimer.Stop();
        }

        /// <summary>
        /// Handles the new incoming Server Heartbeat
        /// </summary>
        /// <param name="serverHeartbeatInterval">Time Interval after which MDE will send response Heartbeat</param>
        public void Update(int serverHeartbeatInterval)
        {
            try
            {
                // Stop Timer for processing
                StopValidationTimer();

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Server Heartbeat received", _type.FullName, "Update");
                }

                // Start Timer after processing
                StartValidationTimer(serverHeartbeatInterval);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "Update");
            }
        }

        /// <summary>
        /// Starts Heartbeat Validation Timer
        /// </summary>
        public void StartValidationTimer(int serverHeartbeatInterval = 120000)
        {
            // Adjust Heartbeat validation timer
            _validationTimer.Interval = serverHeartbeatInterval + _heartbeatValidationInterval;

            // Start Validation Timer
            _validationTimer.Start();
        }

        /// <summary>
        /// Stops Heatbeat validation timer
        /// </summary>
        public void StopValidationTimer()
        {
            // Stop Validation Timer
            _validationTimer.Stop();
        }

        /// <summary>
        /// Raised when Heartbeat Timer is Elapsed
        /// </summary>
        private void OnHeartbeatTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Timer elapsed for heartbeat message.", _type.FullName, "OnHeartbeatTimerElapsed");
                }

                // Raise event to send new heartbeat message
                if (_sendHeartbeat != null)
                {
                    _sendHeartbeat(_heartbeatMessage);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnHeartbeatTimerElapsed");
            }
        }

        /// <summary>
        /// Raised when Server Disconnect Timer Elapse
        /// </summary>
        private void OnServerValidationTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Stop Response Timer
            StopHandler();

            // Stop Validation Timer
            StopValidationTimer();

            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Expected Server shutdown due to missed heartbeat", _type.FullName, "OnServerResponseTimerElapsed");
            }

            // Raise Disconnect Event
            if (_serverDisconnected != null)
            {
                _serverDisconnected();
            }
        }
        
    }
}
