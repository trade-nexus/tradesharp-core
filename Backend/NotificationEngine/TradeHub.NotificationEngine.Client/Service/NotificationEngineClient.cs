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
using TradeHub.NotificationEngine.Common.ValueObject;

namespace TradeHub.NotificationEngine.Client.Service
{
    /// <summary>
    /// Manages all activities to communicate with the Notification Engine Server
    /// </summary>
    public class NotificationEngineClient
    {
        private Type _type = typeof (NotificationEngineClient);

        /// <summary>
        /// Provides communication medium to interact with server
        /// </summary>
        private readonly ICommunicator _communicator;

        /// <summary>
        /// Indicates if the Client is connected or not
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            if (_communicator != null)
            {
                return _communicator.IsConnected();
            }

            return false;
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="communicator">Provides communication medium to interact with server</param>
        public NotificationEngineClient(ICommunicator communicator)
        {
            // Save Instance
            _communicator = communicator;
        }

        /// <summary>
        /// Starts Communicator to open communication medium with clients
        /// </summary>
        public void StartCommunicator()
        {
            // Check for Null Reference
            if (_communicator != null)
            {
                // Check if it not already connected
                if (!_communicator.IsConnected())
                {
                    // Connect Communication Server
                    _communicator.Connect();

                    return;
                }

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Already connected", _type.FullName, "StartCommunicator");
                }
                return;
            }

            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Communicator object not initialized", _type.FullName, "StartCommunicator");
            }
        }

        /// <summary>
        /// Stop Communicator to close communication medium
        /// </summary>
        public void StopCommunicator()
        {
            // Check for Null Reference
            if (_communicator != null)
            {
                // Check if the Communicator is currently active
                if (_communicator.IsConnected())
                {
                    // Stop Communication Server
                    _communicator.Disconnect();

                    return;
                }

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Already disconnected", _type.FullName, "StopCommunicator");
                }
                return;
            }

            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Communicator object not initialized", _type.FullName, "StopCommunicator");
            }
        }

        /// <summary>
        /// Forwards Execution message to Communicator to be sent to Server
        /// </summary>
        /// <param name="notification">Contains Order Notification information to be sent</param>
        public void SendNotification(OrderNotification notification)
        {
            try
            {
                // Forward information to Communicator
                _communicator.SendNotification(notification);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendNotification");
            }
        }
    }
}
