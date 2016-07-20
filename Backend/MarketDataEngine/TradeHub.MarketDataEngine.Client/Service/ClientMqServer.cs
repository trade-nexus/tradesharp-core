using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Disruptor;
using Disruptor.Dsl;
using EasyNetQ;
using EasyNetQ.Topology;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Spring.Context.Support;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.Heartbeat;
using TradeHub.Common.Core.ValueObjects.Inquiry;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataEngine.Client.Constants;
using ZMQ;


using ExchangeType = EasyNetQ.Topology.ExchangeType;
using Exception = System.Exception;

namespace TradeHub.MarketDataEngine.Client.Service
{
    /// <summary>
    /// Handles Rabbit MQ Communications for the Market Data Engine Client
    /// </summary>
    internal class ClientMqServer : IEventHandler<RabbitMqRequestMessage>
    {
        private Type _type = typeof (ClientMqServer);
        private AsyncClassLogger _asyncClassLogger;

        public event Action BusConnected;
        public event Action ServerDisconnected;
        public event Action<string> LogonArrived;
        public event Action<string> LogoutArrived;
        public event Action<Tick> TickArrived;
        public event Action<Bar> LiveBarArrived;
        //public event Action<RabbitMqMessage> MarketDataArrived;
        public event Action<byte[]> MarketDataArrived;
        public event Action<HistoricBarData> HistoricBarsArrived;
        public event Action<InquiryResponse> InquiryResponseArrived;

        /// <summary>
        /// Dedicated task for ZeroMQ receiver
        /// </summary>
        private Task _receiverTask;

        /// <summary>
        /// Dedicated task for data consumer
        /// </summary>
        private Task _dataConsumerTask;
        
        private CancellationTokenSource _cancellationTokenSource;


        private bool _consumeMarketData = false;

        private Context _ctx;
        private Context _ctx1;

        private Socket _socket;
        private int _randomNumber;

        //private Context _ctx;
        //private Socket _socket;

        #region Rabbit MQ Fields

        // Holds reference for the Advance Bus
        private IAdvancedBus _advancedBus;

        // Native Rabbit MQ Fields
        private ConnectionFactory _rabbitMqDataBus;
        private IConnection _rabbitMqDataConnection;
        private IModel _rabbitMqDataChannel;
        private QueueingBasicConsumer _dataConsumer;

        // Exchange containing Queues
        private IExchange _exchange;

        // Queue will contain Admin messages
        private IQueue _adminMessageQueue;

        // Queue will contain Tick messages
        private IQueue _tickDataQueue;

        // Queue will contain Live Bar Data messages
        private IQueue _liveBarDataQueue;

        // Queue will contain Historic Bar Data messages
        private IQueue _historicBarDataQueue;

        // Queue will contain Inquiry Response messages
        private IQueue _inquiryResponseQueue;

        // Queue will contain Heartbeat Response from MDE
        private IQueue _heartbeatResponseQueue;

        #endregion

        private readonly int _ringSize = 65536; //262144; //65536;  // Must be multiple of 2

        private Disruptor<RabbitMqRequestMessage> _dataDisruptor;
        private RingBuffer<RabbitMqRequestMessage> _dataRingBuffer;

        private string _applicationId = string.Empty;

        /// <summary>
        /// Duration between successive Heartbeats in milliseconds
        /// </summary>
        private int _heartbeatInterval = 60000;

        /// <summary>
        /// Holds refernce to the Heartbeat Handler
        /// </summary>
        private ClientHeartBeatHandler _heartBeatHandler;

        /// <summary>
        /// Key = Parameter Name
        /// Value = Parameter Value
        /// </summary>
        private Dictionary<string, string> _mdeMqServerparameters;

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
        /// <param name="mdeMqServerparameters">Contains Parameters required for sending messages to MDE MQ Server</param>
        /// <param name="clientMqParameters">Contains Parameters for getting response messages from MDE MQ Server </param>
        /// <param name="asyncClassLogger">Class Logger Instance to be used</param>
        public ClientMqServer(Dictionary<string, string> mdeMqServerparameters, Dictionary<string, string> clientMqParameters, AsyncClassLogger asyncClassLogger)
        {
            //_asyncClassLogger = ContextRegistry.GetContext()["MDEClientLogger"] as AsyncClassLogger;
            _asyncClassLogger = asyncClassLogger;
            
            // Initialize Disruptor
            //_dataDisruptor = new Disruptor.Dsl.Disruptor<RabbitMqRequestMessage>(() => new RabbitMqRequestMessage(), _ringSize, TaskScheduler.Default);

            //// Add Consumer
            //_dataDisruptor.HandleEventsWith(this);

            //// Start Disruptor
            //_dataRingBuffer = _dataDisruptor.Start();

            // Save Parameters
            _mdeMqServerparameters = mdeMqServerparameters;
            _clientMqParameters = clientMqParameters;
            
            Initialize();
        }

        #region ZeroMQ initialization

