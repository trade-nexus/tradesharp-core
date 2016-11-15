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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using EasyNetQ;
using EasyNetQ.Topology;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.Inquiry;

namespace TradeHub.PositionEngine.Configuration.Service
{
    public class PositionEngineMqServer
    {
        private readonly Type _type = typeof(PositionEngineMqServer);

        //Holds Position Routing key
        private string _postionRoutingKey;

        // Exchange containing Queues
        private IExchange _exchange;

        // Exchange containing Position Engine Queues
        private IExchange _positionExchange;

        // Holds reference for the Advance Bus
        private IAdvancedBus _rabbitBus;

        //Queue will contain position messages
        private IQueue _positionMessageQueue;

        //Queue will contain provider request position messages
        private IQueue _providerRequestMessageQueue;

        //Queue will contain inquiry messages
        private IQueue _inquiryMessageQueue;

        // Queue will contain App Info messages
        private IQueue _appInfoQueue;

        //Name of the Configuration File
        private readonly string _configFile;

        //fired when position message is arrived from provider
        public event Action<IMessage<Position>> PositionMessageArrived;

        //fired when app info is received
        public event Action<IMessage<Dictionary<string, string>>> AppInfoReceived;

        //fired when providers request is received
        public event Action<IMessage<string>> ProviderRequestReceived;

        //fired when inquiry message is received for connection establishments
        public event Action<IMessage<InquiryMessage>> InquiryRequestReceived;


        public PositionEngineMqServer(string configfile)
        {
            _configFile = configfile;

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

        public void Connect()
        {
            // Initializes MQ resources
            IntitializeMqServices();

            // Bind Admin Message Queue
            SubscribePositionMessageQueues();

            // Bind inquiry request Message Queue
            SubscribeInquiryMessageQueue();

            //bind app info queue
            SubscribeAppInfoQueue();

            // Bind provider request Message Queue
            SubscribeProviderRequestQueue();
            
        }

        public void IntitializeMqServices()
        {
            try
            {
                // Create Rabbit MQ Hutch 
                string connectionString = ReadConfigSettings("ConnectionString");
                if (!string.IsNullOrEmpty(connectionString))
                {
                    // Initialize Rabbit MQ Hutch 
                    InitializeRabbitHutch(connectionString);

                    // Get Exchange Name from Config File
                    string exchangeName = ReadConfigSettings("Exchange");
                    string positionExchangeName = ReadConfigSettings("PositionExchange");
                    
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
                    if (!string.IsNullOrEmpty(positionExchangeName))
                    {
                        // Use the Exchange Name to Initialize Rabbit Exchange
                        _positionExchange = InitializeExchange(positionExchangeName);

                        if (_positionExchange != null)
                        {
                            // Initialize required queues
                            RegisterPositionQueues(_positionExchange);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "IntitializeMqServices");
            }
        }

        //listen to position messages
        public void SubscribePositionMessageQueues()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Binding Position Message Queue: " + _positionMessageQueue.Name, _type.FullName,
                                "SubscribePositionMessageQueues");
                }

                // Listening to Position Messages
                _rabbitBus.Consume<Position>(
                    _positionMessageQueue, (msg, messageReceivedInfo) =>
                                        Task.Factory.StartNew(
                                            () =>
                                            {
                                               if (PositionMessageArrived != null)
                                                {
                                                    PositionMessageArrived(msg);
                                                }
                                            }));


            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribePositionMessageQueues");
            }
        }

