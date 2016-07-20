using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Disruptor;
using Spring.Context.Support;
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.CustomAttributes;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.Repositories;
using TradeHub.Common.Core.ValueObjects;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.Inquiry;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.NotificationEngine.Common.Constants;
using TradeHub.NotificationEngine.Common.ValueObject;
using TradeHub.StrategyEngine.HistoricalData;
using TradeHub.StrategyEngine.MarketData;
using TradeHub.StrategyEngine.Notification;
using TradeHub.StrategyEngine.OrderExecution;

namespace TradeHub.StrategyEngine.TradeHub
{
    /// <summary>
    /// Base Class to be used as a tempelate for new strategies
    /// </summary>
    public abstract class TradeHubStrategy : IDisposable
    {
        private Type _type = typeof (TradeHubStrategy);

        private MarketDataService _marketDataService;
        private HistoricalDataService _historicalDataService;
        private OrderExecutionService _orderExecutionService;
        private IPersistRepository<object> _persistRepository;

        /// <summary>
        /// Used for sending notificaitons to Notification Engine - Server
        /// </summary>
        private NotificationService _notificationService;

        //private readonly string _marketDataProviderName;
        //private readonly string _historicalDataProviderName;
        //private readonly string _orderExecutionProviderName;

        private bool _disposed = false;
        private bool _isRunning = false;
        private Strategy _strategy = new Strategy();

        private readonly string[] _marketDataProviderNames;
        private readonly string[] _historicalDataProviderNames;
        private readonly string[] _orderExecutionProviderNames;

        /// <summary>
        /// Saves user intended information during strategy execution
        /// </summary>
        private List<string> _localData = new List<string>();

        #region Events

        // ReSharper disable InconsistentNaming
        private event Action<bool> _onStrategyStatusChanged;
        private event Action<Execution> _onNewExecutionReceived;
        private event Action<Order> _orderAcceptedEvent;
        private event Action<Order> _cancellationArrivedEvent;
        private event Action<Rejection> _rejectionArrivedEvent;
        private event Action<string> _displayMessageEvent;
        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Invoked when strategy status changes depicting execution state
        /// </summary>
        public event Action<bool> OnStrategyStatusChanged
        {
            add { if (_onStrategyStatusChanged == null) _onStrategyStatusChanged += value; }
            remove { _onStrategyStatusChanged -= value; }
        }

        /// <summary>
        /// Invoked when Order Execution is received for Partial/Full fill
        /// </summary>
        public event Action<Execution> OnNewExecutionReceived
        {
            add { if (_onNewExecutionReceived == null) _onNewExecutionReceived += value; }
            remove { _onNewExecutionReceived -= value; }
        }

        /// <summary>
        /// Invoked when requested Order is accepted by the exchange
        /// </summary>
        public event Action<Order> OrderAcceptedEvent
        {
            add { if (_orderAcceptedEvent == null) _orderAcceptedEvent += value; }
            remove { _orderAcceptedEvent -= value; }
        }

        /// <summary>
        /// Invoked when Order is cancelled by the exchange
        /// </summary>
        public event Action<Order> CancellationArrivedEvent
        {
            add { if (_cancellationArrivedEvent == null) _cancellationArrivedEvent += value; }
            remove { _cancellationArrivedEvent -= value; }
        }

        /// <summary>
        /// Invoked when Order request is rejected by the exchange
        /// </summary>
        public event Action<Rejection> RejectionArrivedEvent
        {
            add { if (_rejectionArrivedEvent == null) _rejectionArrivedEvent += value; }
            remove { _rejectionArrivedEvent -= value; }
        }

        /// <summary>
        /// Invoked when user requests information to be displayed on UI
        /// </summary>
        public event Action<string> DisplayMessageEvent
        {
            add { if (_displayMessageEvent == null) _displayMessageEvent += value; }
            remove { _displayMessageEvent -= value; }
        }
        #endregion

        #region Notification Fields

        /// <summary>
        /// Indicates if notification for new order is to be sent
        /// </summary>
        private bool _newOrderNotification = false;

        /// <summary>
        /// Indicates if notification on order acceptance is to be sent
        /// </summary>
        private bool _acceptedOrderNotification = false;

        /// <summary>
        /// Indicates if notification on order execution is to be sent
        /// </summary>
        private bool _executionNotification = false;

        /// <summary>
        /// Indicates if the notification on order rejection is to be sent
        /// </summary>
        private bool _rejectionNotification = false;

        #endregion

        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                // Update Value
                _isRunning = value;

