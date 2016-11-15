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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using EasyNetQ;
using EasyNetQ.Topology;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TraceSourceLogger;
using TradeHub.NotificationEngine.Common.Utility;
using TradeHub.NotificationEngine.Common.ValueObject;

namespace TradeHub.NotificationEngine.CommunicationManager.Service
{
    /// <summary>
    /// Uses Messaging Queues as communication medium with the client
    /// </summary>
    public class NotificationEngineMqServer : ICommunicator
    {
        private Type _type = typeof (NotificationEngineMqServer);

        // Holds reference for the Advance Bus
        private IAdvancedBus _rabbitBus;

        // Exchange containing Queues
        private IExchange _exchange;

        // Native Rabbit MQ Fields
        private ConnectionFactory _rabbitMqConnectionFactory;
        private IConnection _rabbitMqConnection;
        private IModel _rabbitMqChannel;

        // Queue will contain Order Notification messages
        private IQueue _orderNotificationQueue;

        /// <summary>
        /// Dedicated task to consume order notification messages
        /// </summary>
        private Task _orderNotificationConsumerTask;

        /// <summary>
        /// Token source used with Order Notification Consumer Task
        /// </summary>
        private CancellationTokenSource _orderNotificationConsumerCancellationToken;

        /// <summary>
        /// Contains messaging queues configuration information
        /// KEY = Configuration parameter name
        /// VALUE = Parameter value
        /// </summary>
        private Dictionary<string, string> _mqConfigParameters;

        /// <summary>
        /// Holds all incoming order notification messages until they can be processed
        /// </summary>
        private ConcurrentQueue<OrderNotification> _orderNotifications;

        /// <summary>
        /// Wraps the Order Notifications concurrent queue
        /// </summary>
        private BlockingCollection<OrderNotification> _orderNotificationsCollection; 

        /// <summary>
        /// Raised when new Order Notificaiton Message is received
        /// </summary>
        public event Action<OrderNotification> OrderNotificationEvent; 

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="configFile">Name of the configuration file to read</param>
        public NotificationEngineMqServer(string configFile)
        {
            // Initialize Objects
            _orderNotifications = new ConcurrentQueue<OrderNotification>();
            _orderNotificationsCollection= new BlockingCollection<OrderNotification>(_orderNotifications);

            // Read MQ configuration values
            ReadConfigSettings(configFile);
        }

        /// <summary>
        /// Checks if the Advance Bus is connected or not
        /// </summary>
        public bool IsConnected()
        {
            if (_rabbitBus != null)
            {
                return _rabbitBus.IsConnected;
            }
            return false;
        }

        #region StartUp

        /// <summary>
        /// Starts MQ Server
        /// </summary>
        public void Connect()
        {
            // Initializes MQ resources
            IntitializeMqServices();

            // Initialize Consumer Token
            _orderNotificationConsumerCancellationToken = new CancellationTokenSource();

            // Start Consuming Messages Queue
            StartConsumers();
        }

        /// <summary>
        /// Initializes the required parameters and fields for the Rabbit MQ service
        /// </summary>
        private void IntitializeMqServices()
        {
            try
            {
                string connectionString;
                if (!_mqConfigParameters.TryGetValue("ConnectionString", out connectionString))
                    return;

                if (!string.IsNullOrEmpty(connectionString))
                {
                    // Initialize EasyNetQ Rabbit Hutch 
                    InitializeRabbitHutch(connectionString);

                    // Initialize Native RabbitMQ Parameters
                    InitializeNativeRabbitMq(connectionString);

                    // Get Exchange Name from Config File
                    string exchangeName;
                    if (!_mqConfigParameters.TryGetValue("Exchange", out exchangeName))
                        return;

                    if (!string.IsNullOrEmpty(exchangeName))
                    {
                        // Use the Exchange Name to Initialize Rabbit Exchange
                        _exchange = InitializeExchange(exchangeName);

                        if (_exchange != null)
                        {
                            // Initialize required queues
                            RegisterQueues(_exchange);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "IntitializeMqServices");
            }
        }

        /// <summary>
        /// Initializes EasyNetQ's Advance Rabbit Hutch
        /// </summary>
        private void InitializeRabbitHutch(string connectionString)
        {
            try
            {
                // Create a new Rabbit Bus Instance
                _rabbitBus = _rabbitBus ?? RabbitHutch.CreateBus("host=" + connectionString).Advanced;

                _rabbitBus.Connected -= OnBusConnected;
                _rabbitBus.Connected += OnBusConnected;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "InitializeRabbitHutch");
            }
        }

        /// <summary>
        /// Initializes Native Rabbit MQ resources
        /// </summary>
        private void InitializeNativeRabbitMq(string connectionString)
        {
            try
            {
                // Create Native Rabbit MQ Bus
                _rabbitMqConnectionFactory = new ConnectionFactory { HostName = connectionString };

                // Create Native Rabbit MQ Connection to Receive Order Request
                _rabbitMqConnection = _rabbitMqConnectionFactory.CreateConnection();

                // Open Native Rabbbit MQ Channel to Receive Order Request
                _rabbitMqChannel = _rabbitMqConnection.CreateModel();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "InitializeNativeRabbitMq");
            }
        }

