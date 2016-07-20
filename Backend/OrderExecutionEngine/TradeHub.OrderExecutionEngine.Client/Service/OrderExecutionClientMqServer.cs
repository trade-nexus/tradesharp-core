using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Spring.Context.Support;
using TraceSourceLogger;
using Disruptor;
using Disruptor.Dsl;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.ValueObjects;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.Heartbeat;
using TradeHub.Common.Core.ValueObjects.Inquiry;
using TradeHub.OrderExecutionEngine.Client.Constants;
using ExchangeType = EasyNetQ.Topology.ExchangeType;
using TradeHubConstants = TradeHub.Common.Core.Constants;

namespace TradeHub.OrderExecutionEngine.Client.Service
{
    /// <summary>
    /// Handles Rabbit MQ communications for Order Execution Engine Client
    /// </summary>
    internal class OrderExecutionClientMqServer : IEventHandler<RabbitMqRequestMessage>, IEventHandler<RabbitMqResponseMessage>
    {
        private Type _type = typeof(OrderExecutionClientMqServer);
        private AsyncClassLogger _asyncClassLogger;

        public event Action BusConnected;
        public event Action ServerDisconnected;
        public event Action<string> LogonArrived;
        public event Action<string> LogoutArrived;
        public event Action<Order> NewArrived;
        public event Action<Order> CancellationArrived;
        public event Action<LimitOrder> LocateMessageArrived;
        public event Action<Execution> ExecutionArrived;
        public event Action<Rejection> RejectionArrived;
        public event Action<InquiryResponse> InquiryResponseArrived;

        #region Rabbit MQ Fields

        // Holds reference for the Advance Bus
        private IAdvancedBus _advancedBus;

        // Native Rabbit MQ Fields
        private ConnectionFactory _rabbitMqBus;
        private IConnection _rabbitMqOrderRequestConnection;
        private IConnection _rabbitMqOrderResponseConnection;
        private IModel _rabbitMqOrderRequestChannel;
        private IModel _rabbitMqOrderResponseChannel;
        private QueueingBasicConsumer _orderConsumer;
        private QueueingBasicConsumer _executionConsumer;

        // Exchange containing Queues
        private IExchange _exchange;

        // Queue will contain Admin messages
        private IQueue _adminMessageQueue;

        // Queue will contain Order messages
        private IQueue _orderQueue;

        // Queue will contain Execution messages
        private IQueue _executionQueue;

        // Queue will contain Rejection messages
        private IQueue _rejectionQueue;

        // Queue will contain Inquiry Response messages
        private IQueue _inquiryResponseQueue;

        // Queue will contain Heartbeat Response from OEE
        private IQueue _heartbeatResponseQueue;
        
        // Queue will contain Locate messages from OEE
        private IQueue _locateMessageQueue;

        #endregion

        private string _applicationId = string.Empty;

        #region Order Request Disruptor

        // Order Request Disruptor Ring Buffer Size 
        private readonly int _orderRequestDisruptorRingSize = 16384; // 65536; // Must be multiple of 2

        // Handles order request messages
        private Disruptor<RabbitMqRequestMessage> _orderRequestDisruptor;

        // Ring buffer to be used with Order Request disruptor
        private RingBuffer<RabbitMqRequestMessage> _orderRequestRingBuffer;

        #endregion

        #region Order Response Disruptor

        // Order Response Disruptor Ring Buffer Size 
        private readonly int _orderResponseDisruptorRingSize = 16384; // 65536; // Must be multiple of 2

        // Handles order response messages
        private Disruptor<RabbitMqResponseMessage> _orderResponseDisruptor;

        // Ring buffer to be used with Order Response disruptor
        private RingBuffer<RabbitMqResponseMessage> _orderResponseRingBuffer;

        // Publisher to send data to Response Disruptor
        private EventPublisher<RabbitMqResponseMessage> _responseDisruptorPublisher;

        #endregion

        private Task _orderResponseConsumerTask;
        private Task _orderExecutionConsumerTask;

        private bool _consumeMessages = false;

        private CancellationTokenSource _orderResponseCancellationToken;
        private CancellationTokenSource _orderExecutionCancellationToken;

        /// <summary>
        /// Duration between successive Heartbeats in milliseconds
        /// </summary>
        private int _heartbeatInterval = 60000;

        /// <summary>
        /// Holds refernce to the Heartbeat Handler
        /// </summary>
        private ClientHeartBeatHandler _heartBeatHandler;

        /// <summary>
        /// Order Execution Engine MQ-Server Parameters
        /// Key = Parameter Name
        /// Value = Parameter Value
        /// </summary>
        private Dictionary<string, string> _oeeMqServerparameters;

        /// <summary>
        /// Key = Parameter Name
        /// Value = Parameter Value
        /// </summary>
        private Dictionary<string, string> _clientMqParameters;

