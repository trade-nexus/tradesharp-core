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
using TradeHub.NotificationEngine.CommunicationManager.Service;
using TradeHub.NotificationEngine.NotificationCenter.Services;

namespace TradeHub.NotificationEngine.Server.Service
{
    /// <summary>
    /// Controls all the functionality provided by the Notifications Module
    /// </summary>
    public class ApplicationController
    {
        private Type _type = typeof(ApplicationController);

        /// <summary>
        /// Provides communication medium between Server and Client
        /// </summary>
        private ICommunicator _communicator;

        /// <summary>
        /// Handles all activites for the incoming notification messages
        /// </summary>
        private NotificationController _notificationController;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="communicator">Provides communication medium between Server and Client</param>
        /// <param name="notificationController">Handles all activites for the incoming notification messages</param>
        public ApplicationController(ICommunicator communicator, NotificationController notificationController)
        {
            // Save Instances
            _communicator = communicator;
            _notificationController = notificationController;

            // Subscribe Events
            RegisterCommunicatorEvents();
        }

        /// <summary>
        /// Starts Communicator to open communication medium with clients
        /// </summary>
        public void StartCommunicator()
        {
            // Check for Null Reference
            if (_communicator != null)
            {
                // Connect Communication Server
                _communicator.Connect();
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
                }
            }
        }

        /// <summary>
        /// Subscribe Events from Communicator Instance
        /// </summary>
        private void RegisterCommunicatorEvents()
        {
            // Check for Null Reference
            if (_communicator != null)
            {
                // Makes sure that events are not hooked multiple time
                UnregisterCommunicatorEvents();

                // Register event to receive new notification messages
                _communicator.OrderNotificationEvent += OnNewOrderNotificationReceived;
            }
        }

        /// <summary>
        /// Un-Subscribe Events from Communicator Instance
        /// </summary>
        private void UnregisterCommunicatorEvents()
        {
            // Unsubscrive event to stop receiving notification messages
            _communicator.OrderNotificationEvent -= OnNewOrderNotificationReceived;
        }

        /// <summary>
        /// Called when new order notification message is received from Communicator Server
        /// </summary>
        /// <param name="notification">Execution containing Order and Fill information</param>
        private void OnNewOrderNotificationReceived(OrderNotification notification)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("New Execution received " + notification, _type.FullName, "OnNewOrderNotificationReceived");
                }

                // Forward new message received to notification controller
                _notificationController.NewNotificationArrived(notification);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnNewOrderNotificationReceived");
            }
        }
    }
}
