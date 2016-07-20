using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Disruptor;
using Disruptor.Dsl;
using ZMQ;
using EasyNetQ;
using EasyNetQ.Topology;
using RabbitMQ.Client;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.Heartbeat;
using TradeHub.Common.Core.ValueObjects.Inquiry;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataEngine.Configuration.HeartBeat;
using ExchangeType = EasyNetQ.Topology.ExchangeType;
using Exception=System.Exception;

namespace TradeHub.MarketDataEngine.Configuration.Service
{
    /// <summary>
    /// Handles All MQ Communications
    /// </summary>
    public class MqServer : IEventHandler<RabbitMqRequestMessage>
    {
        private readonly Type _type = typeof (MqServer);

        #region Private Fields

        // Holds reference for the Advance Bus
        private IAdvancedBus _easyNetQBus;

        // Exchange containing Queues
        private IExchange _exchange;

        // Native Rabbit MQ Fields
        private ConnectionFactory _rabbitMqDataBus;
        private IConnection _rabbitMqDataConnection;
        private IModel _rabbitMqDataChannel;

        #region Queues

        // Queue will contain Login messages
        private IQueue _loginMessageQueue;

        // Queue will contain Logout messages
        private IQueue _logoutMessageQueue;

        // Queue will contain Subscription messages
        private IQueue _subscriptionMessageQueue;

        // Queue will contain Unsubscription messages
        private IQueue _unsubscriptionMessageQueue;

        // Queue will contain Live Bar Subscribe Request messages
        private IQueue _liveBarSubscribeRequestQueue;

        // Queue will contain Live Bar Unsubscribe Request messages
        private IQueue _liveBarUnsubscribeRequestQueue;

        // Queue will contain Hisrotic Bar Data Request messages
        private IQueue _historicBarDataRequestQueue;

        // Queue will contain Inquiry messages
        private IQueue _inquiryMessageQueue;

        // Queue will contain App Info messages
        private IQueue _appInfoQueue;

        // Queue will contain Heartbeat messages
        private IQueue _heartbeatQueue;

        #endregion

        // Holds reference for the Heartbeat Handler
        private HeartBeatHandler _heartBeatHandler;

        //Name of the Configuration File
        private readonly string _configFile;

        #endregion

        private readonly int _ringSize = 65536;  // Must be multiple of 2

        private Disruptor<RabbitMqRequestMessage> _dataDisruptor;
        private RingBuffer<RabbitMqRequestMessage> _dataRingBuffer;

        public event Action<string> DisconnectApplication;
        public event Action<IMessage<Login>> LogonRequestReceived;
        public event Action<IMessage<Logout>> LogoutRequestReceived;
        public event Action<IMessage<Subscribe>> SubscribeRequestReceived;
        public event Action<IMessage<Unsubscribe>> UnsubscribeRequestReceived;
        public event Action<IMessage<InquiryMessage>> InquiryRequestReceived;
        public event Action<IMessage<Dictionary<string, string>>> AppInfoReceived;
        public event Action<IMessage<BarDataRequest>> LiveBarSubscribeRequestReceived;
        public event Action<IMessage<BarDataRequest>> LiveBarUnsubscribeRequestReceived;
        public event Action<IMessage<HistoricDataRequest>> HistoricBarDataRequestReceived;

        //private ZmqContext _ctx;
        //private ZmqSocket _socket;
        private Context _ctx;
        private Socket _socket;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="configFile">Name of the configuration file to read</param>
        /// <param name="heartbeatThreshold">Duration to wait for the expected heartbeat before disconnecting</param>
        /// <param name="heartbeatResponseInterval">Timer after which server will send Heartbeat to connecting Applications</param>
        public MqServer(string configFile, int heartbeatThreshold, int heartbeatResponseInterval)
        {
            // Initialize Disruptor
            _dataDisruptor = new Disruptor.Dsl.Disruptor<RabbitMqRequestMessage>(() => new RabbitMqRequestMessage(), _ringSize, TaskScheduler.Default);

            // Add Consumer
            _dataDisruptor.HandleEventsWith(this);

            // Start Disruptor
            _dataRingBuffer = _dataDisruptor.Start();

            _configFile = configFile;

            _heartBeatHandler = new HeartBeatHandler(heartbeatThreshold, heartbeatResponseInterval);
            
            _heartBeatHandler.ApplicationDisconnect += OnApplicationDisconnect;
            _heartBeatHandler.ResponseHeartbeat += OnResponseHeartbeat;

            //initialize zmqcontext
           InitializeZmq();
        }

