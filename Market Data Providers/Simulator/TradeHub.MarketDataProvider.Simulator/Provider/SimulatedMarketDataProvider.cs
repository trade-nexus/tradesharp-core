using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.MarketDataProvider;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataProvider.Simulator.Service;
using TradeHub.MarketDataProvider.Simulator.Utility;
using TradeHubConstants = TradeHub.Common.Core.Constants;

namespace TradeHub.MarketDataProvider.Simulator.Provider
{
    /// <summary>
    /// Provides Simulated Market Data Provider
    /// </summary>
    public class SimulatedMarketDataProvider : ILiveTickDataProvider, ILiveBarDataProvider
    {
        private Type _type = typeof (SimulatedMarketDataProvider);

        // Name of the Market Data Provider
        private readonly string _marketDataProviderName = TradeHubConstants.MarketDataProvider.Simulated;

        // Responsible for Transforming incoming message to TradeHub messages
        private readonly SimulatedDataProcessor _dataProcessor;

        // Shows the state of the simulator
        private bool _isConnected = false;

        /// <summary>
        /// Indicates whether the continues tick stream to be kept running or not
        /// </summary>
        private bool _sendTickStream = false;

        // List of Securities which have received susbcription request
        private List<Security> _subscribedSecurities = new List<Security>();

        /// <summary>
        /// Dictionary of Bar Ids which have received susbcription request
        /// Key =  ID (Generated from Symbol +  MarketDataProvider)
        /// Value = Request ID , Info (Generated from Symbol + BarFormat + BarPriceType + BarLength) 
        /// </summary>
        private Dictionary<string, Tuple<string, string>> _subscribedBars = new Dictionary<string, Tuple<string, string>>();

        private Thread _readerThread;
 
        /// <summary>
        /// Argument Message Processer
        /// </summary>
        /// <param name="dataProcessor">Simualated Data Processor</param>
        public SimulatedMarketDataProvider(SimulatedDataProcessor dataProcessor)
        {
            _dataProcessor = dataProcessor;

            // Hook Events
            RegisterDataProcessorEvents();

        }

        /// <summary>
        /// Hooks Simulated Data Processor Events
        /// </summary>
        private void RegisterDataProcessorEvents()
        {
            _dataProcessor.TickArrived += OnTickArrived;
            _dataProcessor.LiveBarArrived += OnLiveBarArrived;
        }

        #region Implementation of IMarketDataProvider

        // Will be raised in resonse to successful Logon Request
        public event Action<string> LogonArrived;
        // Will be raised in resonse to successful Logout Request
        public event Action<string> LogoutArrived;
        // Will be raised in resonse to Rejection for the requested Market Data
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
                    Logger.Info("Starting Simualtor", _type.FullName, "Start");
                }

                ConsoleWriter.WriteLine(ConsoleColor.Green, "Starting simulator");

                // Start Listening to input data
                _readerThread = new Thread(ReadInput);
                _readerThread.Start();

                _isConnected = true;

                // Raise Logon Event
                OnLogonArrived();
                
                return _isConnected;
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
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
                if (_isConnected)
                {
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Stopping Simualtor", _type.FullName, "Stop");
                    }
                    ConsoleWriter.WriteLine(ConsoleColor.Red, "Stopping simulator");

                    // Raise Logout Event
                    OnLogoutArrived();
                    