        /// <summary>
        /// Initializes RabbitMQ Exchange
        /// </summary>
        private IExchange InitializeExchange(string exchangeName)
        {
            try
            {
                // Initialize specified Exchange
                return _rabbitBus.ExchangeDeclare(exchangeName, EasyNetQ.Topology.ExchangeType.Direct, false, true, false);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "InitializeExchange");
                return null;
            }
        }

        /// <summary>
        /// Initializes queues and performs bindings
        /// </summary>
        private void RegisterQueues(IExchange exchange)
        {
            try
            {
                // Bind Inquiry Queue
                BindQueue("OrderMessageQueue", "OrderMessageRoutingKey", ref _orderNotificationQueue, exchange);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "RegisterQueues");
            }
        }

        /// <summary>
        /// Binds the queue with provided info
        /// </summary>
        private void BindQueue(string queueHeader, string routingKeyHeader, ref IQueue queue, IExchange exchange)
        {
            try
            {
                // Get Queue Name from Config File
                string queueName;
                if (!_mqConfigParameters.TryGetValue(queueHeader, out queueName))
                    return;

                // Get Routing Key from Config File
                string routingKey;
                if (!_mqConfigParameters.TryGetValue(routingKeyHeader, out routingKey))
                    return;

                if (!string.IsNullOrEmpty(queueName)
                    && !string.IsNullOrEmpty(routingKey))
                {
                    // Use the initialized Exchange, Queue Name and RoutingKey to initialize Rabbit Queue
                    queue = InitializeQueue(exchange, queueName, routingKey);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "BindQueue");
            }
        }

        /// <summary>
        /// Initializes RabbitMQ Queue
        /// </summary>
        private IQueue InitializeQueue(IExchange exchange, string queueName, string routingKey)
        {
            try
            {
                // Initialize specified Queue
                IQueue queue = _rabbitBus.QueueDeclare(queueName, false, false, true, true);

                // Bind Queue to already initialized Exchange with the specified Routing Key
                _rabbitBus.Bind(exchange, queue, routingKey);
                return queue;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "InitializeQueue");
                return null;
            }
        }

        #endregion

        #region Shut Down

        /// <summary>
        /// Disconnects MQ Server and terminate necessary resources
        /// </summary>
        public void Disconnect()
        {
            try
            {
                // Dispose Rabbit Bus
                if (_rabbitBus != null)
                {
                    _rabbitBus.Dispose();
                    _rabbitMqChannel.Close();
                    _rabbitMqConnection.Close();

                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Advanced Bus disposed off.", _type.FullName, "Disconnect");
                    }
                }

                // Dispose location collection
                _orderNotificationsCollection.Dispose();

                // Stop Consumer
                if (_orderNotificationConsumerCancellationToken != null)
                {
                    _orderNotificationConsumerCancellationToken.Cancel();
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "Disconnect");
            }
        }

        #endregion

        #region Consumers

        /// <summary>
        /// Starts are necessary consumers for the MQ Server
        /// </summary>
        private void StartConsumers()
        {
            // Consumes order notification messages from local collection
            _orderNotificationConsumerTask = Task.Factory.StartNew(ConsumeOrderNotifications, _orderNotificationConsumerCancellationToken.Token);

            // Consumes order notification messages from Messaging Queue
            ConsumeOrderNotificationQueue();
        }

        /// <summary>
        /// Consumes Order Notification messages from queue
        /// </summary>
        private void ConsumeOrderNotificationQueue()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Starting order notification message queue consumer", _type.FullName, "ConsumeOrderNotificationQueue");
                }

                _rabbitBus.Consume<OrderNotification>(_orderNotificationQueue, (msg, messageReceivedInfo) =>
                    Task.Factory.StartNew(() =>
                    {
                        // Add to local collection
                        _orderNotificationsCollection.Add(msg.Body);
                    }));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ConsumeOrderNotificationQueue");
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
                    var notification = _orderNotificationsCollection.Take();

                    // Rasie event to notify listeners
                    if (OrderNotificationEvent != null)
                    {
                        OrderNotificationEvent(notification);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ConsumeOrderNotifications");
            }
        }

        #endregion

        /// <summary>
        /// Raised when Advanced Bus is successfully connected
        /// </summary>
        private void OnBusConnected()
        {
            if (_rabbitBus.IsConnected)
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Successfully connected to MQ Server", _type.FullName, "OnBusConnected");
                }
            }
        }

        /// <summary>
        /// Reads settings parameters from the Config file
        /// </summary>
        private void ReadConfigSettings(string serverConfig)
        {
            try
            {
                _mqConfigParameters = MqConfigurationReader.ReadServerMqProperties(serverConfig);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ReadConfigSettings");
            }
        }
    }
}