        /// <summary>
        /// Duration between successive Heartbeats in milliseconds
        /// </summary>
        public int HeartbeatInterval
        {
            get { return _heartbeatInterval; }
            set { _heartbeatInterval = value; }
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="oeeMqServerparameters">Contains Parameters required for sending messages to OEE MQ Server</param>
        /// <param name="clientMqParameters">Contains Parameters for getting response messages from OEE MQ Server </param>
        /// <param name="asyncClassLogger">Used for adding logs in a separate file</param>
        public OrderExecutionClientMqServer(Dictionary<string, string> oeeMqServerparameters, Dictionary<string, string> clientMqParameters, AsyncClassLogger asyncClassLogger)
        {
            _asyncClassLogger = asyncClassLogger;
            
            // Save MQ Parameters
            _oeeMqServerparameters = oeeMqServerparameters;
            _clientMqParameters = clientMqParameters;
            
            Initialize();
        }

        /// <summary>
        /// Initializes Disruptor to be used for Order Request Messages
        /// </summary>
        private void InitializeOrderRequestDisruptor()
        {
            // Initialize Order Request Disruptor
            _orderRequestDisruptor = new Disruptor<RabbitMqRequestMessage>(() => new RabbitMqRequestMessage(), _orderRequestDisruptorRingSize, TaskScheduler.Default);

            // Add Order Request Disruptor Consumer
            _orderRequestDisruptor.HandleEventsWith(this);

            // Start Order Request Disruptor
            _orderRequestRingBuffer = _orderRequestDisruptor.Start();
        }

        /// <summary>
        /// Initializes Disruptor to be used for Order Response Messages
        /// </summary>
        private void InitializeOrderResponseDisruptor()
        {
            // Initialize Response Disruptor
            _orderResponseDisruptor = new Disruptor<RabbitMqResponseMessage>(() => new RabbitMqResponseMessage(), _orderRequestDisruptorRingSize, TaskScheduler.Default);

            // Add Rersponse Disruptor Consumer
            _orderResponseDisruptor.HandleEventsWith(this);

            // Start Response Disruptor
            _orderResponseRingBuffer = _orderResponseDisruptor.Start();

            // Initialize Order Response Publisher
            _responseDisruptorPublisher = new EventPublisher<RabbitMqResponseMessage>(_orderResponseRingBuffer);
        }

        #region MQ Initialization

        /// <summary>
        /// Initializes MQ Server related parameters
        /// </summary>
        private void InitializeMqServer()
        {
            try
            {
                // Create Rabbit MQ Hutch 
                string connectionString = _oeeMqServerparameters["ConnectionString"];
                if (!string.IsNullOrEmpty(connectionString))
                {
                    // Initialize EasyNetQ Rabbit Hutch 
                    InitializeRabbitHutch(connectionString);

                    // Initialize Native RabbitMQ Parameters
                    InitializeNativeRabbitMq(connectionString);

                    // Get Exchange Name from Config File
                    string exchangeName = _oeeMqServerparameters["Exchange"];

                    if (!string.IsNullOrEmpty(exchangeName))
                    {
                        // Use the Exchange Name to Initialize Rabbit Exchange
                        InitializeExchange(exchangeName);

                        if (_exchange != null)
                        {
                            // Bind Inquiry Response Queue
                            BindQueue(OrderExecutionClientMqParameters.InquiryResponseQueue, OrderExecutionClientMqParameters.InquiryResponseRoutingKey,
                                      ref _inquiryResponseQueue, _exchange, "");
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "InitializeMqServer");
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
                _advancedBus = _advancedBus ?? RabbitHutch.CreateBus(connectionString).Advanced;

                _advancedBus.Connected += OnBusConnected;
                _advancedBus.Connected += OnBusDisconnected;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "InitializeRabbitHutch");
            }
        }

        /// <summary>
        /// Initializes Native Rabbit MQ resources
        /// </summary>
        public void InitializeNativeRabbitMq(string connectionString)
        {
            try
            {
                // Create Native Rabbit MQ Bus
                _rabbitMqBus = new ConnectionFactory { HostName = connectionString.Split('=')[1] };

                // Create Native Rabbit MQ Connection to Send Request
                _rabbitMqOrderRequestConnection = _rabbitMqBus.CreateConnection();
                // Create Native Rabbit MQ Connection to Received Response
                _rabbitMqOrderResponseConnection = _rabbitMqBus.CreateConnection();

                // Open Native Rabbbit MQ Channel to Send Request
                _rabbitMqOrderRequestChannel = _rabbitMqOrderRequestConnection.CreateModel();
                // Open Native Rabbbit MQ Channel to Received Response
                _rabbitMqOrderResponseChannel = _rabbitMqOrderResponseConnection.CreateModel();
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "InitializeNativeRabbitBus");
            }
        }

        /// <summary>
        /// Initializes RabbitMQ Exchange
        /// </summary>
        private void InitializeExchange(string exchangeName)
        {
            try
            {
                // Initialize specified Exchange
                _exchange = _advancedBus.ExchangeDeclare(exchangeName, ExchangeType.Direct, true, false, true);
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "InitializeExchange");
            }
        }

        /// <summary>
        /// Initialize Queues and perform binding
        /// </summary>
        private void RegisterQueues(IExchange exchange, string appId)
        {
            // Bind Admin Message Queue
            BindQueue(OrderExecutionClientMqParameters.AdminMessageQueue, OrderExecutionClientMqParameters.AdminMessageRoutingKey,
                      ref _adminMessageQueue, exchange, appId);

            // Bind Heartbeat response Queue
            BindQueue(OrderExecutionClientMqParameters.HeartbeatResponseQueue, OrderExecutionClientMqParameters.HeartbeatResponseRoutingKey,
                      ref _heartbeatResponseQueue, exchange, appId);

            // Bind Order Response Queue
            DeclareRabbitMqQueues(OrderExecutionClientMqParameters.OrderMessageQueue,
                                  OrderExecutionClientMqParameters.OrderMessageRoutingKey, exchange.Name, appId);

            // Bind Execution Message Queue
            DeclareRabbitMqQueues(OrderExecutionClientMqParameters.ExecutionMessageQueue,
                                  OrderExecutionClientMqParameters.ExecutionMessageRoutingKey, exchange.Name, appId);

            //// Bind Order response Queue
            //BindQueue(OrderExecutionClientMqParameters.OrderMessageQueue, OrderExecutionClientMqParameters.OrderMessageRoutingKey,
            //          ref _orderQueue, exchange, appId);

            //// Bind Execution message Queue
            //BindQueue(OrderExecutionClientMqParameters.ExecutionMessageQueue, OrderExecutionClientMqParameters.ExecutionMessageRoutingKey,
            //          ref _executionQueue, exchange, appId);
            
            // Bind Rejection message Queue
            BindQueue(OrderExecutionClientMqParameters.RejectionMessageQueue, OrderExecutionClientMqParameters.RejectionMessageRoutingKey,
                      ref _rejectionQueue, exchange, appId);

            // Bind Locate message Queue
            BindQueue(OrderExecutionClientMqParameters.LocateMessageQueue, OrderExecutionClientMqParameters.LocateMessageRoutingKey,
                      ref _locateMessageQueue, exchange, appId);
        }

        /// <summary>
        /// Initializes RabbitMQ Queue
        /// </summary>
        private IQueue InitializeQueue(IExchange exchange, string queueName, string routingKey)
        {
            try
            {
                // Initialize specified Queue
                IQueue queue = _advancedBus.QueueDeclare(queueName, false, false, true, true);

                // Bind Queue to already initialized Exchange with the specified Routing Key
                _advancedBus.Bind(exchange, queue, routingKey);
                return queue;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "InitializeQueue");
                return null;
            }
        }

