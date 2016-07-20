using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;
using EasyNetQ;
using EasyNetQ.Topology;
using RabbitMQ.Client;
using TraceSourceLogger;
using TradeHub.Common.Core.ValueObjects;
using TradeHub.TradeManager.Client.Constants;
using TradeHub.TradeManager.Client.Utility;
using ExchangeType = RabbitMQ.Client.ExchangeType;

namespace TradeHub.TradeManager.Client.Service
{
    /// <summary>
    /// Contains MQ infrastructure for managing communications with the Trade Manager Client and Server
    /// </summary>
    public class TradeManagerClientMqServer : ICommunicator, IEventHandler<RabbitMqRequestMessage>
    {
        private Type _type = typeof (TradeManagerClientMqServer);
        
        #region Rabbit MQ Fields

        // Holds reference for the Advance Bus
        private IAdvancedBus _advancedBus;

        // Native Rabbit MQ Fields
        private ConnectionFactory _rabbitMqConnectionFactory;
        private IConnection _rabbitMqConnection;
        private IModel _rabbitMqChannel;

        // Exchange containing Queues
        private IExchange _exchange;

        #endregion

        #region Disruptor Fields

        /// <summary>
        /// Ring buffer size
        /// </summary>
        private const int DisruptorRingSize = 65536; // Must be multiple of 2

        /// <summary>
        /// Disruptor Main object
        /// </summary>
        private Disruptor<RabbitMqRequestMessage> _disruptor;

        /// <summary>
        /// Ring buffer used inside Disruptor Main object
        /// </summary>
        private RingBuffer<RabbitMqRequestMessage> _ringBuffer;

        #endregion

        /// <summary>
        /// Contains MQ parameter values for Trade Manager - Server
        /// </summary>
        private IReadOnlyDictionary<string, string> _serverMqParameters;

        /// <summary>
        /// Contains MQ parameter values for Trade Manager - Client
        /// </summary>
        private IReadOnlyDictionary<string, string> _clientMqParameters;

        /// <summary>
        /// Indicates if the communication medium is open
        /// </summary>
        public bool IsConnected()
        {
            if (_advancedBus != null)
            {
                return _advancedBus.IsConnected;
            }
            return false;
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="serverMqParameters">Contains MQ parameter values for Trade Manager - Server</param>
        /// <param name="clientMqParameters">Contains MQ parameter values for Trade Manager - Client</param>
        public TradeManagerClientMqServer(IReadOnlyDictionary<string, string> serverMqParameters, IReadOnlyDictionary<string, string> clientMqParameters)
        {
            // Save parameter values
            _serverMqParameters = serverMqParameters;
            _clientMqParameters = clientMqParameters;

            // Start Disruptor and relevant objects
            InitializeDisruptor();
        }

        /// <summary>
        /// Initializes Disruptor and relevant objects
        /// </summary>
        private void InitializeDisruptor()
        {
            // Initialize Disruptor
            _disruptor = new Disruptor<RabbitMqRequestMessage>(() => new RabbitMqRequestMessage(), DisruptorRingSize, TaskScheduler.Default);

            // Add Disruptor Consumer
            _disruptor.HandleEventsWith(this);

            // Start Disruptor
            _ringBuffer = _disruptor.Start();
        }

        #region Start

        /// <summary>
        /// Connects the Rabbit MQ session
        /// </summary>
        public void Connect()
        {
            // Initialize MQ Server for communication
            InitializeMqServer();
        }

        /// <summary>
        /// Initializes MQ Server related parameters
        /// </summary>
        private void InitializeMqServer()
        {
            try
            {
                // Create Rabbit MQ Hutch 
                string connectionString = _serverMqParameters["ConnectionString"];
                if (!string.IsNullOrEmpty(connectionString))
                {
                    // Initialize EasyNetQ Rabbit Hutch 
                    InitializeRabbitHutch(connectionString);

                    // Initialize Native RabbitMQ Parameters
                    InitializeNativeRabbitMq(connectionString);

                    // Get Exchange Name from Config File
                    string exchangeName = _serverMqParameters["Exchange"];

                    if (!string.IsNullOrEmpty(exchangeName))
                    {
                        // Use the Exchange Name to Initialize Rabbit Exchange
                        InitializeExchange(exchangeName);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "InitializeMqServer");
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
                _advancedBus = _advancedBus ?? RabbitHutch.CreateBus("host=" +connectionString).Advanced;

                _advancedBus.Connected += OnBusConnected;
                _advancedBus.Connected += OnBusDisconnected;
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

                // Create Native Rabbit MQ Connection to Send Request
                _rabbitMqConnection = _rabbitMqConnectionFactory.CreateConnection();

                // Open Native Rabbbit MQ Channel to Send Request
                _rabbitMqChannel = _rabbitMqConnection.CreateModel();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "InitializeNativeRabbitBus");
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
                Logger.Error(exception, _type.FullName, "InitializeExchange");
            }
        }

        #endregion

        #region Shutdown

        /// <summary>
        /// Closes communication channels
        /// </summary>
        public void Disconnect()
        {
            try
            {
                // Dispose Rabbit Bus
                if (_advancedBus != null)
                {
                    _advancedBus.Dispose();
                    _rabbitMqChannel.Close();
                    _rabbitMqConnection.Close();

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

        #endregion

        /// <summary>
        /// Sends Executions to MQ Exchange on the depending routing key
        /// </summary>
        /// <param name="messageQueueObject">Contains execution stream to be published</param>
        public void SendExecution(RabbitMqRequestMessage messageQueueObject)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Order Request recieved for publishing", _type.FullName, "SendExecutions");
                }

                string routingKey;
                if (_serverMqParameters.TryGetValue(MqParameters.TradeManagerServer.ExecutionMessageRoutingKey, out routingKey))
                {
                    //Send Message for publishing
                    PublishExecutions(messageQueueObject.Message, routingKey);
                }
                else
                {
                    Logger.Info("Execution not sent for publishing as routing key is unavailable.", _type.FullName,
                                "SendExecutions");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendExecutions");
            }
        }

        /// <summary>
        /// Publishes Order Requests message to MQ Exchange using Native Rabbit MQ API
        /// </summary>
        /// <param name="messageBytes">byte message to be published</param>
        /// <param name="routingKey">queue routing on which to publish on</param>
        private void PublishExecutions(byte[] messageBytes, string routingKey)
        {
            try
            {
                // Get next sequence number
                long sequenceNo = _ringBuffer.Next();

                // Get object from ring buffer
                RabbitMqRequestMessage entry = _ringBuffer[sequenceNo];

                // Update object values
                entry.RequestTo = routingKey;
                entry.Message = messageBytes;

                // Publish sequence number for which the object is updated
                _ringBuffer.Publish(sequenceNo);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishExecutions");
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
        /// Called when a publisher has committed an event to the <see cref="T:Disruptor.RingBuffer`1"/>
        /// </summary>
        /// <param name="data">Data committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="sequence">Sequence number committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="endOfBatch">flag to indicate if this is the last event in a batch from the <see cref="T:Disruptor.RingBuffer`1"/></param>
        public void OnNext(RabbitMqRequestMessage data, long sequence, bool endOfBatch)
        {
            string corrId = Guid.NewGuid().ToString();
            IBasicProperties replyProps = _rabbitMqChannel.CreateBasicProperties();
            replyProps.CorrelationId = corrId;

            // Publish Order Reqeusts to MQ Exchange 
            _rabbitMqChannel.BasicPublish(_exchange.Name, data.RequestTo, replyProps, data.Message);

            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Order request published", _type.FullName, "OnNext");
            }
        }
    }
}
