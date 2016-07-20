using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Disruptor;
using Disruptor.Dsl;
using EasyNetQ;
using EasyNetQ.Topology;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.ValueObjects;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.Heartbeat;
using TradeHub.Common.Core.ValueObjects.Inquiry;
using TradeHub.OrderExecutionEngine.Configuration.HeartBeat;
using ExchangeType = EasyNetQ.Topology.ExchangeType;

namespace TradeHub.OrderExecutionEngine.Configuration.Service
{
    /// <summary>
    /// Handles All MQ Communications
    /// </summary>
    public class OrderExecutionMqServer : IEventHandler<RabbitMqRequestMessage>,IEventHandler<RabbitMqResponseMessage>
    {
        private readonly Type _type = typeof(OrderExecutionMqServer);

        #region Private Fields
        //Holds Position Routing key
        private string _postionRoutingKey;

        // Holds reference for the Advance Bus
        private IAdvancedBus _rabbitBus;

        // Exchange containing Queues
        private IExchange _exchange;

        // Native Rabbit MQ Fields
        private ConnectionFactory _rabbitMqOrderBus;
        private IConnection _rabbitMqOrderRequestConnection;
        private IConnection _rabbitMqOrderResponseConnection;
        private IModel _rabbitMqOrderRequestChannel;
        private IModel _rabbitMqOrderResponseChannel;
        private QueueingBasicConsumer _orderRequestConsumer;

        #region Queues

        // Queue will contain Inquiry messages
        private IQueue _inquiryMessageQueue;

        // Queue will contain App Info messages
        private IQueue _appInfoQueue;

        // Queue will contain Heartbeat messages
        private IQueue _heartbeatQueue;

        // Queue will contain Login messages
        private IQueue _loginMessageQueue;

        // Queue will contain Logout messages
        private IQueue _logoutMessageQueue;

        // Queue will contain Limit Order messages
        private IQueue _limitOrderQueue;

        // Queue will contain Market Order messages
        private IQueue _marketOrderQueue;

        // Queue will contain Stop Order messages
        private IQueue _stopOrderQueue;

        // Queue will contain Stop Limit Order messages
        private IQueue _stopLimitOrderQueue;

        // Queue will contain Cancel Order Messages
        private IQueue _cancelOrderQueue;

        // Queue will contain Cancel Order Messages
        private IQueue _locateResponseQueue;

        #endregion

        // Holds reference for the Heartbeat Handler
        private HeartBeatHandler _heartBeatHandler;

        //Name of the Configuration File
        private readonly string _configFile;

        private readonly int _ringSize = 65536;  // Must be multiple of 2

        // Handles Order Request Messages to be processed
        private Disruptor<RabbitMqRequestMessage> _orderRequestDisruptor;
        private RingBuffer<RabbitMqRequestMessage> _orderRequestRingBuffer;

        // Handles Order Response Messages to be sent to Client
        private Disruptor<RabbitMqResponseMessage> _orderResponseDisruptor;
        private RingBuffer<RabbitMqResponseMessage> _orderResponseRingBuffer;
        private EventPublisher<RabbitMqResponseMessage> _responseDisruptorPublisher;

        // Dedicated task to consume order requests
        private Task _orderRequestConsumerTask;

        #endregion

        #region Publis Events

        public event Action<string> DisconnectApplication;
        public event Action<IMessage<Login>> LogonRequestRecieved;
        public event Action<IMessage<Logout>> LogoutRequestRecieved;
        public event Action<IMessage<InquiryMessage>> InquiryRequestReceived;
        public event Action<IMessage<Dictionary<string, string>>> AppInfoReceived;
        //public event Action<IMessage<LimitOrder>> LimitOrderRequestRecieved;
        //public event Action<IMessage<MarketOrder>> MarketOrderRequestRecieved;
        public event Action<IMessage<StopOrder>> StopOrderRequestRecieved;
        public event Action<IMessage<StopLimitOrder>> StopLimitOrderRequestRecieved;
        //public event Action<IMessage<Order>> CancelOrderRequestRecieved;

