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
using EasyNetQ;
using EasyNetQ.Topology;
using RabbitMQ.Client;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.SimulatedExchange.Common.Interfaces;
using ExchangeType = EasyNetQ.Topology.ExchangeType;

namespace TradeHub.SimulatedExchange.SimulatorControler
{
    public class CommunicationController : ICommunicationController
    {
        private Type _type = typeof(CommunicationController);

        private string _adminQueueRoutingKey = "simulated.admin.routingkey";
        private string _marketAdminResponseQueueRoutingKey = "simulated.marketadminresponse.routingkey";
        private string _orderAdminResponseQueueRoutingKey = "simulated.orderadminresponse.routingkey";

        private string _tickRequestQueueRoutingKey = "simulated.tickrequest.routingkey";
        private string _tickDataQueueRoutingKey = "simulated.tickdata.routingkey";

        private string _barRequestQueueRoutingKey = "simulated.barrequest.routingkey";
        private string _barDataQueueRoutingKey = "simulated.bardata.routingkey";

        private string _historicRequestQueueRoutingKey = "simulated.historicrequest.routingkey";
        private string _historickDataQueueRoutingKey = "simulated.historicdata.routingkey";

        // Request Routing Keys
        private string _marketOrderQueueRoutingKey = "simulated.marketorder.routingkey";
        private string _limitOrderQueueRoutingKey = "simulated.limitorder.routingkey";
        private string _cancelOrderRequestQueueRoutingKey = "simulated.cancelorderrequest.routingkey";

        // Response Routing Keys
        private string _newOrderQueueRoutingKey = "simulated.neworder.routingkey";
        private string _rejectionOrderQueueRoutingKey = "simulated.rejectionorder.routingkey";
        private string _executionOrderQueueRoutingKey = "simulated.executionorder.routingkey";
        private string _cancelledOrderQueueRoutingKey = "simulated.cancelledorder.routingkey";

        // Native Rabbit MQ Fields
        private ConnectionFactory _rabbitMqTickBus;
        //private ConnectionFactory _rabbitMqBarBus;
        private IConnection _rabbitMqTickConnection;
        //private IConnection _rabbitMqBarConnection;
        private IModel _rabbitMqTickChannel;
        //private IModel _rabbitMqBarChannel;

        // Holds reference for the EasyNetQ Advance Bus
        private IAdvancedBus _easyNetQBus;

        // EasyNetQ Exchange containing Queues
        private IExchange _easyNetQExchange;

        // Queue will contain Login messages
        private IQueue _adminMessageQueue;

        // Queue will contain Tick Data request messages
        private IQueue _tickDataRequestQueue;

        // Queue will contain Bar Data request messages
        private IQueue _barDataRequestQueue;
        
        // Queue will contain Historic Data request messages
        private IQueue _historicDataRequestQueue;

        // Queue will contain Market Order messages
        private IQueue _marketOrderQueue;

        // Queue will contain Limit Order messages
        private IQueue _limitOrderQueue;

        // Queue will contain Cancel Order Request messages
        private IQueue _cancelOrderRequestQueue;

        public event Action MarketDataLoginRequest;
        public event Action MarketDataLogoutRequest;
        public event Action OrderExecutionLoginRequest;
        public event Action OrderExecutionLogoutRequest;
        public event Action<Subscribe> TickDataRequest;
        public event Action<BarDataRequest> BarDataRequest;
        public event Action<HistoricDataRequest> HistoricDataRequest;
        public event Action<MarketOrder> MarketOrderRequest;
        public event Action<LimitOrder> LimitOrderRequest;
        public event Action<Order> CancelOrderRequest;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CommunicationController()
        {
            Connect();
        }

        /// <summary>
        /// Starts MQ Server
        /// </summary>
        public void Connect()
        {
            // Initializes MQ resources
            InitializeMqServices();
            InitializeRabbitMqService();

            // Subscribe Admin Message Queue
            SubscribeAdminMessageQueues();

            // Subscribe Market Data Request Message Queues
            SubscribeMarketDataRequestMessageQueues();

            // Subscribe Order Message Queues 
            SubscribeOrderMessageQueues();
        }

