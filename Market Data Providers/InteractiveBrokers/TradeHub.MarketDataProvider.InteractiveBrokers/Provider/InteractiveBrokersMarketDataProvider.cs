using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Krs.Ats.IBNet;
using Krs.Ats.IBNet.Contracts;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.MarketDataProvider;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataProvider.InteractiveBrokers.ValueObjects;
using Constants = TradeHub.Common.Core.Constants;

namespace TradeHub.MarketDataProvider.InteractiveBrokers.Provider
{
    public class InteractiveBrokersMarketDataProvider : ILiveTickDataProvider
    {
        private Type _type = typeof (InteractiveBrokersMarketDataProvider);
        private readonly string _marketDataProviderName = Constants.MarketDataProvider.InteractiveBrokers;

        private object _lock = new object();

        private IBClient _ibClient;
        private readonly ConnectionParameters _parameters;

        private Tick _ask;
        private Tick _bid;

        private int _nextValidId = 0;

        private readonly Dictionary<int, Tick> _tickList = new Dictionary<int, Tick>();
        private readonly Dictionary<string, int> _idsMap = new Dictionary<string, int>(); 

        // Field to indicate User Logout request
        private bool _logoutRequest = false;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        public InteractiveBrokersMarketDataProvider(IBClient ibClient, ConnectionParameters parameters)
        {
            _ibClient = ibClient;
            _parameters = parameters;

            _ask = new Tick {MarketDataProvider = _marketDataProviderName};
            _bid = new Tick {MarketDataProvider = _marketDataProviderName};
        }

        #region Implementation of IMarketDataProvider

        public event Action<string> LogonArrived;
        public event Action<string> LogoutArrived;
        public event Action<MarketDataEvent> MarketDataRejectionArrived;

        /// <summary>
        /// Connects/Starts a client
        /// </summary>
        public bool Start()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Sending connection call for IB.", _type.FullName, "Start");
                }
                if (_ibClient==null)
                {
                    _ibClient = new IBClient();
                }

                // Toggle Field Value
                _logoutRequest = false;

                // Hook Gateway Events
                RegisterGatewayEvents();

                _ibClient.Connect(_parameters.Host, _parameters.Port, _parameters.ClientId);

                Task.Factory.StartNew(
                    () =>
                        {
                            while (!IsConnected()){}
                            
                            // Raise Logon Event
                            if (LogonArrived != null)
                            {
                                LogonArrived(_marketDataProviderName);
                            }
                        });