        private void InitializeZmq()
        {
            try
            {

                string[] config = ReadZeroMqConfig("ZmqConfig.xml");
                _ctx = new Context(1);
                _ctx1 = new Context(1);
                _socket = _ctx.Socket(SocketType.SUB);
                _socket.Connect("tcp://" + config[0] + ":" + config[1]);
                _cancellationTokenSource = new CancellationTokenSource();
                _consumeMarketData = true;
                _receiverTask = Task.Factory.StartNew(ZeroMqReceiver, _cancellationTokenSource.Token);
            }
            catch (Exception exception)
            {

                _asyncClassLogger.Error(exception, _type.FullName, "InitializeZmq");
            }
            
        }

        #endregion

        #region MQ Initialization

        /// <summary>
        /// Initializes MQ Server related parameters
        /// </summary>
        private void InitializeMqServer()
        {
            try
            {
                // Create Rabbit MQ Hutch 
                string connectionString = _mdeMqServerparameters["ConnectionString"];
                if (!string.IsNullOrEmpty(connectionString))
                {
                    // Initialize EasyNetQ Bus
                    InitializeRabbitHutch(connectionString);

                    // Initialize Native RabbitMQ Bus
                    InitializeNativeRabbitMq(connectionString);

                    // Get Exchange Name from Config File
                    string exchangeName = _mdeMqServerparameters["Exchange"];

                    if (!string.IsNullOrEmpty(exchangeName))
                    {
                        // Use the Exchange Name to Initialize Rabbit Exchange
                        InitializeExchange(exchangeName);

                        if (_exchange != null)
                        {
                            // Bind Inquiry Response Queue
                            BindQueue(ClientMqParameterNames.InquiryResponseQueue, ClientMqParameterNames.InquiryResponseRoutingKey,
                                      ref _inquiryResponseQueue, _exchange, "");
                        }
                    }
                }
            }
            catch (System.Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "InitializeMqServer");
            }
        }