        /// <summary>
        /// Binds the App Info Queue
        /// Starts listening to the incoming App Info messages
        /// </summary>
        public void SubscribeProviderRequestQueue()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Binding Provider Request Queue Queue: " + _providerRequestMessageQueue.Name, _type.FullName,
                                "SubscribeProviderRequestQueue");
                }

                // Listening to App Info Messages
                _rabbitBus.Consume<string>(
                    _providerRequestMessageQueue, (msg, messageReceivedInfo) =>
                                   Task.Factory.StartNew(() =>
                                   {
                                       if (ProviderRequestReceived != null)
                                       {
                                           ProviderRequestReceived(msg);
                                       }
                                   }));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeProviderRequestQueue");
            }
        }
        /// <summary>
        /// Publishes the Inquiry Response to MQ Exchange
        /// </summary>
        /// <param name="replyTo">Routing Key of queue to publish</param>
        /// <param name="message">TradeHub Inquiry Response message to be sent</param>
        public void PublishMessages(string replyTo, Message<InquiryResponse> message)
        {
            try
            {
                //using (var channel = _rabbitBus.OpenPublishChannel())
                {
                    // Publish Messages to respective Queues
                    _rabbitBus.Publish(_positionExchange, replyTo,true,false, message);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes the Position Message to MQ Exchange
        /// </summary>
        /// <param name="replyTo">Routing Key of queue to publish</param>
        /// <param name="message">TradeHub Position message to be sent</param>
        public void PublishPositionMessages(string replyTo, Message<Position> message)
        {
            try
            {
               // using (var channel = _rabbitBus.OpenPublishChannel())
                {
                    // Publish Messages to respective Queues
                    _rabbitBus.Publish(_positionExchange, replyTo, true, false, message);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishPositionMessages");
            }
        }
        /// <summary>
        /// Disconnects the running intance of the Rabbit Hutch
        /// </summary>
        public void Disconnect()
        {
            try
            {
                // Dispose Rabbit Bus
                if (_rabbitBus != null)
                {
                    _rabbitBus.Dispose();

                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Advanced Bus disposed off.", _type.FullName, "Disconnect");
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "Disconnect");
            }
        }
        /// <summary>
        /// Binds the Inquiry Request Message Queue
        /// Starts listening to the incoming Inquiry messages
        /// </summary>
        public void SubscribeInquiryMessageQueue()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Binding Inquiry Message Queue: " + _inquiryMessageQueue.Name, _type.FullName,
                                "SubscribeInquiryMessageQueue");
                }

                // Listening to Inquiry Messages
                _rabbitBus.Consume<InquiryMessage>(
                    _inquiryMessageQueue, (msg, messageReceivedInfo) =>
                                               Task.Factory.StartNew(() =>
                                               {
                                                   if (InquiryRequestReceived != null)
                                                   {
                                                       InquiryRequestReceived(msg);
                                                   }
                                               }));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeInquiryMessageQueue");
            }
        }
        /// <summary>
        /// Binds the App Info Queue
        /// Starts listening to the incoming App Info messages
        /// </summary>
        public void SubscribeAppInfoQueue()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Binding App Info Queue: " + _appInfoQueue.Name, _type.FullName,
                                "SubscribeAppInfoQueue");
                }

                // Listening to App Info Messages
                _rabbitBus.Consume<Dictionary<string, string>>(
                    _appInfoQueue, (msg, messageReceivedInfo) =>
                                   Task.Factory.StartNew(() =>
                                   {
                                       if (AppInfoReceived != null)
                                       {
                                           AppInfoReceived(msg);
                                       }
                                   }));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeAppInfoQueue");
            }
        }

        /// <summary>
        /// Initializes queues and performs bindings
        /// </summary>
        private void RegisterQueues(IExchange exchange)
        {
            try
            {
                BindQueue("PositionMessageQueue", "PositionRoutingKey", ref _positionMessageQueue, exchange);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "RegisterQueues");
            }
        }

        private void RegisterPositionQueues(IExchange exchange)
        {
            try
            {
                //bind inquiry queue
                BindQueue("InquiryQueue", "InquiryRoutingKey",ref _inquiryMessageQueue,exchange);
                //bind app info queue
                BindQueue("AppInfoQueue", "AppInfoRoutingKey", ref _appInfoQueue, exchange);
                //bind provider request queues
                BindQueue("ProviderRequestQueue", "ProviderRequestRoutingKey", ref _providerRequestMessageQueue,exchange);


            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "RegisterPositionQueues");
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
                return _rabbitBus.ExchangeDeclare(exchangeName, ExchangeType.Direct, false, true, false);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "InitializeExchange");
                return null;
            }
        }

       
        public void InitializeRabbitHutch(string connectionString)
        {
            try
            {
                // Create a new Rabbit Bus Instance
                _rabbitBus = _rabbitBus ?? RabbitHutch.CreateBus(connectionString).Advanced;

                _rabbitBus.Connected -= OnBusConnected;
                _rabbitBus.Connected += OnBusConnected;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "InitializeRabbitHutch");
            }
        }

        //Read configuration parameter from file
        public string ReadConfigSettings(string parameter)
        {
            try
            {
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + _configFile))
                {
                    var doc = new XmlDocument();

                    // Read RabbitMQ configuration file
                    doc.Load(AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + _configFile);

                    // Read the specified Node value
                    XmlNode node = doc.SelectSingleNode(xpath: "RabbitMQ/" + parameter);
                    if (node != null)
                    {
                        return node.InnerText;
                    }
                }
                return string.Empty;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ReadConfigSettings");
                return string.Empty;
            }
        }
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
        /// Initializes RabbitMQ Queue
        /// </summary>
        private IQueue InitializeQueue(IExchange exchange, string queueName, string routingKey)
        {
            try
            {
                // Initialize specified Queue
                //IQueue queue = Queue.Declare(false, true, false, queueName, null);
                IQueue queue = _rabbitBus.QueueDeclare(queueName, false, false, true, true);
                // Bind Queue to already initialized Exchange with the specified Routing Key
                //queue.BindTo(exchange, routingKey);
                _rabbitBus.Bind(exchange, queue, routingKey);
                return queue;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "InitializeQueue");
                return null;
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
                string queueName = ReadConfigSettings(queueHeader);
                // Get Routing Key from Config File
                string routingKey = ReadConfigSettings(routingKeyHeader);

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
    }
}
