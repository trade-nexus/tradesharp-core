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
using System.Threading;
using System.Threading.Tasks;
using TraceSourceLogger;
using TradeHub.NotificationEngine.Client.Service;
using TradeHub.NotificationEngine.Common.ValueObject;

namespace TradeHub.StrategyEngine.Notification
{
    /// <summary>
    /// Provider access to 'Notification Engine - Server' using Client underneath
    /// </summary>
    public class NotificationService
    {
        private Type _type = typeof (NotificationService);

        /// <summary>
        /// Used for communication with the server
        /// </summary>
        private NotificationEngineClient _notificationEngineClient;

        /// <summary>
        /// Dedicated task to consume order notification messages
        /// </summary>
        private Task _orderNotificationConsumerTask;

        /// <summary>
        /// Token source used with Order Notification Consumer Task
        /// </summary>
        private CancellationTokenSource _orderNotificationConsumerCancellationToken;

        /// <summary>
        /// Holds all incoming order notification messages until they can be processed
        /// </summary>
        private ConcurrentQueue<OrderNotification> _orderNotifications;

        /// <summary>
        /// Wraps the Order Notifications concurrent queue
        /// </summary>
        private BlockingCollection<OrderNotification> _orderNotificationsCollection; 

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="notificationEngineClient">Used for communication with the server</param>
        public NotificationService(NotificationEngineClient notificationEngineClient)
        {
            // Save instance
            _notificationEngineClient = notificationEngineClient;

            // Initialize Objects
            _orderNotifications = new ConcurrentQueue<OrderNotification>();
            _orderNotificationsCollection = new BlockingCollection<OrderNotification>(_orderNotifications);
        }

        #region Start/Stop

        /// <summary>
        /// Starts necessary components for Notification Engine Service
        /// </summary>
        /// <returns>Indicates whether the operation was successful or not.</returns>
        public bool StartService()
        {
            if (_notificationEngineClient != null)
            {
                // Start Client
                _notificationEngineClient.StartCommunicator();

                // Initialize Consumer Token
                _orderNotificationConsumerCancellationToken = new CancellationTokenSource();

                // Consumes order notification messages from local collection
                _orderNotificationConsumerTask = Task.Factory.StartNew(ConsumeOrderNotifications, _orderNotificationConsumerCancellationToken.Token);

                return true;
            }

            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Client object not initialized.", _type.FullName, "StartService");
            }

            return false;
        }

        /// <summary>
        /// Stops necessary components for Notification Engine Service
        /// </summary>
        /// <returns>Indicates whether the operation was successful or not.</returns>
        public bool StopService()
        {
            if (_notificationEngineClient != null)
            {
                // Stop Client
                _notificationEngineClient.StopCommunicator();
                _orderNotificationConsumerCancellationToken.Cancel();
                return true;
            }

            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Client object not initialized.", _type.FullName, "StopService");
            }

            return false;
        }

        #endregion

        /// <summary>
        /// Starts the process to send incoming notification to the server
        /// </summary>
        /// <param name="notification"></param>
        public void SendNotification(OrderNotification notification)
        {
            // Add to local collection
            _orderNotificationsCollection.Add(notification);

            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("New Order Notification received for publishing", _type.FullName, "SendNotifications");
            }
        }

        /// <summary>
        /// Consumes Order Notification messages from local map
        /// </summary>
        private void ConsumeOrderNotifications()
        {
            try
            {
                while (true)
                {
                    // Break thread if cancellation is requested
                    if (_orderNotificationConsumerCancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    // Consume notification
                    var notification = _orderNotificationsCollection.Take();

                    // Send notifications to server
                    _notificationEngineClient.SendNotification(notification);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ConsumeOrderNotifications");
            }
        }
    }
}