        /// <summary>
        /// Initializes EasyNetQ's Advacne Rabbit Hutch
        /// </summary>
        private void InitializeRabbitHutch(string connectionString)
        {
            try
            {
                // Create a new Advance Rabbit Bus Instance
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
                // Create Bus
                _rabbitMqDataBus = new ConnectionFactory {HostName = "localhost"};

                // Create Connection
                _rabbitMqDataConnection = _rabbitMqDataBus.CreateConnection();

                // Open Channel
                _rabbitMqDataChannel = _rabbitMqDataConnection.CreateModel();
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
            BindQueue(ClientMqParameterNames.AdminMessageQueue, ClientMqParameterNames.AdminMessageRoutingKey,
                      ref _adminMessageQueue, exchange, appId);

            // Bind Tick Data Queue
            //BindQueue(ClientMqParameterNames.TickDataQueue, ClientMqParameterNames.TickDataRoutingKey,
            //          ref _tickDataQueue, exchange, appId);

            // Bind Live Bar Data Queue
            //BindQueue(ClientMqParameterNames.LiveBarDataQueue, ClientMqParameterNames.LiveBarDataRoutingKey,
            //          ref _liveBarDataQueue, exchange, appId);

            // Bind Data Queue
            DeclareRabbitMqQueues(ClientMqParameterNames.TickDataQueue, ClientMqParameterNames.TickDataRoutingKey, appId);

            // Bind Historic Bar Data Queue
            BindQueue(ClientMqParameterNames.HistoricBarDataQueue, ClientMqParameterNames.HistoricBarDataRoutingKey,
                      ref _historicBarDataQueue, exchange, appId);

            // Bind Heartbeat response Queue
            BindQueue(ClientMqParameterNames.HeartbeatResponseQueue, ClientMqParameterNames.HeartbeatResponseRoutingKey,
                      ref _heartbeatResponseQueue, exchange, appId);
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
        public void DeclareRabbitMqQueues(string queueHeader, string routingKeyHeader, string appId)
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
                    _rabbitMqDataChannel.QueueDeclare(queueName, false, false, true, null);
                    _rabbitMqDataChannel.QueueBind(queueName, _exchange.Name, routingKey);

                    // Create Data Consumer
                    _dataConsumer = new QueueingBasicConsumer(_rabbitMqDataChannel);
                    _rabbitMqDataChannel.BasicConsume(queueName, true, _dataConsumer);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "DeclareRabbitMqQueues");
            }
        }

        #endregion

        #region Outgoing message to Market Data Engine

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
                if (_mdeMqServerparameters.TryGetValue(MdeMqServerParameterNames.InquiryRoutingKey, out routingKey))
                {
                    Message<InquiryMessage> inquiryMessage = new Message<InquiryMessage>(inquiry);
                    inquiryMessage.Properties.AppId = _applicationId;
                    inquiryMessage.Properties.ReplyTo = _clientMqParameters[ClientMqParameterNames.InquiryResponseRoutingKey];

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
                if (_mdeMqServerparameters.TryGetValue(MdeMqServerParameterNames.LoginRoutingKey, out routingKey))
                {
                    Message<Login> loginMessage = new Message<Login>(login);
                    loginMessage.Properties.AppId = _applicationId;
                    loginMessage.Properties.ReplyTo = _clientMqParameters[ClientMqParameterNames.AdminMessageRoutingKey];

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
                if (_mdeMqServerparameters.TryGetValue(MdeMqServerParameterNames.LogoutRoutingKey, out routingKey))
                {
                    Message<Logout> logoutMessage = new Message<Logout>(logout);
                    logoutMessage.Properties.AppId = _applicationId;
                    logoutMessage.Properties.ReplyTo = _clientMqParameters[ClientMqParameterNames.AdminMessageRoutingKey];

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
        /// Sends TradeHub Subscribe Message to MQ Exchange on the depending routing key
        /// </summary>
        /// <param name="subscribe">TradeHub Subscribe Message</param>
        public void SendTickSubscriptionMessage(Subscribe subscribe)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Subscribe message recieved for publishing", _type.FullName, "SendTickSubscriptionMessage");
                }

                string routingKey;
                if (_mdeMqServerparameters.TryGetValue(MdeMqServerParameterNames.SubscribeRoutingKey, out routingKey))
                {
                    Message<Subscribe> subscribeMessage = new Message<Subscribe>(subscribe);
                    subscribeMessage.Properties.AppId = _applicationId;
                    subscribeMessage.Properties.ReplyTo = _clientMqParameters[ClientMqParameterNames.TickDataRoutingKey];

                    // Send Message for publishing
                    PublishMessages(subscribeMessage, routingKey);

                    //also subscribe to zeromq
                    _socket.Subscribe(Encoding.UTF8.GetBytes(subscribe.Security.Symbol));
                    
                }
                else
                {
                    _asyncClassLogger.Info("Subscribe message not sent for publishing as routing key is unavailable.", _type.FullName,
                                "SendTickSubscriptionMessage");
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendTickSubscriptionMessage");
            }
        }

        /// <summary>
        /// Sends TradeHub Unsubscribe Message to MQ Exchange on the depending routing key
        /// </summary>
        /// <param name="unsubscribe">TradeHub Unsubscribe Message</param>
        public void SendTickUnsubscriptionMessage(Unsubscribe unsubscribe)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Unsubscribe message recieved for publishing", _type.FullName, "SendTickUnsubscriptionMessage");
                }

                string routingKey;
                if (_mdeMqServerparameters.TryGetValue(MdeMqServerParameterNames.UnsubscribeRoutingKey, out routingKey))
                {
                    Message<Unsubscribe> unsubscribeMessage = new Message<Unsubscribe>(unsubscribe);
                    unsubscribeMessage.Properties.AppId = _applicationId;

                    // Send Message for publishing
                    PublishMessages(unsubscribeMessage, routingKey);
                    _socket.Unsubscribe(Encoding.UTF8.GetBytes(unsubscribe.Security.Symbol));
                }
                else
                {
                    _asyncClassLogger.Info("Unsubscribe message not sent for publishing as routing key is unavailable.", _type.FullName,
                                "SendTickUnsubscriptionMessage");
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendTickUnsubscriptionMessage");
            }
        }

        /// <summary>
        /// Sends TradeHub HistoricDataRequest Message to MQ Exchange on the depending routing key
        /// </summary>
        /// <param name="historicDataRequest">TradeHub HistoricDataRequest Message</param>
        public void SendHistoricalBarDataRequestMessage(HistoricDataRequest historicDataRequest)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("HistoricDataRequest message recieved for publishing", _type.FullName, "SendHistoricalBarDataRequestMessage");
                }

                string routingKey;
                if (_mdeMqServerparameters.TryGetValue(MdeMqServerParameterNames.HistoricBarDataRoutingKey, out routingKey))
                {
                    Message<HistoricDataRequest> historicalDataRequestMessage = new Message<HistoricDataRequest>(historicDataRequest);
                    historicalDataRequestMessage.Properties.AppId = _applicationId;
                    historicalDataRequestMessage.Properties.ReplyTo =
                        _clientMqParameters[ClientMqParameterNames.HistoricBarDataRoutingKey];

                    // Send Message for publishing
                    PublishMessages(historicalDataRequestMessage, routingKey);
                }
                else
                {
                    _asyncClassLogger.Info("HistoricDataRequest message not sent for publishing as routing key is unavailable.", _type.FullName,
                                "SendHistoricalBarDataRequestMessage");
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendHistoricalBarDataRequestMessage");
            }
        }

        /// <summary>
        /// Sends TradeHub Live Bar Data Request Message for subscription 
        /// to MQ Exchange on the depending routing key
        /// </summary>
        /// <param name="barDataRequest">TradeHub BarDataRequest Message</param>
        public void SendLiveBarSubscriptionMessage(BarDataRequest barDataRequest)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("BarDataRequest susbcription message recieved for publishing", _type.FullName,
                                 "SendLiveBarSubscriptionMessage");
                }

                string routingKey;
                if (_mdeMqServerparameters.TryGetValue(MdeMqServerParameterNames.LiveBarSubscribeRoutingKey, out routingKey))
                {
                    var liveBarRequestMessage = new Message<BarDataRequest>(barDataRequest);
                    liveBarRequestMessage.Properties.AppId = _applicationId;
                    liveBarRequestMessage.Properties.ReplyTo =
                        _clientMqParameters[ClientMqParameterNames.LiveBarDataQueue];

                    // Send Message for publishing
                    PublishMessages(liveBarRequestMessage, routingKey);
                    _socket.Subscribe(Encoding.UTF8.GetBytes(barDataRequest.Id));
                }
                else
                {
                    _asyncClassLogger.Info("BarDataRequest subscription message not sent for publishing as routing key is unavailable.",
                                _type.FullName,
                                "SendLiveBarSubscriptionMessage");
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendLiveBarSubscriptionMessage");
            }
        }

        /// <summary>
        /// Sends TradeHub Live Bar Data Request Message for unsubscription 
        /// to MQ Exchange on the depending routing key
        /// </summary>
        /// <param name="barDataRequest">TradeHub BarDataRequest Message</param>
        public void SendLiveBarUnsubscriptionMessage(BarDataRequest barDataRequest)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("BarDataRequest unsusbcription message recieved for publishing", _type.FullName,
                                 "SendLiveBarUnsubscriptionMessage");
                }

                string routingKey;
                if (_mdeMqServerparameters.TryGetValue(MdeMqServerParameterNames.LiveBarUnsubscribeRoutingKey, out routingKey))
                {
                    var liveBarRequestMessage = new Message<BarDataRequest>(barDataRequest);
                    liveBarRequestMessage.Properties.AppId = _applicationId;

                    // Send Message for publishing
                    PublishMessages(liveBarRequestMessage, routingKey);
                    _socket.Unsubscribe(Encoding.UTF8.GetBytes(barDataRequest.Id));
                }
                else
                {
                    _asyncClassLogger.Info("BarDataRequest unsubscription message not sent for publishing as routing key is unavailable.",
                                _type.FullName,
                                "SendLiveBarUnsubscriptionMessage");
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SendLiveBarUnsubscriptionMessage");
            }
        }

        #endregion

        #region Publish Messages to MQ Exchange

        /// <summary>
        /// Publishes Login messages to the MQ Exchange
        /// </summary>
        private void PublishMessages(Message<Login> loginMessage, string routingKey)
        {
            try
            {
                //using (var channel = _advancedBus.OpenPublishChannel())
                {
                    // Publish Messages to respective Queues
                    _advancedBus.Publish(_exchange, routingKey, true, false, loginMessage);

                    if (_asyncClassLogger.IsDebugEnabled)
                    {
                        _asyncClassLogger.Debug("Login request published", _type.FullName, "PublishMessages");
                    }
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes Logout messages to the MQ Exchange
        /// </summary>
        private void PublishMessages(Message<Logout> logoutMessage, string routingKey)
        {
            try
            {
                //using (var channel = _advancedBus.OpenPublishChannel())
                {
                    // Publish Messages to respective Queues
                    _advancedBus.Publish(_exchange, routingKey, true, false, logoutMessage);

                    if (_asyncClassLogger.IsDebugEnabled)
                    {
                        _asyncClassLogger.Debug("Logout request published", _type.FullName, "PublishMessages");
                    }
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes Tick Subscription messages to the MQ Exchange
        /// </summary>
        private void PublishMessages(Message<Subscribe> subscribeMessage, string routingKey)
        {
            try
            {
                //using (var channel = _advancedBus.OpenPublishChannel())
                {
                    // Publish Messages to respective Queues
                    _advancedBus.Publish(_exchange, routingKey, true, false, subscribeMessage);

                    if (_asyncClassLogger.IsDebugEnabled)
                    {
                        _asyncClassLogger.Debug("Tick subscription request published", _type.FullName, "PublishMessages");
                    }
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes Tick Unsubscription messages to the MQ Exchange
        /// </summary>
        private void PublishMessages(Message<Unsubscribe> unsubscribeMessage, string routingKey)
        {
            try
            {
                //using (var channel = _advancedBus.OpenPublishChannel())
                {
                    // Publish Messages to respective Queues
                    _advancedBus.Publish(_exchange, routingKey, true, false, unsubscribeMessage);

                    if (_asyncClassLogger.IsDebugEnabled)
                    {
                        _asyncClassLogger.Debug("Tick unsubscription request published", _type.FullName, "PublishMessages");
                    }
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes Bar Data Request Message to the MQ Exchange
        /// </summary>
        private void PublishMessages(Message<BarDataRequest> liveBarDataRequestMessage, string routingKey)
        {
            try
            {
                //using (var channel = _advancedBus.OpenPublishChannel())
                {
                    // Publish Messages to respective Queues
                    _advancedBus.Publish(_exchange, routingKey, true, false, liveBarDataRequestMessage);

                    if (_asyncClassLogger.IsDebugEnabled)
                    {
                        _asyncClassLogger.Debug("Live Bar data request published", _type.FullName, "PublishMessages");
                    }
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes Historic Bar Data Request messages to the MQ Exchange
        /// </summary>
        private void PublishMessages(Message<HistoricDataRequest> historicDataRequestMessage, string routingKey)
        {
            try
            {
                //using (var channel = _advancedBus.OpenPublishChannel())
                {
                    // Publish Messages to respective Queues
                    _advancedBus.Publish(_exchange, routingKey, true, false, historicDataRequestMessage);

                    if (_asyncClassLogger.IsDebugEnabled)
                    {
                        _asyncClassLogger.Debug("Historical Bar data request published", _type.FullName, "PublishMessages");
                    }
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes Inquiry messages to the MQ Exchange
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
                                                    if (LogonArrived!=null)
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
        /// Binds the Tick Data Queue
        /// Starts listening to the incoming Tick Data messages
        /// </summary>
        private void SubscribeTickMarketDataMessageQueues()
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Binding Tick Data Message Queue: " + _tickDataQueue.Name, _type.FullName,
                                "SubscribeTickMarketDataMessageQueue");
                }

                // Listening to Tick Messages
                _advancedBus.Consume<Tick>(_tickDataQueue,
                                             (msg, messageReceivedInfo) =>
                                             Task.Factory.StartNew(() =>
                                             {
                                                 if (_asyncClassLogger.IsDebugEnabled)
                                                 {
                                                     _asyncClassLogger.Debug("Tick Data recieved: " + msg.Body, _type.FullName,
                                                                 "SubscribeTickMarketDataMessageQueues");
                                                 }
                                                 if (TickArrived != null)
                                                 {
                                                     TickArrived(msg.Body);
                                                 }
                                             }));

                //_basicBus.Subscribe<Tick>(_clientMqParameters[ClientMqParameterNames.TickDataRoutingKey], TickDataArrived);
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SubscribeTickMarketDataMessageQueues");
            }
        }

        /// <summary>
        /// Binds the Historical Bar Data Message Queue
        /// Starts listening to the incoming Historic Bar Data messages
        /// </summary>
        private void SubscribeHistoricBarMessageQueues()
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Binding Historic Bar Data Message Queue: " + _historicBarDataQueue.Name, _type.FullName,
                                "SubscribeHistoricBarMessageQueues");
                }

                // Listening to Historic Bar Data Messages
                _advancedBus.Consume<HistoricBarData>(
                    _historicBarDataQueue, (msg, messageReceivedInfo) =>
                                               Task.Factory.StartNew(() =>
                                               {
                                                   if (_asyncClassLogger.IsDebugEnabled)
                                                   {
                                                       _asyncClassLogger.Debug("Historic Bar Data recieved: " + msg.Body, _type.FullName,
                                                                   "SubscribeHistoricBarMessageQueues");
                                                   }
                                                   if (HistoricBarsArrived!=null)
                                                   {
                                                       HistoricBarsArrived(msg.Body); 
                                                   }
                                               }));
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SubscribeHistoricBarMessageQueues");
            }
        }

        /// <summary>
        /// Binds the Live Bar Data Message Queue
        /// Starts listening to the incoming Live Bar messages
        /// </summary>
        private void SubscribeLiveBarMessageQueues()
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Binding Live Bar Data Message Queue: " + _liveBarDataQueue.Name, _type.FullName,
                                "SubscribeLiveBarMessageQueues");
                }

                // Listening to Live Bar Data Messages
                _advancedBus.Consume<Bar>(
                    _liveBarDataQueue, (msg, messageReceivedInfo) =>
                                               Task.Factory.StartNew(() =>
                                               {
                                                   if (_asyncClassLogger.IsDebugEnabled)
                                                   {
                                                       _asyncClassLogger.Debug("Live Bar Data recieved: " + msg.Body, _type.FullName,
                                                                   "SubscribeLiveBarMessageQueues");
                                                   }
                                                   if (LiveBarArrived != null)
                                                   {
                                                       LiveBarArrived(msg.Body);
                                                   }
                                               }));
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "SubscribeLiveBarMessageQueues");
            }
        }

        /// <summary>
        /// Binds the Inquiry Response Message Queue
        /// Starts listening to the incoming Inquiry Response messages
        /// </summary>
        private void SubscribeInquiryResponseMessageQueues()
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Binding Inquiry Response Message Queue: " + _inquiryResponseQueue.Name, _type.FullName,
                                "SubscribeInquiryResponseMessageQueues");
                }

                // Listening to Inquiry Response Messages
                _advancedBus.Consume<InquiryResponse>(
                    _inquiryResponseQueue, (msg, messageReceivedInfo) =>
                                               Task.Factory.StartNew(() =>
                                               {
                                                   if (_asyncClassLogger.IsDebugEnabled)
                                                   {
                                                       _asyncClassLogger.Debug("Inquiry Response recieved: " + msg.Body.Type, _type.FullName,
                                                                   "SubscribeInquiryResponseMessageQueues");
                                                   }
                                                   if (InquiryResponseArrived != null)
                                                   {
                                                       InquiryResponseArrived(msg.Body);
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

                // Listening to Inquiry Response Messages
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
        /// Consumes Tick and Bar Data from queue
        /// </summary>
        private void ConsumeMarketDataQueue()
        {
            try
            {
                while (_consumeMarketData)
                {
                    BasicDeliverEventArgs ea = (BasicDeliverEventArgs) _dataConsumer.Queue.Dequeue();

                    // Get message bytes
                    byte[] body = ea.Body;

                    // Notify Listeners
                    MarketDataArrived(body);
                    //// Get next sequence number
                    //long sequenceNo = _dataRingBuffer.Next();

                    //// Get object from ringbuffer
                    //RabbitMqMessage entry = _dataRingBuffer[sequenceNo];

                    //// Update object parameters
                    //entry.Message = body;

                    //// Publish updated object sequence number
                    //_dataRingBuffer.Publish(sequenceNo);
                    ////_rabbitMqDataChannel.BasicAck(ea.DeliveryTag, false);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "ConsumeMarketDataQueue");
            }
        }

        #endregion

        #region Start/Shutdown

        /// <summary>
        /// Initializes all necessary connections for communication - Needs to be called manually if client MQ-Server was disconnected
        /// </summary>
        public void Initialize()
        {
            // Initialize MQ Server for communication
            InitializeMqServer();

            // Bind Inquiry Reponse Message Queue
            SubscribeInquiryResponseMessageQueues();

            //zeroMqInitialization
            InitializeZmq();
        }

        /// <summary>
        /// Connects the Rabbit MQ session
        /// </summary>
        /// <param name="appId">Unique Application ID</param>
        public void Connect(string appId)
        {
            _applicationId = appId;

            // Register Required Queues
            RegisterQueues(_exchange, appId);

            // Bind Admin Message Queue
            SubscribeAdminMessageQueues();

            //// Bind Tick Data Message Queue
            //SubscribeTickMarketDataMessageQueues();

            //// Bind Live Bar Message Queue
            //SubscribeLiveBarMessageQueues();

            // Bind Historic Bar Data Message Queue
            SubscribeHistoricBarMessageQueues();

            // Binding Heartbeat Message Queue
            SubscribeHeartbeatResponseQueue();

            //// Start Consuming Data Queue
            //_dataConsumerTask = Task.Factory.StartNew(ConsumeMarketDataQueue);
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
                    if (_heartBeatHandler!=null)
                    {
                        _heartBeatHandler.StopHandler();   
                        _heartBeatHandler.StopValidationTimer();
                    }

                    _consumeMarketData = false;

                    _advancedBus.Dispose();
                    _rabbitMqDataChannel.Close();
                    _rabbitMqDataConnection.Close();
                    _cancellationTokenSource.Cancel();
                    //_socket.Dispose();

                    _advancedBus.Connected -= OnBusConnected;
                    _advancedBus.Connected -= OnBusDisconnected;

                    _advancedBus = null;
                    _rabbitMqDataChannel = null;
                    _rabbitMqDataConnection = null;
                    _cancellationTokenSource = null;

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
        /// Sends Application Response Routing Keys Info to MDE
        /// </summary>
        /// <param name="appId">Unique Application ID</param>
        public void SendAppInfoMessage(string appId)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Sending Application Info to Market Data Engine", _type.FullName, "SendAppInfoMessage");
                }

                // Get Application Info Message
                var appInfo = CreateAppInfoMessage(appId);

                if (appInfo != null)
                {
                    var appInfoMessage= new Message<Dictionary<string, string>>(appInfo);
                    appInfoMessage.Properties.AppId = appId;
                    string routingKey = _mdeMqServerparameters[Constants.MdeMqServerParameterNames.AppInfoRoutingKey];

                    //using (var channel = _advancedBus.OpenPublishChannel())
                    {
                        // Publish Messages to respective Queues
                        _advancedBus.Publish(_exchange, routingKey, true, false, appInfoMessage);

                        if (_asyncClassLogger.IsDebugEnabled)
                        {
                            _asyncClassLogger.Debug("Application Info published", _type.FullName, "SendAppInfoMessage");
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
        /// Sends Heartbeat message to MDE
        /// </summary>
        private void OnSendHeartbeat(HeartbeatMessage heartbeat)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Sending heartbeat message to MDE", _type.FullName, "OnSendHeartbeat");
                }

                string routingKey = _mdeMqServerparameters["HeartbeatRoutingKey"];
                
                //using (var channel = _advancedBus.OpenPublishChannel())
                {
                    // Add the Routing Key to which the MDE-Server can respond to
                    heartbeat.ReplyTo = _clientMqParameters[Constants.ClientMqParameterNames.HeartbeatResponseRoutingKey];

                    // Create EasyNetQ Message for publishing
                    IMessage<HeartbeatMessage> heartbeatMessage = new Message<HeartbeatMessage>(heartbeat);

                    // Publish Messages to respective Queues
                    _advancedBus.Publish(_exchange, routingKey, true, false, heartbeatMessage);

                    if (_asyncClassLogger.IsDebugEnabled)
                    {
                        _asyncClassLogger.Debug("Application Info published", _type.FullName, "OnSendHeartbeat");
                    }
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnSendHeartbeat");
            }
        }

        /// <summary>
        /// Called when new Market Data message is received and processed by Disruptor
        /// </summary>
        /// <param name="rabbitMqMessage"></param>
        private void OnMarketDataReceived(RabbitMqRequestMessage rabbitMqMessage)
        {
            // Notify listeners
            //MarketDataArrived(rabbitMqMessage);
        }

        /// <summary>
        /// Called when new Tick message is received and processed by Disruptor
        /// </summary>
        /// <param name="message">message array consumed</param>
        private void OnTickDataReceived(string[] message)
        {
            try
            {
                Tick entry = new Tick();

                if (ParseToTick(entry, message))
                {
                    TickArrived(entry);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnTickDataReceived");
            }
        }

        /// <summary>
        /// Called when new Bar message is received and processed by Disruptor
        /// </summary>
        /// <param name="message">message array consumed</param>
        private void OnBarDataReceived(string[] message)
        {
            try
            {
                Bar entry = new Bar("");

                if (ParseToBar(entry, message))
                {
                    LiveBarArrived(entry);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnBarDataReceived");
            }
        }

        /// <summary>
        /// Creats tick object from incoming string message
        /// </summary>
        /// <param name="tick">Tick to add values to</param>
        /// <param name="message">Received message</param>
        /// <returns></returns>
        private bool ParseToTick(Tick tick, string[] message)
        {
            try
            {
                // Get Bid Values
                tick.BidPrice = Convert.ToDecimal(message[1]);
                tick.BidSize = Convert.ToDecimal(message[2]);

                // Get Ask Values
                tick.AskPrice = Convert.ToDecimal(message[3]);
                tick.AskSize = Convert.ToDecimal(message[4]);

                // Get Last Values
                tick.LastPrice = Convert.ToDecimal(message[5]);
                tick.LastSize = Convert.ToDecimal(message[6]);

                // Get Symbol
                tick.Security = new Security() { Symbol = message[7] };
                // Get Time Value
                tick.DateTime = DateTime.ParseExact(message[8], "M/d/yyyy h:mm:ss.fff tt", CultureInfo.InvariantCulture);
                // Get Provider name
                tick.MarketDataProvider = message[9];
                return true;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "ParseToTick");
                return true;
            }
        }

        /// <summary>
        /// Parse String into bar
        /// </summary>
        /// <param name="bar">Bar to add values to</param>
        /// <param name="message">Received message</param>
        /// <returns></returns>
        private bool ParseToBar(Bar bar, string[] message)
        {
            try
            {
                bar.Security = new Security { Symbol = message[6] };
                bar.Close = Convert.ToDecimal(message[1]);
                bar.Open = Convert.ToDecimal(message[2]);
                bar.High = Convert.ToDecimal(message[3]);
                bar.Low = Convert.ToDecimal(message[4]);
                bar.Volume = Convert.ToInt64((message[5]));
                bar.DateTime = DateTime.ParseExact(message[7], "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                bar.MarketDataProvider = message[8];
                bar.RequestId = message[9];
                return true;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "ParseToBar");
                return false;
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
        /// Creates Application Info message
        /// </summary>
        /// <param name="appId">Unique Application ID</param>
        private Dictionary<string, string> CreateAppInfoMessage(string appId)
        {
            try
            {
                var appInfo = new Dictionary<string, string>();

                // Add Admin Info;
                appInfo.Add("Admin", _clientMqParameters[Constants.ClientMqParameterNames.AdminMessageRoutingKey]);

                // Add Tick Info
                appInfo.Add("Tick", _clientMqParameters[Constants.ClientMqParameterNames.TickDataRoutingKey]);

                // Add Live Bar Info
                appInfo.Add("LiveBar", _clientMqParameters[Constants.ClientMqParameterNames.TickDataRoutingKey]);
                //appInfo.Add("LiveBar", _clientMqParameters[Constants.ClientMqParameterNames.LiveBarDataRoutingKey]);

                // Add Historic Info
                appInfo.Add("HistoricBar", _clientMqParameters[Constants.ClientMqParameterNames.HistoricBarDataRoutingKey]);

                return appInfo;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "CreateAppInfoMessage");
                return null;
            }
        }

        /// <summary>
        /// Starts the Heartbeat sequence
        /// </summary>
        public void StartHeartbeat()
        {
            if (_heartBeatHandler == null)
            {
                _heartBeatHandler = new ClientHeartBeatHandler(_asyncClassLogger, _applicationId, _heartbeatInterval);
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

        /// <summary>
        /// Read ZeroMQ IP and Port from file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public string[] ReadZeroMqConfig(string filename)
        {
            string[] config=new string[2];
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

                            if (_asyncClassLogger.IsInfoEnabled)
                            {
                                _asyncClassLogger.Info(
                                    "Adding parameter: " + node.Name + " | Value: " + node.InnerText, _type.FullName,
                                    "ReadZeroMqConfig");
                            }
                        }
                    }
                   

                }
               
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "ReadZeroMqConfig");
            }

            return config;

        }

        #region ZeroMQ Receiver

        private void ZeroMqReceiver()
        {
            try
            {
                //ZmqContext ctxContext = ZmqContext.Create();
              //  using (var socket = _ctx1.CreateSocket(SocketType.PUSH))
              //  {
                //    socket.Bind("inproc://tradehubtest"+_randomNumber);
                    while (_consumeMarketData)
                    {
                        //byte[] array = _socket.Recv();
                        //var msg = UTF8Encoding.UTF8.GetString(array);
                        var msg = _socket.Recv(Encoding.UTF8);
                        string[] tick = msg.Split('|');
                      //  Logger.Info("rcvd", "", "");
                       // socket.Send(_socket.Receive(Encoding.UTF8), Encoding.UTF8);

                         MarketDataArrived(Encoding.UTF8.GetBytes(tick[1]));

                        //string[] tick = msg.Split('|');
                        //if (tick[1].StartsWith("TICK"))
                        //{
                        //    OnTickDataReceived(tick[1].Split(','));
                        //}
                        //else
                        //{
                        //    OnBarDataReceived(tick[1].Split(','));
                        //}

                    }
               // }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception,_type.FullName,"ZeroMqReceiver");
            }

        }

        private void ZmqPull1()
        {
            //try
            //{
            //    Thread.Sleep(2000);
            //    Logger.Info("Started pull1 InProcID="+_randomNumber, _type.FullName, "ZmqPull1");
            //    // ZmqContext ctxContext = ZmqContext.Create();
            //    using (var socket = _ctx1.CreateSocket(SocketType.PULL))
            //    {
            //        socket.Connect("inproc://tradehubtest"+_randomNumber);
            //       // socket.SubscribeAll();
            //        while (true)
            //        {
            //            //byte[] array = _socket.Recv();
            //            //var msg = UTF8Encoding.UTF8.GetString(array);
            //            var msg1 = socket.Receive(Encoding.UTF8);
            //            string[] tick = msg1.Split('|');
            //            MarketDataArrived(Encoding.UTF8.GetBytes(tick[1]));
            //            //if (msg[1].StartsWith("TICK"))
            //            //{
            //            //    OnTickDataReceived(msg[1].Split(','));
            //            //    //Logger.Debug("rcv1", "", "");

            //            //}
            //            //else
            //            //{
            //            //    OnBarDataReceived(msg[1].Split(','));
            //            //}

            //        }
            //    }
            //}
            //catch (Exception exception)
            //{
            //    Logger.Error(exception,_type.FullName,"ZmqPull1");
            //}
        }
        private void ZmqPull2()
        {
            //Thread.Sleep(2000);
            //Logger.Info("Started pull2", "", "");
            ////ZmqContext ctxContext = ZmqContext.Create();
            //ZmqSocket socket = _ctx1.CreateSocket(SocketType.PULL);
            //socket.Connect("inproc://tradehubtest");
            ////socket.SubscribeAll();
            //while (true)
            //{
            //    //byte[] array = _socket.Recv();
            //    //var msg = UTF8Encoding.UTF8.GetString(array);
            //    var msg1 = socket.Receive(Encoding.UTF8);

            //    string[] msg = msg1.Split('|');
            //    if (msg[1].StartsWith("TICK"))
            //    {
            //        OnTickDataReceived(msg[1].Split(','));
            //        //Logger.Debug("rcv2","","");
            //    }
            //    else
            //    {
            //        OnBarDataReceived(msg[1].Split(','));
            //    }

            //}
        }
        private void ZmqPull3()
        {
           // Thread.Sleep(2000);
           // Logger.Info("Started pull3", "", "");
           // //ZmqContext ctxContext = ZmqContext.Create();
           // ZmqSocket socket = _ctx1.CreateSocket(SocketType.PULL);
           // socket.Connect("inproc://tradehubtest");
           //// socket.SubscribeAll();
           // while (true)
           // {
           //     //byte[] array = _socket.Recv();
           //     //var msg = UTF8Encoding.UTF8.GetString(array);
           //     var msg1 = socket.Receive(Encoding.UTF8);

           //     string[] msg = msg1.Split('|');
           //     if (msg[1].StartsWith("TICK"))
           //     {
           //         OnTickDataReceived(msg[1].Split(','));
           //         //Logger.Debug("rcv2","","");
           //     }
           //     else
           //     {
           //         OnBarDataReceived(msg[1].Split(','));
           //     }

           // }
        }

        #endregion

        #region Implementation of IEventHandler<in RabbitMqMessage>

        /// <summary>
        /// Called when a publisher has committed an event to the <see cref="T:Disruptor.RingBuffer`1"/>
        /// </summary>
        /// <param name="data">Data committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="sequence">Sequence number committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="endOfBatch">flag to indicate if this is the last event in a batch from the <see cref="T:Disruptor.RingBuffer`1"/></param>
        public void OnNext(RabbitMqRequestMessage data, long sequence, bool endOfBatch)
        {
            try
            {
                //NOTE: Now Sending out orignal message to accpected listeners
                //string message = Encoding.UTF8.GetString(data.Message);

                //var messageArray = message.Split(',');

                //if (messageArray[0].Equals("TICK"))
                //    OnTickDataReceived(messageArray);
                //else
                //    OnBarDataReceived(messageArray);

                OnMarketDataReceived(data);
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnNext");
            }
        }

        #endregion
    }
}
