using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataEngine.Client.Constants;

namespace TradeHub.MarketDataEngine.Client.Service
{
    /// <summary>
    /// Handles Rabbit MQ Communications for the Market Data Engine Client
    /// </summary>
    internal class MqServer
    {
        private Type _type = typeof (MqServer);

        public event Action<string> LogonArrived;
        public event Action<string> LogoutArrived;
        public event Action<Tick> TickArrived;
        public event Action<HistoricBarData> HistoricBarsArrived;

        // Holds reference for the Advance Bus
        private IAdvancedBus _advancedBus;

        // Exchange containing Queues
        private IExchange _exchange;

        // Queue will contain Admin messages
        private IQueue _adminMessageQueue;

        // Queue will contain Tick messages
        private IQueue _tickDataQueue;

        // Queue will contain Historic Bar Data messages
        private IQueue _historicBarDataQueue;

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
        /// Argument Constructor
        /// </summary>
        /// <param name="mdeMqServerparameters">Contains Parameters required for sending messages to MDE MQ Server</param>
        /// <param name="clientMqParameters">Contains Parameters for getting response messages from MDE MQ Server </param>
        public MqServer(Dictionary<string, string> mdeMqServerparameters, Dictionary<string, string> clientMqParameters)
        {
            _mdeMqServerparameters = mdeMqServerparameters;
            _clientMqParameters = clientMqParameters;

            // Initialize MQ Server for communication
            InitializeMqServer();

            // Bind Admin Message Queue
            SubscribeAdminMessageQueues();

            // Bind Tick Data Message Queue
            SubscribeTickMarketDataMessageQueues();

            // Bind Historic Bar Data Message Queue
            SubscribeHistoricBarMessageQueues();
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
                string connectionString = _mdeMqServerparameters["ConnectionString"];
                if (!string.IsNullOrEmpty(connectionString))
                {
                    // Initialize Rabbit MQ Hutch 
                    InitializeRabbitHutch(connectionString);

                    // Get Exchange Name from Config File
                    string exchangeName = _mdeMqServerparameters["Exchange"];

                    if (!string.IsNullOrEmpty(exchangeName))
                    {
                        // Use the Exchange Name to Initialize Rabbit Exchange
                        InitializeExchange(exchangeName);

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
                Logger.Error(exception, _type.FullName, "InitializeMqServer");
            }
        }

        /// <summary>
        /// Initializes EasyNetQ's Advacne Rabbit Hutch
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
                Logger.Error(exception, _type.FullName, "InitializeRabbitHutch");
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
                _exchange = Exchange.DeclareDirect(exchangeName);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "InitializeExchange");
            }
        }

        /// <summary>
        /// Initialize Queues and perform binding
        /// </summary>
        private void RegisterQueues(IExchange exchange)
        {
            // Bind Admin Message Queue
            BindQueue(ClientMqParameterNames.AdminMessageQueue, ClientMqParameterNames.AdminMessageRoutingKey,
                      ref _adminMessageQueue, exchange);

            // Bind Tick Data Queue
            BindQueue(ClientMqParameterNames.TickDataQueue, ClientMqParameterNames.TickDataRoutingKey,
                      ref _tickDataQueue, exchange);

            // Bind Historic Bar Data Queue
            BindQueue(ClientMqParameterNames.HistoricBarDataQueue, ClientMqParameterNames.HistoricBarDataRoutingKey,
                      ref _historicBarDataQueue, exchange);
        }

        /// <summary>
        /// Initializes RabbitMQ Queue
        /// </summary>
        private IQueue InitializeQueue(IExchange exchange, string queueName, string routingKey)
        {
            try
            {
                // Initialize specified Queue
                IQueue queue = Queue.Declare(false, true, false, queueName, null);

                // Bind Queue to already initialized Exchange with the specified Routing Key
                queue.BindTo(exchange, routingKey);
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
                // Get Queue Name from Parameters Dictionary
                string queueName = _clientMqParameters[queueHeader];
                // Get Routing Key from Parameters Dictionary
                string routingKey = _clientMqParameters[routingKeyHeader];

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

        #region Outgoing message to Market Data Engine

        /// <summary>
        /// Sends TradeHub Login Message to MQ Exchange on the depending routing key
        /// </summary>
        /// <param name="login">TradeHub Login Message</param>
        /// <param name="appId">Application ID to uniquely identify the running instance</param>
        public void SendLoginMessage(Login login, string appId)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Login message recieved for publishing", _type.FullName, "SendLoginMessage");
                }

                string routingKey;
                if (_mdeMqServerparameters.TryGetValue(MdeMqServerParameterNames.LoginRoutingKey, out routingKey))
                {
                    Message<Login> loginMessage = new Message<Login>(login);
                    loginMessage.Properties.AppId = appId;
                    loginMessage.Properties.ReplyTo = _clientMqParameters[ClientMqParameterNames.AdminMessageRoutingKey];

                    // Send Message for publishing
                    PublishMessages(loginMessage, routingKey);
                }
                else
                {
                    Logger.Info("Login message not sent for publishing as routing key is unavailable.", _type.FullName,
                                "SendLoginMessage");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendLoginMessage");
            }
        }

        /// <summary>
        /// Sends TradeHub Logout Message to MQ Exchange on the depending routing key
        /// </summary>
        /// <param name="logout">TradeHub Logout Message</param>
        /// <param name="appId">Application ID to uniquely identify the running instance</param>
        public void SendLogoutMessage(Logout logout, string appId)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Logout message recieved for publishing", _type.FullName, "SendLogoutMessage");
                }

                string routingKey;
                if (_mdeMqServerparameters.TryGetValue(MdeMqServerParameterNames.LogoutRoutingKey, out routingKey))
                {
                    Message<Logout> logoutMessage = new Message<Logout>(logout);
                    logoutMessage.Properties.AppId = appId;
                    logoutMessage.Properties.ReplyTo = _clientMqParameters[ClientMqParameterNames.AdminMessageRoutingKey];

                    // Send Message for publishing
                    PublishMessages(logoutMessage, routingKey);
                }
                else
                {
                    Logger.Info("Logout message not sent for publishing as routing key is unavailable.", _type.FullName,
                                "SendLogoutMessage");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendLogoutMessage");
            }
        }

        /// <summary>
        /// Sends TradeHub Subscribe Message to MQ Exchange on the depending routing key
        /// </summary>
        /// <param name="subscribe">TradeHub Subscribe Message</param>
        /// <param name="appId">Application ID to uniquely identify the running instance</param>
        public void SendTickSubscriptionMessage(Subscribe subscribe, string appId)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Subscribe message recieved for publishing", _type.FullName, "SendTickSubscriptionMessage");
                }

                string routingKey;
                if (_mdeMqServerparameters.TryGetValue(MdeMqServerParameterNames.SubscribeRoutingKey, out routingKey))
                {
                    Message<Subscribe> subscribeMessage = new Message<Subscribe>(subscribe);
                    subscribeMessage.Properties.AppId = appId;
                    subscribeMessage.Properties.ReplyTo = _clientMqParameters[ClientMqParameterNames.TickDataRoutingKey];

                    // Send Message for publishing
                    PublishMessages(subscribeMessage, routingKey);
                }
                else
                {
                    Logger.Info("Subscribe message not sent for publishing as routing key is unavailable.", _type.FullName,
                                "SendTickSubscriptionMessage");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendTickSubscriptionMessage");
            }
        }

        /// <summary>
        /// Sends TradeHub Unsubscribe Message to MQ Exchange on the depending routing key
        /// </summary>
        /// <param name="unsubscribe">TradeHub Unsubscribe Message</param>
        /// <param name="appId">Application ID to uniquely identify the running instance</param>
        public void SendTickUnsubscriptionMessage(Unsubscribe unsubscribe, string appId)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Unsubscribe message recieved for publishing", _type.FullName, "SendTickUnsubscriptionMessage");
                }

                string routingKey;
                if (_mdeMqServerparameters.TryGetValue(MdeMqServerParameterNames.SubscribeRoutingKey, out routingKey))
                {
                    Message<Unsubscribe> unsubscribeMessage = new Message<Unsubscribe>(unsubscribe);
                    unsubscribeMessage.Properties.AppId = appId;

                    // Send Message for publishing
                    PublishMessages(unsubscribeMessage, routingKey);
                }
                else
                {
                    Logger.Info("Unsubscribe message not sent for publishing as routing key is unavailable.", _type.FullName,
                                "SendTickUnsubscriptionMessage");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendTickUnsubscriptionMessage");
            }
        }

        /// <summary>
        /// Sends TradeHub HistoricDataRequest Message to MQ Exchange on the depending routing key
        /// </summary>
        /// <param name="historicDataRequest">TradeHub HistoricDataRequest Message</param>
        /// <param name="appId">Application ID to uniquely identify the running instance</param>
        public void SendHistoricalBarDataRequestMessage(HistoricDataRequest historicDataRequest, string appId)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("HistoricDataRequest message recieved for publishing", _type.FullName, "SendHistoricalBarDataRequestMessage");
                }

                string routingKey;
                if (_mdeMqServerparameters.TryGetValue(MdeMqServerParameterNames.SubscribeRoutingKey, out routingKey))
                {
                    Message<HistoricDataRequest> historicalDataRequestMessage = new Message<HistoricDataRequest>(historicDataRequest);
                    historicalDataRequestMessage.Properties.AppId = appId;
                    historicalDataRequestMessage.Properties.ReplyTo =
                        _clientMqParameters[ClientMqParameterNames.HistoricBarDataRoutingKey];

                    // Send Message for publishing
                    PublishMessages(historicalDataRequestMessage, routingKey);
                }
                else
                {
                    Logger.Info("HistoricDataRequest message not sent for publishing as routing key is unavailable.", _type.FullName,
                                "SendHistoricalBarDataRequestMessage");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendHistoricalBarDataRequestMessage");
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
                using (var channel = _advancedBus.OpenPublishChannel())
                {
                    // Publish Messages to respective Queues
                    channel.Publish(_exchange, routingKey, loginMessage);

                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Login request published", _type.FullName, "PublishMessages");
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes Logout messages to the MQ Exchange
        /// </summary>
        private void PublishMessages(Message<Logout> logoutMessage, string routingKey)
        {
            try
            {
                using (var channel = _advancedBus.OpenPublishChannel())
                {
                    // Publish Messages to respective Queues
                    channel.Publish(_exchange, routingKey, logoutMessage);

                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Logout request published", _type.FullName, "PublishMessages");
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes Tick Subscription messages to the MQ Exchange
        /// </summary>
        private void PublishMessages(Message<Subscribe> subscribeMessage, string routingKey)
        {
            try
            {
                using (var channel = _advancedBus.OpenPublishChannel())
                {
                    // Publish Messages to respective Queues
                    channel.Publish(_exchange, routingKey, subscribeMessage);

                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Tick subscription request published", _type.FullName, "PublishMessages");
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes Tick Unsubscription messages to the MQ Exchange
        /// </summary>
        private void PublishMessages(Message<Unsubscribe> unsubscribeMessage, string routingKey)
        {
            try
            {
                using (var channel = _advancedBus.OpenPublishChannel())
                {
                    // Publish Messages to respective Queues
                    channel.Publish(_exchange, routingKey, unsubscribeMessage);

                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Tick unsubscription request published", _type.FullName, "PublishMessages");
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes Historic Bar Data Request messages to the MQ Exchange
        /// </summary>
        private void PublishMessages(Message<HistoricDataRequest> historicDataRequestMessage, string routingKey)
        {
            try
            {
                using (var channel = _advancedBus.OpenPublishChannel())
                {
                    // Publish Messages to respective Queues
                    channel.Publish(_exchange, routingKey, historicDataRequestMessage);

                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Historical Bar data request published", _type.FullName, "PublishMessages");
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMessages");
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
                    Logger.Info("Binding Admin Message Queue: " + _adminMessageQueue, _type.FullName,
                                "SubscribeAdminMessageQueues");
                }

                // Listening to Login Messages
                _advancedBus.Subscribe<string>(
                    _adminMessageQueue, (msg, messageReceivedInfo) =>
                                        Task.Factory.StartNew(
                                            () =>
                                            {
                                                if (msg.Body.Contains("Logon"))
                                                {
                                                    if (Logger.IsDebugEnabled)
                                                    {
                                                        Logger.Debug("Login message recieved: " + msg.Body, _type.FullName,
                                                                    "SubscribeAdminMessageQueues");
                                                    }
                                                    if (LogonArrived!=null)
                                                    {
                                                        LogonArrived(msg.Body);
                                                    }
                                                }
                                                else if (msg.Body.Contains("Logout"))
                                                {
                                                    if (Logger.IsDebugEnabled)
                                                    {
                                                        Logger.Debug("Logout Message recieved: " + msg.Body, _type.FullName,
                                                                    "SubscribeAdminMessageQueues");
                                                    }
                                                    if (LogoutArrived != null)
                                                    {
                                                        LogoutArrived(msg.Body);
                                                    }
                                                }
                                            }));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeAdminMessageQueues");
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
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Binding Tick Data Message Queue: " + _tickDataQueue, _type.FullName,
                                "SubscribeTickMarketDataMessageQueue");
                }

                // Listening to Subscription Messages
                _advancedBus.Subscribe<Tick>(
                    _tickDataQueue, (msg, messageReceivedInfo) =>
                                               Task.Factory.StartNew(() =>
                                               {
                                                   if (Logger.IsDebugEnabled)
                                                   {
                                                       Logger.Debug("Tick Data Message recieved: " + msg.Body, _type.FullName,
                                                                   "SubscribeTickMarketDataMessageQueue");
                                                   }

                                                   if(TickArrived!= null)
                                                   {
                                                       TickArrived(msg.Body); 
                                                       if (Logger.IsDebugEnabled)
                                                       {
                                                           Logger.Debug("Tick event fired: " + msg.Body, _type.FullName,
                                                                       "SubscribeTickMarketDataMessageQueue");
                                                       }
                                                   }
                                               }));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeTickMarketDataMessageQueues");
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
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Binding Historic Bar Data Message Queue: " + _historicBarDataQueue, _type.FullName,
                                "SubscribeHistoricBarMessageQueues");
                }

                // Listening to Subscription Messages
                _advancedBus.Subscribe<HistoricBarData>(
                    _historicBarDataQueue, (msg, messageReceivedInfo) =>
                                               Task.Factory.StartNew(() =>
                                               {
                                                   if (Logger.IsDebugEnabled)
                                                   {
                                                       Logger.Debug("Historic Bar Data recieved: " + msg.Body, _type.FullName,
                                                                   "SubscribeHistoricBarMessageQueues");
                                                   }
                                                   if (HistoricBarsArrived!=null)
                                                   {
                                                       HistoricBarsArrived(msg.Body); 
                                                       if (Logger.IsDebugEnabled)
                                                       {
                                                           Logger.Debug("Historic Bar event fired: " + msg.Body, _type.FullName,
                                                                       "SubscribeHistoricBarMessageQueues");
                                                       }
                                                   }
                                               }));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeHistoricBarMessageQueues");
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
                if (_advancedBus != null)
                {
                    _advancedBus.Connected -= OnBusConnected;
                    _advancedBus.Connected -= OnBusDisconnected;

                    _advancedBus.Dispose();

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

        /// <summary>
        /// Raised when Advanced Bus is successfully connected
        /// </summary>
        private void OnBusConnected()
        {
            if (_advancedBus.IsConnected)
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Successfully connected to MQ Server", _type.FullName, "OnBusConnected");
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
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Successfully disconnected to MQ Server", _type.FullName, "OnBusDisconnected");
                }
            }
        }

    }
}
