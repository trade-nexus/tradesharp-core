using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using RabbitMQ.Client;
using TraceSourceLogger;
using TradeHub.NotificationEngine.Client.Constants;
using TradeHub.NotificationEngine.Common.Utility;
using TradeHub.NotificationEngine.Common.ValueObject;
using ExchangeType = RabbitMQ.Client.ExchangeType;

namespace TradeHub.NotificationEngine.Client.Service
{
    public class ClientMqServer : ICommunicator
    {
        private Type _type = typeof (ClientMqServer);
        
        #region MQ Fields

        // Holds reference for the Advance Bus
        private IAdvancedBus _advancedBus;

        // Exchange containing Queues
        private IExchange _exchange;

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
        /// <param name="serverMqConfigFile">File from which to read MQ Server parameters</param>
        /// <param name="clientMqConfigFile">File from which to read MQ Client parameters</param>
        public ClientMqServer(string serverMqConfigFile, string clientMqConfigFile)
        {
            // Save parameter values
            _serverMqParameters = MqConfigurationReader.ReadServerMqProperties(serverMqConfigFile);
            _clientMqParameters = MqConfigurationReader.ReadServerMqProperties(clientMqConfigFile);
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
                _advancedBus = _advancedBus ?? RabbitHutch.CreateBus("host=" + connectionString).Advanced;

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
        /// Sends Executions to MQ Exchange on the depending routing key
        /// </summary>
        /// <param name="notification">Contains execution stream to be published</param>
        public void SendNotification(OrderNotification notification)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Order notification recieved for publishing", _type.FullName, "SendNotification");
                }

                string routingKey;
                if (_serverMqParameters.TryGetValue(MqParameters.NotificationEngineServer.OrderMessageRoutingKey, out routingKey))
                {
                    //Send Message for publishing
                    PublishNotifications(notification, routingKey);
                }
                else
                {
                    Logger.Info("Notification not sent for publishing as routing key is unavailable.", _type.FullName, "SendNotification");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendNotification");
            }
        }

        /// <summary>
        /// Publishes received message to respective queue
        /// </summary>
        /// <param name="notification">notification to be sent to server</param>
        /// <param name="routingKey">routing key to send message to respective queue</param>
        private void PublishNotifications(OrderNotification notification, string routingKey)
        {
            // Wrap notification in EasyNetQ message interface 
            IMessage<OrderNotification> message = new Message<OrderNotification>(notification);

            // Send message to queue
            _advancedBus.Publish(_exchange, routingKey, true, false, message);
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
    }
}