                // Indicating calls were successfully sent
                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "Start");
                return false;
            }
        }

        /// <summary>
        /// Disconnects/Stops a client
        /// </summary>
        public bool Stop()
        {
            try
            {
                if (_ibClient != null)
                {
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Sending disconnect call for IB.", _type.FullName, "Stop");
                    }
                    // Toggle Field Value
                    _logoutRequest = true;

                    foreach (var ids in _idsMap.Values)
                    {
                        // Send Unsubscription Map
                        _ibClient.CancelMarketData(ids);
                    }

                    _ibClient = null;

                    // Raised Logout Event
                    if (LogoutArrived!=null)
                    {
                        LogoutArrived(_marketDataProviderName);
                    }
                }
                else
                {
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Connection no longer exists.", _type.FullName, "Stop");
                    }
                }

                // Indicating calls were successfully sent
                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "Stop");
                return false;
            }
        }

        /// <summary>
        /// Is Market Data client connected
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            try
            {
                if (_ibClient != null)
                {
                    // Check whether the Market Data session is connected or not
                    if (_ibClient.Connected)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "IsConnected");
                return false;
            }
        }

        #endregion

        #region Implementation of ILiveTickDataProvider

        public event Action<Tick> TickArrived;

        /// <summary>
        /// Market data request message
        /// </summary>
        public bool SubscribeTickData(Subscribe request)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Sending market data request for: " + request.Security.Symbol, _type.FullName,
                                "SubscribeTickData");
                }

                _nextValidId++;

                // Update IDs Map
                _idsMap.Add(request.Id, _nextValidId);

                // Add Value to the Internal Ticks Map
                _tickList.Add(_nextValidId, new Tick { Security = request.Security ,MarketDataProvider = _marketDataProviderName});

                var list = new Collection<GenericTickType> {GenericTickType.MarkPrice};

                _ibClient.RequestMarketData(Convert.ToInt32(_nextValidId), new Equity(request.Security.Symbol), list, snapshot: false, marketDataOff: false);
                _ibClient.RequestMarketDepth(Convert.ToInt32(_nextValidId), new Equity(request.Security.Symbol), 1);

                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeTickData");
                return false;
            }
        }

        /// <summary>
        /// Unsubscribe Market data message
        /// </summary>
        public bool UnsubscribeTickData(Unsubscribe request)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Unsubscribing market data request for: " + request.Security.Symbol, _type.FullName,
                                "UnsubscribeTickData");
                }

                // Send Unsubscription Map
                _ibClient.CancelMarketData(_idsMap[request.Id]);
                _ibClient.CancelMarketDepth(_idsMap[request.Id]);
                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "UnsubscribeTickData");
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Hooks InteractiveBrokers Events
        /// </summary>
        private void RegisterGatewayEvents()
        {
            UnregisterGatewayEvents();

            _ibClient.UpdateMarketDepth += OnUpdateMarketDepth;
            _ibClient.Error += OnErrorReceived;
            _ibClient.TickPrice += OnTickPrice;
            _ibClient.TickSize += OnTickSize;
            _ibClient.ConnectionClosed += OnConnectionClosed;
        }

        /// <summary>
        /// Unhooks InteractiveBrokers Events
        /// </summary>
        private void UnregisterGatewayEvents()
        {
            _ibClient.UpdateMarketDepth -= OnUpdateMarketDepth;
            _ibClient.Error -= OnErrorReceived;
            _ibClient.TickPrice -= OnTickPrice;
            _ibClient.TickSize -= OnTickSize;
            _ibClient.ConnectionClosed -= OnConnectionClosed;
        }

        /// <summary>
        /// Raised when New Quote Prices are received
        /// </summary>
        private void OnUpdateMarketDepth(object sender, UpdateMarketDepthEventArgs eventArgs)
        {
            try
            {
                lock (_lock)
                {
                    MarketDepthSide type = eventArgs.Side;
                    if (type.Equals(MarketDepthSide.Ask))
                    {
                        Tick tick = _tickList[eventArgs.TickerId];
                        _ask.Security.Symbol = tick.Security.Symbol;
                        _ask.AskSize = eventArgs.Size;
                        _ask.AskPrice = eventArgs.Price;
                        _ask.DateTime = DateTime.Now;
                        if (TickArrived != null)
                        {
                            TickArrived(_ask);
                        }
                    }

                    else if (type.Equals(MarketDepthSide.Bid))
                    {
                        Tick tick = _tickList[eventArgs.TickerId];
                        _bid.Security.Symbol = tick.Security.Symbol;
                        _bid.BidSize = eventArgs.Size;
                        _bid.BidPrice = eventArgs.Price;
                        _bid.DateTime = DateTime.Now;
                        if (TickArrived != null)
                        {
                            TickArrived(_bid);
                        }
                    }
                }
            }
            catch(Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnUpdateMarketDepth");
            }
        }

        /// <summary>
        /// Event raised when new Trade Size is receieved
        /// </summary>
        private void OnTickSize(object sender, TickSizeEventArgs eventArgs)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("New Trade Size receieved: " + eventArgs.Size, _type.FullName, "OnTickSize");
                }

                lock (_lock)
                {
                    TickType type = eventArgs.TickType;
                    if (type.Equals(TickType.LastSize))
                    {
                        Tick tick = _tickList[eventArgs.TickerId];
                        tick.LastSize = (decimal)eventArgs.Size;
                        tick.DateTime = DateTime.Now;
                        // Raise Event
                        if (TickArrived != null)
                        {
                            TickArrived(tick);
                        }
                    }
                }
            }
            catch(Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnTickSize");
            }
        }

        /// <summary>
        /// Event raised when new Trade Price is received
        /// </summary>
        private void OnTickPrice(object sender, TickPriceEventArgs eventArgs)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("New Trade Price receieved: " + eventArgs.Price, _type.FullName, "OnTickSize");
                }

                lock (_lock)
                {
                    TickType type = eventArgs.TickType;
                    if (type.Equals(TickType.LastPrice))
                    {
                        Tick tick = _tickList[eventArgs.TickerId];
                        tick.LastPrice = eventArgs.Price;
                    }
                }
            }
            catch(Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnTickSize");
            }
        }

        /// <summary>
        /// Raised when Error Message is Received from Gateway
        /// </summary>
        private void OnErrorReceived(object sender, ErrorEventArgs eventArgs)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Error message receievd from Gateway: " + eventArgs.ErrorMsg, _type.FullName, "OnErrorReceived");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnErrorReceived");
            }
        }

        /// <summary>
        /// Rasied when Gateway Connection is closed
        /// </summary>
        private void OnConnectionClosed(object sender, ConnectionClosedEventArgs eventArgs)
        {
            try
            {
                if (LogoutArrived != null)
                {
                    LogonArrived(_marketDataProviderName);
                }

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Connection closed", _type.FullName, "OnConnectionClosed");
                }

                // Unhook Gateway Events
                UnregisterGatewayEvents();

                // Attempt Logon if the Logout was not requested by the user
                if (!_logoutRequest)
                {
                    // Send Logon request
                    Start();
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnConnectionClosed");
            }
        }

    }
}