        public event Action<LimitOrder, string> LimitOrderRequestRecieved;
        public event Action<MarketOrder, string> MarketOrderRequestRecieved;
        public event Action<Order, string> CancelOrderRequestRecieved;

        public event Action<IMessage<LocateResponse>> LocateResponseRecieved;
        // public event Action<IMessage<Position>> PositionMessageRecieved; 

        #endregion

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="configFile">Name of the configuration file to read</param>
        /// <param name="heartbeatThreshold">Duration to wait for the expected heartbeat before disconnecting</param>
        /// <param name="heartbeatResponseInterval">Timer after which server will send Heartbeat to connecting Applications</param>
        public OrderExecutionMqServer(string configFile, int heartbeatThreshold, int heartbeatResponseInterval)
        {
            //Initialize Disruptor
            InitializeOrderRequestDisruptor();
            InitializeOrderResponseDisruptor();

            _configFile = configFile;
            _heartBeatHandler = new HeartBeatHandler(heartbeatThreshold, heartbeatResponseInterval);
            _heartBeatHandler.ApplicationDisconnect += OnApplicationDisconnect;
            _heartBeatHandler.ResponseHeartbeat += OnResponseHeartbeat;
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

        /// <summary>
        /// Initializes Order Request Disruptor
        /// </summary>
        private void InitializeOrderRequestDisruptor()
        {
            _orderRequestDisruptor = new Disruptor<RabbitMqRequestMessage>(() => new RabbitMqRequestMessage(), _ringSize, TaskScheduler.Default);
            _orderRequestDisruptor.HandleEventsWith(this);
            _orderRequestRingBuffer = _orderRequestDisruptor.Start();
        }

        /// <summary>
        /// Initializes Order Response Disruptor
        /// </summary>
        private void InitializeOrderResponseDisruptor()
        {
            _orderResponseDisruptor = new Disruptor<RabbitMqResponseMessage>(() => new RabbitMqResponseMessage(), _ringSize, TaskScheduler.Default);
            _orderResponseDisruptor.HandleEventsWith(this);
            _orderResponseRingBuffer = _orderResponseDisruptor.Start();
            _responseDisruptorPublisher = new EventPublisher<RabbitMqResponseMessage>(_orderResponseRingBuffer);
        }

        #region StartUp

        public void Connect()
        {
            // Initializes MQ resources
            IntitializeMqServices();

            // Bind Admin Message Queue
            SubscribeAdminMessageQueues();

            // Bind Application Info Message Queue
            SubscribeAppInfoQueue();

            // Bind Inquiry Message Queue
            SubscribeInquiryMessageQueue();

            // Bind Heartbeat Queue
            SubscribeHeartbeatQueue();

            // Start Consuming Order Request Queue
            _orderRequestConsumerTask = Task.Factory.StartNew(ConsumeOrderRequestQueue);

            // Bind Locate Response Queue
            SubscribeLocateResponseQueues();
        }

        /// <summary>
        /// Initializes the required parameters and fields for the Rabbit MQ service
        /// </summary>
        public void IntitializeMqServices()
        {
            try
            {
                // Create Rabbit MQ Hutch 
                string connectionString = ReadConfigSettings("ConnectionString");
                if (!string.IsNullOrEmpty(connectionString))
                {
                    // Initialize EasyNetQ Rabbit Hutch 
                    InitializeRabbitHutch(connectionString);

                    // Initialize Native RabbitMQ Parameters
                    InitializeNativeRabbitMq(connectionString);

                    // Get Exchange Name from Config File
                    string exchangeName = ReadConfigSettings("Exchange");

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
        /// Initializes queues and performs bindings
        /// </summary>
        private void RegisterQueues(IExchange exchange)
        {
            try
            {
                // Bind Inquiry Queue
                BindQueue("InquiryQueue", "InquiryRoutingKey", ref _inquiryMessageQueue, exchange);

                // Bind App Info Queue
                BindQueue("AppInfoQueue", "AppInfoRoutingKey", ref _appInfoQueue, exchange);

                // Bind Heartbeat Queue
                BindQueue("HeartbeatQueue", "HeartbeatRoutingKey", ref _heartbeatQueue, exchange);

                // Bind Login Queue
                BindQueue("LoginQueue", "LoginRoutingKey", ref _loginMessageQueue, exchange);

                // Bind Logout Queue
                BindQueue("LogoutQueue", "LogoutRoutingKey", ref _logoutMessageQueue, exchange);

                // Bind Order Request Queue
                DeclareRabbitMqQueues("OrderRequestQueue", "OrderRequestRoutingKey", "orderexecution_exchange");

                //// Bind Limit Order Queue
                //BindQueue("LimitOrderQueue", "LimitOrderRoutingKey", ref _limitOrderQueue, exchange);

                //// Bind Market Order Queue
                //BindQueue("MarketOrderQueue", "MarketOrderRoutingKey", ref _marketOrderQueue, exchange);

                //// Bind Stop Order Queue
                //BindQueue("StopOrderQueue", "StopOrderRoutingKey", ref _stopOrderQueue, exchange);

                //// Bind Stop Limit Order Queue
                //BindQueue("StopLimitOrderQueue", "StopLimitOrderRoutingKey", ref _stopLimitOrderQueue, exchange);

                //// Bind Cancel Order Queue
                //BindQueue("CancelOrderQueue", "CancelOrderRoutingKey", ref _cancelOrderQueue, exchange);

                // Bind Locate Response Queue
                BindQueue("LocateResponseQueue", "LocateResponseRoutingKey", ref _locateResponseQueue, exchange);

                //get position routing key from config file
                _postionRoutingKey = ReadConfigSettings("PositionRoutingKey");
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "RegisterQueues");
            }
        }

        /// <summary>
        /// Declares Rabbit Mq Queues
        /// </summary>
        private void DeclareRabbitMqQueues(string queueHeader, string routingKeyHeader, string exchange)
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
                    //Declare Order Request Queue
                    _rabbitMqOrderRequestChannel.QueueDeclare(queueName, false, false, true, null);
                    _rabbitMqOrderRequestChannel.QueueBind(queueName, exchange, routingKey);

                    // Create Order Request Consumer
                    _orderRequestConsumer = new QueueingBasicConsumer(_rabbitMqOrderRequestChannel);
                    _rabbitMqOrderRequestChannel.BasicConsume(queueName, true, _orderRequestConsumer);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "DeclareRabbitMqQueues");
            }
        }