        /// <summary>
        /// Stop MQ Server
        /// </summary>
        public void Disconnect()
        {
            _easyNetQBus.Dispose();
            _rabbitMqTickConnection.Close();
            //_rabbitMqBarConnection.Close();
        }

        /// <summary>
        /// Initializes the required parameters and fields for the EasyNetQ service
        /// </summary>
        public void InitializeMqServices()
        {
            try
            {
                // Create Rabbit MQ Hutch 
                string connectionString = "host=localhost";
                if (!string.IsNullOrEmpty(connectionString))
                {
                    // Initialize Rabbit MQ Hutch 
                    InitializeRabbitHutch(connectionString);

                    string exchangeName = "simulated_exchange";

                    if (!string.IsNullOrEmpty(exchangeName))
                    {
                        // Use the Exchange Name to Initialize Rabbit Exchange
                        _easyNetQExchange = InitializeExchange(exchangeName);

                        if (_easyNetQExchange != null)
                        {
                            // Initialize required queues
                            RegisterQueues(_easyNetQExchange);
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
        /// Initializes the required parameters and fields for the Rabbit MQ service
        /// </summary>
        public void InitializeRabbitMqService()
        {
            try
            {
                // Create Bus
                _rabbitMqTickBus = new ConnectionFactory();
                _rabbitMqTickBus.HostName = "localhost";

                //_rabbitMqBarBus = new ConnectionFactory();
                //_rabbitMqBarBus.HostName = "localhost";

                // Create Connection
                _rabbitMqTickConnection = _rabbitMqTickBus.CreateConnection();
                //_rabbitMqBarConnection = _rabbitMqBarBus.CreateConnection();
                
                // Open Channel
                _rabbitMqTickChannel = _rabbitMqTickConnection.CreateModel();
                //_rabbitMqBarChannel = _rabbitMqBarConnection.CreateModel();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "InitializeRabbitMqService");
            }
        }

        /// <summary>
        /// Initializes EasyNetQ's Advacne Rabbit Hutch
        /// </summary>
        public void InitializeRabbitHutch(string connectionString)
        {
            try
            {
                // Create a new Rabbit Bus Instance
                _easyNetQBus = _easyNetQBus ?? RabbitHutch.CreateBus(connectionString).Advanced;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "InitializeRabbitHutch");
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
                return _easyNetQBus.ExchangeDeclare(exchangeName, ExchangeType.Direct, false, false, true);
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
                // Bind Admin Queue
                BindQueue("simulated.admin.queue", _adminQueueRoutingKey, ref _adminMessageQueue, exchange);

                // Bind Tick Request Queue
                BindQueue("simulated.tickrequest.queue", _tickRequestQueueRoutingKey, ref _tickDataRequestQueue, exchange);

                // Bind Bar Request Queue
                BindQueue("simulated.barrequest.queue", _barRequestQueueRoutingKey, ref _barDataRequestQueue, exchange);

                // Bind Historic Request Queue
                BindQueue("simulated.historicrequest.queue", _historicRequestQueueRoutingKey, ref _historicDataRequestQueue, exchange);

                // Bind Market Order Queue
                BindQueue("simulated.marketorder.queue", _marketOrderQueueRoutingKey, ref _marketOrderQueue, exchange);

                // Bind Limit Order Queue
                BindQueue("simulated.limitorder.queue", _limitOrderQueueRoutingKey, ref _limitOrderQueue, exchange);

                // Bind Cancel Order Request Queue
                BindQueue("simulated.cancelorderrequest.queue", _cancelOrderRequestQueueRoutingKey, ref _cancelOrderRequestQueue, exchange);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "InitializeQueues");
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
                string queueName = queueHeader;
                // Get Routing Key from Config File
                string routingKey = routingKeyHeader;

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
                IQueue queue = _easyNetQBus.QueueDeclare(queueName, false, false, true, true);

                // Bind Queue to already initialized Exchange with the specified Routing Key
                _easyNetQBus.Bind(exchange, queue, routingKey);
                return queue;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "InitializeQueue");
                return null;
            }
        }

        /// <summary>
        /// Binds the Admin Message Queue 
        /// Starts listening to the incoming Admin Level messages
        /// </summary>
        public void SubscribeAdminMessageQueues()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Binding Admin Message Queue: " + _adminMessageQueue.Name, _type.FullName,
                                "SubscribeAdminMessageQueues");
                }