                // Raise Event 
                if (_onStrategyStatusChanged != null)
                    _onStrategyStatusChanged(value);
            }
        }

        /// <summary>
        /// Read-Only array containing market data provider names
        /// </summary>
        public IReadOnlyList<string> MarketDataProviderNames
        {
            get { return _marketDataProviderNames; }
        }

        /// <summary>
        /// Read-Only array containing historical data provider names
        /// </summary>
        public IReadOnlyList<string> HistoricalDataProviderNames
        {
            get { return _historicalDataProviderNames; }
        }

        /// <summary>
        /// Read-Only array containing order execution provider names
        /// </summary>
        public IReadOnlyList<string> OrderExecutionProviderNames
        {
            get { return _orderExecutionProviderNames; }
        }

        /// <summary>
        /// Current Strategy instance name
        /// </summary>
        public string StrategyName
        {
            get { return _strategy.Name; }
            set { _strategy.Name = value; }
        }

        /// <summary>
        /// Timer is elapsed indicating strategy execution stop
        /// </summary>
        public Timer ConnectivityTimer;

        #region Constructors

        /// <summary>
        /// Argument constructor
        /// </summary>
        /// <param name="marketDataProvider">name of the data provider to be used for live data</param>
        /// <param name="orderExecutionProvider">name of the order execution provider to be used for order manipulation</param>
        /// <param name="historicalDataProvider">name of the data provider to be used to historical data </param>
        protected TradeHubStrategy(string marketDataProvider, string orderExecutionProvider, string historicalDataProvider)
        {
            Logger.SetLoggingLevel();

            _marketDataProviderNames = new[] {marketDataProvider};
            _orderExecutionProviderNames = new[] {orderExecutionProvider};
            _historicalDataProviderNames = new[] {historicalDataProvider};
            
            InitializeServices();

            // Initialize
            ConnectivityTimer = new Timer();
            ConnectivityTimer.Interval = 5000;
            ConnectivityTimer.Elapsed += OnConnectivityTimerElapses;

            // Start Market/Historical Data Services
            StartDataServices();

            // Start Order Service
            StartOrderExecutionService();

            // Start Notification Service
            StartNotificationService();
        }

        /// <summary>
        /// Argument constructor for multiple connecting with multiple providers
        /// </summary>
        /// <param name="marketDataProviders">names of the data providers to be used for live data</param>
        /// <param name="orderExecutionProviders">names of the order execution providers to be used for order manipulation</param>
        /// <param name="historicalDataProviders">names of the data providers to be used to historical data </param>
        protected TradeHubStrategy(string[] marketDataProviders, string[] orderExecutionProviders, string[] historicalDataProviders)
        {
            Logger.SetLoggingLevel();

            _marketDataProviderNames = marketDataProviders;
            _orderExecutionProviderNames = orderExecutionProviders;
            _historicalDataProviderNames = historicalDataProviders;

            InitializeServicesForMultipleProviders();

            // Initialize
            ConnectivityTimer = new Timer();
            ConnectivityTimer.Interval = 5000;
            ConnectivityTimer.Elapsed += OnConnectivityTimerElapses;

            // Start Market/Historical Data Services
            StartDataServices();

            // Start Order Service
            StartOrderExecutionService();

            // Start Notification Service
            StartNotificationService();
        }


        #endregion

        /// <summary>
        /// Sets Additional Parameters for the strategy
        /// </summary>
        /// <param name="arguments">Parameter values to be used</param>
        public virtual void SetParameters(object[] arguments)
        {
            //TODO: Provide Implementation
        }

        #region Override Market/Order requests

        /// <summary>
        /// Will initialize the Market Data Service Disruptor with the given event handler
        /// </summary>
        /// <param name="handler">Event Handler which will recieve the dirsuptor events</param>
        public void InitializeMarketDataServiceDisruptor(IEventHandler<RabbitMqRequestMessage>[] handler)
        {
            _marketDataService.InitializeDistuptor(handler);
        }

        /// <summary>
        /// Will initialize the Order Execution Service Disruptor with the given event handler
        /// </summary>
        /// <param name="handler">Event Handler which will recieve the dirsuptor events</param>
        public void InitializeOrderExecutionServiceDisruptor(IEventHandler<RabbitMqRequestMessage>[] handler)
        {
            _orderExecutionService.InitializeDistuptor(handler);
        }

        /// <summary>
        /// Overrides the Send Order request calls for order execution service
        /// </summary>
        public void OverrideOrderRequest(Action<byte[]> sendOrderAction)
        {
            _orderExecutionService.OverrideOrderRequest(sendOrderAction);
        }

        /// <summary>
        /// Overrides Market Order request calls for order execution service
        /// </summary>
        /// <param name="marketOrderAction"></param>
        public void OverrideMarketOrderRequest(Action<MarketOrder> marketOrderAction)
        {
            _orderExecutionService.OverrideMarketOrderRequest(marketOrderAction);
        }

        /// <summary>
        /// Overrides Limit Order request calls for order execution service
        /// </summary>
        /// <param name="limitOrderAction"></param>
        public void OverrideLimitOrderRequest(Action<LimitOrder> limitOrderAction)
        {
            _orderExecutionService.OverrideLimitOrderRequest(limitOrderAction);
        }

        /// <summary>
        /// Overrides Cancel Order request calls for order execution service
        /// </summary>
        /// <param name="cancelOrderAction"></param>
        public void OverrideCancelOrderRequest(Action<Order> cancelOrderAction)
        {
            _orderExecutionService.OverrideCancelOrderRequest(cancelOrderAction);
        }

        /// <summary>
        /// Overrides the Tick subscription request calls for market data service
        /// </summary>
        public void OverrideTickSubscriptionRequest(Action<Subscribe> tickSubscribeAction)
        {
            _marketDataService.OverrideTickSubscriptionRequest(tickSubscribeAction);
        }

        /// <summary>
        /// Overrides the Live Bar subscription request calls for market data service
        /// </summary>
        public void OverrideBarSubscriptionRequest(Action<BarDataRequest> barSubscribeAction)
        {
            _marketDataService.OverrideBarSubscriptionRequest(barSubscribeAction);
        }

        /// <summary>
        /// Overrides the Live Bar subscription request calls for market data service
        /// </summary>
        public void OverrideBarSubscriptionRequest(Action<BarDataRequest[]> barSubscribeAction)
        {
            _marketDataService.OverrideBarSubscriptionRequest(barSubscribeAction);
        }

        /// <summary>
        /// Overrides the Tick un-subscription request calls for market data service
        /// </summary>
        public void OverrideTickUnsubscriptionRequest(Action<Unsubscribe> tickUnsubscribeAction)
        {
            _marketDataService.OverrideTickUnsubscriptionRequest(tickUnsubscribeAction);
        }

        /// <summary>
        /// Overrides the Live Bar un-subscription request calls for market data service
        /// </summary>
        public void OverriderBarUnsubscriptionRequest(Action<BarDataRequest> barUnsubscribeAction)
        {
            _marketDataService.OverriderBarUnsubscriptionRequest(barUnsubscribeAction);
        }

        #endregion

        /// <summary>
        /// Distructor
        /// </summary>
        ~TradeHubStrategy()
        {
            Dispose(false);
        }

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
                    Stop();
                    StoptDataServices();
                    StopOrderExecutionService();
                    StopNotificationService();
                    _localData.Clear();
                }
                // Release unmanaged resources.
                _disposed = true;
            }
        }

        #region Hook/Unhook Data Service Events

        /// <summary>
        /// Hooks Market Data Service Events
        /// </summary>
        private void RegisterMarketDataServiceEvents()
        {
            _marketDataService.Connected += OnMarketDataServiceConnected;
            _marketDataService.LogonArrived += OnMarketDataServiceLogonArrived;
            _marketDataService.LogoutArrived += OnMarketDataServiceLogoutArrived;
            _marketDataService.TickArrived += OnTickArrived;
            _marketDataService.BarArrived += OnBarArrived;
            _marketDataService.InquiryResponseArrived += OnInquiryResponseArrived;
        }

        /// <summary>
        /// Hooks Historical Data Service Events
        /// </summary>
        private void RegisterHistoricalDataServiceEvents()
        {
            _historicalDataService.Connected += OnHistoricalDataServiceConnected;
            _historicalDataService.LogonArrived += OnHistoricalDataServiceLogonArrived;
            _historicalDataService.LogoutArrived += OnHistoricalDataServiceLogoutArrived;
            _historicalDataService.HistoricalDataArrived += OnHistoricalDataArrived;
            _historicalDataService.InquiryResponseArrived += OnInquiryResponseArrived;
        }

        /// <summary>
        /// Unhooks Market Data Service Events
        /// </summary>
        private void UnregisterMarketDataServiceEvents()
        {
            _marketDataService.Connected -= OnMarketDataServiceConnected;
            _marketDataService.LogonArrived -= OnMarketDataServiceLogonArrived;
            _marketDataService.LogoutArrived -= OnMarketDataServiceLogoutArrived;
            _marketDataService.TickArrived -= OnTickArrived;
            _marketDataService.BarArrived -= OnBarArrived;
            _marketDataService.InquiryResponseArrived -= OnInquiryResponseArrived;
        }

        /// <summary>
        /// Unhooks Historical Data Service Events
        /// </summary>
        private void UnregisterHistoricalDataServiceEvents()
        {
            _historicalDataService.Connected -= OnHistoricalDataServiceConnected;
            _historicalDataService.LogonArrived -= OnHistoricalDataServiceLogonArrived;
            _historicalDataService.LogoutArrived -= OnHistoricalDataServiceLogoutArrived;
            _historicalDataService.HistoricalDataArrived -= OnHistoricalDataArrived;
            _historicalDataService.InquiryResponseArrived -= OnInquiryResponseArrived;
        }

        #endregion

        #region Hook/Unhook Order Execution Service Events

        /// <summary>
        /// Hooks Order Execution Service events
        /// </summary>
        private void RegisterOrderExecutionServiceEvents()
        {
            _orderExecutionService.Connected += OnOrderExecutionServiceConnected;
            _orderExecutionService.LogonArrived += OnOrderExecutionServiceLogonArrived;
            _orderExecutionService.LogoutArrived += OnOrderExecutionServiceLogoutArrived;
            _orderExecutionService.NewArrived += NewArrived;
            _orderExecutionService.CancellationArrived += CancellationArrived;
            _orderExecutionService.ExecutionArrived += ExecutionArrived;
            _orderExecutionService.RejectionArrived += RejectionArrived;
            _orderExecutionService.LocateMessageArrived += OnLocateMessageArrived;
        }

        /// <summary>
        /// Unhooks Order Execution Service events
        /// </summary>
        private void UnregisterOrderExecutionServiceEvents()
        {
            _orderExecutionService.Connected -= OnOrderExecutionServiceConnected;
            _orderExecutionService.LogonArrived -= OnOrderExecutionServiceLogonArrived;
            _orderExecutionService.LogoutArrived -= OnOrderExecutionServiceLogoutArrived;
            _orderExecutionService.NewArrived -= NewArrived;
            _orderExecutionService.CancellationArrived -= CancellationArrived;
            _orderExecutionService.ExecutionArrived -= ExecutionArrived;
            _orderExecutionService.RejectionArrived -= RejectionArrived;
            _orderExecutionService.LocateMessageArrived -= OnLocateMessageArrived;
        }

        #endregion

        /// <summary>
        /// Initializes Service objects to communicate with the X-Engine's windows services
        /// </summary>
        private void InitializeServices()
        {
            // Get Market Data Service object from spring or create a new object
            if (!string.IsNullOrEmpty(_marketDataProviderNames[0]))
                if (_marketDataProviderNames[0].Equals(MarketDataProvider.SimulatedExchange))
                    _marketDataService = new MarketDataService();
                else
                    _marketDataService = ContextRegistry.GetContext()["MarketDataService"] as MarketDataService;

            // Get Order Execution Service object from spring or create a new object
            if (!string.IsNullOrEmpty(_orderExecutionProviderNames[0]))
                if (_orderExecutionProviderNames[0].Equals(OrderExecutionProvider.SimulatedExchange))
                    _orderExecutionService = new OrderExecutionService();
                else
                    _orderExecutionService =
                        ContextRegistry.GetContext()["OrderExecutionService"] as OrderExecutionService;

            // Get Historical Data Service object from spring or create a new object
            if (!string.IsNullOrEmpty(_historicalDataProviderNames[0]))
                _historicalDataService = ContextRegistry.GetContext()["HistoricalDataService"] as HistoricalDataService;

            // Get Notification Service object from spring
            _notificationService = ContextRegistry.GetContext()["NotificationService"] as NotificationService;

            // Initiliaze persitance repositories
            _persistRepository = ContextRegistry.GetContext()["PersistRepository"] as IPersistRepository<object>;

            // Hook Market Data Service Events
            if (!string.IsNullOrEmpty(_marketDataProviderNames[0]))
                RegisterMarketDataServiceEvents();

            // Hook Order Service Events
            if (!string.IsNullOrEmpty(_orderExecutionProviderNames[0]))
                RegisterOrderExecutionServiceEvents();

            // Hook Historical Data Service Events
            if (!string.IsNullOrEmpty(_historicalDataProviderNames[0]))
                RegisterHistoricalDataServiceEvents();
        }

        /// <summary>
        /// Initializes Service objects to communicate with the X-Engine's windows services
        /// </summary>
        private void InitializeServicesForMultipleProviders()
        {
            if (_marketDataProviderNames != null && _marketDataProviderNames.Any())
            {
                if (_marketDataProviderNames.Count().Equals(1) &&
                    _marketDataProviderNames[0].Equals(MarketDataProvider.SimulatedExchange))
                {
                    _marketDataService = new MarketDataService();
                }
                else
                {
                    // Get Market Data Service object from spring or create a new object
                    _marketDataService = ContextRegistry.GetContext()["MarketDataService"] as MarketDataService;

                    // Hook Market Data Service Events
                    RegisterMarketDataServiceEvents();
                }
            }

            if (_orderExecutionProviderNames != null && _orderExecutionProviderNames.Any())
            {
                if (_orderExecutionProviderNames.Count().Equals(1) && _orderExecutionProviderNames[0].Equals(OrderExecutionProvider.SimulatedExchange))
                {
                    _orderExecutionService = new OrderExecutionService();
                }
                else
                {
                    // Get Order Execution Service object from spring or create a new object
                    _orderExecutionService =
                        ContextRegistry.GetContext()["OrderExecutionService"] as OrderExecutionService;

                    // Hook Order Service Events
                    RegisterOrderExecutionServiceEvents();
                }
            }
            
            // Get Historical Data Service object from spring or create a new object
            if (_historicalDataProviderNames != null && _historicalDataProviderNames.Any())
            {
                _historicalDataService = ContextRegistry.GetContext()["HistoricalDataService"] as HistoricalDataService;

                // Hook Historical Data Service Events
                RegisterHistoricalDataServiceEvents();
            }

            // Get Notification Service object from spring
            _notificationService = ContextRegistry.GetContext()["NotificationService"] as NotificationService;

            // Initiliaze persitance repositories
            _persistRepository = ContextRegistry.GetContext()["PersistRepository"] as IPersistRepository<object>;
        }

        /// <summary>
        /// Starts Market/Historical data services
        /// </summary>
        private void StartDataServices()
        {
            // Start Market Data Service
            if (_marketDataProviderNames != null)
            {
                // Raise Logon event if connecting to 'Simulated Exchange'
                if (_marketDataProviderNames[0].Equals(MarketDataProvider.SimulatedExchange))
                {
                    Task.Run(() =>
                    {
                        Task.Delay(100).Wait();
                        //OnMarketDataServiceLogonArrived(_marketDataProviderName);
                        OnMarketDataServiceConnected();
                    });
                }
                // Start service to connect with available brokers
                else
                {
                    if (_marketDataService != null) _marketDataService.StartService();
                }
            }

            // Start Historical Data Service
            if (_historicalDataProviderNames != null)
            {
                // Raise Logon event if connecting to 'Simulated Exchange'
                if (_historicalDataProviderNames[0].Equals(MarketDataProvider.SimulatedExchange))
                {
                    Task.Run(() =>
                    {
                        Task.Delay(100).Wait();
                        //OnHistoricalDataServiceLogonArrived(_historicalDataProviderName);
                        OnHistoricalDataServiceConnected();
                    });
                }
                // Start service to connect with available brokers
                else
                {
                    if (_historicalDataService != null) _historicalDataService.StartService();
                }
            }
        }

        /// <summary>
        /// Starts Order Execution Service
        /// </summary>
        private void StartOrderExecutionService()
        {
            // Start Order Execution Service
            if (_orderExecutionProviderNames != null)
            {
                // Raise Logon event if connecting with 'Simulated Exchange'
                if (_orderExecutionProviderNames[0].Equals(OrderExecutionProvider.SimulatedExchange))
                {
                    Task.Run(() =>
                    {
                        Task.Delay(100).Wait();
                        //OnOrderExecutionServiceLogonArrived(_orderExecutionProviderName);
                        OnOrderExecutionServiceConnected();
                    });
                }
                // Start service to connect with available Brokers
                else
                {
                    if (_orderExecutionService != null) _orderExecutionService.StartService();
                }
            }
        }

        /// <summary>
        /// Starts Notification Engine Service
        /// </summary>
        private void StartNotificationService()
        {
            if (_notificationService != null)
            {
                // Start service to connect with available brokers
                _notificationService.StartService();
            }
        }

        /// <summary>
        /// Stops Market/Historical data services
        /// </summary>
        private void StoptDataServices()
        {
            // Stop Market Data Service
            // if (!string.IsNullOrEmpty(_marketDataProviderName))
            if (_marketDataService != null)
            {
                foreach (string marketDataProviderName in _marketDataProviderNames)
                {
                    // Unsubscribe all securities
                    _marketDataService.UnsubscribeAllSecurities(marketDataProviderName);
                    // Unsubscribe all live bars
                    _marketDataService.UnsubscribeAllLiveBars(marketDataProviderName);

                    // Raise Logout event if communicating with 'Simulated Exchange'
                    if (marketDataProviderName.Equals(MarketDataProvider.SimulatedExchange))
                    {
                        OnMarketDataServiceLogoutArrived(marketDataProviderName);
                        _marketDataService.StopService();
                    }
                    // Send Logout to connected Broker and stop Service
                    else
                    {
                        Logout mdLogout = new Logout() { MarketDataProvider = marketDataProviderName };

                        // Send Logout before stoping Service
                        _marketDataService.Logout(mdLogout);
                    }
                }

                //_marketDataService.StopService();
                _marketDataService.Dispose();
            }

            // Stop Historical Data Service
            //if (!string.IsNullOrEmpty(_historicalDataProviderName))
            if (_historicalDataService != null)
            {
                foreach (string historicalDataProviderName in _historicalDataProviderNames)
                {
                    Logout hdLogout = new Logout() {MarketDataProvider = historicalDataProviderName};

                    // Send Logout before stoping Service
                    _historicalDataService.Logout(hdLogout);
                    _historicalDataService.StopService();
                }
            }
        }

        /// <summary>
        /// Stops Order Execution Service
        /// </summary>
        private void StopOrderExecutionService()
        {
            // Stop Order Execution Service
            if (_orderExecutionService != null)
            {
                // Send Logout to connected Broker and stop Service
                foreach (string orderExecutionProviderName in _orderExecutionProviderNames)
                {
                    // Raise Logout event while communicating with 'Simulated Exchange'
                    if (orderExecutionProviderName.Equals(OrderExecutionProvider.SimulatedExchange))
                    {
                        OnOrderExecutionServiceLogoutArrived(orderExecutionProviderName);
                        _orderExecutionService.StopService();
                    }
                    else
                    {
                        Logout logout = new Logout() {OrderExecutionProvider = orderExecutionProviderName};

                        // Send Logout before stoping Service
                        _orderExecutionService.Logout(logout);
                    }
                }

                //_orderExecutionService.StopService();
                _orderExecutionService.Dispose();
            }
        }

        /// <summary>
        /// Stops Notification Engine Service
        /// </summary>
        private void StopNotificationService()
        {
            if (_notificationService != null)
            {
                // Send request to close connection with the server
                _notificationService.StopService();
            }
        }

        /// <summary>
        /// Starts Executing the Strategy calls overriden funtion from within
        /// </summary>
        public void Run()
        {
            IsRunning = true;

            // Save time at which the Strategy is executed
            _strategy.StartDateTime = DateTime.UtcNow;
            _persistRepository.AddUpdate(_strategy);

            // Call Virtual Function incase user has specified some additional functionality
            OnRun();
        }

        /// <summary>
        /// Can be overriden to provides additional funtionality for Function 'Run()'
        /// </summary>
        protected virtual void OnRun()
        {
            //TODO: Provide Implementation
        }

        /// <summary>
        /// Stops Executing the Strategy calls overriden funtion from within
        /// </summary>
        public void Stop()
        {
            // Call Virtual Function incase user has specified some additional functionality
            OnStop();

            IsRunning = false;
        }

        /// <summary>
        /// Can be overriden to provides additional funtionality for Function 'Stop()'
        /// </summary>
        protected virtual void OnStop()
        {
            //TODO: Provide Implementation
        }

        #region Market/Historic Data Provier Requests

        /// <summary>
        /// Sends Market Data Provider LOG-ON request to MDE
        /// </summary>
        /// <param name="marketDataProvider">Market data provider to connect to</param>
        public void LogonRequestMarketData(string marketDataProvider)
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Sending login request to MDE-Server: " + marketDataProvider, _type.FullName,
                    "LogonRequestMarketData");
            }

            if (string.IsNullOrEmpty(marketDataProvider))
            {
                return;
            }

            if (marketDataProvider.Equals(MarketDataProvider.SimulatedExchange))
            {
                OnMarketDataServiceLogonArrived(marketDataProvider);
                return;
            }

            // Create TradeHub Login message for Live Market Data
            Login login = new Login { MarketDataProvider = marketDataProvider };

            // Send Login request
            _marketDataService.Login(login);
        }

        /// <summary>
        /// Sends Historical Data Provider LOG-ON request to MDE
        /// </summary>
        /// <param name="historicalDataProvider">Historical data provider to connect to</param>
        public void LogonRequestHistoricalData(string historicalDataProvider)
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Sending login request to MDE-Server: " + historicalDataProvider, _type.FullName,
                    "LogonRequestHistoricalData");
            }

            if (string.IsNullOrEmpty(historicalDataProvider))
            {
                return;
            }

            if (historicalDataProvider.Equals(MarketDataProvider.SimulatedExchange))
            {
                OnHistoricalDataServiceLogonArrived(historicalDataProvider);
                return;
            }

            // Create TradeHub Login message for Historical Market Data
            Login login = new Login { MarketDataProvider = historicalDataProvider };

            // Send Login request
            _historicalDataService.Login(login);
        }

        /// <summary>
        /// Sends Market Data Provider LOG-OUT request to MDE
        /// </summary>
        /// <param name="marketDataProvider">Market data provider to disconnect from</param>
        public void LogoutRequestMarketData(string marketDataProvider)
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Sending logout request to MDE-Server: " + marketDataProvider, _type.FullName,
                    "LogonRequestMarketData");
            }

            if (string.IsNullOrEmpty(marketDataProvider))
            {
                return;
            }

            if (marketDataProvider.Equals(MarketDataProvider.SimulatedExchange))
            {
                OnMarketDataServiceLogoutArrived(marketDataProvider);
                return;
            }

            // create logout message to be sent
            Logout mdLogout = new Logout() { MarketDataProvider = marketDataProvider };

            // Send market data provider logout request
            _marketDataService.Logout(mdLogout);
        }

        /// <summary>
        /// Sends Historical Data Provider LOG-OUT request to MDE
        /// </summary>
        /// <param name="historicalDataProvider">Hsitorical data provider to disconnect from</param>
        public void LogoutRequestHistoricalData(string historicalDataProvider)
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Sending logout request to MDE-Server: " + historicalDataProvider, _type.FullName,
                    "LogoutRequestHistoricalData");
            }

            if (string.IsNullOrEmpty(historicalDataProvider))
            {
                return;
            }

            if (historicalDataProvider.Equals(MarketDataProvider.SimulatedExchange))
            {
                OnHistoricalDataServiceLogoutArrived(historicalDataProvider);
                return;
            }

            // Create logout message to be sent
            Logout mdLogout = new Logout() { MarketDataProvider = historicalDataProvider };

            // Send historical data provider logout request
            _historicalDataService.Logout(mdLogout);
        }

        /// <summary>
        /// Sends Tick subscription requests to Market Data Engine
        /// </summary>
        /// <param name="subscribe">Contains tick subscription info</param>
        public void Subscribe(Subscribe subscribe)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("New Tick subscription request received: " + subscribe, _type.FullName, "Subscribe");
                }
                _marketDataService.Subscribe(subscribe);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "Subscribe");
            }
        }

        /// <summary>
        /// Sends Live Bar subscription requests to Market Data Engine
        /// </summary>
        /// <param name="barDataRequest">Contains live bar susbcription info</param>
        public void Subscribe(BarDataRequest barDataRequest)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("New live bar subscription request received: " + barDataRequest, _type.FullName,
                        "Subscribe");
                }
                _marketDataService.Subscribe(barDataRequest);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "Subscribe");
            }
        }

        /// <summary>
        /// Sends Live Bar subscription requests to Market Data Engine
        /// </summary>
        /// <param name="barDataRequest">Contains live bar susbcription info</param>
        public void Subscribe(BarDataRequest[] barDataRequest)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("New multi-symbol live bar subscription request received: " + barDataRequest, _type.FullName,
                        "Subscribe");
                }
                _marketDataService.Subscribe(barDataRequest);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "Subscribe");
            }
        }

        /// <summary>
        /// Sends Historical Bar subscription requests to Market Data Engine
        /// </summary>
        /// <param name="historicDataRequest">Contains historical bar susbcription info</param>
        public void Subscribe(HistoricDataRequest historicDataRequest)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("New historical bar subscription request received: " + historicDataRequest,
                        _type.FullName, "Subscribe");
                }

                // Send request to Historical Data Service
                _historicalDataService.Subscribe(historicDataRequest);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "Subscribe");
            }
        }

        /// <summary>
        /// Sends Tick unsubscription request to Market Data Engine
        /// </summary>
        /// <param name="unsubscribe">Contains tick unsubscription info</param>
        public void Unsubscribe(Unsubscribe unsubscribe)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Tick data unsubscription request received: " + unsubscribe,
                        _type.FullName, "Unsubscribe");
                }

                // Send request to Market Data Service
                _marketDataService.Unsubscribe(unsubscribe);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "Unsubscribe");
            }
        }

        /// <summary>
        /// Sends Live Bar unsubscription request to Market Data Engine
        /// </summary>
        /// <param name="barDataRequest">Contains live bar unsubscription info</param>
        public void Unsubscribe(BarDataRequest barDataRequest)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Live bar unsubscription request received: " + barDataRequest, _type.FullName,
                        "Unsubscribe");
                }

                // Send request to Market Data Service
                _marketDataService.Unsubscribe(barDataRequest);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "Unsubscribe");
            }
        }

        #endregion

        #region Order Execution Provider Requests

        /// <summary>
        /// Sends Order Execution Provider LOG-ON request to OEE
        /// </summary>
        /// <param name="orderExecutionProvider">Order execution provider to connect to</param>
        public void LogonRequestOrderExecution(string orderExecutionProvider)
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Sending login request to OEE-Server: " + orderExecutionProvider, _type.FullName,
                    "LogonRequestOrderExecution");
            }

            if (string.IsNullOrEmpty(orderExecutionProvider))
            {
                return;
            }

            if (orderExecutionProvider.Equals(OrderExecutionProvider.SimulatedExchange))
            {
                OnOrderExecutionServiceLogonArrived(orderExecutionProvider);
                return;
            }

            // Create TradeHub Login message for Order Execution Provider
            Login login = new Login { OrderExecutionProvider = orderExecutionProvider };

            // Send Login request
            _orderExecutionService.Login(login);
        }

        /// <summary>
        /// Sends Order Execution Provider LOG-OUT request to OEE
        /// </summary>
        /// <param name="orderExecutionProvider">Order execution provider to disconnect from</param>
        public void LogoutRequestOrderExecution(string orderExecutionProvider)
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Sending logout request to OEE-Server: " + orderExecutionProvider, _type.FullName,
                    "LogoutRequestOrderExecution");
            }

            if (string.IsNullOrEmpty(orderExecutionProvider))
            {
                return;
            }

            if (orderExecutionProvider.Equals(OrderExecutionProvider.SimulatedExchange))
            {
                OnOrderExecutionServiceLogoutArrived(orderExecutionProvider);
                return;
            }

            // Create logout message to send
            Logout mdLogout = new Logout() { OrderExecutionProvider = orderExecutionProvider };

            // Send order execution provider logout request
            _orderExecutionService.Logout(mdLogout);
        }

        /// <summary>
        /// Sends new MARKET ORDER request to Order Execution Engine 
        /// </summary>
        /// <param name="marketOrder">TradeHub Market order containing info required to execute market order on the exchange</param>
        public void SendOrder(MarketOrder marketOrder)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("New Market Order request received: " + marketOrder, _type.FullName, "SendOrder");
                }
                //append strategy id
                marketOrder.StrategyId = _strategy.Id;
                // Send market order to OEE through Order Execution Service
                _orderExecutionService.SendOrder(marketOrder);
                //_orderRepository.AddUpdate(marketOrder);

                // Send a new notification message
                SendNewOrderNotification(marketOrder, default(decimal));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendOrder");
            }
        }

        /// <summary>
        /// Sends new LIMIT ORDER request to Order Execution Engine
        /// </summary>
        /// <param name="limitOrder">TradeHub Limit order containing info required to execute limit order on the exchange</param>
        public void SendOrder(LimitOrder limitOrder)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("New Limit Order request received: " + limitOrder, _type.FullName, "SendOrder");
                }
                //append strategy id
                limitOrder.StrategyId = _strategy.Id;

                // Send limit order to OEE through Order Execution Service
                _orderExecutionService.SendOrder(limitOrder);
                //_orderRepository.AddUpdate(limitOrder);

                // Send a new notification message
                SendNewOrderNotification(limitOrder, limitOrder.LimitPrice);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendOrder");
            }
        }

        /// <summary>
        /// Sends CANCEL ORDER request to the Order Execution Engine
        /// </summary>
        /// <param name="orderId">Order ID of the intended order to be cancelled</param>
        public void CancelOrder(string orderId)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("New order cancellation request received: " + orderId, _type.FullName, "CancelOrder");
                }

                // Send cancel request to OEE through Order Execution Service
                _orderExecutionService.CancelOrder(orderId);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "CancelOrder");
            }
        }

        /// <summary>
        /// Sends LOCATE RESPONSE to the Order Execution Engine
        /// </summary>
        /// <param name="locateResponse">TradeHub LocateResponse containing acceptance/rejection of LocateMessage</param>
        public void SendLocateResponse(LocateResponse locateResponse)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Locate Response received: " + locateResponse, _type.FullName, "SendLocateResponse");
                }

                // Send response to OEE through Order Execution Service
                _orderExecutionService.SendLocateResponse(locateResponse);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendLocateResponse");
            }
        }

        #endregion

        #region Market Data Service Events

        /// <summary>
        /// Called when Market Data Service is connected to MDE-Server
        /// </summary>
        public virtual void OnMarketDataServiceConnected()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Market data service successfully connected to MDE-Server.", _type.FullName,
                        "OnMarketDataServiceConnected");
                }

                //if (string.IsNullOrEmpty(MarketDataProviderName))
                //{
                //    return;
                //}

                //// Create TradeHub Login message for Live Market Data
                //Login login = new Login {MarketDataProvider = MarketDataProviderName};

                //// Send Login request
                //_marketDataService.Login(login);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnMarketDataServiceConnected");
            }
        }

        /// <summary>
        /// Called when Logon is received from Market Data Service
        /// </summary>
        /// <param name="marketDataProvider">Name of the market data provider</param>
        public virtual void OnMarketDataServiceLogonArrived(string marketDataProvider)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Logon received from: " + marketDataProvider, _type.FullName,
                        "OnMarketDataServiceLogonArrived");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnMarketDataServiceLogonArrived");
            }
        }

        /// <summary>
        /// Called when logout is received from Market Data Service
        /// </summary>
        /// <param name="marketDataProvider">Name of the market data provider</param>
        public virtual void OnMarketDataServiceLogoutArrived(string marketDataProvider)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Logout received from: " + marketDataProvider, _type.FullName,
                        "OnMarketDataServiceLogoutArrived");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnMarketDataServiceLogoutArrived");
            }
        }

        /// <summary>
        /// Called when new tick is received from Market Data Service
        /// </summary>
        /// <param name="tick">TradeHub Tick object containing latest tick info</param>
        public virtual void OnTickArrived(Tick tick)
        {
            try
            {
                //Stop Timer
                ConnectivityTimer.Stop();

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("New tick received : " + tick, _type.FullName, "Base.OnTickArrived");
                }

                //Start Timer
                ConnectivityTimer.Start();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnTickArrived");
            }
        }

        /// <summary>
        /// Called when new bar is received from Market Data Service
        /// </summary>
        /// <param name="bar">TradeHub Bar object containing latest bar info</param>
        public virtual void OnBarArrived(Bar bar)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("New bar received : " + bar, _type.FullName, "Base.OnBarArrived");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnBarArrived");
            }
        }

        #endregion

        #region Historical Data Servie Events

        /// <summary>
        /// Called when Historical Data Service is connected to MDE-Server
        /// </summary>
        public virtual void OnHistoricalDataServiceConnected()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Historical data service successfully connected to MDE-Server.", _type.FullName,
                        "OnHistoricalDataServiceConnected");
                }

                //if (string.IsNullOrEmpty(HistoricalDataProviderName))
                //{
                //    return;
                //}

                //// Create TradeHub Login message for Historica Data
                //Login login = new Login {MarketDataProvider = HistoricalDataProviderName};

                //// Send Login request
                //_historicalDataService.Login(login);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnHistoricalDataServiceConnected");
            }
        }

        /// <summary>
        /// Called when Logon is received from Historical Data Service
        /// </summary>
        /// <param name="marketDataProvider">Name of the market data provider</param>
        public virtual void OnHistoricalDataServiceLogonArrived(string marketDataProvider)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Logon received from : " + marketDataProvider, _type.FullName,
                        "OnHistoricalDataServiceLogonArrived");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnHistoricalDataServiceLogonArrived");
            }
        }

        /// <summary>
        /// Called when Logout is received from Historical Data Service
        /// </summary>
        /// <param name="marketDataProvider">Name of the market data provider</param>
        public virtual void OnHistoricalDataServiceLogoutArrived(string marketDataProvider)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Logout received from : " + marketDataProvider, _type.FullName,
                        "OnHistoricalDataServiceLogoutArrived");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnHistoricalDataServiceLogoutArrived");
            }
        }

        /// <summary>
        /// Called when historical bar data is received from Historical Data Service
        /// </summary>
        /// <param name="historicBarData">TradeHub HistoricalBarData object containing received historical bars info</param>
        public virtual void OnHistoricalDataArrived(HistoricBarData historicBarData)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Historical bar data received : " + historicBarData, _type.FullName,
                        "OnHistoricalDataArrived");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnHistoricalDataArrived");
            }
        }

        #endregion

        #region Order Execution Service Events

        /// <summary>
        /// Called when Order Execution Service is connected to OEE-Server
        /// </summary>
        public virtual void OnOrderExecutionServiceConnected()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Order Execution service successfully connected to OEE-Server.", _type.FullName,
                        "OnOrderExecutionServiceConnected");
                }

                //if (string.IsNullOrEmpty(OrderExecutionProviderName))
                //{
                //    return;
                //}

                //// Create TradeHub Login message for Order Execution Provider session
                //Login login = new Login {OrderExecutionProvider = OrderExecutionProviderName};

                //// Send Login request
                //_orderExecutionService.Login(login);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnOrderExecutionServiceConnected");
            }
        }

        /// <summary>
        /// Called when logon is received from Order Execution Service
        /// </summary>
        /// <param name="orderExecutionProvider">Name of the order execution provider</param>
        public virtual void OnOrderExecutionServiceLogonArrived(string orderExecutionProvider)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Logon received from : " + orderExecutionProvider, _type.FullName,
                        "OnOrderExecutionServiceLogonArrived");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnOrderExecutionServiceLogonArrived");
            }
        }

        /// <summary>
        /// Called when logout is received from Order Execution Service
        /// </summary>
        /// <param name="orderExecutionProvider">Name of the order execution provider</param>
        public virtual void OnOrderExecutionServiceLogoutArrived(string orderExecutionProvider)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Logout received from : " + orderExecutionProvider, _type.FullName,
                        "OnOrderExecutionServiceLogoutArrived");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnOrderExecutionServiceLogoutArrived");
            }
        }

        /// <summary>
        /// Called when New/Submitted order is received from Order Execution Service
        /// </summary>
        /// <param name="order">TradeHub Order containing info regarding accepted order</param>
        public void NewArrived(Order order)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Order status new received : " + order, _type.FullName, "NewArrived");
            }

            // Raise event to notify any available listeners
            if (_orderAcceptedEvent != null)
            {
                _orderAcceptedEvent(order);
            }

            // Call Virtual Function incase user has specified some additional functionality
            OnNewArrived(order);

            // Send a new notification message
            SendAcceptedNotification(order);
        }

        /// <summary>
        /// Can be overriden to provides additional funtionality for Function 'NewArrived(Order order)'
        /// </summary>
        /// <param name="order">TradeHub Order containing info regarding accepted order</param>
        public virtual void OnNewArrived(Order order)
        {
            // TODO: Provide Additional Functionality
        }

        /// <summary>
        /// Called when Order cancellation is received from Order Execution Service
        /// </summary>
        /// <param name="order">TradeHub Order containing the info regarding cancelled order</param>
        public void CancellationArrived(Order order)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Order cancellation received : " + order, _type.FullName, "CancellationArrived");
            }

            // Call Virtual Function incase user has specified some additional functionality
            OnCancellationArrived(order);

            // Raise event to notify any available listeners
            if (_cancellationArrivedEvent != null)
            {
                _cancellationArrivedEvent(order);
            }
        }

        /// <summary>
        /// Can be overriden to provides additional funtionality for Function 'CancellationArrived(Order order)'
        /// </summary>
        /// <param name="order">TradeHub Order containing the info regarding cancelled order</param>
        public virtual void OnCancellationArrived(Order order)
        {
            // TODO: Provide Additional Functionality
        }

        /// <summary>
        /// Called when Order execution is received from Order Execution Service
        /// </summary>
        /// <param name="execution">TradeHub Execution containing latest fill info</param>
        public void ExecutionArrived(Execution execution)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Order execution received : " + execution, _type.FullName, "ExecutionArrived");
            }

            // Call Virtual Function incase user has specified some additional functionality
            OnExecutionArrived(execution);

            // Raise event to notify any available listeners
            if (_onNewExecutionReceived != null)
            {
                _onNewExecutionReceived(execution);
            }

            // Send a new notification message
            SendExecutionNotification(execution);
        }

        /// <summary>
        /// Can be overriden to provides additional funtionality for Function 'ExecutionArrived(Execution execution)'
        /// </summary>
        /// <param name="execution">TradeHub Execution containing latest fill info</param>
        public virtual void OnExecutionArrived(Execution execution)
        {
            // TODO: Provide Additional Functionality
        }

        /// <summary>
        /// Called when Rejection is received from Order Execution Service
        /// </summary>
        /// <param name="rejection">TradeHub Rejection containing info/reason for message rejection by the server</param>
        public void RejectionArrived(Rejection rejection)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Rejection event received : " + rejection, _type.FullName, "RejectionArrived");
            }

            // Call Virtual Function incase user has specified some additional functionality
            OnRejectionArrived(rejection);

            // Raise event to notify any available listeners
            if (_rejectionArrivedEvent != null)
            {
                _rejectionArrivedEvent(rejection);
            }

            // Send a new notification message
            SendRejectionNotification(rejection);
        }

        /// <summary>
        /// Can be overriden to provides additional funtionality for Function 'RejectionArrived(Rejection rejection)'
        /// </summary>
        /// <param name="rejection">TradeHub Rejection containing info/reason for message rejection by the server</param>
        public virtual void OnRejectionArrived(Rejection rejection)
        {
            // TODO: Provide Additional Functionality
        }

        /// <summary>
        /// Called when Locate Message is received from Order Execution Service
        /// </summary>
        /// <param name="locateMessage">TradeHub LimitOrder containing LocateMessage details</param>
        public virtual void OnLocateMessageArrived(LimitOrder locateMessage)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Locate message received : " + locateMessage, _type.FullName, "OnLocateMessageArrived");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnLocateMessageArrived");
            }
        }

        #endregion

        /// <summary>
        /// Called market data provider info is received from Market/Historical Data Service
        /// </summary>
        /// <param name="marketDataProviderInfo">Contains request market data provider's available functionality info</param>
        public virtual void OnInquiryResponseArrived(MarketDataProviderInfo marketDataProviderInfo)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Market data provider info received : " + marketDataProviderInfo, _type.FullName,
                        "OnInquiryResponseArrived");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnInquiryResponseArrived");
            }
        }

        /// <summary>
        /// Raised on Connectivity Timer Elapse
        /// </summary>
        public virtual void OnConnectivityTimerElapses(object sender, ElapsedEventArgs e)
        {
            // Stop Timer
            ConnectivityTimer.Stop();

            // Strategy is no longer running
            IsRunning = false;
        }

        /// <summary>
        /// Clears orders map in OrderExecution Service
        /// </summary>
        public void ClearOrderMap()
        {
            _orderExecutionService.ClearOrderMap();
        }

        /// <summary>
        /// Returns New Unique ID to be used for Order
        /// </summary>
        /// <returns>Unique Order Id</returns>
        public string GetNewOrderId()
        {
            // Request Unique Order ID
            return _orderExecutionService.GetOrderId();
        }

        /// <summary>
        /// Saves data in a string collection to be used later.
        /// </summary>
        /// <param name="data">information to be saved</param>
        public void SaveLocalData(string data)
        {
            // Add incoming value to local Map
            _localData.Add(data);
        }

        /// <summary>
        /// Returns saved local data collection
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<string> GetLocalData()
        {
            // Return saved information
            return _localData;
        }

        /// <summary>
        /// Clears existing saved local data
        /// </summary>
        public void ClearLocalData()
        {
            // Clear all values from local map
            _localData.Clear();
        }

        /// <summary>
        /// The given message is displayed on the UI
        /// </summary>
        /// <param name="message"></param>
        public void DisplayMessage(string message)
        {
            // Raise event to notify listeners
            if (_displayMessageEvent != null)
            {
                _displayMessageEvent(message);
            }
        }

        #region Notifications

        /// <summary>
        /// Sets property to indicate that notification is to be sent on new order request
        /// </summary>
        /// <param name="sendNotification"></param>
        public void SetNewOrderNotification(bool sendNotification)
        {
            if (_notificationService != null)
            {
                _newOrderNotification = sendNotification;
            }
        }

        /// <summary>
        /// Sets property to indicate that notification is to be sent on order acceptance
        /// </summary>
        /// <param name="sendNotification"></param>
        public void SetAcceptedOrderNotification(bool sendNotification)
        {
            if (_notificationService != null)
            {
                _acceptedOrderNotification = sendNotification;
            }
        }

        /// <summary>
        /// Sets property to indicate that notification is to be sent on order execution
        /// </summary>
        /// <param name="sendNotification"></param>
        public void SetExecutionNotification(bool sendNotification)
        {
            if (_notificationService != null)
            {
                _executionNotification = sendNotification;
            }
        }

        /// <summary>
        /// Sets property to indicate that notification is to be sent on order rejection
        /// </summary>
        /// <param name="sendNotification"></param>
        public void SetRejectionNotification(bool sendNotification)
        {
            if (_notificationService != null)
            {
                _rejectionNotification = sendNotification;
            }
        }

        /// <summary>
        /// Sends notification when a new order is sent
        /// </summary>
        /// <param name="order"></param>
        /// <param name="price"></param>
        private void SendNewOrderNotification(Order order, decimal price)
        {
            // Send Notification
            if (_newOrderNotification)
            {
                Task.Factory.StartNew(() =>
                {
                    // Create new Notitication object for order
                    OrderNotification orderNotification = new OrderNotification(NotificationType.Email, OrderNotificationType.New);

                    // Set order properties
                    orderNotification.SetOrder(order);

                    // Set Limit price
                    if (!price.Equals(default(decimal)))
                    {
                        orderNotification.SetLimitPrice(price);
                    }

                    _notificationService.SendNotification(orderNotification);
                });
            }
        }

        /// <summary>
        /// Sends notification when a new order is being accepted by the exchange
        /// </summary>
        /// <param name="order"></param>
        private void SendAcceptedNotification(Order order)
        {
            // Send Notification
            if (_acceptedOrderNotification)
            {
                Task.Factory.StartNew(() =>
                {
                    // Create new Notitication object for order
                    OrderNotification orderNotification = new OrderNotification(NotificationType.Email, OrderNotificationType.Accepted);

                    // Set order properties
                    orderNotification.SetOrder(order);

                    _notificationService.SendNotification(orderNotification);
                });
            }
        }

        /// <summary>
        /// Sends notification when an order is executed by the exchange
        /// </summary>
        /// <param name="execution"></param>
        private void SendExecutionNotification(Execution execution)
        {
            // Send Notification
            if (_executionNotification)
            {
                Task.Factory.StartNew(() =>
                {
                    // Create new Notitication object for order
                    OrderNotification orderNotification = new OrderNotification(NotificationType.Email, OrderNotificationType.Executed);

                    // Set order properties
                    orderNotification.SetOrder(execution.Order);

                    // Set execution details
                    orderNotification.SetFill(execution.Fill);

                    _notificationService.SendNotification(orderNotification);
                });
            }
        }

        /// <summary>
        /// Sends notification when a new order is rejected by the exchange
        /// </summary>
        /// <param name="rejection"></param>
        private void SendRejectionNotification(Rejection rejection)
        {
            // Send Notification
            if (_rejectionNotification)
            {
                Task.Factory.StartNew(() =>
                {
                    // Create new Notitication object for order
                    OrderNotification orderNotification = new OrderNotification(NotificationType.Email, OrderNotificationType.Accepted);

                    // Set rejection properties
                    orderNotification.SetRejection(rejection);

                    _notificationService.SendNotification(orderNotification);
                });
            }
        }

        #endregion

        public virtual decimal GetObjectiveFunctionValue()
        {
            return 0;
        }
    }
}
