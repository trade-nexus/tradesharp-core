using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Spring.Context.Support;
using TraceSourceLogger;
using Disruptor;
using Disruptor.Dsl;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.FactoryMethods;
using TradeHub.Common.Core.Utility;
using TradeHub.Common.Core.ValueObjects;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.Inquiry;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataEngine.Client.Service;

namespace TradeHub.StrategyEngine.MarketData
{
    /// <summary>
    /// Responsible for handling all market data related requests
    /// </summary>
    public class MarketDataService : IEventHandler<RabbitMqRequestMessage>, IDisposable
    {
        private Type _type = typeof (MarketDataService);
        private AsyncClassLogger _asyncClassLogger;

        #region Events

        // ReSharper disable InconsistentNaming
        private event Action _connected;
        private event Action _disconnected;
        private event Action<string> _logonArrived;
        private event Action<string> _logoutArrived;
        private event Action<Tick> _tickArrived;
        private event Action<Bar> _barArrived;
        private event Action<MarketDataProviderInfo> _inquiryResponseArrived; 

        // Local Events
        private event Action<Subscribe> _tickSubscribeRequest;
        private event Action<BarDataRequest> _barSubscribeRequest;
        private event Action<BarDataRequest[]> _multiBarSubscribeRequest;
        private event Action<Unsubscribe> _tickUnsubscribeRequest;
        private event Action<BarDataRequest> _barUnsubscribeRequest;

        // ReSharper restore InconsistentNaming

        public event Action Connected
        {
            add
            {
                if (_connected==null)
                {
                    _connected += value;
                }
            }
            remove { _connected -= value; }
        }

        public event Action Disconnected
        {
            add
            {
                if (_disconnected == null)
                {
                    _disconnected += value;
                }
            }
            remove { _disconnected -= value; }
        }

        public event Action<string> LogonArrived
        {
            add
            {
                if (_logonArrived == null)
                {
                    _logonArrived += value;
                }
            }
            remove { _logonArrived -= value; }
        }

        public event Action<string> LogoutArrived
        {
            add
            {
                if (_logoutArrived == null)
                {
                    _logoutArrived += value;
                }
            }
            remove { _logoutArrived -= value; }
        }

        public event Action<Tick> TickArrived
        {
            add
            {
                if (_tickArrived == null)
                {
                    _tickArrived += value;
                }
            }
            remove { _tickArrived -= value; }
        }

        public event Action<Bar> BarArrived
        {
            add
            {
                if (_barArrived == null)
                {
                    _barArrived += value;
                }
            }
            remove { _barArrived -= value; }
        }

        public event Action<MarketDataProviderInfo> InquiryResponseArrived
        {
            add
            {
                if (_inquiryResponseArrived == null)
                {
                    _inquiryResponseArrived += value;
                }
            }
            remove { _inquiryResponseArrived -= value; }
        }

        #endregion

        // Disruptor Ring Buffer Size 
        private readonly int _ringSize = 65536;  // Must be multiple of 2

        // Handles order request message
        private Disruptor<RabbitMqRequestMessage> _dataDisruptor;
        // Ring buffer to be used with disruptor
        private RingBuffer<RabbitMqRequestMessage> _dataRingBuffer;

        /// <summary>
        /// Provides connection with MDE
        /// </summary>
        private MarketDataEngineClient _dataEngineClient;

        /// <summary>
        /// Keeps tracks of all the subcribed securities
        /// Key = Market data provider name
        /// Value = { KEY = Security | Value = ID }
        /// </summary>
        private ConcurrentDictionary<string, Dictionary<Security, string>> _tickSubscriptions;

        /// <summary>
        /// Keeps track of all the subscribed bars
        /// Key = Market data provider name
        /// Value = Bar data requests
        /// </summary>
        private ConcurrentDictionary<string, List<BarDataRequest>> _barSubscriptions; 

        private bool _isConnected = false;
        private bool _disposed = false;

        /// <summary>
        /// Indicates whether the market data service is connected to MDE-Server or not
        /// </summary>
        public bool IsConnected
        {
            get { return _isConnected; }
            set { _isConnected = value; }
        }