                    _isConnected = false;
                    _sendTickStream = false;
                }
                else
                {
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Simualtor already stopped", _type.FullName, "Stop");
                    }
                    ConsoleWriter.WriteLine(ConsoleColor.Red, "Simualtor already stopped");
                }
                return true;
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
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
            return _isConnected;
        }
        
        #endregion

        #region Implementation of ILiveTickDataProvider

        // Will be raised when a new Tick is Recieved
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
                    Logger.Info("Tick data susbcription request recieved for: " + request, _type.FullName,
                                "SubscribeTickData");
                }

                ConsoleWriter.WriteLine(ConsoleColor.Green, "Susbcribe request received for: " + request);

                if (!_subscribedSecurities.Contains(request.Security))
                {
                    // Add New Security to the List
                    _subscribedSecurities.Add(request.Security);
                }
                return true;
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
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
                    Logger.Info("Tick data unsusbcription request recieved for: " + request, _type.FullName,
                                "UnsubscribeTickData");
                }

                ConsoleWriter.WriteLine(ConsoleColor.Green, "Unsusbcribe request received for: " + request);

                // Remove Security from the List
                _subscribedSecurities.Remove(request.Security);

                return true;
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "UnsubscribeTickData");
                return false;
            }
        }
        
        #endregion

        #region Implementation of ILiveBarDataProvider

        // Raised when a new Bar is received
        public event Action<Bar, string> BarArrived;

        /// <summary>
        /// Request to get Bar Data
        /// </summary>
        /// <param name="barDataRequest">TradeHub Bar Data Request Message</param>
        /// <returns></returns>
        public bool SubscribeBars(BarDataRequest barDataRequest)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("New Bar Subscription request received: " + barDataRequest, _type.FullName, "SubscribeBars");
                }

                ConsoleWriter.WriteLine(ConsoleColor.Green, "New Bar Subscription request received: " + barDataRequest);

                string key = barDataRequest.Security.Symbol + barDataRequest.MarketDataProvider;
                string info = barDataRequest.Security.Symbol + barDataRequest.BarFormat + barDataRequest.BarPriceType +
                             barDataRequest.BarLength.ToString(CultureInfo.InvariantCulture);

                Tuple<string, string> value = new Tuple<string, string>(barDataRequest.Id, info);

                // Add Request to Internal Map
                _subscribedBars.Add(key, value);

                return true;
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "SubscribeBars");
                return false;
            }
        }

        /// <summary>
        /// Unsubscribe Bar data
        /// </summary>
        /// <param name="barDataRequest">TradeHub Bar Data Request Message</param>
        /// <returns></returns>
        public bool UnsubscribeBars(BarDataRequest barDataRequest)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("New Bar Unsubscription request received: " + barDataRequest, _type.FullName, "UnsubscribeBars");
                }

                ConsoleWriter.WriteLine(ConsoleColor.Green, "New Bar Unsubscription request received: " + barDataRequest);

                string key = barDataRequest.Security.Symbol + barDataRequest.MarketDataProvider;

                // Remove request from internal Map
                _subscribedBars.Remove(key);

                return true;
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "UnsubscribeBars");
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Start reading for input data
        /// </summary>
        private void ReadInput()
        {
            while (_isConnected)
            {
                {
                    ConsoleWriter.WriteLine(ConsoleColor.Green, "Enter Input");
                    string response = ConsoleWriter.Prompt();
                    if (response != null)
                    {
                        if (response.ToLower().Equals("exit"))
                        {
                            Stop();
                            break;
                        }

                        // Process incoming message
                        switch (response.Trim().ToLower())
                        {
                            case "help":
                                _dataProcessor.DisplayHelp();
                                break;
                            case "sub info tick":
                                _dataProcessor.DisplayTickSubscriptionInfo(_subscribedSecurities);
                                break;
                            case "sub info bar":
                                _dataProcessor.DisplayBarSubscriptionInfo(_subscribedBars);
                                break;
                            case "start":
                                StartTickDataStream();
                                break;
                            default:
                                _dataProcessor.ProcessIncomingMessage(response);
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Raised when Logon is received from the Gateway
        /// </summary>
        private void OnLogonArrived()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Logon Arrived for Market Data Simulator", _type.FullName, "OnLogonArrived");
                }

                // Raise Logon Event
                if (LogonArrived != null)
                {
                    LogonArrived(_marketDataProviderName);
                }
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "OnLogonArrived");
            }
        }

        /// <summary>
        /// Raised when Logout is received from the Gateway
        /// </summary>
        private void OnLogoutArrived()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Logout Arrived for Market Data Simulator", _type.FullName, "OnLogoutArrived");
                }

                // Raise Logout Event
                if (LogoutArrived != null)
                {
                    LogoutArrived(_marketDataProviderName);
                }
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "OnLogoutArrived");
            }
        }

        /// <summary>
        /// Raised when a new Tick Arrives from the Gateway
        /// </summary>
        /// <param name="tick">TradeHub Tick Message</param>
        private void OnTickArrived(Tick tick)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Tick Arrived for Market Data Simulator: " + tick, _type.FullName, "OnTickArrived");
                }
               
                // Raise Tick Event
                if (TickArrived != null)
                {
                    //for (int i = 0; i < 10; i++)
                    //{
                    //    tick.DateTime = DateTime.Now;
                        TickArrived(tick);
                    //}
                }
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "OnTickArrived");
            }
        }

        /// <summary>
        /// Raised when a new Bar Arrives from the Gateway
        /// </summary>
        /// <param name="bar">TradeHub Bar Message</param>
        private void OnLiveBarArrived(Bar bar)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Bar Arrived for Market Data Simulator: " + bar, _type.FullName, "OnLiveBarArrived");
                }

                // Raise Bar Event
                if (BarArrived != null)
                {
                    string key = bar.Security.Symbol + bar.MarketDataProvider;
                    Tuple<string, string> subscribedBar = _subscribedBars[key];
                    BarArrived(bar, subscribedBar.Item1);
                }
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "OnLiveBarArrived");
            }
        }

        /// <summary>
        /// Continuously sends tick data for requested symbols 
        /// </summary>
        private void StartTickDataStream()
        {
            if (!(_subscribedSecurities.Count > 0))
            {
                ConsoleWriter.WriteLine(ConsoleColor.Blue, "No symbol is subscribed for TICK data");
                ConsoleWriter.WriteLine(ConsoleColor.Blue, "");
                return;
            }

            _sendTickStream = true;

            while (_sendTickStream)
            {
                Tick tick;

                int bidStartValue = 110;
                int bidEndValue = 140;

                int askStartValue = 142;
                int askEndValue = 172;

                foreach (Security security in _subscribedSecurities.ToList())
                {
                    tick = new Tick()
                    {
                        AskSize = 10,
                        BidSize = 9,
                        LastPrice = 141,
                        LastSize = 22,
                        MarketDataProvider = Common.Core.Constants.MarketDataProvider.Simulated,
                        DateTime = DateTime.Now
                    };

                    // set security
                    tick.Security = security;

                    Random random = new Random();

                    // Generate BID price
                    var bid = random.Next(bidStartValue, bidEndValue);
                    // Generate ASK price
                    var ask = random.Next(askStartValue, askEndValue);
                    // Generate Depth level
                    var depth = random.Next(0, 7);

                    tick.BidPrice = bid;
                    tick.AskPrice = ask;
                    tick.Depth = depth;

                    // Send data to client
                    OnTickArrived(tick);
                }
            }
        }
    }
}
