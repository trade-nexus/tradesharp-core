using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.Inquiry;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataEngine.Client.Service;

namespace TradeHub.StrategyEngine.HistoricalData
{
    /// <summary>
    /// Responsible for handling all historical data related requests
    /// </summary>
    public class HistoricalDataService
    {
        private Type _type = typeof(HistoricalDataService);

        private AsyncClassLogger _asyncClassLogger;

        #region Events

        // ReSharper disable InconsistentNaming
        private event Action _connected;
        private event Action _disconnected;
        private event Action<string> _logonArrived;
        private event Action<string> _logoutArrived;
        private event Action<HistoricBarData> _historicalDataArrived;
        private event Action<MarketDataProviderInfo> _inquiryResponseArrived;
        // ReSharper restore InconsistentNaming

        public event Action Connected
        {
            add
            {
                if (_connected == null)
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

        public event Action<HistoricBarData> HistoricalDataArrived
        {
            add
            {
                if (_historicalDataArrived == null)
                {
                    _historicalDataArrived += value;
                }
            }
            remove { _historicalDataArrived -= value; }
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

        /// <summary>
        /// Provides connection with MDE
        /// </summary>
        private MarketDataEngineClient _dataEngineClient;

        private bool _isConnected = false;
        
        /// <summary>
        /// Keeps track of all the subscription requests
        /// Key = Unique ID
        /// Value = Historical Bar data request
        /// </summary>
        private ConcurrentDictionary<string, HistoricDataRequest> _subscriptionRequests; 

        /// <summary>
        /// Indicates whether the market data service is connected to MDE-Server or not
        /// </summary>
        public bool IsConnected
        {
            get { return _isConnected; }
            set { _isConnected = value; }
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="marketDataEngineClient">MDE-Client for communication with the MDE-Server</param>
        public HistoricalDataService(MarketDataEngineClient marketDataEngineClient)
        {
            _asyncClassLogger = new AsyncClassLogger("HistoricalDataService");
            // Set logging level
            _asyncClassLogger.SetLoggingLevel();
            //set logging path
            _asyncClassLogger.LogDirectory(DirectoryStructure.CLIENT_LOGS_LOCATION);

            // Initialize objects
            _subscriptionRequests = new ConcurrentDictionary<string, HistoricDataRequest>();

            // Copy reference
            _dataEngineClient = marketDataEngineClient;

            // Register required MDE-Client Events
            RegisterDataEngineClientEvents();
        }

        /// <summary>
        /// Initializes necessary components for communication - Needs to be called if the Service was stopped
        /// </summary>
        public void InitializeService()
        {
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
                        _asyncClassLogger.Info("Historical data service already running.", _type.FullName, "StartService");
                    }

                    return true;
                }

                // Start MDE-Client
                _dataEngineClient.Start();

                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Historical data service started.", _type.FullName, "StartService");
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
                // Stop MDE-Client
                _dataEngineClient.Shutdown();

                // Clear local map
                _subscriptionRequests.Clear();

                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Historical data service stopped.", _type.FullName, "StopService");
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
        /// Registers required MDE-Client events
        /// </summary>
        private void RegisterDataEngineClientEvents()
        {
            _dataEngineClient.ServerConnected += OnServerConnected;
            _dataEngineClient.ServerDisconnected += OnServerDisconnected;
            _dataEngineClient.LogonArrived += OnLogonArrived;
            _dataEngineClient.LogoutArrived += OnLogoutArrived;
            _dataEngineClient.HistoricBarsArrived += OnHistoricBarsArrived;
            _dataEngineClient.InquiryResponseArrived += OnInquiryResponseArrived;
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
            _dataEngineClient.HistoricBarsArrived -= OnHistoricBarsArrived;
            _dataEngineClient.InquiryResponseArrived -= OnInquiryResponseArrived;
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
        /// Called when MDE-Client receives new historic bar data message from MDE-Server
        /// </summary>
        private void OnHistoricBarsArrived(HistoricBarData historicBarData)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Data received from: " + historicBarData.MarketDataProvider, _type.FullName, "OnHistoricBarsArrived");
                }

                HistoricDataRequest barsInformation;
                if (_subscriptionRequests.TryGetValue(historicBarData.ReqId, out barsInformation))
                {
                    // Insert received bars information
                    historicBarData.BarsInformation = barsInformation;
                }

                if (_historicalDataArrived != null)
                {
                    _historicalDataArrived(historicBarData);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnHistoricBarsArrived");
            }
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

        #region Incoming requests from MDE-Server

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
        /// Sends Historic Bar data subscription request to MDP through MDE-Server
        /// </summary>
        public bool Subscribe(HistoricDataRequest historicDataRequest)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("New subscription request received: " + historicDataRequest, _type.FullName, "Subscribe");
                }

                // Check if MDE-Client is connected to MDE
                if (_isConnected)
                {
                    if (_subscriptionRequests.ContainsKey(historicDataRequest.Id))
                    {
                        if (_asyncClassLogger.IsInfoEnabled)
                        {
                            _asyncClassLogger.Info("Subscription ID already used." + historicDataRequest, _type.FullName, "Subscribe");
                        }

                        return false;
                    }

                    // Save request for future use
                    _subscriptionRequests.AddOrUpdate(historicDataRequest.Id, historicDataRequest, (key, value) => historicDataRequest);

                    // Send subscription request to MDE-Server
                    _dataEngineClient.SendHistoricBarDataRequest(historicDataRequest);

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
        
        #endregion
    }
}