        /// <summary>
        /// Keeps tracks of all the subcribed securities
        /// Key = Market data provider name
        /// Value = Securities
        /// </summary>
        public ReadOnlyConcurrentDictionary<string, Dictionary<Security, string>> TickSubscriptions
        {
            get { return new ReadOnlyConcurrentDictionary<string, Dictionary<Security, string>>(_tickSubscriptions); }
        }

        /// <summary>
        /// Keeps track of all the subscribed bars
        /// Key = Market data provider name
        /// Value = Bar data requests
        /// </summary>
        public ReadOnlyConcurrentDictionary<string, List<BarDataRequest>> BarSubscriptions
        {
            get { return new ReadOnlyConcurrentDictionary<string, List<BarDataRequest>>(_barSubscriptions); }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public MarketDataService()
        {
            _asyncClassLogger = new AsyncClassLogger("MarketDataService");
            // Set logging level
            _asyncClassLogger.SetLoggingLevel();
            //set logging path
            _asyncClassLogger.LogDirectory(DirectoryStructure.CLIENT_LOGS_LOCATION);

            // Initialize Distruptor
            InitializeDistuptor(new IEventHandler<RabbitMqRequestMessage>[] { this });

            // Initialize local maps
            _tickSubscriptions = new ConcurrentDictionary<string, Dictionary<Security, string>>();
            _barSubscriptions = new ConcurrentDictionary<string, List<BarDataRequest>>();

            // Register local data request events
            RegisterLocalEvents();
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="marketDataEngineClient">MDE-Client for communication with the MDE-Server</param>
        public MarketDataService(MarketDataEngineClient marketDataEngineClient)
        {
            _asyncClassLogger = new AsyncClassLogger("MarketDataService");
            // Set logging level
            _asyncClassLogger.SetLoggingLevel();
            //set logging path
            _asyncClassLogger.LogDirectory(DirectoryStructure.CLIENT_LOGS_LOCATION);

            // Save Market Data Engine Client Instance
            _dataEngineClient = marketDataEngineClient;

            // Initialize Distruptor
            InitializeDistuptor(new IEventHandler<RabbitMqRequestMessage>[] { this });

            // Initialize local maps
            _tickSubscriptions = new ConcurrentDictionary<string, Dictionary<Security, string>>();
            _barSubscriptions= new ConcurrentDictionary<string, List<BarDataRequest>>();

            // Register local data request events
            RegisterLocalEvents();
            // Register required MDE-Client Events
            RegisterDataEngineClientEvents();
        }

        /// <summary>
        /// Initialize Disruptor and adds required Handler
        /// </summary>
        public void InitializeDistuptor(IEventHandler<RabbitMqRequestMessage>[] handler)
        {
            if (_dataDisruptor != null)
                _dataDisruptor.Shutdown();

            // Initialize Disruptor
            _dataDisruptor = new Disruptor<RabbitMqRequestMessage>(() => new RabbitMqRequestMessage(), _ringSize, TaskScheduler.Default);

            // Add Disruptor Consumer
            _dataDisruptor.HandleEventsWith(handler);

            // Start Disruptor
            _dataRingBuffer = _dataDisruptor.Start();
        }

        /// <summary>
        /// Initializes necessary components for communication - Needs to be called if the Service was stopped
        /// </summary>
        public void InitializeService()
        {
            // Initialize Distruptor
            InitializeDistuptor(new IEventHandler<RabbitMqRequestMessage>[] { this });

            // Initialize Client
            _dataEngineClient.Initialize();
        }

        /// <summary>
        /// Start Market Data Service
        /// </summary>
        public bool StartService()
        {
            try
            {
                if (_isConnected)
                {
                    if (_asyncClassLogger.IsInfoEnabled)
                    {
                        _asyncClassLogger.Info("Market data service already running.", _type.FullName, "StartService");
                    }

                    return true;
                }

                // Start MDE-Client
                _dataEngineClient.Start();

                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Market data service started.", _type.FullName, "StartService");
                }

                return true;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "StartService");
                return false;
            }
        }