        /// <summary>
        /// Initialize ZeroMQ
        /// </summary>
        private void InitializeZmq()
        {
            try
            {
                string[] config = ReadZeroMqConfig("ZmqConfig.xml");
                _ctx = new Context(1);
                _socket = _ctx.Socket(SocketType.PUB);
                _socket.Bind("tcp://" + config[0] + ":" + config[1]);
                //_socket.SendHighWatermark = 0;
                //_socket.ClearTcpAcceptFilter();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "InitializeZmq");
            }
        }

        /// <summary>
        /// Checks if the Advance Bus is connected or not
        /// </summary>
        public bool IsConnected()
        {
            if (_easyNetQBus != null)
            {
                return _easyNetQBus.IsConnected;
            }
            return false;
        }

        #region Initialization

        /// <summary>
        /// Starts MQ Server
        /// </summary>
        public void Connect()
        {

            // Initializes MQ resources
            IntitializeMqServices();

            // Bind Admin Message Queue
            SubscribeAdminMessageQueues();

            // Bind Tick Subscription Message Queue
            SubscribeTickMarketDataMessageQueues();

            // Bind Livr Bar Data Message Queue
            SubscribeLiveBarRequestMessageQueues();

            // Bind Historic Bar Data Message Queue
            SubscribeHistoricBarRequestMessageQueues();

            // Bind Inquiry Message Queue
            SubscribeInquiryMessageQueue();

            // Bind Applicaiton Info Message Queue
            SubscribeAppInfoQueue();

            // Bind Heartbeat Message Queue
            SubscribeHeartbeatQueue();
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
                    // Initialize EasyNetQ Bus
                    InitializeRabbitHutch(connectionString);

                    // Initialize Native Rabbit MQ services
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
                // Bind Login Queue
                BindQueue("LoginQueue", "LoginRoutingKey", ref _loginMessageQueue, exchange);

                // Bind Logout Queue
                BindQueue("LogoutQueue", "LogoutRoutingKey", ref _logoutMessageQueue, exchange);

                // Bind Subscribe Queue
                BindQueue("SubscribeQueue", "SubscribeRoutingKey", ref _subscriptionMessageQueue, exchange);

                // Bind Unsubscribe Queue
                BindQueue("UnsubscribeQueue", "UnsubscribeRoutingKey", ref _unsubscriptionMessageQueue, exchange);
                
                // Bind Live Bar Subscribe Queue
                BindQueue("LiveBarSubscribeQueue", "LiveBarSubscribeRoutingKey", ref _liveBarSubscribeRequestQueue, exchange);

                // Bind Live Bar Unsubscribe Queue
                BindQueue("LiveBarUnsubscribeQueue", "LiveBarUnsubscribeRoutingKey", ref _liveBarUnsubscribeRequestQueue, exchange);

                // Bind Historic Bar Data Queue
                BindQueue("HistoricBarDataQueue", "HistoricBarDataRoutingKey", ref _historicBarDataRequestQueue, exchange);

                // Bind Inquiry Queue
                BindQueue("InquiryQueue", "InquiryRoutingKey", ref _inquiryMessageQueue, exchange);

                // Bind App Info Queue
                BindQueue("AppInfoQueue", "AppInfoRoutingKey", ref _appInfoQueue, exchange);

                // Bind Heartbeat Queue
                BindQueue("HeartbeatQueue", "HeartbeatRoutingKey", ref _heartbeatQueue, exchange);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "InitializeQueues");
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
                // Create a new EasyNetQ Bus Instance
                _easyNetQBus = _easyNetQBus ?? RabbitHutch.CreateBus(connectionString).Advanced;
                
                _easyNetQBus.Connected -= OnBusConnected;
                _easyNetQBus.Connected += OnBusConnected;
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
                // Create Bus
                _rabbitMqDataBus = new ConnectionFactory {HostName = "localhost"};

                // Create Connection
                _rabbitMqDataConnection = _rabbitMqDataBus.CreateConnection();

