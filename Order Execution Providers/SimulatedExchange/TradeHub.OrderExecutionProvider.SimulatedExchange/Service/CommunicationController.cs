using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;
using EasyNetQ;
using EasyNetQ.Topology;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.ValueObjects.MarketData;
using ExchangeType = EasyNetQ.Topology.ExchangeType;

namespace TradeHub.OrderExecutionProvider.SimulatedExchange.Service
{
    public class CommunicationController
    {
        private Type _type = typeof(CommunicationController);

        private string _adminQueueRoutingKey = "simulated.admin.routingkey";
        private string _orderAdminResponseQueueRoutingKey = "simulated.orderadminresponse.routingkey";

        // Request Routing Keys
        private string _marketOrderQueueRoutingKey = "simulated.marketorder.routingkey";
        private string _limitOrderQueueRoutingKey = "simulated.limitorder.routingkey";
        private string _cancelOrderRequestQueueRoutingKey = "simulated.cancelorderrequest.routingkey";

        // Response Routing Keys
        private string _newOrderQueueRoutingKey = "simulated.neworder.routingkey";
        private string _rejectionOrderQueueRoutingKey = "simulated.rejectionorder.routingkey";
        private string _executionOrderQueueRoutingKey = "simulated.executionorder.routingkey";
        private string _cancelledOrderQueueRoutingKey = "simulated.cancelledorder.routingkey";

        // Holds reference for the EasyNetQ Advance Bus
        private IAdvancedBus _easyNetQBus;

        // EasyNetQ Exchange containing Queues
        private IExchange _easyNetQExchange;

        // Queue will contain Login messages
        private IQueue _adminMessageQueue;

        // Queue will contain new Order status messages
        private IQueue _newOrderStatusQueue;

        // Queue will contain rejection Order messages
        private IQueue _rejectionOrderQueue;

        // Queue will contain Execution messages
        private IQueue _executionOrderQueue;

        // Queue will contain Cancelled Order messages
        private IQueue _cancelledOrderQueue;

        public event Action OrderExecutionLoginRequest;
        public event Action OrderExecutionLogoutRequest;
        public event Action<Order> NewOrderStatusReceived;
        public event Action<Execution> ExecutionOrderReceived;
        public event Action<Rejection> RejectionOrderReceived;
        public event Action<Order> CancelledOrderReceived;

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

            // Subscribe Admin Message Queue
            SubscribeAdminMessageQueues();

            // Subscribe Order Message Queues 
            SubscribeOrderMessageQueues();
        }

        /// <summary>
        /// Stop MQ Server
        /// </summary>
        public void Disconnect()
        {
            _easyNetQBus.Dispose();
            _easyNetQBus = null;
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
                BindQueue("simulated.orderadmin.queue", _orderAdminResponseQueueRoutingKey, ref _adminMessageQueue, exchange);

                // Bind New Order Status Queue
                BindQueue("simulated.neworderstatus.queue", _newOrderQueueRoutingKey, ref _newOrderStatusQueue, exchange);

                // Bind Execution Order Queue
                BindQueue("simulated.executionorder.queue", _executionOrderQueueRoutingKey, ref _executionOrderQueue, exchange);

                // Bind Rejection Order Queue
                BindQueue("simulated.rejectionorder.queue", _rejectionOrderQueueRoutingKey, ref _rejectionOrderQueue, exchange);

                // Bind Cancelled Order Queue
                BindQueue("simulated.cancelledorder.queue", _cancelledOrderQueueRoutingKey, ref _cancelledOrderQueue, exchange);
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
                                                if (msg.Body.Equals("OrderLogin"))
                                                {
                                                    OrderExecutionLoginRequest();
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

                // Listening to New Order Status Messages
                _easyNetQBus.Consume<Order>(
                    _newOrderStatusQueue, (msg, messageReceivedInfo) =>
                                                 Task.Factory.StartNew(() =>
                                                 {
                                                     NewOrderStatusReceived(msg.Body);
                                                 }));


                // Listening to Rejection Order Messages
                _easyNetQBus.Consume<Rejection>(
                    _rejectionOrderQueue, (msg, messageReceivedInfo) =>
                                                 Task.Factory.StartNew(() =>
                                                 {
                                                     RejectionOrderReceived(msg.Body);
                                                 }));

                // Listening to Rejection Order Messages
                _easyNetQBus.Consume<Execution>(
                    _executionOrderQueue, (msg, messageReceivedInfo) =>
                                                 Task.Factory.StartNew(() =>
                                                 {
                                                     ExecutionOrderReceived(msg.Body);
                                                 }));

                // Listening to Cancelled Order Messages
                _easyNetQBus.Consume<Order>(
                    _cancelledOrderQueue, (msg, messageReceivedInfo) =>
                                                 Task.Factory.StartNew(() =>
                                                 {
                                                     CancelledOrderReceived(msg.Body);
                                                 }));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeOrderMessageQueues");
            }
        }

        /// <summary>
        /// Publishes Admin Level request for Order Executions
        /// </summary>
        public void PublishOrderAdminMessage(string response)
        {
            try
            {
                // Message to be published
                IMessage<string> responseMessage = new Message<string>(response);

                _easyNetQBus.Publish(_easyNetQExchange, _adminQueueRoutingKey, true, false, responseMessage);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishOrderAdminMessage");
            }
        }

        /// <summary>
        /// Publishes market order message
        /// </summary>
        public void PublishMarketOrder(MarketOrder marketOrder)
        {
            try
            {
                // Message to be published
                IMessage<MarketOrder> message = new Message<MarketOrder>(marketOrder);
                // Publish message
                _easyNetQBus.Publish(_easyNetQExchange, _marketOrderQueueRoutingKey, true, false, message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMarketOrder");
            }
        }

        /// <summary>
        /// Publishes limit order message
        /// </summary>
        public void PublishLimitOrder(LimitOrder limitOrder)
        {
            try
            {
                // Message to be published
                IMessage<LimitOrder> message = new Message<LimitOrder>(limitOrder);
                // Publish message
                _easyNetQBus.Publish(_easyNetQExchange, _limitOrderQueueRoutingKey, true, false, message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishLimitOrder");
            }
        }

        /// <summary>
        /// Publishes cancel order request message
        /// </summary>
        public void PublishCancelOrderRequest(Order order)
        {
            try
            {
                // Message to be published
                IMessage<Order> message = new Message<Order>(order);
                // Publish message
                _easyNetQBus.Publish(_easyNetQExchange, _cancelOrderRequestQueueRoutingKey, true, false, message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishCancelOrderRequest");
            }
        }
    }
}