        /// <summary>
        /// Reads settings parameters from the Config file
        /// </summary>
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
        /// Initializes EasyNetQ's Advacne Rabbit Hutch
        /// </summary>
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

        /// <summary>
        /// Initializes Native Rabbit MQ resources
        /// </summary>
        public void InitializeNativeRabbitMq(string connectionString)
        {
            try
            {
                // Create Native Rabbit MQ Bus
                _rabbitMqOrderBus = new ConnectionFactory { HostName = "localhost" };

                // Create Native Rabbit MQ Connection to Receive Order Request
                _rabbitMqOrderRequestConnection = _rabbitMqOrderBus.CreateConnection();
                // Create Native Rabbit MQ Connection to Send Order Response
                _rabbitMqOrderResponseConnection = _rabbitMqOrderBus.CreateConnection();

                // Open Native Rabbbit MQ Channel to Receive Order Request
                _rabbitMqOrderRequestChannel = _rabbitMqOrderRequestConnection.CreateModel();
                // Open Native Rabbbit MQ Channel to Send Order Response
                _rabbitMqOrderResponseChannel = _rabbitMqOrderResponseConnection.CreateModel();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "InitializeNativeRabbitBus");
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

        #endregion

        #region Shutdown

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

                    _rabbitMqOrderRequestChannel.Close();
                    _rabbitMqOrderRequestConnection.Close();

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
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Binding Admin Message Queue: " + _loginMessageQueue.Name, _type.FullName,
                                "SubscribeAdminMessageQueues");
                }