                // Open Channel
                _rabbitMqDataChannel = _rabbitMqDataConnection.CreateModel();
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
                return _easyNetQBus.ExchangeDeclare(exchangeName, ExchangeType.Direct, false, true, false);
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
                if (_easyNetQBus != null)
                {
                    _easyNetQBus.Dispose();

                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Advanced Bus disposed off.", _type.FullName, "Disconnect");
                    }
                }
                _socket.Dispose();
                _ctx.Dispose();
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
        public void SubscribeAdminMessageQueues()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Binding Admin Message Queue: " + _loginMessageQueue.Name, _type.FullName,
                                "SubscribeAdminMessageQueues");
                }

                // Listening to Login Messages
                _easyNetQBus.Consume<Login>(
                    _loginMessageQueue, (msg, messageReceivedInfo) =>
                                        Task.Factory.StartNew(
                                            () =>
                                                {
                                                    if (LogonRequestReceived != null)
                                                    {
                                                        LogonRequestReceived(msg);
                                                    }
                                                }));

                // Listening to Logout Message
                _easyNetQBus.Consume<Logout>(
                    _logoutMessageQueue, (msg, messageReceivedInfo) =>
                                         Task.Factory.StartNew(
                                             () =>
                                                 {
                                                     if (LogoutRequestReceived != null)
                                                     {
                                                         LogoutRequestReceived(msg);
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
        /// Binds the Subscription Message Queue
        /// Starts listening to the incoming Tick Subscription Level messages
        /// </summary>
        public void SubscribeTickMarketDataMessageQueues()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Binding Tick Subscription Message Queue: " + _subscriptionMessageQueue.Name, _type.FullName,
                                "SubscribeTickMarketDataMessageQueues");
                }

                // Listening to Subscription Messages
                _easyNetQBus.Consume<Subscribe>(
                    _subscriptionMessageQueue, (msg, messageReceivedInfo) =>
                                               Task.Factory.StartNew(() =>
                                                   {
                                                       if (SubscribeRequestReceived != null)
                                                       {
                                                           SubscribeRequestReceived(msg);
                                                       }
                                                   }));

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Binding Tick Unsubscription Message Queue: " + _unsubscriptionMessageQueue.Name, _type.FullName,
                                "SubscribeTickMarketDataMessageQueues");
                }

                // Listening to Unsubscription Messages
                _easyNetQBus.Consume<Unsubscribe>(
                    _unsubscriptionMessageQueue, (msg, messageReceivedInfo) =>
                                                 Task.Factory.StartNew(() =>
                                                     {
                                                         if (UnsubscribeRequestReceived != null)
                                                         {
                                                             UnsubscribeRequestReceived(msg);
                                                         }
                                                     }));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeTickMarketDataMessageQueues");
            }
        }

        /// <summary>
        /// Binds the Live Bar Data Request Message Queue
        /// Starts listening to the incoming Live Bar Data Request messages
        /// </summary>
        public void SubscribeLiveBarRequestMessageQueues()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Binding Live Bar Subscription Message Queue: " + _liveBarSubscribeRequestQueue.Name, _type.FullName,
                                "SubscribeLiveBarRequestMessageQueues");
                }

                // Listening to Subscription Messages
                _easyNetQBus.Consume<BarDataRequest>(
                    _liveBarSubscribeRequestQueue, (msg, messageReceivedInfo) =>
                                               Task.Factory.StartNew(() =>
                                               {
                                                   if (LiveBarSubscribeRequestReceived != null)
                                                   {
                                                       LiveBarSubscribeRequestReceived(msg);
                                                   }
                                               }));

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Binding Live Bar Unsubscription Message Queue: " + _liveBarUnsubscribeRequestQueue.Name, _type.FullName,
                                "SubscribeLiveBarRequestMessageQueues");
                }

                // Listening to Unsubscription Messages
                _easyNetQBus.Consume<BarDataRequest>(
                    _liveBarUnsubscribeRequestQueue, (msg, messageReceivedInfo) =>
                                               Task.Factory.StartNew(() =>
                                               {
                                                   if (LiveBarUnsubscribeRequestReceived != null)
                                                   {
                                                       LiveBarUnsubscribeRequestReceived(msg);
                                                   }
                                               }));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeLiveBarRequestMessageQueues");
            }
        }

        /// <summary>
        /// Binds the Historical Bar Data Request Message Queue
        /// Starts listening to the incoming Historic Bar Data Request messages
        /// </summary>
        public void SubscribeHistoricBarRequestMessageQueues()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Binding Historic Bar Data Message Queue: " + _historicBarDataRequestQueue.Name, _type.FullName,
                                "SubscribeHistoricBarRequestMessageQueues");
                }

                // Listening to Subscription Messages
                _easyNetQBus.Consume<HistoricDataRequest>(
                    _historicBarDataRequestQueue, (msg, messageReceivedInfo) =>
                                               Task.Factory.StartNew(() =>
                                               {
                                                   if (HistoricBarDataRequestReceived != null)
                                                   {
                                                       HistoricBarDataRequestReceived(msg);
                                                   }
                                               }));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeHistoricBarRequestMessageQueues");
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
                _easyNetQBus.Consume<InquiryMessage>(
                    _inquiryMessageQueue, (msg, messageReceivedInfo) =>
                        {
                            var tcs = new TaskCompletionSource<object>();
                            try
                            {
                                if (InquiryRequestReceived != null)
                                {
                                    InquiryRequestReceived(msg);
                                }
                                tcs.SetResult(null);
                            }
                            catch (Exception exception)
                            {
                                tcs.SetException(exception);
                            }
                            return tcs.Task;
                        });
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
                    Logger.Info("Binding App Info Queue: " + _inquiryMessageQueue.Name, _type.FullName,
                                "SubscribeAppInfoQueue");
                }

                // Listening to App Info Messages
                _easyNetQBus.Consume<Dictionary<string, string>>(
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
        public void SubscribeHeartbeatQueue()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Binding Heatbeat Queue: " + _heartbeatQueue.Name, _type.FullName,
                                "SubscribeHeartbeatQueue");
                }

                // Listening to App Info Messages
                _easyNetQBus.Consume<HeartbeatMessage>(
                    _heartbeatQueue, (msg, messageReceivedInfo) =>
                                   Task.Factory.StartNew(() => _heartBeatHandler.Update(msg.Body)));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeHeartbeatQueue");
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
        /// Called when notified by Heartbeat Handler to send heartbeat response from MDE
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
                _easyNetQBus.Publish(_exchange, heartbeat.ReplyTo, true, false, heartbeatMessage);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnResponseHeartbeat");
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
                _easyNetQBus.Publish(_exchange, strategyInfo.ReplyTo, true, false, message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes Tick messages to the MQ Exchange
        /// </summary>
        public void PublishMessages(ClientMqParameters strategyInfo, Message<Tick> message)
        {
            try
            {
                //// Publish Messages to respective Queues
                //_easyNetQBus.Publish(_exchange, strategyInfo.ReplyTo, true, false, message);

                // Publish Message Using Native Rabbit MQ
                PublishTickMessageToRabbitMq(strategyInfo.ReplyTo, message.Body);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes Live Bars to the MQ Exchange
        /// </summary>
        public void PublishMessages(ClientMqParameters strategyInfo, Message<Bar> message)
        {
            try
            {
                //// Publish Messages to respective Queues
                //_easyNetQBus.Publish(_exchange, strategyInfo.ReplyTo, true, false, message);

                // Publish Message Using Native Rabbit MQ
                PublishBarMessageToRabbitMq(strategyInfo.ReplyTo, message.Body);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes Historic Bars to the MQ Exchange
        /// </summary>
        public void PublishMessages(ClientMqParameters strategyInfo, Message<HistoricBarData> message)
        {
            try
            {
                // Publish Messages to respective Queues
                _easyNetQBus.Publish(_exchange, strategyInfo.ReplyTo, true, false, message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes the Inquiry Response
        /// </summary>
        /// <param name="replyTo">Routing Key of queue to publish</param>
        /// <param name="message">TradeHub Inquiry Response message to be sent</param>
        public void PublishMessages(string replyTo, Message<InquiryResponse> message)
        {
            try
            {
                // Publish Messages to respective Queues
                _easyNetQBus.Publish(_exchange, replyTo, true, false, message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publish Tick messages to ZeroMQ
        /// </summary>
        ///  <param name="tick"></param>
        public void PublishTickMessageToZeroMq(Tick tick)
        {
            try
            {
                string message =tick.Security.Symbol+"|"+ tick.DataToPublish();
                _socket.Send(message, Encoding.UTF8);
                // Publish Data
                //PublishMessageToZeroMq(message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishTickMessageToRabbitMq");
            }
        }

        /// <summary>
        /// Gateway for sending messages
        /// </summary>
        /// <param name="message"></param>
        private void PublishMessageToZeroMq(string message)
        {
            //byte[] array = Encoding.UTF8.GetBytes(message);
            _socket.Send(message, Encoding.UTF8);
            //_socket.Send(array);

        }

        /// <summary>
        /// Publishes Tick messages to MQ Exchange using native RabbitMQ
        /// </summary>
        /// <param name="replyTo">Routing Key of queue to publish</param>
        /// <param name="tick">TradeHub Tick message to be published</param>
        private void PublishTickMessageToRabbitMq(string replyTo, Tick tick)
        {
            try
            {
                // Create message bytes to be published
                byte[] responseBytes = Encoding.UTF8.GetBytes(tick.DataToPublish());

                // Publish Data
                PublishDataMessageToRabbitMq(replyTo, responseBytes);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishTickMessageToRabbitMq");
            }
        }

        /// <summary>
        /// Publishes Bar messages to MQ Exchange using native RabbitMQ
        /// </summary>
        /// <param name="replyTo">Routing Key of queue to publish</param>
        /// <param name="bar">TradeHub Bar message to be published</param>
        private void PublishBarMessageToRabbitMq(string replyTo, Bar bar)
        {
            try
            {
                // Create message bytes to be published
                byte[] responseBytes = Encoding.UTF8.GetBytes(bar.DataToPublish());

                // Publish Data
                PublishDataMessageToRabbitMq(replyTo, responseBytes);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishBarMessageToRabbitMq");
            }
        }

        public void PublishBarToZeroMq(Bar bar)
        {
            try
            {
              string message =bar.RequestId+"|"+ bar.DataToPublish();
              PublishMessageToZeroMq(message);
            }
            catch (Exception exception)
            {

                Logger.Error(exception, _type.FullName, "PublishBarMessageToZeroMq");
            }
        }

        /// <summary>
        /// Publishes byte data to Rabbit MQ Data Channel
        /// </summary>
        /// <param name="replyTo">Routing Key of queue to publish</param>
        /// <param name="responseBytes">byte message to be published</param>
        [MethodImpl(MethodImplOptions.Synchronized)] 
        private void PublishDataMessageToRabbitMq(string replyTo, byte[] responseBytes)
        {
            try
            {
                // Get next sequence number
                long sequenceNo = _dataRingBuffer.Next();

                // Get object from ring buffer
                RabbitMqRequestMessage entry = _dataRingBuffer[sequenceNo];

                // Update object values
                entry.RequestTo = replyTo;
                entry.Message = responseBytes;

                // Publish sequence number for which the object is updated
                _dataRingBuffer.Publish(sequenceNo);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishDataMessageToRabbitMq");
            }
        }

        #endregion

        /// <summary>
        /// Raised when Advanced Bus is successfully connected
        /// </summary>
        private void OnBusConnected()
        {
            if (_easyNetQBus.IsConnected)
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Successfully connected to MQ Server", _type.FullName, "OnBusConnected");
                }
            }
        }

        /// <summary>
        /// Read ZeroMQ IP and Port from file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public string[] ReadZeroMqConfig(string filename)
        {
            string[] config = new string[2];
            try
            {
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + filename))
                {
                    
                    var doc = new XmlDocument();

                    // Read Specified configuration file
                    doc.Load(AppDomain.CurrentDomain.BaseDirectory + @"\Config\" + filename);

                    // Read the specified Node values
                    XmlNodeList nodes = doc.SelectNodes(xpath: "ZeroMqParams/*");
                    if (nodes != null)
                    {
                        foreach (XmlNode node in nodes)
                        {
                            if (node.Name.Equals("IP"))
                            {
                                config[0] = node.InnerText;
                            }
                            else if (node.Name.Equals("Port"))
                            {
                                config[1] = node.InnerText;
                            }

                            Logger.Info("Adding parameter: " + node.Name + " | Value: " + node.InnerText, _type.FullName, "ReadZeroMqConfig");
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ReadZeroMqConfig");
            }

            return config;
        }

        #region Implementation of IEventHandler<in RabbitMqMessage>

        /// <summary>
        /// Called when a publisher has committed an event to the <see cref="T:Disruptor.RingBuffer`1"/>
        /// </summary>
        /// <param name="data">Data committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="sequence">Sequence number committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="endOfBatch">flag to indicate if this is the last event in a batch from the <see cref="T:Disruptor.RingBuffer`1"/></param>
        public void OnNext(RabbitMqRequestMessage data, long sequence, bool endOfBatch)
        {
            try
            {
                string corrId = Guid.NewGuid().ToString();
                IBasicProperties replyProps = _rabbitMqDataChannel.CreateBasicProperties();
                replyProps.CorrelationId = corrId;

                // Publish Data
                _rabbitMqDataChannel.BasicPublish(_exchange.Name, data.RequestTo, replyProps, data.Message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnNext");
            }
        }

        #endregion
    }
}
