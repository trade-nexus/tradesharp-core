using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.Inquiry;
using TradeHub.PositionEngine.Client.Constants;

namespace TradeHub.PositionEngine.Client.Service
{
    public class PositionEngineClientMqServer
    {
        private Type _type = typeof (PositionEngineClientMqServer);
        #region Rabbit MQ Fields

        // Holds reference for the Advance Bus
        private IAdvancedBus _advancedBus;

        // Exchange containing Queues
        private IExchange _exchange;

        // Queue will contain Admin messages
        private IQueue _inquiryResponseQueue;

     // Queue will contain Locate messages from OEE
        private IQueue _positionMessageQueue;

        #endregion


        #region Events

        public event Action BusConnected;
        public event Action<Position> PositionArrived;
        public event Action<InquiryResponse> InquiryResponseArrived;

        #endregion

        private string _applicationId = string.Empty;

        /// <summary>
        /// Order Execution Engine MQ-Server Parameters
        /// Key = Parameter Name
        /// Value = Parameter Value
        /// </summary>
        private Dictionary<string, string> _peMqServerParameters;

        /// <summary>
        /// Key = Parameter Name
        /// Value = Parameter Value
        /// </summary>
        private Dictionary<string, string> _clientMqParameters;

        public PositionEngineClientMqServer(Dictionary<string,string> peMqserverParameter,Dictionary<string,string> clientMqParameters)
        {
            _peMqServerParameters = peMqserverParameter;
            _clientMqParameters = clientMqParameters;

            // Initialize MQ Server for communication
            InitializeMqServer();

            // Bind Inquiry Reponse Message Queue
            SubscribeInquiryResponseMessageQueue();

        }
        /// <summary>
        /// Initializes MQ Server related parameters
        /// </summary>
        private void InitializeMqServer()
        {
            try
            {
                // Create Rabbit MQ Hutch 
                string connectionString = _peMqServerParameters["ConnectionString"];
                if (!string.IsNullOrEmpty(connectionString))
                {
                    // Initialize Rabbit MQ Hutch 
                    InitializeRabbitHutch(connectionString);

                    // Get Exchange Name from Config File
                    string exchangeName = _peMqServerParameters["Exchange"];

                    if (!string.IsNullOrEmpty(exchangeName))
                    {
                        // Use the Exchange Name to Initialize Rabbit Exchange
                        InitializeExchange(exchangeName);

                        if (_exchange != null)
                        {
                            // Bind Inquiry Response Queue
                            BindQueue(PeClientMqParameters.InquiryResponseQueue, PeClientMqParameters.InquiryResponseRoutingKey,
                                      ref _inquiryResponseQueue, _exchange, "");
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
        /// Connects the Rabbit MQ session
        /// </summary>
        /// <param name="appId">Unique Application ID</param>
        public void Connect(string appId)
        {
            _applicationId = appId;

            // Register Reqired Queues
            RegisterQueues(_exchange, appId);

            //Listen Position Messages
            SubscribePositionMessageQueues();

            
        }

        /// <summary>
        /// Initialize Queues and perform binding
        /// </summary>
        private void RegisterQueues(IExchange exchange, string appId)
        {
            BindQueue(PeClientMqParameters.PositionMessageQueue,PeClientMqParameters.PositionMessageRoutingKey,ref _positionMessageQueue,exchange,appId);
        }

        /// <summary>
        /// Request Provider Position
        /// </summary>
        public void SubscribeProviderPosition(string provider)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Provider message recieved for publishing", _type.FullName, "SubscribeProviderPosition");
            }

            string routingKey;
            if (_peMqServerParameters.TryGetValue("ProviderRequestRoutingKey", out routingKey))
            {
                Message<string> providerRequestMessage =new Message<string>(provider);
                providerRequestMessage.Properties.AppId = _applicationId;
                providerRequestMessage.Properties.ReplyTo = _clientMqParameters["InquiryResponseRoutingKey"];

                // Send Message for publishing
                PublishProviderRequestMessages(providerRequestMessage, routingKey);
            }
            else
            {
                Logger.Info("Provider request message not sent for publishing as routing key is unavailable.", _type.FullName,
                            "SubscribeProviderPosition");
            }

        }

        /// <summary>
        /// Request Provider Position
        /// </summary>
        public void UnSubscribeProviderPosition(string provider)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Provider message recieved for publishing", _type.FullName, "SubscribeProviderPosition");
            }

            string routingKey;
            if (_peMqServerParameters.TryGetValue("ProviderRequestRoutingKey", out routingKey))
            {
                Message<string> providerRequestMessage = new Message<string>(provider+",unsubscribe");
                providerRequestMessage.Properties.AppId = _applicationId;
                providerRequestMessage.Properties.ReplyTo = _clientMqParameters["InquiryResponseRoutingKey"];

                // Send Message for publishing
                PublishProviderRequestMessages(providerRequestMessage, routingKey);
            }
            else
            {
                Logger.Info("Provider request message not sent for publishing as routing key is unavailable.", _type.FullName,
                            "SubscribeProviderPosition");
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
                IQueue queue = _advancedBus.QueueDeclare(queueName, false, false, true, true);
                // Bind Queue to already initialized Exchange with the specified Routing Key
                //queue.BindTo(exchange, routingKey);
                _advancedBus.Bind(exchange, queue, routingKey);
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
                Logger.Error(exception, _type.FullName, "BindQueue");
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
                //_exchange = Exchange.DeclareDirect(exchangeName);
                _exchange = _advancedBus.ExchangeDeclare(exchangeName, ExchangeType.Direct, true, false, true);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "InitializeExchange");
            }
        }

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

