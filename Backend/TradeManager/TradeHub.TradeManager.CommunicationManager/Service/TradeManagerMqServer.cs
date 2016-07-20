using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using EasyNetQ;
using EasyNetQ.Topology;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TraceSourceLogger;
using Disruptor;
using Disruptor.Dsl;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.ValueObjects;

namespace TradeHub.TradeManager.CommunicationManager.Service
{
    /// <summary>
    /// Contains MQ infrastructure for managing communications for the Trade Manager Server
    /// </summary>
    public class TradeManagerMqServer : ICommunicator, IEventHandler<MessageQueueObject>
    {
        private Type _type = typeof (TradeManagerMqServer);

        /// <summary>
        ///Name of the Configuration File
        /// </summary>
        private readonly string _configFile;

        // Holds reference for the Advance Bus
        private IAdvancedBus _rabbitBus;

        // Exchange containing Queues
        private IExchange _exchange;

        // Native Rabbit MQ Fields
        private ConnectionFactory _rabbitMqConnectionFactory;
        private IConnection _rabbitMqConnection;
        private IModel _rabbitMqChannel;
        private QueueingBasicConsumer _executionMessageConsumer;

        /// <summary>
        /// Ring size for Disruptor
        /// </summary>
        private readonly int _ringSize = 65536;  // Must be multiple of 2

        // Handles Order Request Messages to be processed
        private Disruptor<MessageQueueObject> _executionMessageDisruptor;
        private RingBuffer<MessageQueueObject> _executionMessageRingBuffer;
        private EventPublisher<MessageQueueObject> _executionMessagePublisher;

        /// <summary>
        /// Dedicated task to consume execution messages
        /// </summary>
        private Task _executionMessageConsumerTask;

        /// <summary>
        /// Token source used with Execution Message Consumer Task
        /// </summary>
        private CancellationTokenSource _executionMessageConsumerCancellationToken;

        /// <summary>
        /// Raised when new Execution Message is received
        /// </summary>
        public event Action<Execution> NewExecutionReceivedEvent; 

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="configFile">Name of the configuration file to read</param>
        public TradeManagerMqServer(string configFile)
        {
            // Save Configuration file info
            _configFile = configFile;

            // Setup Disruptor
            InitializeDisruptor();
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
        /// Initializes Disruptor
        /// </summary>
        private void InitializeDisruptor()
        {
            // Initialize Disruptor
            _executionMessageDisruptor = new Disruptor<MessageQueueObject>(() => new MessageQueueObject(), _ringSize, TaskScheduler.Default);
            
            // Set Event Handler
            _executionMessageDisruptor.HandleEventsWith(this);
            
            // Start Ring Buffer
            _executionMessageRingBuffer = _executionMessageDisruptor.Start();

            // Start Event Publisher
            _executionMessagePublisher = new EventPublisher<MessageQueueObject>(_executionMessageRingBuffer);
        }

        #region StartUp

        /// <summary>
        /// Starts MQ Server
        /// </summary>
        public void Connect()
        {
            // Initializes MQ resources
            IntitializeMqServices();

            // Initialize Consumer Token
            _executionMessageConsumerCancellationToken = new CancellationTokenSource();

            // Start Consuming Execution Message Queue
            _executionMessageConsumerTask = Task.Factory.StartNew(ConsumeExecutionMessages,
                _executionMessageConsumerCancellationToken.Token);
        }

        /// <summary>
        /// Initializes the required parameters and fields for the Rabbit MQ service
        /// </summary>
        private void IntitializeMqServices()
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
        /// Initializes EasyNetQ's Advance Rabbit Hutch
        /// </summary>
        private void InitializeRabbitHutch(string connectionString)
        {
            try
            {
                // Create a new Rabbit Bus Instance
                _rabbitBus = _rabbitBus ?? RabbitHutch.CreateBus("host=" + connectionString).Advanced;

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
        private void InitializeNativeRabbitMq(string connectionString)
        {
            try
            {
                // Create Native Rabbit MQ Bus
                _rabbitMqConnectionFactory = new ConnectionFactory { HostName = connectionString };

                // Create Native Rabbit MQ Connection to Receive Order Request
                _rabbitMqConnection = _rabbitMqConnectionFactory.CreateConnection();

                // Open Native Rabbbit MQ Channel to Receive Order Request
                _rabbitMqChannel = _rabbitMqConnection.CreateModel();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "InitializeNativeRabbitMq");
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
                return _rabbitBus.ExchangeDeclare(exchangeName, EasyNetQ.Topology.ExchangeType.Direct, false, true, false);
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
                // Bind Queue to Receive Executions
                DeclareRabbitMqQueue("ExecutionMessageQueue", "ExecutionMessageRoutingKey", exchange.Name);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "RegisterQueues");
            }
        }

        /// <summary>
        /// Declares Rabbit Mq Queue
        /// </summary>
        private void DeclareRabbitMqQueue(string queueHeader, string routingKeyHeader, string exchange)
        {
            try
            {
                // Get Queue Name from Config File
                string queueName = ReadConfigSettings(queueHeader);

                // Get Routing Key from Config File
                string routingKey = ReadConfigSettings(routingKeyHeader);

                if (!string.IsNullOrEmpty(queueName) && !string.IsNullOrEmpty(routingKey))
                {
                    //Declare Order Request Queue
                    _rabbitMqChannel.QueueDeclare(queueName, false, false, true, null);
                    _rabbitMqChannel.QueueBind(queueName, exchange, routingKey);

                    // Create Order Request Consumer
                    _executionMessageConsumer = new QueueingBasicConsumer(_rabbitMqChannel);
                    _rabbitMqChannel.BasicConsume(queueName, true, _executionMessageConsumer);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "DeclareRabbitMqQueue");
            }
        }

        #endregion

        #region Shut Down

        /// <summary>
        /// Disconnects MQ Server and terminate necessary resources
        /// </summary>
        public void Disconnect()
        {
            try
            {
                // Dispose Rabbit Bus
                if (_rabbitBus != null)
                {
                    _rabbitBus.Dispose();
                    _rabbitMqChannel.Close();
                    _rabbitMqConnection.Close();

                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Advanced Bus disposed off.", _type.FullName, "Disconnect");
                    }
                }

                // Stop Consumer
                if (_executionMessageConsumerCancellationToken != null)
                {
                    _executionMessageConsumerCancellationToken.Cancel();
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "Disconnect");
            }
        }

