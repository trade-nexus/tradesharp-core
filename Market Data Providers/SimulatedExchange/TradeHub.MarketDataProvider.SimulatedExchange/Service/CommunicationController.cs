using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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
using TradeHub.Common.Core.ValueObjects;
using TradeHub.Common.Core.ValueObjects.MarketData;
using ExchangeType = EasyNetQ.Topology.ExchangeType;

namespace TradeHub.MarketDataProvider.SimulatedExchange.Service
{
    public class CommunicationController : IEventHandler<RabbitMqRequestMessage>
    {
        private Type _type = typeof(CommunicationController);

        private string _adminQueueRoutingKey = "simulated.admin.routingkey";
        private string _marketAdminResponseQueueRoutingKey = "simulated.marketadminresponse.routingkey";

        private string _tickRequestQueueRoutingKey = "simulated.tickrequest.routingkey";
        private string _tickDataQueueRoutingKey = "simulated.tickdata.routingkey";

        private string _barRequestQueueRoutingKey = "simulated.barrequest.routingkey";
        private string _barDataQueueRoutingKey = "simulated.bardata.routingkey";

        private string _historicRequestQueueRoutingKey = "simulated.historicrequest.routingkey";
        private string _historicDataQueueRoutingKey = "simulated.historicdata.routingkey";

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
        private IQueue _adminResponseMessageQueue;

        // Queue will contain Tick Data messages
        private IQueue _tickDataQueue;

        // Queue will contain Bar Data messages
        private IQueue _barDataQueue;
        
        // Queue will contain Historic Data messages
        private IQueue _historicQueue;

        private readonly int _ringSize = 65536;  // Must be multiple of 2

        private Disruptor<RabbitMqRequestMessage> _tickDisruptor;
        private RingBuffer<RabbitMqRequestMessage> _tickRingBuffer;
        
        private Task _dataConsumerTask;
        private Task _barTask;
        private QueueingBasicConsumer _tickconsumer;
        private QueueingBasicConsumer _barConsumer;
        private CancellationTokenSource _tokenSource;

        public event Action MarketDataLoginRequest;
        public event Action MarketDataLogoutRequest;
        public event Action<Tick> TickData;
        public event Action<Bar> BarData;
        public event Action<HistoricBarData> HistoricData;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CommunicationController()
        {
            // Initialize Disruptor
            _tickDisruptor = new Disruptor.Dsl.Disruptor<RabbitMqRequestMessage>(() => new RabbitMqRequestMessage(), _ringSize, TaskScheduler.Default);

            // Add Consumer
            _tickDisruptor.HandleEventsWith(this);

            // Start Disruptor
            _tickRingBuffer = _tickDisruptor.Start();

            //Connect();
        }

        /// <summary>
        /// Starts MQ Server
        /// </summary>
        public void Connect()
        {
            // Initializes MQ resources
            InitializeMqServices();
            InitializeRabbitMqService();
            DeclareRabbitMqQueues();

            // Subscribe Admin Message Queue
            SubscribeAdminMessageQueues();

            // Subscribe Market Data Request Message Queues
            SubscribeMarketDataMessageQueues();

            // Create Tick Consumer
            _tickconsumer = new QueueingBasicConsumer(_rabbitMqTickChannel);
            _rabbitMqTickChannel.BasicConsume("simulated.tickdata.queue", true, _tickconsumer);

            // Create Bar Consumer
            //_barConsumer = new QueueingBasicConsumer(_rabbitMqBarChannel);
            //_rabbitMqBarChannel.BasicConsume("simulated.bardata.queue", false, _barConsumer);

            //_tickTask = Task.Factory.StartNew(ConsumeTickDataQueue);
            //_barTask = Task.Factory.StartNew(ConsumeBarDataQueue);
            _tokenSource = new CancellationTokenSource();
            //_dataConsumerTask = Task.Factory.StartNew(ConsumeMarketDataQueue);
            _dataConsumerTask = Task.Factory.StartNew(ConsumeMarketDataQueue, _tokenSource.Token);
        }