                if (BusConnected != null)
                {
                    BusConnected();
                }
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
                if (_advancedBus != null)
                {
                   
                    _advancedBus.Dispose();

                    _advancedBus.Connected -= OnBusConnected;
                    _advancedBus.Connected -= OnBusDisconnected;

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

        /// <summary>
        /// Binds the Inquiry Response Message Queue
        /// Starts listening to the incoming Inquiry Response messages
        /// </summary>
        private void SubscribeInquiryResponseMessageQueue()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Binding Inquiry Response Message Queue: " + _inquiryResponseQueue.Name, _type.FullName,
                                "SubscribeInquiryResponseMessageQueue");
                }

                // Listening to Inquiry Response Messages
                _advancedBus.Consume<InquiryResponse>(
                    _inquiryResponseQueue, (msg, messageReceivedInfo) =>
                                               Task.Factory.StartNew(() =>
                                               {
                                                   if (Logger.IsDebugEnabled)
                                                   {
                                                       Logger.Debug("Inquiry Response recieved: " + msg.Body.Type, _type.FullName,
                                                                   "SubscribeInquiryResponseMessageQueue");
                                                   }
                                                   if (InquiryResponseArrived != null)
                                                   {
                                                       InquiryResponseArrived(msg.Body);
                                                       if (Logger.IsDebugEnabled)
                                                       {
                                                           Logger.Debug("Inquiry Response event fired: " + msg.Body.Type, _type.FullName,
                                                                       "SubscribeInquiryResponseMessageQueue");
                                                       }
                                                   }
                                               }));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeInquiryResponseMessageQueues");
            }
        }

        //Listen to Position Message.
        private void SubscribePositionMessageQueues()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Binding Admin Message Queue: " + _positionMessageQueue.Name, _type.FullName,
                                "SubscribeAdminMessageQueues");
                }

                // Listening to Admin Messages
                _advancedBus.Consume<Position>(
                    _positionMessageQueue, (msg, messageReceivedInfo) =>
                                        Task.Factory.StartNew(
                                            () =>
                                            {
                                              
                                                    if (PositionArrived != null)
                                                    {
                                                        PositionArrived(msg.Body);
                                                    }
                                               
                                              
                                            }));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeAdminMessageQueues");
            }
        }

        /// <summary>
        /// Sends TradeHub Inquiry Message to MQ Exchange on the depending routing key
        /// </summary>
        /// <param name="inquiry">TradeHub Inquiry Message</param>
        public void SendInquiryMessage(InquiryMessage inquiry)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Inquiry message recieved for publishing", _type.FullName, "SendInquiryMessage");
                }

                string routingKey;
                if (_peMqServerParameters.TryGetValue("InquiryRoutingKey", out routingKey))
                {
                    Message<InquiryMessage> inquiryMessage = new Message<InquiryMessage>(inquiry);
                    inquiryMessage.Properties.AppId = _applicationId;
                    inquiryMessage.Properties.ReplyTo = _clientMqParameters["InquiryResponseRoutingKey"];

                    // Send Message for publishing
                    PublishMessages(inquiryMessage, routingKey);
                }
                else
                {
                    Logger.Info("Inquiry message not sent for publishing as routing key is unavailable.", _type.FullName,
                                "SendInquiryMessage");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendInquiryMessage");
            }
        }
        /// <summary>
        /// Publishes Inquiry messages to the MQ Exchange
        /// </summary>
        private void PublishMessages(Message<InquiryMessage> inquiryMessage, string routingKey)
        {
            try
            {
               // using (var channel = _advancedBus.OpenPublishChannel())
                {
                    // Publish Messages to respective Queues
                    _advancedBus.Publish(_exchange, routingKey,true,false, inquiryMessage);

                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Inquiry request published", _type.FullName, "PublishMessages");
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMessages");
            }
        }

        /// <summary>
        /// Publishes Provider Position Request messages to the MQ Exchange
        /// </summary>
        private void PublishProviderRequestMessages(Message<string> providerRequest, string routingKey)
        {
            try
            {
              //  using (var channel = _advancedBus.OpenPublishChannel())
                {
                    // Publish Messages to respective Queues
                    _advancedBus.Publish(_exchange, routingKey, true, false, providerRequest);

                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Provider request published", _type.FullName, " PublishProviderRequestMessages");
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishProviderRequestMessages");
            }
        }

        /// <summary>
        /// Sends Application Response Routing Keys Info to OEE
        /// </summary>
        /// <param name="appId">Unique Application ID</param>
        public void SendAppInfoMessage(string appId)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Sending Application Info to Order Execution Engine", _type.FullName, "SendAppInfoMessage");
                }

                // Get Application Info Message
                var appInfo = CreateAppInfoMessage(appId);

                if (appInfo != null)
                {
                    var appInfoMessage = new Message<Dictionary<string, string>>(appInfo);
                    appInfoMessage.Properties.AppId = appId;
                    string routingKey = _peMqServerParameters[Constants.PeMqServerParameters.AppInfoRoutingKey];

                  //  using (var channel = _advancedBus.OpenPublishChannel())
                    {
                        // Publish Messages to respective Queues
                        _advancedBus.Publish(_exchange, routingKey, true, false, appInfoMessage);

                        if (Logger.IsDebugEnabled)
                        {
                            Logger.Debug("Application Info published", _type.FullName, "PublishMessages");
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendAppInfoMessage");
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
                appInfo.Add("Position", _clientMqParameters[PeClientMqParameters.PositionMessageRoutingKey]);

                return appInfo;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "CreateAppInfoMessage");
                return null;
            }
        }
    }
}