        /// <summary>
        /// Binds the queue with provided info
        /// </summary>
        private void BindQueue(string queueHeader, string routingKeyHeader, ref IQueue queue, IExchange exchange, string appId)
        {
            try
            {
                // Get Queue Name from Parameters Dictionary
                string queueName = _clientMqParameters[queueHeader] = appId + "_" + _clientMqParameters[queueHeader];
                // Get Routing Key from Parameters Dictionary
                string routingKey = _clientMqParameters[routingKeyHeader] = appId + "." + _clientMqParameters[routingKeyHeader];

                if (!string.IsNullOrEmpty(queueName)
                    && !string.IsNullOrEmpty(routingKey))
                {
                    // Use the initialized Exchange, Queue Name and RoutingKey to initialize Rabbit Queue
                    queue = InitializeQueue(exchange, queueName, routingKey);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "BindQueue");
            }
        }

        /// <summary>
        /// Declares Rabbit Mq Queues
        /// </summary>
        private void DeclareRabbitMqQueues(string queueHeader, string routingKeyHeader, string exchange,string appId)
        {
            try
            {
                // Get Queue Name from Parameters Dictionary
                string queueName = _clientMqParameters[queueHeader] = appId + "_" + _clientMqParameters[queueHeader];
                // Get Routing Key from Parameters Dictionary
                string routingKey = _clientMqParameters[routingKeyHeader] = appId + "." + _clientMqParameters[routingKeyHeader];

                if (!string.IsNullOrEmpty(queueName)
                    && !string.IsNullOrEmpty(routingKey))
                {
                    //Declare Rabbit MQ Queues
                    _rabbitMqOrderResponseChannel.QueueDeclare(queueName, false, false, true, null);
                    _rabbitMqOrderResponseChannel.QueueBind(queueName, exchange, routingKey);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "DeclareRabbitMqQueues");
            }
        }