                // Listening to Login Messages
                _easyNetQBus.Consume<string>(
                    _adminMessageQueue, (msg, messageReceivedInfo) =>
                                        Task.Factory.StartNew(
                                            () =>
                                            {
                                                if (msg.Body.Equals("DataLogin"))
                                                {
                                                    MarketDataLoginRequest();
                                                }
                                                else if (msg.Body.Equals("OrderLogin"))
                                                {
                                                    OrderExecutionLoginRequest();
                                                }
                                                else if (msg.Body.Equals("DataLogout"))
                                                {
                                                    MarketDataLogoutRequest();
                                                }
                                                else if (msg.Body.Equals("OrderLogout"))
                                                {
                                                    OrderExecutionLogoutRequest();
                                                }
                                            }));

            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeAdminMessageQueues");
            }
        }

        /// <summary>
        /// Binds the Market Data Request Message Queue
        /// Starts listening to the incoming Market Data Request messages
        /// </summary>
        public void SubscribeMarketDataRequestMessageQueues()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Binding Market Data Request Message Queues", _type.FullName, "SubscribeMarketDataRequestMessageQueues");
                }

                // Listening to Tick Request Messages
                _easyNetQBus.Consume<Subscribe>(
                    _tickDataRequestQueue, (msg, messageReceivedInfo) =>
                                                 Task.Factory.StartNew(() => TickDataRequest(msg.Body)));

                // Listening to Bar Request Messages
                _easyNetQBus.Consume<BarDataRequest>(
                    _barDataRequestQueue, (msg, messageReceivedInfo) =>
                                                 Task.Factory.StartNew(() => BarDataRequest(msg.Body)));

                // Listening to Historic Request Messages
                _easyNetQBus.Consume<HistoricDataRequest>(
                    _historicDataRequestQueue, (msg, messageReceivedInfo) =>
                                                 Task.Factory.StartNew(() => HistoricDataRequest(msg.Body)));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeMarketDataRequestMessageQueues");
            }
        }

        /// <summary>
        /// Binds the Order Message Queue
        /// Starts listening to the incoming Market/Limit order messages
        /// </summary>
        public void SubscribeOrderMessageQueues()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Binding Order Message Queues", _type.FullName, "SubscribeOrderMessageQueues");
                }

                // Listening to Market Order Messages
                _easyNetQBus.Consume<MarketOrder>(
                    _marketOrderQueue, (msg, messageReceivedInfo) =>
                                                 Task.Factory.StartNew(() =>
                                                     {
                                                         MarketOrderRequest(msg.Body);
                                                     }));


                // Listening to Limit Order Messages
                _easyNetQBus.Consume<LimitOrder>(
                    _limitOrderQueue, (msg, messageReceivedInfo) =>
                                                 Task.Factory.StartNew(() =>
                                                 {
                                                     LimitOrderRequest(msg.Body);
                                                 }));

                // Listening to Cancel Order Request Messages
                _easyNetQBus.Consume<Order>(
                    _cancelOrderRequestQueue, (msg, messageReceivedInfo) =>
                                                 Task.Factory.StartNew(() =>
                                                 {
                                                     CancelOrderRequest(msg.Body);
                                                 }));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeOrderMessageQueues");
            }
        }

        /// <summary>
        /// Publishes Admin Level request response for Market Data
        /// </summary>
        public void PublishMarketAdminMessageResponse(string response)
        {
            try
            {
                // Message to be published
                IMessage<string> responseMessage = new Message<string>(response);

                _easyNetQBus.Publish(_easyNetQExchange, _marketAdminResponseQueueRoutingKey, true, false, responseMessage);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMarketAdminMessageResponse");
            }
        }

        /// <summary>
        /// Publishes Admin Level request response for Order Executions
        /// </summary>
        public void PublishOrderAdminMessageResponse(string response)
        {
            try
            {
                // Message to be published
                IMessage<string> responseMessage = new Message<string>(response);

                _easyNetQBus.Publish(_easyNetQExchange, _orderAdminResponseQueueRoutingKey, true, false, responseMessage);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishOrderAdminMessageResponse");
            }
        }

        /// <summary>
        /// Publishes Tick Data
        /// </summary>
        public void PublishTickData(Tick tick)
        {
            try
            {
                //NOTE: Using same channel and Queue to Publish both Tick and Bars
                string corrId = Guid.NewGuid().ToString();
                IBasicProperties replyProps = _rabbitMqTickChannel.CreateBasicProperties();
                replyProps.CorrelationId = corrId;

                // Create message bytes to be published
                byte[] responseBytes = Encoding.UTF8.GetBytes(tick.DataToPublish());

                // Publish Data
                _rabbitMqTickChannel.BasicPublish("simulated_exchange", _tickDataQueueRoutingKey, replyProps,
                                                  responseBytes);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishTickData");
            }
        }

        /// <summary>
        /// Publishes Bar Data
        /// </summary>
        public void PublishBarData(Bar bar)
        {
            try
            {
                //NOTE: Using same channel and Queue to Publish both Tick and Bars
                string corrId = Guid.NewGuid().ToString();
                IBasicProperties replyProps = _rabbitMqTickChannel.CreateBasicProperties();
                replyProps.CorrelationId = corrId;

                // Create message bytes to be published
                byte[] responseBytes = Encoding.UTF8.GetBytes(bar.DataToPublish());

                // Publish Data 
                _rabbitMqTickChannel.BasicPublish("simulated_exchange", _tickDataQueueRoutingKey, replyProps,
                                                  responseBytes);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishBarData");
            }
        }

        /// <summary>
        /// Publishes Bar Data
        /// </summary>
        public void PublishHistoricData(HistoricBarData historicBarData)
        {
            try
            {
                // Message to be published
                IMessage<HistoricBarData> message = new Message<HistoricBarData>(historicBarData);
                // Publish message
                _easyNetQBus.Publish(_easyNetQExchange, _historickDataQueueRoutingKey, true, false, message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishHistoricData");
            }
        }

        /// <summary>
        /// Publishes New Order status message
        /// </summary>
        public void PublishNewOrder(Order order)
        {
            try
            {
                // Message to be published
                IMessage<Order> message = new Message<Order>(order);
                // Publish message
                _easyNetQBus.Publish(_easyNetQExchange, _newOrderQueueRoutingKey, true, false, message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishNewOrder");
            }
        }

        /// <summary>
        /// Publishes Order Rejection
        /// </summary>
        public void PublishOrderRejection(Rejection rejection)
        {
            try
            {
                // Message to be published
                IMessage<Rejection> message = new Message<Rejection>(rejection);
                // Publish message
                _easyNetQBus.Publish(_easyNetQExchange, _rejectionOrderQueueRoutingKey, true, false, message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishOrderRejection");
            }
        }

        /// <summary>
        /// Publishes Order Executions
        /// </summary>
        public void PublishExecutionOrder(Execution execution)
        {
            try
            {
                // Message to be published
                IMessage<Execution> message = new Message<Execution>(execution);
                // Publish message
                _easyNetQBus.Publish(_easyNetQExchange, _executionOrderQueueRoutingKey, true, false, message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishExecutionOrder");
            }
        }

        /// <summary>
        /// Publishes Cancelled Orders
        /// </summary>
        public void PublishCancelledOrder(Order order)
        {
            try
            {
                // Message to be published
                IMessage<Order> message = new Message<Order>(order);
                // Publish message
                _easyNetQBus.Publish(_easyNetQExchange, _cancelledOrderQueueRoutingKey, true, false, message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishCancelledOrder");
            }
        }
    }
}