                // Listening to Login Messages
                _rabbitBus.Consume<Login>(
                    _loginMessageQueue, (msg, messageReceivedInfo) =>
                                        Task.Factory.StartNew(
                                            () =>
                                            {
                                                if (LogonRequestRecieved != null)
                                                {
                                                    LogonRequestRecieved(msg);
                                                }
                                            }));

                // Listening to Logout Message
                _rabbitBus.Consume<Logout>(
                    _logoutMessageQueue, (msg, messageReceivedInfo) =>
                                         Task.Factory.StartNew(
                                             () =>
                                             {
                                                 if (LogoutRequestRecieved != null)
                                                 {
                                                     LogoutRequestRecieved(msg);
                                                 }
                                             }
                                             ));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeAdminMessageQueues");
            }
        }

        /// <summary>
        /// Binds the Market Order Message Queue
        /// Starts listening to the incoming Market Order messages
        /// </summary>
        private void SubscribeMarketOrderMessageQueues()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Binding Market Order Message Queue: " + _marketOrderQueue.Name, _type.FullName,
                                "SubscribeMarketOrderMessageQueues");
                }
                
                // Listening to Market Order Messages
                _rabbitBus.Consume<MarketOrder>(
                    _marketOrderQueue, (msg, messageReceivedInfo) =>
                                                 Task.Factory.StartNew(() =>
                                                 {
                                                     if (MarketOrderRequestRecieved != null)
                                                     {
                                                         MarketOrderRequestRecieved(msg.Body, msg.Properties.AppId);
                                                     }
                                                 }));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeMarketOrderMessageQueues");
            }
        }

        /// <summary>
        /// Binds the Limit Order Message Queue
        /// Starts listening to the incoming Limit Order messages
        /// </summary>
        private void SubscribeLimitOrderMessageQueues()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Binding Limit Order Message Queue: " + _limitOrderQueue.Name, _type.FullName,
                                "SubscribeLimitOrderMessageQueues");
                }

                // Listening to Limit Order Messages
                _rabbitBus.Consume<LimitOrder>(
                    _limitOrderQueue, (msg, messageReceivedInfo) =>
                                                 Task.Factory.StartNew(() =>
                                                 {
                                                     if (LimitOrderRequestRecieved != null)
                                                     {
                                                         LimitOrderRequestRecieved(msg.Body, msg.Properties.AppId);
                                                     }
                                                 }));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeLimitOrderMessageQueues");
            }
        }

        /// <summary>
        /// Binds the Stop Order Message Queue
        /// Starts listening to the incoming Stop Order messages
        /// </summary>
        private void SubscribeStopOrderMessageQueues()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Binding Stop Order Message Queue: " + _stopOrderQueue.Name, _type.FullName,
                                "SubscribeStopOrderMessageQueues");
                }

                // Listening to Stop Order Messages
                _rabbitBus.Consume<StopOrder>(
                    _stopOrderQueue, (msg, messageReceivedInfo) =>
                                                 Task.Factory.StartNew(() =>
                                                 {
                                                     if (StopOrderRequestRecieved != null)
                                                     {
                                                         StopOrderRequestRecieved(msg);
                                                     }
                                                 }));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeStopOrderMessageQueues");
            }
        }

        /// <summary>
        /// Binds the Stop Limit Order Message Queue
        /// Starts listening to the incoming Stop Limit Order messages
        /// </summary>
        private void SubscribeStopLimitOrderMessageQueues()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Binding Stop Limit Order Message Queue: " + _stopLimitOrderQueue.Name, _type.FullName,
                                "SubscribeStopLimitOrderMessageQueues");
                }

                // Listening to Stop Limit Order Messages
                _rabbitBus.Consume<StopLimitOrder>(
                    _stopLimitOrderQueue, (msg, messageReceivedInfo) =>
                                                 Task.Factory.StartNew(() =>
                                                 {
                                                     if (StopLimitOrderRequestRecieved != null)
                                                     {
                                                         StopLimitOrderRequestRecieved(msg);
                                                     }
                                                 }));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeStopLimitOrderMessageQueues");
            }
        }

        /// <summary>
        /// Binds the Cancel Order Message Queue
        /// Starts listening to the incoming Cancel Order messages
        /// </summary>
        private void SubscribeCancelOrderMessageQueues()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Binding Cancel Order Message Queue: " + _stopOrderQueue.Name, _type.FullName,
                                "SubscribeCancelOrderMessageQueues");
                }

                // Listening to Cancel Order Messages
                _rabbitBus.Consume<Order>(
                    _cancelOrderQueue, (msg, messageReceivedInfo) =>
                                                 Task.Factory.StartNew(() =>
                                                 {
                                                     if (CancelOrderRequestRecieved!= null)
                                                     {
                                                         CancelOrderRequestRecieved(msg.Body, msg.Properties.AppId);
                                                     }
                                                 }));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeStopOrderMessageQueues");
            }
        }

        /// <summary>
        /// Binds the Inquiry Request Message Queue
        /// Starts listening to the incoming Inquiry messages
        /// </summary>
        private void SubscribeInquiryMessageQueue()
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
        private void SubscribeAppInfoQueue()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Binding App Info Queue: " + _inquiryMessageQueue.Name, _type.FullName,
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
        /// Binds the Heartbeat Queue
        /// Starts listening to the incoming Heartbeat messages
        /// </summary>
        private void SubscribeHeartbeatQueue()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Binding Heatbeat Queue: " + _heartbeatQueue.Name, _type.FullName,
                                "SubscribeHeartbeatQueue");
                }

                // Listening to App Info Messages
                _rabbitBus.Consume<HeartbeatMessage>(
                    _heartbeatQueue, (msg, messageReceivedInfo) =>
                                   Task.Factory.StartNew(() => _heartBeatHandler.Update(msg.Body)));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeHeartbeatQueue");
            }
        }

        /// <summary>
        /// Binds the LocateResponse Queue
        /// Starts listening to the incoming Locate Response messages
        /// </summary>
        private void SubscribeLocateResponseQueues()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Binding Locate Response Queue: " + _heartbeatQueue.Name, _type.FullName,
                                "SubscribeLocateResponseQueues");
                }

                // Listening to App Info Messages
                _rabbitBus.Consume<LocateResponse>(
                    _locateResponseQueue, (msg, messageReceivedInfo) =>
                                          Task.Factory.StartNew(() =>
                                          {
                                              if (LocateResponseRecieved != null)
                                              {
                                                  LocateResponseRecieved(msg);
                                              }
                                          }));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeLocateResponseQueues");
            }
        }

        /// <summary>
        /// Consumes Order Requests from queue
        /// </summary>
        private void ConsumeOrderRequestQueue()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Starting order request consumer" , _type.FullName, "ConsumeOrderRequestQueue");
                }

                while (true)
                {
                    BasicDeliverEventArgs ea = (BasicDeliverEventArgs) _orderRequestConsumer.Queue.Dequeue();

                    // Get message bytes
                    byte[] body = ea.Body;

                    // Get next sequence number
                    long sequenceNo = _orderRequestRingBuffer.Next();

                    // Get object from ringbuffer
                    RabbitMqRequestMessage entry = _orderRequestRingBuffer[sequenceNo];

                    // Update object parameters
                    entry.Message = body;

                    // Publish updated object sequence number
                    _orderRequestRingBuffer.Publish(sequenceNo);
                    //_rabbitMqDataChannel.BasicAck(ea.DeliveryTag, false);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ConsumeOrderRequestQueue");
            }
        }

        #endregion

        /// <summary>
        /// Called when Application Disconnect event is raised from the <see cref="HeartBeatHandler"/>
        /// </summary>
        /// <param name="applicationId">Application ID which is disconnceted</param>
        private void OnApplicationDisconnect(string applicationId)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Disconnect event received for: " + applicationId, _type.FullName, "OnApplicationDisconnect");
                }

                // Raise event
                if (DisconnectApplication != null)
                {
                    DisconnectApplication(applicationId);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnApplicationDisconnect");
            }
        }

        /// <summary>
        /// Called when notified by Heartbeat Handler to send heartbeat response from OEE
        /// </summary>
        /// <param name="heartbeat">TradeHub Heartbeat Message</param>
        private void OnResponseHeartbeat(HeartbeatMessage heartbeat)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Publishing Heartbeat response: " + heartbeat, _type.FullName, "OnResponseHeartbeat");
                }

                // Generate EasyNetQ Message for publishing
                IMessage<HeartbeatMessage> heartbeatMessage = new Message<HeartbeatMessage>(heartbeat);

                // Publish Messages to respective Queues
                _rabbitBus.Publish(_exchange, heartbeat.ReplyTo, true, false, heartbeatMessage);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnResponseHeartbeat");
            }
        }

        /// <summary>
        /// Publishes Position messages to the MQ Exchange
        /// </summary>
        public void PublishMessages(Message<Position> message)
        {
            try
            {
                // Publish Messages to respective Queues
                _rabbitBus.Publish(_exchange, _postionRoutingKey, true, false, message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Called when Market Order Request is receieved
        /// </summary>
        /// <param name="messageArray"></param>
        private void OnMarketOrderReceived(string[] messageArray)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Market Order request received: " + messageArray[0] + messageArray[2], _type.FullName, "OnMarketOrderReceived");
            }

            MarketOrder marketOrder = new MarketOrder(messageArray[8]);

            if (ParseToMarketOrder(marketOrder, messageArray))
            {
                MarketOrderRequestRecieved(marketOrder, messageArray[0]);
            }
        }

        /// <summary>
        /// Called when Limit Order Request is recieved
        /// </summary>
        /// <param name="messageArray"></param>
        private void OnLimitOrderReceived(string[] messageArray)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Limit Order request received: " + messageArray[0] + messageArray[2], _type.FullName, "OnLimitOrderReceived");
            }

            LimitOrder limitOrder = new LimitOrder(messageArray[9]);

            if (ParseToLimitOrder(limitOrder, messageArray))
            {
                LimitOrderRequestRecieved(limitOrder, messageArray[0]);
            }
        }

        /// <summary>
        /// Called when Cancel Order request is received
        /// </summary>
        /// <param name="messageArray"></param>
        private void OnCancelOrderReceived(string[] messageArray)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Cancel Order request received: " + messageArray[0] + messageArray[2], _type.FullName, "OnCancelOrderReceived");
            }

            Order order = new Order(messageArray[9]);

            if (ParseToOrder(order, messageArray))
            {
                CancelOrderRequestRecieved(order, messageArray[0]);
            }
        }

        /// <summary>
        /// Creats market order object from incoming string message
        /// </summary>
        /// <param name="marketOrder">Market Order to add values to</param>
        /// <param name="message">Received message</param>
        /// <returns></returns>
        private bool ParseToMarketOrder(MarketOrder marketOrder, string[] message)
        {
            try
            {
                // Get Order ID
                marketOrder.OrderID = message[2];
                // Get Order Side
                marketOrder.OrderSide = message[3];
                // Get Order Size
                marketOrder.OrderSize = Convert.ToInt32(message[4]);
                // Get Order TIF value
                marketOrder.OrderTif = message[5];
                // Get Symbol
                marketOrder.Security = new Security() { Symbol = message[6] };
                // Get Time Value
                marketOrder.OrderDateTime = DateTime.ParseExact(message[7], "M/d/yyyy h:mm:ss.fff tt", CultureInfo.InvariantCulture);
                // Get Order Trigger Price
                marketOrder.TriggerPrice = Convert.ToDecimal(message[9]);
                // Get Slippage Value
                marketOrder.Slippage = Convert.ToDecimal(message[10]);
                // Get Order Remarks
                marketOrder.Remarks = message[11];
                // Get Order Exchange
                marketOrder.Exchange = message[12];

                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ParseToMarketOrder");
                return true;
            }
        }

        /// <summary>
        /// Creats limit order object from incoming string message
        /// </summary>
        /// <param name="limitOrder">Limit Order to add values to</param>
        /// <param name="message">Received message</param>
        /// <returns></returns>
        private bool ParseToLimitOrder(LimitOrder limitOrder, string[] message)
        {
            try
            {
                // Get Order ID
                limitOrder.OrderID = message[2];
                // Get Order Side
                limitOrder.OrderSide = message[3];
                // Get Order Size
                limitOrder.OrderSize = Convert.ToInt32(message[4]);
                // Get Limit Price
                limitOrder.LimitPrice = Convert.ToDecimal(message[5]);
                // Get Order TIF value
                limitOrder.OrderTif = message[6];
                // Get Symbol
                limitOrder.Security = new Security() { Symbol = message[7] };
                // Get Time Value
                limitOrder.OrderDateTime = DateTime.ParseExact(message[8], "M/d/yyyy h:mm:ss.fff tt", CultureInfo.InvariantCulture);
                // Get Order Trigger Price
                limitOrder.TriggerPrice = Convert.ToDecimal(message[10]);
                // Get Slippage Price
                limitOrder.Slippage = Convert.ToDecimal(message[11]);
                // Get Order Remarks
                limitOrder.Remarks = message[12];
                // Get Order Exchange
                limitOrder.Exchange = message[13];

                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ParseToLimitOrder");
                return true;
            }
        }

        /// <summary>
        /// Creats order object from incoming string message
        /// </summary>
        /// <param name="order">Order to add values to</param>
        /// <param name="message">Received message</param>
        /// <returns></returns>
        private bool ParseToOrder(Order order, string[] message)
        {
            try
            {
                // Get Order ID
                order.OrderID = message[2];
                // Get Order Side
                order.OrderSide = message[3];
                // Get Order Size
                order.OrderSize = Convert.ToInt32(message[4]);
                // Get Order TIF value
                order.OrderTif = message[5];
                // Get Order TIF value
                order.OrderStatus = message[6];
                // Get Symbol
                order.Security = new Security() { Symbol = message[7] };
                // Get Time Value
                order.OrderDateTime = DateTime.ParseExact(message[8], "M/d/yyyy h:mm:ss.fff tt", CultureInfo.InvariantCulture);
                // Get Exchange Value
                order.Exchange = message[12];

                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ParseToLimitOrder");
                return true;
            }
        }

        #region Publish Messages

        /// <summary>
        /// Publishes string messages to the MQ Exchange
        /// </summary>
        public void PublishMessages(ClientMqParameters strategyInfo, Message<string> message)
        {
            try
            {
                // Publish Messages to respective Queues
                _rabbitBus.Publish(_exchange, strategyInfo.ReplyTo, true, false, message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes TradeHub <see cref="Order"/> messages to the MQ Exchange
        /// </summary>
        public void PublishMessages(ClientMqParameters strategyInfo, Message<Order> message)
        {
            try
            {
                //// Publish Messages to respective Queues
                //_rabbitBus.Publish(_exchange, strategyInfo.ReplyTo, true, false, message);

                // Convert Order to Byte Stream and pass it onto distuptor
                byte[] responseBytes = Encoding.UTF8.GetBytes(message.Body.DataToPublish(message.Body.OrderStatus));

                // Send message for publication
                PublishOrderResponseToRabbitMq(strategyInfo.ReplyTo, responseBytes);

                //// Send to Order Response Disruptor
                //_responseDisruptorPublisher.PublishEvent((entry, sequenceNo) =>
                //{
                //    entry.Message = new byte[responseBytes.Length];
                //    responseBytes.CopyTo(entry.Message, 0);
                //    entry.ReplyTo = strategyInfo.ReplyTo;
                //    return entry;
                //});
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes TradeHub <see cref="LimitOrder"/> messages to the MQ Exchange
        /// </summary>
        public void PublishMessages(ClientMqParameters strategyInfo, Message<LimitOrder> message)
        {
            try
            {
                // Publish Messages to respective Queues
                _rabbitBus.Publish(_exchange, strategyInfo.ReplyTo, true, false, message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes TradeHub <see cref="Execution"/>messages to the MQ Exchange
        /// </summary>
        public void PublishMessages(ClientMqParameters strategyInfo, Message<Execution> message)
        {
            try
            {
                //// Publish Messages to respective Queues
                //_rabbitBus.Publish(_exchange, strategyInfo.ReplyTo, true, false, message);

                // Convert Execution to Byte Stream and pass it onto distuptor
                byte[] responseBytes = Encoding.UTF8.GetBytes(message.Body.DataToPublish());

                // Send message for publication
                PublishOrderResponseToRabbitMq(strategyInfo.ReplyTo, responseBytes);

                //// Send to Order Response Disruptor
                //_responseDisruptorPublisher.PublishEvent((entry, sequenceNo) =>
                //{
                //    entry.Message = new byte[responseBytes.Length];
                //    responseBytes.CopyTo(entry.Message, 0);
                //    entry.ReplyTo = strategyInfo.ReplyTo;
                //    return entry;
                //});
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes TradeHub <see cref="Rejection"/> messages to the MQ Exchange
        /// </summary>
        public void PublishMessages(ClientMqParameters strategyInfo, Message<Rejection> message)
        {
            try
            {
                // Publish Messages to respective Queues
                _rabbitBus.Publish(_exchange, strategyInfo.ReplyTo, true, false, message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMessages");
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
                // Publish Messages to respective Queues
                _rabbitBus.Publish(_exchange, replyTo, true, false, message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes byte data to Rabbit MQ Order Response Channel
        /// </summary>
        /// <param name="replyTo">Routing Key of queue to publish</param>
        /// <param name="responseBytes">byte message to be published</param>
        private void PublishOrderResponseToRabbitMq(string replyTo, byte[] responseBytes)
        {
            try
            {
                // Send to Order Response Disruptor
                _responseDisruptorPublisher.PublishEvent((entry, sequenceNo) =>
                {
                    entry.Message= new byte[responseBytes.Length];
                    responseBytes.CopyTo(entry.Message, 0);
                    entry.ReplyTo = replyTo;
                    return entry;
                });
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishOrderResponseToRabbitMq");
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

        #region Implementation of IEventHandler<in RabbitMqRequestMessage>

        /// <summary>
        /// Called when a publisher has committed an event to the <see cref="T:Disruptor.RingBuffer`1"/>
        /// </summary>
        /// <param name="data">Data committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="sequence">Sequence number committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="endOfBatch">flag to indicate if this is the last event in a batch from the <see cref="T:Disruptor.RingBuffer`1"/></param>
        public void OnNext(RabbitMqRequestMessage data, long sequence, bool endOfBatch)
        {
            try
            {
                string message = Encoding.UTF8.GetString(data.Message);

                var messageArray = message.Split(',');

                if (messageArray[1].Equals("Market"))
                    OnMarketOrderReceived(messageArray);
                else if (messageArray[1].Equals("Limit"))
                    OnLimitOrderReceived(messageArray);
                else if (messageArray[1].Equals("Cancel"))
                    OnCancelOrderReceived(messageArray);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnNext");
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
            string corrId = Guid.NewGuid().ToString();
            IBasicProperties replyProps = _rabbitMqOrderRequestChannel.CreateBasicProperties();
            replyProps.CorrelationId = corrId;

            // Publish Order Response to Requesting Client
            _rabbitMqOrderResponseChannel.BasicPublish(_exchange.Name, data.ReplyTo, replyProps, data.Message);
            
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Order response published", _type.FullName, "OnNext");
            }
        }

        #endregion
    }
}