        #endregion

        #region Consumers

        /// <summary>
        /// Consumes Execution messages from queue
        /// </summary>
        private void ConsumeExecutionMessages()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Starting execution message consumer", _type.FullName, "ConsumeExecutionMessages");
                }

                while (true)
                {
                    BasicDeliverEventArgs incomingMessage = (BasicDeliverEventArgs)_executionMessageConsumer.Queue.Dequeue();

                    // Add incoming message to Disruptor
                    _executionMessagePublisher.PublishEvent((messageQueueObject, sequenceNo) =>
                    {
                        // Initialize Parameter
                        messageQueueObject.Message = new byte[incomingMessage.Body.Length];

                        // Copy information
                        incomingMessage.Body.CopyTo(messageQueueObject.Message, 0);

                        // Return updated object
                        return messageQueueObject;
                    });
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ConsumeExecutionMessages");
            }
        }

        #endregion

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
                Fill fill = new Fill(new Security() { Symbol = messageArray[3] }, messageArray[9], messageArray[0])
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
                Logger.Error(exception, _type.FullName, "ParseExecution");
                return null;
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
        /// Called when a publisher has committed an event to the <see cref="T:Disruptor.RingBuffer`1"/>
        /// </summary>
        /// <param name="data">Data committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="sequence">Sequence number committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="endOfBatch">flag to indicate if this is the last event in a batch from the <see cref="T:Disruptor.RingBuffer`1"/></param>
        public void OnNext(MessageQueueObject data, long sequence, bool endOfBatch)
        {
            // Convert Byte Stream to Byte Array
            string message = Encoding.UTF8.GetString(data.Message);
            var messageArray = message.Split(',');

            // Parse String to create Execution Object
            var execution = ParseExecution(messageArray);

            // Check for valid object creation
            if (execution != null)
            {
                // Raise Execution Event
                if (NewExecutionReceivedEvent != null)
                {
                    NewExecutionReceivedEvent(execution);
                }
            }
        }
    }
}