        /// <summary>
        /// Create Native Rabbit MQ Consumers
        /// </summary>
        private void CreateConsumers()
        {
            try
            {
                // Get Order Queue Name from Parameters Dictionary
                string orderQueueName = _clientMqParameters[OrderExecutionClientMqParameters.OrderMessageQueue];
                // Get Execution Queue Name from Parameters Dictionary
                string executionQueueName = _clientMqParameters[OrderExecutionClientMqParameters.ExecutionMessageQueue];

                // Create Order Response Consumers
                _orderConsumer = new QueueingBasicConsumer(_rabbitMqOrderResponseChannel);
                _executionConsumer = new QueueingBasicConsumer(_rabbitMqOrderResponseChannel);

                _rabbitMqOrderResponseChannel.BasicConsume(orderQueueName, true, _orderConsumer);
                _rabbitMqOrderResponseChannel.BasicConsume(executionQueueName, true, _executionConsumer);
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "CreateConsumers");
            }
        }

        #endregion

        #region Queue Subscriptions

        /// <summary>
        /// Binds the Admin Message Queue 
        /// Starts listening to the incoming Admin Level messages
        /// </summary>
        private void SubscribeAdminMessageQueues()
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Binding Admin Message Queue: " + _adminMessageQueue.Name, _type.FullName,
                                "SubscribeAdminMessageQueues");
                }

                // Listening to Admin Messages
                _advancedBus.Consume<string>(
                    _adminMessageQueue, (msg, messageReceivedInfo) =>
                                        Task.Factory.StartNew(
                                            () =>
                                            {
                                                if (msg.Body.Contains("Logon"))
                                                {
                                                    if (_asyncClassLogger.IsDebugEnabled)
                                                    {
                                                        _asyncClassLogger.Debug("Logon message recieved: " + msg.Body, _type.FullName,
                                                                    "SubscribeAdminMessageQueues");
                                                    }
                                                    if (LogonArrived != null)
                                                    {
                                                        LogonArrived(msg.Body.Remove(0, 6));
                                                    }
                                                }
                                                else if (msg.Body.Contains("Logout"))
                                                {
                                                    if (_asyncClassLogger.IsDebugEnabled)
                                                    {
                                                        _asyncClassLogger.Debug("Logout Message recieved: " + msg.Body, _type.FullName,
                                                                    "SubscribeAdminMessageQueues");
                                                    }
                                                    if (LogoutArrived != null)
                                                    {
                                                        LogoutArrived(msg.Body.Remove(0, 7));
                                                    }
                                                }
                                            }));
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SubscribeAdminMessageQueues");
            }
        }

        /// <summary>
        /// Binds the Inquiry Response Message Queue
        /// Starts listening to the incoming Inquiry Response messages
        /// </summary>
        private void SubscribeInquiryResponseMessageQueue()
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Binding Inquiry Response Message Queue: " + _inquiryResponseQueue.Name, _type.FullName,
                                "SubscribeInquiryResponseMessageQueue");
                }

                // Listening to Inquiry Response Messages
                _advancedBus.Consume<InquiryResponse>(
                    _inquiryResponseQueue, (msg, messageReceivedInfo) =>
                                               Task.Factory.StartNew(() =>
                                               {
                                                   if (_asyncClassLogger.IsDebugEnabled)
                                                   {
                                                       _asyncClassLogger.Debug("Inquiry Response recieved: " + msg.Body.Type, _type.FullName,
                                                                   "SubscribeInquiryResponseMessageQueue");
                                                   }
                                                   if (InquiryResponseArrived != null)
                                                   {
                                                       InquiryResponseArrived(msg.Body);
                                                       if (_asyncClassLogger.IsDebugEnabled)
                                                       {
                                                           _asyncClassLogger.Debug("Inquiry Response event fired: " + msg.Body.Type, _type.FullName,
                                                                       "SubscribeInquiryResponseMessageQueue");
                                                       }
                                                   }
                                               }));
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SubscribeInquiryResponseMessageQueues");
            }
        }

        /// <summary>
        /// Binds the Heartbeat Response Message Queue
        /// Starts listening to the Heartbeat messages
        /// </summary>
        private void SubscribeHeartbeatResponseQueue()
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Binding Heartbeat Response Message Queue: " + _heartbeatResponseQueue.Name, _type.FullName,
                                "SubscribeHeartbeatResponseQueue");
                }

                // Listening to Heartbeat Response Messages
                _advancedBus.Consume<HeartbeatMessage>(
                    _heartbeatResponseQueue, (msg, messageReceivedInfo) =>
                                               Task.Factory.StartNew(() =>
                                               {
                                                   if (_asyncClassLogger.IsDebugEnabled)
                                                   {
                                                       _asyncClassLogger.Debug("Heartbeat receieved", _type.FullName,
                                                                   "SubscribeHeartbeatResponseQueue");
                                                   }

                                                   // handle incoming Heartbeat message
                                                   _heartBeatHandler.Update(msg.Body.HeartbeatInterval);
                                               }));
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SubscribeHeartbeatResponseQueue");
            }
        }

        /// <summary>
        /// Binds the Order Message Queue
        /// Starts listening to incoming Order messages
        /// </summary>
        private void SubscribeOrderMessageQueue()
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Binding Order Message Queue: " + _orderQueue.Name, _type.FullName,
                                "SubscribeOrderMessageQueue");
                }

                // Listening to Order Messages
                _advancedBus.Consume<Order>(
                    _orderQueue, (msg, messageReceivedInfo) =>
                                               Task.Factory.StartNew(() =>
                                               {
                                                   // handle incoming Order message
                                                   HandleNewIncomingOrderMessage(msg.Body);
                                               }));
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SubscribeOrderMessageQueue");
            }
        }

        /// <summary>
        /// Binds the Execution Message Queue
        /// Starts listening to incoming Execution messages
        /// </summary>
        private void SubscribeExecutionMessageQueue()
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Binding Execution Message Queue: " + _executionQueue.Name, _type.FullName,
                                "SubscribeExecutionMessageQueue");
                }

                // Listening to Execution Messages
                _advancedBus.Consume<Execution>(
                    _executionQueue, (msg, messageReceivedInfo) =>
                                               Task.Factory.StartNew(() =>
                                               {
                                                   if (_asyncClassLogger.IsDebugEnabled)
                                                   {
                                                       _asyncClassLogger.Debug("Execution receieved: " + msg.Body, _type.FullName,
                                                                   "SubscribeExecutionMessageQueue");
                                                   }

                                                   // Raise Execution Event
                                                   if (ExecutionArrived != null)
                                                   {
                                                       ExecutionArrived(msg.Body);
                                                   }
                                               }));
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SubscribeExecutionMessageQueue");
            }
        }

        /// <summary>
        /// Binds the Rejection Message Queue
        /// Starts listening to incoming Rejection messages
        /// </summary>
        private void SubscribeRejectionMessageQueue()
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Binding Rejection Message Queue: " + _rejectionQueue.Name, _type.FullName,
                                "SubscribeRejectionMessageQueue");
                }

                // Listening to Rejection Messages
                _advancedBus.Consume<Rejection>(
                    _rejectionQueue, (msg, messageReceivedInfo) =>
                                               Task.Factory.StartNew(() =>
                                               {
                                                   if (_asyncClassLogger.IsDebugEnabled)
                                                   {
                                                       _asyncClassLogger.Debug("Rejeciton receieved", _type.FullName,
                                                                   "SubscribeRejectionMessageQueue");
                                                   }

                                                   // Raise Rejection Event
                                                   if (RejectionArrived != null)
                                                   {
                                                       RejectionArrived(msg.Body);
                                                   }
                                               }));
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SubscribeRejectionMessageQueue");
            }
        }

        /// <summary>
        /// Binds the Locate Message Queue
        /// Starts listening to incoming Locate messages
        /// </summary>
        private void SubscribeLocateMessageQueue()
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Binding Locate Message Queue: " + _rejectionQueue.Name, _type.FullName,
                                "SubscribeLocateMessageQueue");
                }

                // Listening to Rejection Messages
                _advancedBus.Consume<LimitOrder>(
                    _locateMessageQueue, (msg, messageReceivedInfo) =>
                                               Task.Factory.StartNew(() =>
                                               {
                                                   if (_asyncClassLogger.IsDebugEnabled)
                                                   {
                                                       _asyncClassLogger.Debug("Locate message receieved", _type.FullName,
                                                                   "SubscribeLocateMessageQueue");
                                                   }

                                                   // Raise Rejection Event
                                                   if (LocateMessageArrived != null)
                                                   {
                                                       LocateMessageArrived(msg.Body);
                                                   }
                                               }));
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SubscribeLocateMessageQueue");
            }
        }

        /// <summary>
        /// Consumes Order Response from queue
        /// </summary>
        private void ConsumeOrderResponseQueue()
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Starting order response consumer", _type.FullName, "ConsumeOrderResponseQueue");
                }

                while (_consumeMessages)
                {
                    BasicDeliverEventArgs ea = (BasicDeliverEventArgs)_orderConsumer.Queue.Dequeue();

                    // Get message bytes
                    byte[] body = ea.Body;

                    // Publish Events to Disruptor
                    _responseDisruptorPublisher.PublishEvent((entry, sequenceNo) =>
                    {
                        entry.Message = new byte[body.Length];
                        body.CopyTo(entry.Message, 0);
                        entry.Type = MessageType.Order;
                        return entry;
                    });
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "ConsumeOrderResponseQueue");
            }
        }

        /// <summary>
        /// Consumes Order Executions from queue
        /// </summary>
        private void ConsumeExecutionQueue()
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Starting order execution consumer", _type.FullName, "ConsumeExecutionQueue");
                }

                while (_consumeMessages)
                {
                    BasicDeliverEventArgs ea = (BasicDeliverEventArgs)_executionConsumer.Queue.Dequeue();

                    // Get message bytes
                    byte[] body = ea.Body;

                    // Publish Events to Disruptor
                    _responseDisruptorPublisher.PublishEvent((entry, sequenceNo) =>
                    {
                        entry.Message = new byte[body.Length];
                        body.CopyTo(entry.Message, 0);
                        entry.Type = MessageType.Execution;
                        return entry;
                    });
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "ConsumeExecutionQueue");
            }
        }

        #endregion

        #region Outgoing message to Order Execution Engine

        /// <summary>
        /// Sends TradeHub Inquiry Message to MQ Exchange on the depending routing key
        /// </summary>
        /// <param name="inquiry">TradeHub Inquiry Message</param>
        public void SendInquiryMessage(InquiryMessage inquiry)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Inquiry message recieved for publishing", _type.FullName, "SendInquiryMessage");
                }

                string routingKey;
                if (_oeeMqServerparameters.TryGetValue(OeeMqServerParameters.InquiryRoutingKey, out routingKey))
                {
                    Message<InquiryMessage> inquiryMessage = new Message<InquiryMessage>(inquiry);
                    inquiryMessage.Properties.AppId = _applicationId;
                    inquiryMessage.Properties.ReplyTo = _clientMqParameters[OrderExecutionClientMqParameters.InquiryResponseRoutingKey];

                    // Send Message for publishing
                    PublishMessages(inquiryMessage, routingKey);
                }
                else
                {
                    _asyncClassLogger.Info("Inquiry message not sent for publishing as routing key is unavailable.", _type.FullName,
                                "SendInquiryMessage");
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendInquiryMessage");
            }
        }

        /// <summary>
        /// Sends TradeHub Login Message to MQ Exchange on the depending routing key
        /// </summary>
        /// <param name="login">TradeHub Login Message</param>
        public void SendLoginMessage(Login login)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Login message recieved for publishing", _type.FullName, "SendLoginMessage");
                }

                string routingKey;
                if (_oeeMqServerparameters.TryGetValue(OeeMqServerParameters.LoginRoutingKey, out routingKey))
                {
                    // Create EasyNetQ message for publishing
                    Message<Login> loginMessage = new Message<Login>(login);
                    loginMessage.Properties.AppId = _applicationId;
                    loginMessage.Properties.ReplyTo = _clientMqParameters[OrderExecutionClientMqParameters.AdminMessageRoutingKey];

                    // Send Message for publishing
                    PublishMessages(loginMessage, routingKey);
                }
                else
                {
                    _asyncClassLogger.Info("Login message not sent for publishing as routing key is unavailable.", _type.FullName,
                                "SendLoginMessage");
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendLoginMessage");
            }
        }

        /// <summary>
        /// Sends TradeHub Logout Message to MQ Exchange on the depending routing key
        /// </summary>
        /// <param name="logout">TradeHub Logout Message</param>
        public void SendLogoutMessage(Logout logout)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Logout message recieved for publishing", _type.FullName, "SendLogoutMessage");
                }

                string routingKey;
                if (_oeeMqServerparameters.TryGetValue(OeeMqServerParameters.LogoutRoutingKey, out routingKey))
                {
                    // Create EasyNetQ message for publishing
                    Message<Logout> logoutMessage = new Message<Logout>(logout);
                    logoutMessage.Properties.AppId = _applicationId;
                    logoutMessage.Properties.ReplyTo = _clientMqParameters[OrderExecutionClientMqParameters.AdminMessageRoutingKey];

                    // Send Message for publishing
                    PublishMessages(logoutMessage, routingKey);
                }
                else
                {
                    _asyncClassLogger.Info("Logout message not sent for publishing as routing key is unavailable.", _type.FullName,
                                "SendLogoutMessage");
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendLogoutMessage");
            }
        }

        /// <summary>
        /// Sends TradeHub Market Order request to MQ Exchange on the depending routing key
        /// </summary>
        /// <param name="marketOrder">TradeHub Market Order object</param>
        public void SendMarketOrderRequestMessage(MarketOrder marketOrder)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Market Order request recieved for publishing", _type.FullName, "SendMarketOrderRequestMessage");
                }

                string routingKey;
                if (_oeeMqServerparameters.TryGetValue(OeeMqServerParameters.MarketOrderRoutingKey, out routingKey))
                {
                    // Create EasyNetQ message for publishing
                    Message<MarketOrder> marketOrderMessage = new Message<MarketOrder>(marketOrder);
                    marketOrderMessage.Properties.AppId = _applicationId;

                    // Send Message for publishing
                    PublishMessages(marketOrderMessage, routingKey);
                }
                else
                {
                    _asyncClassLogger.Info("Market Order request not sent for publishing as routing key is unavailable.", _type.FullName,
                                "SendMarketOrderRequestMessage");
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendMarketOrderRequestMessage");
            }
        }

        /// <summary>
        /// Sends TradeHub Limit Order request to MQ Exchange on the depending routing key
        /// </summary>
        /// <param name="limitOrder">TradeHub Limit Order object</param>
        public void SendLimitOrderRequestMessage(LimitOrder limitOrder)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Limit Order request recieved for publishing", _type.FullName, "SendLimitOrderRequestMessage");
                }

                string routingKey;
                if (_oeeMqServerparameters.TryGetValue(OeeMqServerParameters.LimitOrderRoutingKey, out routingKey))
                {
                    // Create EasyNetQ message for publishing
                    Message<LimitOrder> limitOrderMessage = new Message<LimitOrder>(limitOrder);
                    limitOrderMessage.Properties.AppId = _applicationId;

                    // Send Message for publishing
                    PublishMessages(limitOrderMessage, routingKey);
                }
                else
                {
                    _asyncClassLogger.Info("Limit Order request not sent for publishing as routing key is unavailable.", _type.FullName,
                                "SendLimitOrderRequestMessage");
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendLimitOrderRequestMessage");
            }
        }

        /// <summary>
        /// Sends TradeHub Cancel Order request to MQ Exchange on the depending routing key
        /// </summary>
        /// <param name="cancelOrder">TradeHub Order object</param>
        public void SendCancelOrderRequestMessage(Order cancelOrder)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Cancel Order request recieved for publishing", _type.FullName, "SendCancelOrderRequestMessage");
                }

                string routingKey;
                if (_oeeMqServerparameters.TryGetValue(OeeMqServerParameters.CancelOrderRoutingKey, out routingKey))
                {
                    // Create EasyNetQ message for publishing
                    Message<Order> cancelOrderMessage = new Message<Order>(cancelOrder);
                    cancelOrderMessage.Properties.AppId = _applicationId;

                    // Send Message for publishing
                    PublishMessages(cancelOrderMessage, routingKey);
                }
                else
                {
                    _asyncClassLogger.Info("Cancel Order request not sent for publishing as routing key is unavailable.", _type.FullName,
                                "SendCancelOrderRequestMessage");
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendCancelOrderRequestMessage");
            }
        }

        /// <summary>
        /// Sends TradeHub Locate Response to MQ Exchange on the depending routing key
        /// </summary>
        /// <param name="locateResponse">TradeHub Order object</param>
        public void SendLocateResponse(LocateResponse locateResponse)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Locate response recieved for publishing", _type.FullName, "SendLocateResponse");
                }

                string routingKey;
                if (_oeeMqServerparameters.TryGetValue(OeeMqServerParameters.LocateResponseRoutingKey, out routingKey))
                {
                    // Create EasyNetQ message for publishing
                    Message<LocateResponse> locateResponseMessage = new Message<LocateResponse>(locateResponse);
                    locateResponseMessage.Properties.AppId = _applicationId;

                    // Send Message for publishing
                    PublishMessages(locateResponseMessage, routingKey);
                }
                else
                {
                    _asyncClassLogger.Info("Locate response not sent for publishing as routing key is unavailable.", _type.FullName,
                                "SendLocateResponse");
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendLocateResponse");
            }
        }

        /// <summary>
        /// Sends Order Resquest to MQ Exchange on the depending routing key
        /// </summary>
        /// <param name="rabbitMqMessage">Contains order stream to be published</param>
        public void SendOrderRequests(RabbitMqRequestMessage rabbitMqMessage)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Order Request recieved for publishing", _type.FullName, "SendOrderRequests");
                }

                string routingKey;
                if (_oeeMqServerparameters.TryGetValue(OeeMqServerParameters.OrderRequestRoutingKey, out routingKey))
                {
                     //Send Message for publishing
                    PublishOrderRequests(rabbitMqMessage.Message, routingKey);
                }
                else
                {
                    _asyncClassLogger.Info("Order request not sent for publishing as routing key is unavailable.", _type.FullName,
                                "SendOrderRequests");
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendOrderRequests");
            }
        }

        #endregion

        #region Publish Messages to MQ Exchange

        /// <summary>
        /// Publishes Login messages to MQ Exchange
        /// </summary>
        private void PublishMessages(Message<Login> loginMessage, string routingKey)
        {
            try
            {
                // Publish Messages to respective Queues
                _advancedBus.Publish(_exchange, routingKey, true, false, loginMessage);

                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Login request published", _type.FullName, "PublishMessages");
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes Logout messages to MQ Exchange
        /// </summary>
        private void PublishMessages(Message<Logout> logoutMessage, string routingKey)
        {
            try
            {
                // Publish Messages to respective Queues
                _advancedBus.Publish(_exchange, routingKey, true, false, logoutMessage);

                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Logout request published", _type.FullName, "PublishMessages");
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes Market Order messages to MQ Exchange
        /// </summary>
        private void PublishMessages(Message<MarketOrder> marketOrderMessage, string routingKey)
        {
            try
            {
                // Publish Messages to respective Queues
                _advancedBus.Publish(_exchange, routingKey, true, false, marketOrderMessage);

                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Market Order request published", _type.FullName, "PublishMessages");
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes Limit Order messages to MQ Exchange
        /// </summary>
        private void PublishMessages(Message<LimitOrder> limitOrderMessage, string routingKey)
        {
            try
            {
                // Publish Messages to respective Queues
                _advancedBus.Publish(_exchange, routingKey, true, false, limitOrderMessage);

                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Limit Order request published", _type.FullName, "PublishMessages");
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes Cancel Order messages to MQ Exchange
        /// </summary>
        private void PublishMessages(Message<Order> cancelOrderMessage, string routingKey)
        {
            try
            {
                // Publish Messages to respective Queues
                _advancedBus.Publish(_exchange, routingKey, true, false, cancelOrderMessage);

                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Cancel Order request published", _type.FullName, "PublishMessages");
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes Inquiry messages to MQ Exchange
        /// </summary>
        private void PublishMessages(Message<InquiryMessage> inquiryMessage, string routingKey)
        {
            try
            {
                //using (var channel = _advancedBus.OpenPublishChannel())
                {
                    // Publish Messages to respective Queues
                    _advancedBus.Publish(_exchange, routingKey, true, false, inquiryMessage);

                    if (_asyncClassLogger.IsDebugEnabled)
                    {
                        _asyncClassLogger.Debug("Inquiry request published", _type.FullName, "PublishMessages");
                    }
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes Locate response messages to MQ Exchange
        /// </summary>
        private void PublishMessages(Message<LocateResponse> locateResponse, string routingKey)
        {
            try
            {
                // Publish Messages to respective Queues
                _advancedBus.Publish(_exchange, routingKey, true, false, locateResponse);

                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Locate response published", _type.FullName, "PublishMessages");
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes Order Requests message to MQ Exchange using Native Rabbit MQ API
        /// </summary>
        /// <param name="messageBytes">byte message to be published</param>
        /// <param name="routingKey">queue routing on which to publish on</param>
        private void PublishOrderRequests(byte[] messageBytes, string routingKey)
        {
            try
            {
                // Get next sequence number
                long sequenceNo = _orderRequestRingBuffer.Next();

                // Get object from ring buffer
                RabbitMqRequestMessage entry = _orderRequestRingBuffer[sequenceNo];

                // Update object values
                entry.RequestTo = routingKey;
                entry.Message = messageBytes;

                // Publish sequence number for which the object is updated
                _orderRequestRingBuffer.Publish(sequenceNo);
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "PublishOrderRequests");
            }
        }

        #endregion

        #region Start/Shutdown
        
        /// <summary>
        /// Initializes all necessary connections for communication - Needs to be called manually if client MQ-Server was disconnected
        /// </summary>
        public void Initialize()
        {
            // Initiliaze Cancellation Tokens
            _orderResponseCancellationToken = new CancellationTokenSource();
            _orderExecutionCancellationToken = new CancellationTokenSource();

            // Initialize Disruptors
            InitializeOrderRequestDisruptor();
            InitializeOrderResponseDisruptor();

            // Initialize MQ Server for communication
            InitializeMqServer();

            // Bind Inquiry Reponse Message Queue
            SubscribeInquiryResponseMessageQueue();
        }

        /// <summary>
        /// Connects the Rabbit MQ session
        /// </summary>
        /// <param name="appId">Unique Application ID</param>
        public void Connect(string appId)
        {
            _applicationId = appId;

            // Register Reqired Queues
            RegisterQueues(_exchange, appId);

            // Susbcribing Admin Message Queue
            SubscribeAdminMessageQueues();

            // Susbcribing Heartbeat Message Queue
            SubscribeHeartbeatResponseQueue();

            //Create Order And Execution Consumer
            CreateConsumers();

            //// Susbcribing Order Message Queue
            //SubscribeOrderMessageQueue();

            //// Subscribing Execution Message Queue
            //SubscribeExecutionMessageQueue();

            // Subscribing Rejection Message Queue
            SubscribeRejectionMessageQueue();

            _consumeMessages = true;

            //Consumer for Order Response messages
            _orderResponseConsumerTask = Task.Factory.StartNew(ConsumeOrderResponseQueue, _orderResponseCancellationToken.Token);

            // Consumer for Order Execution Messages
            _orderExecutionConsumerTask = Task.Factory.StartNew(ConsumeExecutionQueue, _orderExecutionCancellationToken.Token);

            // Subscribing Locate Message Queue
            SubscribeLocateMessageQueue();
        }

        /// <summary>
        /// Disconnects the running intance of the Rabbit Hutch
        /// </summary>
        public void Disconnect()
        {
            try
            {
                // Dispose Rabbit Bus
                if (_advancedBus != null)
                {
                    if (_heartBeatHandler != null)
                    {
                        _heartBeatHandler.StopHandler();
                        _heartBeatHandler.StopValidationTimer();
                    }

                    _orderRequestDisruptor.Shutdown();
                    _orderResponseDisruptor.Shutdown();

                    _orderExecutionCancellationToken.Cancel();
                    _orderResponseCancellationToken.Cancel();

                    _consumeMessages = false;
                    
                    _advancedBus.Dispose();
                    _rabbitMqOrderRequestChannel.Close();
                    _rabbitMqOrderResponseChannel.Close();
                    _rabbitMqOrderRequestConnection.Close();
                    _rabbitMqOrderResponseConnection.Close();
                    
                    _advancedBus.Connected -= OnBusConnected;
                    _advancedBus.Connected -= OnBusDisconnected;

                    _advancedBus = null;
                    _rabbitMqOrderRequestChannel = null;
                    _rabbitMqOrderResponseChannel = null;
                    _rabbitMqOrderRequestConnection = null;
                    _rabbitMqOrderResponseConnection = null;

                    if (_asyncClassLogger.IsInfoEnabled)
                    {
                        _asyncClassLogger.Info("Advanced Bus disposed off.", _type.FullName, "Disconnect");
                    }
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "Disconnect");
            }
        }

        #endregion

        /// <summary>
        /// Sends Application Response Routing Keys Info to OEE
        /// </summary>
        /// <param name="appId">Unique Application ID</param>
        public void SendAppInfoMessage(string appId)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Sending Application Info to Order Execution Engine", _type.FullName, "SendAppInfoMessage");
                }

                // Get Application Info Message
                var appInfo = CreateAppInfoMessage(appId);

                if (appInfo != null)
                {
                    var appInfoMessage = new Message<Dictionary<string, string>>(appInfo);
                    appInfoMessage.Properties.AppId = appId;
                    string routingKey = _oeeMqServerparameters[Constants.OeeMqServerParameters.AppInfoRoutingKey];

                    //using (var channel = _advancedBus.OpenPublishChannel())
                    {
                        // Publish Messages to respective Queues
                        _advancedBus.Publish(_exchange, routingKey, true, false, appInfoMessage);

                        if (_asyncClassLogger.IsDebugEnabled)
                        {
                            _asyncClassLogger.Debug("Application Info published", _type.FullName, "PublishMessages");
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendAppInfoMessage");
            }
        }

        /// <summary>
        /// Sends Heartbeat message to OEE
        /// </summary>
        private void OnSendHeartbeat(HeartbeatMessage heartbeat)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Sending heartbeat message to OEE", _type.FullName, "OnSendHeartbeat");
                }

                string routingKey = _oeeMqServerparameters["HeartbeatRoutingKey"];

                //using (var channel = _advancedBus.OpenPublishChannel())
                {
                    // Add the Routing Key to which the MDE-Server can respond to
                    heartbeat.ReplyTo = _clientMqParameters[OrderExecutionClientMqParameters.HeartbeatResponseRoutingKey];

                    // Create EasyNetQ Message for publishing
                    IMessage<HeartbeatMessage> heartbeatMessage = new Message<HeartbeatMessage>(heartbeat);

                    // Publish Messages to respective Queue
                    _advancedBus.Publish(_exchange, routingKey, true, false, heartbeatMessage);

                    if (_asyncClassLogger.IsDebugEnabled)
                    {
                        _asyncClassLogger.Debug("Application Info published", _type.FullName, "PublishMessages");
                    }
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnSendHeartbeat");
            }
        }

        /// <summary>
        /// Raised when Server connection is lost
        /// </summary>
        private void OnServerDisconnected()
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Server Disconnected event resceived from Heartbeat handler", _type.FullName, "OnServerDisconnected");
                }

                // Raise event to Notify about lost connection
                if (ServerDisconnected != null)
                {
                    ServerDisconnected();
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnServerDisconnected");
            }
        }
        
        /// <summary>
        /// Starts the Heartbeat sequence
        /// </summary>
        public void StartHeartbeat()
        {
            if (_heartBeatHandler == null)
            {
                _heartBeatHandler = new ClientHeartBeatHandler(_applicationId, _asyncClassLogger, _heartbeatInterval);
                _heartBeatHandler.SendHeartbeat += OnSendHeartbeat;
                _heartBeatHandler.ServerDisconnected += OnServerDisconnected;
            }

            _heartBeatHandler.StartHandler();
            _heartBeatHandler.StartValidationTimer();
        }

        /// <summary>
        /// Stops the Heartbeat sequence
        /// </summary>
        public void StopHeartbeat()
        {
            _heartBeatHandler.StopHandler();
        }

        /// <summary>
        /// Handles New Incoming Order message from OEE
        /// </summary>
        /// <param name="order">TradeHub Order object</param>
        private void HandleNewIncomingOrderMessage(Order order)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Incoming Order message received from OEE", _type.FullName, "HandleNewIncomingOrderMessage");
                }

                // Raise New Order Event
                if (order.OrderStatus.Equals(TradeHubConstants.OrderStatus.SUBMITTED))
                {
                    if (NewArrived != null)
                    {
                        NewArrived(order);
                    }
                }
                // Raise Cancel Order Event
                else if (order.OrderStatus.Equals(TradeHubConstants.OrderStatus.CANCELLED))
                {
                    if (CancellationArrived!=null)
                    {
                        CancellationArrived(order);
                    }
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "HandleNewIncomingOrderMessage");
            }
        }

        /// <summary>
        /// Creates Application Info message
        /// </summary>
        /// <param name="appId">Unique Application ID</param>
        private Dictionary<string, string> CreateAppInfoMessage(string appId)
        {
            try
            {
                var appInfo = new Dictionary<string, string>();

                // Add Admin Info;
                appInfo.Add("Admin", _clientMqParameters[OrderExecutionClientMqParameters.AdminMessageRoutingKey]);

                // Add Tick Info
                appInfo.Add("Order", _clientMqParameters[OrderExecutionClientMqParameters.OrderMessageRoutingKey]);

                // Add Live Bar Info
                appInfo.Add("Execution", _clientMqParameters[OrderExecutionClientMqParameters.ExecutionMessageRoutingKey]);

                // Add Historic Info
                appInfo.Add("Rejection", _clientMqParameters[OrderExecutionClientMqParameters.RejectionMessageRoutingKey]);

                // Add Locate Info
                appInfo.Add("Locate", _clientMqParameters[OrderExecutionClientMqParameters.LocateMessageRoutingKey]);

                return appInfo;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "CreateAppInfoMessage");
                return null;
            }
        }

        #region Message Parsing

        /// <summary>
        /// Parses incoming message array to create TradeHub Order
        /// </summary>
        /// <param name="messageArray">string array containing order information</param>
        /// <returns></returns>
        private Order ParseOrder(string[] messageArray)
        {
            try
            {
                Order order = new Order(messageArray[8]);
                // Get Order ID
                order.OrderID = messageArray[1];
                // Get Order Side
                order.OrderSide = messageArray[2];
                // Get Order Size
                order.OrderSize = Convert.ToInt32(messageArray[3]);
                // Get Order TIF value
                order.OrderTif = messageArray[4];
                // Get Order TIF value
                order.OrderStatus = messageArray[5];
                // Get Symbol
                order.Security = new Security() { Symbol = messageArray[6] };
                // Get Time Value
                order.OrderDateTime = DateTime.ParseExact(messageArray[7], "M/d/yyyy h:mm:ss.fff tt", CultureInfo.InvariantCulture);
                // Get Exchange Value
                order.Exchange = messageArray[11];
                // Return value
                return order;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "ParseOrder");
                return null;
            }
        }

        /// <summary>
        /// Parses incoming message array to create TradeHub Execution
        /// </summary>
        /// <param name="messageArray">string array containing Order Execution Information</param>
        /// <returns></returns>
        private Execution ParseExecution(string[] messageArray)
        {
            try
            {
                // Extract Fill Information
                Fill fill = new Fill(new Security() {Symbol = messageArray[3]}, messageArray[9], messageArray[0])
                    {
                        ExecutionPrice = Convert.ToDecimal(messageArray[4]),
                        ExecutionSize = Convert.ToInt32(messageArray[5]),
                        AverageExecutionPrice = Convert.ToDecimal(messageArray[6]),
                        LeavesQuantity = Convert.ToInt32(messageArray[7]),
                        CummalativeQuantity = Convert.ToInt32(messageArray[8]),
                        ExecutionDateTime = DateTime.ParseExact(messageArray[10], "M/d/yyyy h:mm:ss.fff tt", CultureInfo.InvariantCulture),
                        ExecutionId = messageArray[12],
                        ExecutionSide = messageArray[13]
                    };

                // Extract Order Information
                Order order = new Order(messageArray[9])
                    {
                        OrderID = messageArray[0],
                        OrderSide = messageArray[1],
                        OrderSize = Convert.ToInt32(messageArray[2]),
                        Security = new Security() { Symbol = messageArray[3] }
                    };

                // Add Info to Execution Object
                Execution execution = new Execution(fill, order);

                // Return Information
                return execution;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "ParseExecution");
                return null;
            }
        }

        #endregion

        /// <summary>
        /// Raised when Advanced Bus is successfully connected
        /// </summary>
        private void OnBusConnected()
        {
            if (_advancedBus.IsConnected)
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Successfully connected to MQ Server", _type.FullName, "OnBusConnected");
                }

                if (BusConnected != null)
                {
                    BusConnected();
                }
            }
        }

        /// <summary>
        /// Raised when Advanced Bus is successfully disconnected
        /// </summary>
        private void OnBusDisconnected()
        {
            if (_advancedBus.IsConnected)
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Successfully disconnected to MQ Server", _type.FullName, "OnBusDisconnected");
                }
            }
        }

        #region Implementation of IEventHandler<in RabbitMqRequestMessage>

        /// <summary>
        /// Called when a publisher has committed an event to the <see cref="T:Disruptor.RingBuffer`1"/>
        /// </summary>
        /// <param name="data">Data committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="sequence">Sequence number committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="endOfBatch">flag to indicate if this is the last event in a batch from the <see cref="T:Disruptor.RingBuffer`1"/></param>
        public void OnNext(RabbitMqRequestMessage data, long sequence, bool endOfBatch)
        {
            string corrId = Guid.NewGuid().ToString();
            IBasicProperties replyProps = _rabbitMqOrderRequestChannel.CreateBasicProperties();
            replyProps.CorrelationId = corrId;

            // Publish Order Reqeusts to MQ Exchange 
            _rabbitMqOrderRequestChannel.BasicPublish(_exchange.Name, data.RequestTo, replyProps, data.Message);

            if (_asyncClassLogger.IsDebugEnabled)
            {
                _asyncClassLogger.Debug("Order request published", _type.FullName, "OnNext");
            }
        }

        #endregion

        #region Implementation of IEventHandler<in RabbitMqResponseMessage>

        /// <summary>
        /// Called when a publisher has committed an event to the <see cref="T:Disruptor.RingBuffer`1"/>
        /// </summary>
        /// <param name="data">Data committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="sequence">Sequence number committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="endOfBatch">flag to indicate if this is the last event in a batch from the <see cref="T:Disruptor.RingBuffer`1"/></param>
        public void OnNext(RabbitMqResponseMessage data, long sequence, bool endOfBatch)
        {
            string message = Encoding.UTF8.GetString(data.Message);
            var messageArray = message.Split(',');

            // Parse Message into TradeHub Order
            if (data.Type.Equals(MessageType.Order))
            {
                var order = ParseOrder(messageArray);
                if (order != null)
                {
                    HandleNewIncomingOrderMessage(order);
                }
            }
            // Parse Message into TradeHub Execution
            else if (data.Type.Equals(MessageType.Execution))
            {
                var execution = ParseExecution(messageArray);
                if (execution != null)
                {
                    // Raise Execution Event
                    if (ExecutionArrived != null)
                    {
                        ExecutionArrived(execution);
                    }
                }
            }
        }

        #endregion
    }
}