        /// <summary>
        /// Starts Market Data Service
        /// </summary>
        public bool StopService()
        {
            try
            {
                if (_dataEngineClient != null)
                {
                    // Stop MDE-Client
                    _dataEngineClient.Shutdown();
                }

                // Shutdown Disruptor
                _dataDisruptor.Shutdown();

                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Market data service stopped.", _type.FullName, "StopService");
                }
                return true;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "StopService");
                return false;
            }
        }

        /// <summary>
        /// Registers local events for market data requests
        /// </summary>
        private void RegisterLocalEvents()
        {
            UnregisterLocalEvents();

            // Data requests subscription events
            _tickSubscribeRequest += SendTickSubscriptionRequest;
            _barSubscribeRequest += SendBarSubscriptionRequest;

            // Data requests un-subscription events
            _tickUnsubscribeRequest += SendTickUnsubscriptionRequest;
            _barUnsubscribeRequest += SendBarUnsubscriptionRequest;
        }

        /// <summary>
        /// Registers required MDE-Client events
        /// </summary>
        private void RegisterDataEngineClientEvents()
        {
            _dataEngineClient.ServerConnected += OnServerConnected;
            _dataEngineClient.ServerDisconnected += OnServerDisconnected;
            _dataEngineClient.LogonArrived += OnLogonArrived;
            _dataEngineClient.LogoutArrived += OnLogoutArrived;
            _dataEngineClient.TickArrived += OnTickArrived;
            _dataEngineClient.LiveBarArrived += OnLiveBarArrived;
            _dataEngineClient.MarketDataArrived += OnDataArrived;
            _dataEngineClient.InquiryResponseArrived += OnInquiryResponseArrived;
        }

        /// <summary>
        /// Unregisters local events for market data requests
        /// </summary>
        private void UnregisterLocalEvents()
        {
            // Data requests subscription events
            _tickSubscribeRequest -= SendTickSubscriptionRequest;
            _barSubscribeRequest -= SendBarSubscriptionRequest;

            // Data requests un-subscription events
            _tickUnsubscribeRequest -= SendTickUnsubscriptionRequest;
            _barUnsubscribeRequest -= SendBarUnsubscriptionRequest;
        }

        /// <summary>
        /// Unregisters required MDE-Client events
        /// </summary>
        private void UnregisterDataEngineClientEvents()
        {
            _dataEngineClient.ServerConnected -= OnServerConnected;
            _dataEngineClient.ServerDisconnected -= OnServerDisconnected;
            _dataEngineClient.LogonArrived -= OnLogonArrived;
            _dataEngineClient.LogoutArrived -= OnLogoutArrived;
            _dataEngineClient.TickArrived -= OnTickArrived;
            _dataEngineClient.LiveBarArrived -= OnLiveBarArrived;
            _dataEngineClient.MarketDataArrived-= OnDataArrived;
            _dataEngineClient.InquiryResponseArrived -= OnInquiryResponseArrived;
        }

        /// <summary>
        /// Overrides the Tick subscription request calls
        /// </summary>
        public void OverrideTickSubscriptionRequest(Action<Subscribe> tickSubscribeAction)
        {
            // Unregister local call
            _tickSubscribeRequest -= SendTickSubscriptionRequest;

            // Register incoming action
            _tickSubscribeRequest += tickSubscribeAction;
        }

        /// <summary>
        /// Overrides the Live Bar subscription request calls
        /// </summary>
        public void OverrideBarSubscriptionRequest(Action<BarDataRequest> barSubscribeAction)
        {
            // Unregister local call
            _barSubscribeRequest -= SendBarSubscriptionRequest;

            // Register incoming action
            _barSubscribeRequest += barSubscribeAction;
        }

        /// <summary>
        /// Overrides the Live Bar subscription request calls
        /// </summary>
        public void OverrideBarSubscriptionRequest(Action<BarDataRequest[]> barSubscribeAction)
        {
            // Register incoming action after unregistering previous
            _multiBarSubscribeRequest -= barSubscribeAction;
            _multiBarSubscribeRequest += barSubscribeAction;
        }

        /// <summary>
        /// Overrides the Tick un-subscription request calls
        /// </summary>
        public void OverrideTickUnsubscriptionRequest(Action<Unsubscribe> tickUnsubscribeAction)
        {
            // Unregister local call
            _tickUnsubscribeRequest -= SendTickUnsubscriptionRequest;

            // Register incoming action
            _tickUnsubscribeRequest += tickUnsubscribeAction;
        }

        /// <summary>
        /// Overrides the Live Bar un-subscription request
        /// </summary>
        public void OverriderBarUnsubscriptionRequest(Action<BarDataRequest> barUnsubscribeAction)
        {
            // Unregister local call
            _barUnsubscribeRequest -= SendBarUnsubscriptionRequest;

            // Register incoming action
            _barUnsubscribeRequest += barUnsubscribeAction;
        }

        #region MDE-Client Events

        /// <summary>
        /// Called when MDE-Client successfully connects with MDE-Server
        /// </summary>
        private void OnServerConnected()
        {
            try
            {
                _isConnected = true;
             
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Successfully connected with MDE-Server.", _type.FullName, "OnServerConnected");
                }

                if (_connected != null)
                {
                    _connected();
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnServerConnected");
            }
        }

        /// <summary>
        /// Called when MDE-Client disconnects with MDE-Server
        /// </summary>
        private void OnServerDisconnected()
        {
            try
            {
                _isConnected = false;

                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Disconnected with MDE-Server", _type.FullName, "OnServerDisconnected");
                }

                if (_disconnected != null)
                {
                    _disconnected();
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnServerDisconnected");
            }
        }

        /// <summary>
        /// Called when MDE-Client receives logon message from MDE-Server
        /// </summary>
        private void OnLogonArrived(string providerName)
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Logon message arrived from: " + providerName, _type.FullName, "OnLogonArrived");
                }

                if (_logonArrived != null)
                {
                    _logonArrived(providerName);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnLogonArrived");
            }
        }

        /// <summary>
        /// Called when MDE-Client receives logout message from MDE-Server
        /// </summary>
        private void OnLogoutArrived(string providerName)
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Logout message arrived from: " + providerName, _type.FullName, "OnLogoutArrived");
                }

                if (_logoutArrived != null)
                {
                    _logoutArrived(providerName);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnLogoutArrived");
            }
        }

        /// <summary>
        /// Called when MDE-Client receives new tick message from MDE-Server
        /// </summary>
        private void OnTickArrived(Tick tick)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("New tick received from: " + tick.MarketDataProvider, _type.FullName, "OnTickArrived");
                }

                if (_tickArrived != null)
                {
                    _tickArrived(tick);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnTickArrived");
            }
        }

        /// <summary>
        /// Called when MDE-Client receives new live bar from MDE-Server
        /// </summary>
        private void OnLiveBarArrived(Bar bar)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("New bar received from: " + bar.MarketDataProvider, _type.FullName, "OnLiveBarArrived");
                }

                if (_barArrived != null)
                {
                    _barArrived(bar);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnLiveBarArrived");
            }
        }

        /// <summary>
        /// Called when MDE-Client recieves new data message from MDE-Server
        /// </summary>
        /// <param name="bytes">Message consumed from messaging queue</param>
        private void OnDataArrived(byte[] bytes)
        {
            // Get Next Sequence number
            long sequenceNumber = _dataRingBuffer.Next();

            // Get new entry
            var entry = _dataRingBuffer[sequenceNumber];

            // Update values
            entry.Message = bytes;

            // Publish updates
            _dataRingBuffer.Publish(sequenceNumber);
        }

        /// <summary>
        /// Called when MDE-Client receives inquiry response from MDE-Server
        /// </summary>
        private void OnInquiryResponseArrived(MarketDataProviderInfo marketDataProviderInfo)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("New bar received from: " + marketDataProviderInfo.DataProviderName, _type.FullName,
                                 "OnLiveBarArrived");
                }

                if (_inquiryResponseArrived != null)
                {
                    _inquiryResponseArrived(marketDataProviderInfo);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnLiveBarArrived");
            }
        }

        #endregion

        /// <summary>
        /// Sends tick subscription request to MDE-Server using MDE-Client
        /// </summary>
        /// <param name="subscribe">Contains Tick subscription info</param>
        private void SendTickSubscriptionRequest(Subscribe subscribe)
        {
            // Send request to MDE-Client
            _dataEngineClient.SendTickSubscriptionRequest(subscribe);
        }

        /// <summary>
        /// Sends live bar subscription request to MDE-Server using MDE-Client
        /// </summary>
        /// <param name="barDataRequest">Contains Live Bar subscription info</param>
        private void SendBarSubscriptionRequest(BarDataRequest barDataRequest)
        {
            // Send request to MDE-Client
            _dataEngineClient.SendLiveBarSubscriptionRequest(barDataRequest);
        }

        /// <summary>
        /// Sends tick un-subscription request to MDE-Server using MDE-Client
        /// </summary>
        /// <param name="unsubscribe">Contains Tick un-subscription info</param>
        private void SendTickUnsubscriptionRequest(Unsubscribe unsubscribe)
        {
            // Send request to MDE-Client
            _dataEngineClient.SendTickUnsubscriptionRequest(unsubscribe);
        }

        /// <summary>
        /// Sends live bar un-subscription request to MDE-Server using MDE-Client
        /// </summary>
        /// <param name="barDataRequest">Contains Live Bar un-subscription info</param>
        private void SendBarUnsubscriptionRequest(BarDataRequest barDataRequest)
        {
            // Send request to MDE-Client
            _dataEngineClient.SendLiveBarUnsubscriptionRequest(barDataRequest);
        }

        /// <summary>
        /// Called when new Tick message is received and processed by Disruptor
        /// </summary>
        /// <param name="message"></param>
        private void OnTickDataReceived(string[] message)
        {
            try
            {
                Tick entry = new Tick();

                if (ParseToTick(entry, message))
                {
                    OnTickArrived(entry);
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
        /// <param name="message"></param>
        private void OnBarDataReceived(string[] message)
        {
            try
            {
                Bar entry = new Bar("");

                if (ParseToBar(entry, message))
                {
                    OnLiveBarArrived(entry);
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

                // Get Depth
                tick.Depth = Convert.ToInt32(message[10]);

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
                bar.IsBarCopied = Convert.ToBoolean(message[10]);
                return true;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "ParseToBar");
                return false;
            }
        }

        #region Incoming requests for MDE-Server
        
        /// <summary>
        /// Sends Login request to MDE
        /// </summary>
        /// <param name="login">TradeHub Login object</param>
        public bool Login(Login login)
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Sending Login request for: " + login.MarketDataProvider, _type.FullName, "Login");
                }

                // Check if MDE-Client is connected to MDE
                if (_isConnected)
                {
                    // Send login request to MDE
                   _dataEngineClient.SendLoginRequest(login);
                    return true;
                }

                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Request not sent to MDE as MDE-Client is not connected.", _type.FullName, "Login");
                }
                return false;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "Login");
                return false;
            }
        }

        /// <summary>
        /// Sends Logout request to MDE
        /// </summary>
        /// <param name="logout">TradeHub Logout object</param>
        public bool Logout(Logout logout)
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Sending logout request for: " + logout.MarketDataProvider, _type.FullName, "Logout");
                }

                // Check if MDE-Client is connected to MDE
                if (_isConnected)
                {
                    // Send logout request to MDE
                    _dataEngineClient.SendLogoutRequest(logout);
                    return true;
                }

                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Request not sent to MDE as MDE-Client is not connected.", _type.FullName, "Logout");
                }
                return false;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "Logout");
                return false;
            }
        }

        /// <summary>
        /// Sends Tick data subscription request to MDP through MDE-Server
        /// </summary>
        public bool Subscribe(Subscribe subscribe)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("New subscription request received: " + subscribe, _type.FullName, "Subscribe");
                }
                
                // Check if MDE-Client is connected to MDE
                if (_isConnected || subscribe.MarketDataProvider.Equals(MarketDataProvider.SimulatedExchange))
                {
                    Dictionary<Security, string> securities;
                    if (!_tickSubscriptions.TryGetValue(subscribe.MarketDataProvider, out securities))
                    {
                        securities = new Dictionary<Security, string>();
                    }

                    // Check if the Security is already subscribed
                    if(securities.ContainsKey(subscribe.Security))
                    {
                        if (_asyncClassLogger.IsDebugEnabled)
                        {
                            _asyncClassLogger.Debug("Request not sent to MDE as security is already subscribed.",
                                         _type.FullName, "Subscribe");
                        }
                        return false;
                    }

                    // Update list
                    securities.Add(subscribe.Security, subscribe.Id);

                    // Send subscription request to MDE-Server
                    //_dataEngineClient.SendTickSubscriptionRequest(subscribe);
                    _tickSubscribeRequest(subscribe);

                    // Update the local tick subscriptions map
                    _tickSubscriptions.AddOrUpdate(subscribe.MarketDataProvider, securities, (key, value) => securities);

                    return true;
                }

                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Request not sent to MDE as MDE-Client is not connected.", _type.FullName, "Subscribe");
                }
                return false;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "Subscribe");
                return true;
            }
        }

        /// <summary>
        /// Sends Live bar data subscription request to MDP through MDE-Server
        /// </summary>
        public bool Subscribe(BarDataRequest barDataRequest)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("New subscription request received: " + barDataRequest, _type.FullName, "Subscribe");
                }

                // Check if MDE-Client is connected to MDE
                if (_isConnected || barDataRequest.MarketDataProvider.Equals(MarketDataProvider.SimulatedExchange))
                {
                    List<BarDataRequest> barDataRequests ;
                    if (!_barSubscriptions.TryGetValue(barDataRequest.MarketDataProvider, out barDataRequests))
                    {
                        barDataRequests = new List<BarDataRequest>();
                    }

                    // Check if the Security is already subscribed
                    if (barDataRequests.Contains(barDataRequest))
                    {
                        if (_asyncClassLogger.IsInfoEnabled)
                        {
                            _asyncClassLogger.Info("Request not sent to MDE as bar is already subscribed.", _type.FullName, "Subscribe");
                        }
                        return false;
                    }

                    // Update list
                    barDataRequests.Add(barDataRequest);

                    // Send subscription request to MDE-Server
                    //_dataEngineClient.SendLiveBarSubscriptionRequest(barDataRequest);
                    _barSubscribeRequest(barDataRequest);

                    // Update the local bar subscriptions map
                    _barSubscriptions.AddOrUpdate(barDataRequest.MarketDataProvider, barDataRequests, (key, value) => barDataRequests);

                    return true;
                }

                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Request not sent to MDE as MDE-Client is not connected.", _type.FullName, "Subscribe");
                }
                return false;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "Subscribe");
                return false;
            }
        }

        /// <summary>
        /// Sends Live bar data subscription request to MDP through MDE-Server
        /// </summary>
        public bool Subscribe(BarDataRequest[] barDataRequest)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("New subscription request received: " + barDataRequest, _type.FullName, "Subscribe");
                }

                if (barDataRequest.Length <= 0)
                {
                    if (_asyncClassLogger.IsInfoEnabled)
                    {
                        _asyncClassLogger.Info("Request not sent as received bar request array is empty.", _type.FullName, "Subscribe");
                    }
                    return false;
                }

                for (int i = 0; i < barDataRequest.Length; i++)
                {
                    // Check if MDE-Client is connected to MDE
                    if (_isConnected || barDataRequest[i].MarketDataProvider.Equals(MarketDataProvider.SimulatedExchange))
                    {
                        List<BarDataRequest> barDataRequests;
                        if (!_barSubscriptions.TryGetValue(barDataRequest[i].MarketDataProvider, out barDataRequests))
                        {
                            barDataRequests = new List<BarDataRequest>();
                        }

                        // Check if the Security is already subscribed
                        if (barDataRequests.Contains(barDataRequest[i]))
                        {
                            if (_asyncClassLogger.IsInfoEnabled)
                            {
                                _asyncClassLogger.Info("Request not sent to MDE as bar is already subscribed.",
                                    _type.FullName, "Subscribe");
                            }
                            return false;
                        }

                        // Update list
                        barDataRequests.Add(barDataRequest[i]);

                        // Send subscription request to MDE-Server
                        //_dataEngineClient.SendLiveBarSubscriptionRequest(barDataRequest);
                        //_barSubscribeRequest(barDataRequest[i]);

                        // Update the local bar subscriptions map
                        _barSubscriptions.AddOrUpdate(barDataRequest[i].MarketDataProvider, barDataRequests,
                            (key, value) => barDataRequests);
                        
                    }

                    else
                    {
                        if (_asyncClassLogger.IsInfoEnabled)
                        {
                            _asyncClassLogger.Info("Request not sent to MDE as MDE-Client is not connected.", _type.FullName,
                                "Subscribe");
                        }
                    }
                }
                _multiBarSubscribeRequest(barDataRequest);
                return true;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "Subscribe");
                return false;
            }
        }

        /// <summary>
        /// Sends tick unsubscription request to MDP through MDE-Server
        /// </summary>
        public bool Unsubscribe(Unsubscribe unsubscribe)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("New unsubscription request received: " + unsubscribe, _type.FullName, "Unsubscribe");
                }

                // Check if MDE-Client is connected to MDE
                if (_isConnected || unsubscribe.MarketDataProvider.Equals(MarketDataProvider.SimulatedExchange))
                {
                    Dictionary<Security, string> securities;
                    if (_tickSubscriptions.TryGetValue(unsubscribe.MarketDataProvider, out securities))
                    {
                        // Check if the Security is already subscribed
                        if (securities.ContainsKey(unsubscribe.Security))
                        {
                            // Get subscription ID
                            unsubscribe.Id = securities[unsubscribe.Security];

                            // Send subscription request to MDE-Server
                            //_dataEngineClient.SendTickUnsubscriptionRequest(unsubscribe);
                            _tickUnsubscribeRequest(unsubscribe);

                            // Update list
                            securities.Remove(unsubscribe.Security);

                            return true;
                        }
                    }

                    if (_asyncClassLogger.IsDebugEnabled)
                    {
                        _asyncClassLogger.Debug("Request not sent to MDE as security is not subscribed.", _type.FullName, "Unsubscribe");
                    }
                    return false;
                }

                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Request not sent to MDE as MDE-Client is not connected.", _type.FullName, "Unsubscribe");
                }
                return false;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "Unsubscribe");
                return false;
            }
        }

        /// <summary>
        /// Sends bar unsubscription request to MDP through MDE-Server
        /// </summary>
        public bool Unsubscribe(BarDataRequest barDataRequest)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("New unsubscription request received: " + barDataRequest, _type.FullName, "Unsubscribe");
                }

                // Check if MDE-Client is connected to MDE
                if (_isConnected || barDataRequest.MarketDataProvider.Equals(MarketDataProvider.SimulatedExchange))
                {
                    List<BarDataRequest> barDataRequests;
                    if (_barSubscriptions.TryGetValue(barDataRequest.MarketDataProvider, out barDataRequests))
                    {
                        // Check if the Bar is already subscribed
                        if (barDataRequests.Contains(barDataRequest))
                        {
                            // Send subscription request to MDE-Server
                            //_dataEngineClient.SendLiveBarUnsubscriptionRequest(barDataRequest);
                            _barUnsubscribeRequest(barDataRequest);

                            // Update list
                            barDataRequests.Remove(barDataRequest);

                            return true;
                        }
                    }

                    if (_asyncClassLogger.IsDebugEnabled)
                    {
                        _asyncClassLogger.Debug("Request not sent to MDE as bar is not subscribed.", _type.FullName, "Unsubscribe");
                    }
                    return false;
                }

                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Request not sent to MDE as MDE-Client is not connected.", _type.FullName, "Unsubscribe");
                }
                return false;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "Unsubscribe");
                return false;
            }
        }

        /// <summary>
        /// Sends Tick data unsubscription request for all subscribed securities through MDE-Server
        /// </summary>
        /// <param name="marketDataProvider">Name of the Market Data Provider to be used</param>
        public bool UnsubscribeAllSecurities(string marketDataProvider)
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Unsubscribing all securities on: " + marketDataProvider, _type.FullName, "UnsubscribeAllSecurities");
                }

                // Check if MDE-Client is connected to MDE
                if (_isConnected || marketDataProvider.Equals(MarketDataProvider.SimulatedExchange))
                {
                    Dictionary<Security, string> securities;
                    // Get all securities for the requested MDP
                    if (_tickSubscriptions.TryRemove(marketDataProvider, out securities))
                    {
                        foreach (KeyValuePair<Security, string> security in securities)
                        {
                            // Create TradeHun Unsubscription message
                            Unsubscribe unsubscribe = SubscriptionMessage.TickUnsubscription(security.Value,
                                                                                             security.Key,
                                                                                             marketDataProvider);
                            
                            // Send tick unsubscription message
                            _dataEngineClient.SendTickUnsubscriptionRequest(unsubscribe);
                        }   
                    }
                }

                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Request not sent to MDE as MDE-Client is not connected.", _type.FullName, "UnsubscribeAllSecurities");
                }
                return false;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "UnsubscribeAllSecurities");
                return false;
            }
        }

        /// <summary>
        /// Sends Tick data unsubscription request for all subscribed securities on all MDPs through MDE-Server
        /// </summary>
        public bool UnsubscribeAllSecurities()
        {
            try
            {
                // Get all the data provider names on which data is subscribed
                var marketDataProviderList = _tickSubscriptions.Keys;

                // Unsubcribe Tick Data on all Market Data Providers
                foreach (string marketDataProvider in marketDataProviderList)
                {
                    UnsubscribeAllSecurities(marketDataProvider);
                }

                return true;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "UnsubscribeAllSecurities");
                return false;
            }
        }

        /// <summary>
        /// Sends bar unsubscription request for all susbcribed bars through MDP-Server
        /// </summary>
        /// <param name="marketDataProvider">Name of the Market Data Provider</param>
        public bool UnsubscribeAllLiveBars(string marketDataProvider)
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Unsubscribing all live bars", _type.FullName, "UnsubscribeAllLiveBars");
                }

                // Check if MDE-Client is connected to MDE
                if (_isConnected || marketDataProvider.Equals(MarketDataProvider.SimulatedExchange))
                {
                    List<BarDataRequest> barDataRequests;
                    // Get all the Bar Data Requests
                    if (_barSubscriptions.TryRemove(marketDataProvider, out barDataRequests))
                    {
                        // Send bar unsubscribption for each subscribed bar
                        foreach (BarDataRequest barDataRequest in barDataRequests)
                        {
                            // Send request to Market Data Engine
                            _dataEngineClient.SendLiveBarUnsubscriptionRequest(barDataRequest);
                        }

                        barDataRequests.Clear();
                    }
                }

                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Request not sent to MDE as MDE-Client is not connected.", _type.FullName, "UnsubscribeAllLiveBars");
                }
                return false;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "UnsubscribeAllLiveBars");
                return false;
            }
        }

        /// <summary>
        /// Sends bar unsubscription request for all susbcribed bars on all MDPs through MDP-Server
        /// </summary>
        public bool UnsubscribeAllLiveBars()
        {
            try
            {
                // Get name of all data providers on which live bars are subscribed
                var marketDataProviderList = _barSubscriptions.Keys;

                // Unsubcribe Live Bar Data on all Market Data Providers
                foreach (string marketDataProvider in marketDataProviderList)
                {
                    UnsubscribeAllLiveBars(marketDataProvider);
                }

                return true;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "UnsubscribeAllLiveBars");
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    StopService();
                }
                // Release unmanaged resources.
                _disposed = true;
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