        /// <summary>
        /// Stop MQ Server
        /// </summary>
        public void Disconnect()
        {
            _tickconsumer.OnCancel();
            _tokenSource.Cancel();
            //_dataConsumerTask.Dispose();
            //_barTask.Dispose();

            _easyNetQBus.Dispose();
            _easyNetQBus = null;

            _rabbitMqTickChannel.QueueDelete("simulated.tickdata.queue");

            _rabbitMqTickChannel.Close();
            _rabbitMqTickConnection.Close();
            _rabbitMqTickBus = null;

            //_rabbitMqBarChannel.Close();
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
                //_rabbitMqBarBus = new ConnectionFactory();
                _rabbitMqTickBus.HostName = "localhost";
                //_rabbitMqBarBus.HostName = "localhost";

                // Create Connection
                _rabbitMqTickConnection = _rabbitMqTickBus.CreateConnection();
                //_rabbitMqBarConnection = _rabbitMqBarBus.CreateConnection();
                
                // Open Channel
                _rabbitMqTickChannel = _rabbitMqTickConnection.CreateModel();
                //_rabbitMqBarChannel = _rabbitMqTickConnection.CreateModel();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "InitializeRabbitMqService");
            }
        }

        /// <summary>
        /// Declares Rabbit Mq Queues
        /// </summary>
        public void DeclareRabbitMqQueues()
        {
            try
            {
                //Declare Data Queue
                _rabbitMqTickChannel.QueueDeclare("simulated.tickdata.queue", false, false, true, null);
                _rabbitMqTickChannel.QueueBind("simulated.tickdata.queue", "simulated_exchange", _tickDataQueueRoutingKey);

                //Declare Bar Data Queue
                //_rabbitMqBarChannel.QueueDeclare("simulated.bardata.queue", false, false, true, null);
                //_rabbitMqBarChannel.QueueBind("simulated.bardata.queue", "simulated_exchange", _barDataQueueRoutingKey);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "DeclareRabbitMqQueues");
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
                BindQueue("simulated.marketadmin.queue", _marketAdminResponseQueueRoutingKey, ref _adminResponseMessageQueue, exchange);

                // Bind Historic Request Queue
                BindQueue("simulated.historicdata.queue", _historicDataQueueRoutingKey, ref _historicQueue, exchange);
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
                    Logger.Info("Binding Admin Message Queue: " + _adminResponseMessageQueue.Name, _type.FullName,
                                "SubscribeAdminMessageQueues");
                }

                // Listening to Login Messages
                _easyNetQBus.Consume<string>(
                    _adminResponseMessageQueue, (msg, messageReceivedInfo) =>
                                        Task.Factory.StartNew(
                                            () =>
                                            {
                                                if (msg.Body.Equals("DataLogin"))
                                                {
                                                    MarketDataLoginRequest();
                                                }
                                                else if (msg.Body.Equals("DataLogout"))
                                                {
                                                    MarketDataLogoutRequest();
                                                }
                                            }));

            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeAdminMessageQueues");
            }
        }

        /// <summary>
        /// Binds the Market Data Message Queues
        /// Starts listening to the incoming Market Data messages
        /// </summary>
        public void SubscribeMarketDataMessageQueues()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Binding Market Data Request Message Queues", _type.FullName, "SubscribeMarketDataRequestMessageQueues");
                }

                // Listening to Historic Data Messages
                _easyNetQBus.Consume<HistoricBarData>(
                    _historicQueue, (msg, messageReceivedInfo) =>
                                                 Task.Factory.StartNew(() => HistoricData(msg.Body)));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeMarketDataRequestMessageQueues");
            }
        }

        /// <summary>
        /// Consumes Tick and Bar Data from queue
        /// </summary>
        public void ConsumeMarketDataQueue()
        {
            try
            {
                while (true)
                {
                    BasicDeliverEventArgs ea = (BasicDeliverEventArgs) _tickconsumer.Queue.Dequeue();
                    byte[] body = ea.Body;

                    long sequenceNo = _tickRingBuffer.Next();
                    RabbitMqRequestMessage entry = _tickRingBuffer[sequenceNo];
                    entry.Message = body;

                    _tickRingBuffer.Publish(sequenceNo);
                    //_rabbitMqTickChannel.BasicAck(ea.DeliveryTag, false);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ConsumeMarketDataQueue");
            }
        }

        /// <summary>
        /// Consumes Tick Data from queue
        /// </summary>
        public void ConsumeTickDataQueue()
        {
            try
            {
                while (true)
                {
                    BasicDeliverEventArgs ea = (BasicDeliverEventArgs)_tickconsumer.Queue.Dequeue();

                    byte[] body = ea.Body;
                    string message = Encoding.UTF8.GetString(body);
                    //OnTickDataReceived(message);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ConsumeTickDataQueue");
            }
        }

        /// <summary>
        /// Consumes Bar Data from queue
        /// </summary>
        public void ConsumeBarDataQueue()
        {
            try
            {
                while (true)
                {
                    BasicDeliverEventArgs ea = (BasicDeliverEventArgs)_barConsumer.Queue.Dequeue();

                    byte[] body = ea.Body;
                    string message = Encoding.UTF8.GetString(body);
                    //OnBarDataReceived(message);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ConsumeBarDataQueue");
            }
        }

        /// <summary>
        /// Publishes Admin Level request for Market Data
        /// </summary>
        public void PublishMarketAdminMessage(string response)
        {
            try
            {
                // Message to be published
                IMessage<string> responseMessage = new Message<string>(response);

                _easyNetQBus.Publish(_easyNetQExchange, _adminQueueRoutingKey, true, false, responseMessage);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishMarketAdminMessageResponse");
            }
        }

        /// <summary>
        /// Publishes Tick Data Request
        /// </summary>
        public void PublishTickRequest(Subscribe subscribe)
        {
            try
            {
                // Message to be published
                IMessage<Subscribe> message = new Message<Subscribe>(subscribe);
                // Publish message
                _easyNetQBus.Publish(_easyNetQExchange, _tickRequestQueueRoutingKey, true, false, message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishTickRequest");
            }
        }

        /// <summary>
        /// Publishes Bar Data Request
        /// </summary>
        public void PublishBarDataRequest(BarDataRequest barDataRequest)
        {
            try
            {
                // Message to be published
                IMessage<BarDataRequest> message = new Message<BarDataRequest>(barDataRequest);
                // Publish message
                _easyNetQBus.Publish(_easyNetQExchange, _barRequestQueueRoutingKey, true, false, message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishBarDataRequest");
            }
        }

        /// <summary>
        /// Publishes Bar Data
        /// </summary>
        public void PublishHistoricDataRequest(HistoricDataRequest historicDataRequest)
        {
            try
            {
                // Message to be published
                IMessage<HistoricDataRequest> message = new Message<HistoricDataRequest>(historicDataRequest);
                // Publish message
                _easyNetQBus.Publish(_easyNetQExchange, _historicRequestQueueRoutingKey, true, false, message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "PublishHistoricData");
            }
        }

        /// <summary>
        /// Called when new Tick message is received and processed by Disruptor
        /// </summary>
        /// <param name="message"></param>
        public void OnTickDataReceived(string[] message)
        {
            try
            {
                Tick entry = new Tick();

                if(ParseToTick(entry, message))
                {
                    TickData.Invoke(entry);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnTickDataReceived_Disruptor");
            }
        }

        /// <summary>
        /// Called when new Bar message is received and processed by Disruptor
        /// </summary>
        /// <param name="message"></param>
        private void OnBarDataReceived(string[] message)
        {
            try
            {
                Bar entry = new Bar("");

                if (ParseToBar(entry, message))
                {
                    BarData.Invoke(entry);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnBarDataReceived");
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
                Logger.Error(exception, _type.FullName, "ParseToTick");
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
                Logger.Error(exception, _type.FullName, "ParseToBar");
                return false;
            }
        }

        #region Implementation of IEventHandler<in RabbitMqMessage>

        /// <summary>
        /// Called when a publisher has committed an event to the <see cref="T:Disruptor.RingBuffer`1"/>
        /// </summary>
        /// <param name="data">Data committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="sequence">Sequence number committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="endOfBatch">flag to indicate if this is the last event in a batch from the <see cref="T:Disruptor.RingBuffer`1"/></param>
        public void OnNext(RabbitMqRequestMessage data, long sequence, bool endOfBatch)
        {
            string message = Encoding.UTF8.GetString(data.Message);

            var messageArray = message.Split(',');

            if (messageArray[0].Equals("TICK"))
                OnTickDataReceived(messageArray);
            else
                OnBarDataReceived(messageArray);
        }

        #endregion
    }
}
