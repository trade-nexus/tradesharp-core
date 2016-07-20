using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TraceSourceLogger;
using TradeHub.Common.Core.CustomAttributes;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.FactoryMethods;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.StrategyEngine.Testing.SimpleStrategy.Utility;
using TradeHub.StrategyEngine.TradeHub;
using TradeHubConstants = TradeHub.Common.Core.Constants;

namespace TradeHub.StrategyEngine.Testing.SimpleStrategy.EMA
{
    /// <summary>
    /// Implements Basic 2EMA strategy using TradeHubStrategy as base skeleton
    /// </summary>
    [TradeHubAttributes("2EMA", typeof(EmaStrategy))]
    public class EmaStrategy : TradeHubStrategy
    {
        private readonly Type _type = typeof(EmaStrategy);

        private readonly string _emaPriceType;
        private readonly string _symbol;
        private readonly string _barFormat;
        private readonly string _barPriceType;
        private readonly string _marketDataProvider;
        private readonly string _orderExecutionProvider;

        private decimal _barLength;

        private int _shortEma;
        private int _longEma;
        private int _currentEntryState;
        private int _previousEntryState;
        private int _liveBarId = 0xA00;
        private int _tickDataId = 0xA00;
        private int _hitoricalDataId = 0xA00;
        private int _orderId = 0xA00;
        private int _count = 0;

        private bool _entryOrderSent;

        private readonly Indicator.EMA _emaCalculator;
        private BarDataRequest _barSubscribeRequest;

        [TradeHubAttributes("Lenght of the bar to be used", typeof(decimal), 0)]
        public decimal BarLength
        {
            get { return _barLength; }
            set { _barLength = value; }
        }

        [TradeHubAttributes("Short EMA value", typeof(int), 1)]
        public int ShortEma
        {
            get { return _shortEma; }
            set { _shortEma = value; }
        }

        [TradeHubAttributes("Long EMA value", typeof(int), 2)]
        public int LongEma
        {
            get { return _longEma; }
            set { _longEma = value; }
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="shortEma">Value for short EMA</param>
        /// <param name="longEma">Value for Long EMA</param>
        /// <param name="emaPriceType">Price type to be used for EMA Calculations</param>
        /// <param name="symbol">Symbol on which to run the strategy</param>
        /// <param name="barLength">Length of the bar to be used for EMA</param>
        /// <param name="barFormat">Bar generator formate to be used</param>
        /// <param name="barPriceType">Price type to be used for creating bars</param>
        /// <param name="marketDataProvider">Name of market data provider to be used</param>
        /// <param name="orderExecutionProvider">Name of order execution provider to be used</param>
        public EmaStrategy(int shortEma, int longEma, string emaPriceType, string symbol, float barLength,
                                                string barFormat, string barPriceType, string marketDataProvider, string orderExecutionProvider)
            : base(marketDataProvider, orderExecutionProvider, "")
        {
            _shortEma = shortEma;
            _longEma = longEma;
            _emaPriceType = emaPriceType;
            _symbol = symbol;
            _barLength = (decimal)barLength;
            _barFormat = barFormat;
            _barPriceType = barPriceType;
            _marketDataProvider = marketDataProvider;
            _orderExecutionProvider = orderExecutionProvider;
            
            _emaCalculator = new Indicator.EMA(_shortEma, _longEma, _emaPriceType);
        }

        /// <summary>
        /// Starts Strategy Execution
        /// </summary>
        protected override void OnRun()
        { 
            // Indidicates strategy is running
            if (IsRunning)
            {
                // Send subscription request for live bars
                SubscribeLiveBars();

                // Send subscription request for tick data
                SubscribeTickData();
            }
        }

        /// <summary>
        /// Stops Strategy Execution
        /// </summary>
        protected override void OnStop()
        {
            if (IsRunning)
            {
                // Send unsubscription request for tick data
                UnsubscribeTickData();

                // Send unsubscription request for live bars
                UnsubscribeLiveBars();
            }
        }

        /// <summary>
        /// Sends live bar subscription request to Market Data Service using the base class
        /// </summary>
        public void SubscribeLiveBars()
        {
            try
            {
                // Get new ID to be used
                var id = (_liveBarId++).ToString("X");

                // Get new Security object
                Security security = new Security {Symbol = _symbol};

                // Get Bar subscription message
                _barSubscribeRequest = SubscriptionMessage.LiveBarSubscription(id, security, _barFormat, _barPriceType,
                                                                        BarLength, 0.0001M, 0, _marketDataProvider);

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Sending live bar subscription request for: " + _barSubscribeRequest, _type.FullName,
                                "SubscribeLiveBars");
                }

                // Send subscription request
                Subscribe(_barSubscribeRequest);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeLiveBars");
            }
        }

        /// <summary>
        /// Sends live bar unsubscription request to Market Data Service using the base class
        /// </summary>
        public void UnsubscribeLiveBars()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Sending live bar unsubscription request for: " + _barSubscribeRequest, _type.FullName,
                                "UnsubscribeLiveBars");
                }

                // Send unsubscription request
                Unsubscribe(_barSubscribeRequest);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "UnsubscribeLiveBars");
            }
        }

        /// <summary>
        /// Sends tick data subscription request to Market Data Service using the base class
        /// </summary>
        public void SubscribeTickData()
        {
            try
            {
                // Get new ID to be used
                var id = (_tickDataId++).ToString("X");

                // Get new Security object
                Security security = new Security {Symbol = _symbol};

                // Get Tick subscription message
                var subscribe = SubscriptionMessage.TickSubscription(id, security, _marketDataProvider);

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Sending tick data subscription request for: " + subscribe, _type.FullName,
                                "SubscribeTickData");
                }

                // Send subscription request
                this.Subscribe(subscribe);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeTickData");
            }
        }

        /// <summary>
        /// Sends tick data unsubscription request to Market Data Service using the base class
        /// </summary>
        public void UnsubscribeTickData()
        {
            try
            {
                // Get new ID to be used
                var id = (_tickDataId++).ToString("X");

                // Get new Security object
                Security security = new Security {Symbol = _symbol};

                // Get Tick subscription message
                var subscribe = SubscriptionMessage.TickUnsubscription(id, security, _marketDataProvider);

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Sending tick data unsubscription request for: " + subscribe, _type.FullName,
                                "UnsubscribeTickData");
                }

                // Send unsubscription request
                Unsubscribe(subscribe);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "UnsubscribeTickData");
            }
        }

        /// <summary>
        /// Sends historical bar data request to Historical Data Service using the base class
        /// </summary>
        public void SubscribeHistoricalData()
        {
            try
            {
                // Get new ID to be used
                var id = (_hitoricalDataId++).ToString("X");

                // Create TradeHub subscription Message
                HistoricDataRequest historicBarData = new HistoricDataRequest()
                {
                    Security = new Security() { Symbol = _symbol },
                    BarType = TradeHubConstants.BarType.DAILY,
                    StartTime = DateTime.ParseExact("20130825", "yyyyMMdd", CultureInfo.InvariantCulture),
                    EndTime = DateTime.ParseExact("20130827", "yyyyMMdd", CultureInfo.InvariantCulture),
                    Interval = 10,
                    Id = id,
                    MarketDataProvider = _marketDataProvider
                };

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Sending historical data request for: " + historicBarData, _type.FullName,
                                "SubscribeHistoricalData");
                }

                // Send subscription request
                Subscribe(historicBarData);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeHistoricalData");
            }
        }

        /// <summary>
        /// Initiates Trading by sending out Entry Order
        /// </summary>
        /// <param name="state"> </param>
        public void InitiateTrade(object state)
        {
            try
            {
                lock (state)
                {
                    Bar bar = state as Bar;

                    if (bar == null) return;

                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Initiating Trade Algo", _type.FullName, "InitiateTrade");
                    }

                    decimal[] ema = _emaCalculator.GetEMA(bar);

                    if (ema[0] > 0 && ema[1] > 0)
                    {
                        ConsoleWriter.WriteLine(ConsoleColor.Green,
                                                "New EMA calulated: Long = " + ema[0] + " | Short = " + ema[1]);

                        if (Logger.IsDebugEnabled)
                        {
                            Logger.Debug("New EMA calulated: Long = " + ema[0] + " | Short = " + ema[1], _type.FullName,
                                         "InitiateTrade");
                        }

                        //Update Entry States using latest EMA
                        ManageEntryStates(ema[0], ema[1]);

                        // Check conditions if Entry Order is yet to be sent
                        if (!_entryOrderSent)
                        {
                            // Check for ENTRY Condition
                            var orderSide = EntrySignal();
                            if (Logger.IsDebugEnabled)
                            {
                                Logger.Debug("New entry signal generated: " + orderSide, _type.FullName, "InitiateTrade");
                            }

                            ConsoleWriter.WriteLine(ConsoleColor.Green, "New entry signal generated: " + orderSide);

                            // Send Entry Order
                            SendEntryOrder(orderSide, bar.Security.Symbol);
                        }

                        // Update previous state value
                        _previousEntryState = _currentEntryState;
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "InitiateTrade");
            }
        }

        /// <summary>
        /// Manage Entry States
        /// </summary>
        /// <param name="currentLongEMA">newly calculated Long EMA</param>
        /// <param name="currentShortEMA">newly calculated Short EMA</param>
        private void ManageEntryStates(decimal currentLongEMA, decimal currentShortEMA)
        {
            try
            {
                if (currentShortEMA > currentLongEMA)
                {
                    _currentEntryState = 1;
                }
                else if (currentShortEMA < currentLongEMA)
                {
                    _currentEntryState = -1;
                }

                ConsoleWriter.WriteLine(ConsoleColor.Green,
                                                "Previous Entry State: " + _previousEntryState + " | Current Entry State: " + _currentEntryState);

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Previous Entry State: " + _previousEntryState + " | Current Entry State: " + _currentEntryState,
                                    _type.FullName, "ManageEntryStates");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ManageEntryStates");
            }
        }

        /// <summary>
        /// Checks the entry states and  generates Entry signal if required
        /// </summary>
        public string EntrySignal()
        {
            try
            {
                string orderSide = TradeHubConstants.OrderSide.NONE;

                    if (_previousEntryState == -1 && _currentEntryState == 1)
                    {
                        orderSide = TradeHubConstants.OrderSide.BUY;
                    }
                    else if (_previousEntryState == 1 && _currentEntryState == -1)
                    {
                        orderSide = TradeHubConstants.OrderSide.SELL;
                    }

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Order Side : " + orderSide, _type.FullName, "EntrySignal");
                }
                return orderSide;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "EntrySignal");
                return TradeHubConstants.OrderSide.NONE;
            }
        }

        /// <summary>
        /// Sends enrty order to the Order Execution Service
        /// </summary>
        /// <param name="orderSide">Order side on which to open position</param>
        /// <param name="symbol">Symbol on which to send order</param>
        private void SendEntryOrder(string orderSide, string symbol)
        {
            try
            {
                if (!orderSide.Equals(TradeHubConstants.OrderSide.NONE))
                {
                    _entryOrderSent = true;

                    ConsoleWriter.WriteLine(ConsoleColor.Green, "Sending" + orderSide + " entry order.");

                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Sending" + orderSide + " entry order.", _type.FullName, "SendEntryOrder");
                    }

                    // Get new unique ID
                    var id = (_orderId++).ToString("X");

                    Security security= new Security{Symbol = symbol};

                    //// Create new Limit Order
                    //LimitOrder limitOrder = OrderMessage.GenerateLimitOrder(id, security, TradeHubConstants.OrderSide.BUY, 100,
                    //                                                        1.24M, _orderExecutionProvider);

                    // Create Market Order
                    MarketOrder marketOrder = OrderMessage.GenerateMarketOrder(id, security, orderSide, 10,
                        _orderExecutionProvider);

                    // Send Market Order to OEE
                    SendOrder(marketOrder);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendEntryOrder");
            }
        }

        /// <summary>
        /// Sends market order to the Order Execution Service
        /// </summary>
        /// <param name="orderSide">Order side on which to open position</param>
        /// <param name="symbol">Symbol on which to send order</param>
        /// <param name="dateTime"> </param>
        private void SendMarketOrder(string orderSide, string symbol, DateTime dateTime)
        {
            try
            {
                if (!orderSide.Equals(TradeHubConstants.OrderSide.NONE))
                {
                    _entryOrderSent = true;

                    ConsoleWriter.WriteLine(ConsoleColor.Green, "Sending" + orderSide + " market order.");

                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Sending" + orderSide + " market order.", _type.FullName, "SendMarketOrder");
                    }

                    // Get new unique ID
                    var id = (_orderId++).ToString("X");

                    Security security = new Security { Symbol = symbol };

                    // Create new Limit Order
                    MarketOrder marketOrder = OrderMessage.GenerateMarketOrder(id, security,
                                                                               TradeHubConstants.OrderSide.BUY, 100
                                                                               , _orderExecutionProvider);
                    marketOrder.OrderDateTime = dateTime;

                    // Send Limit Order to OEE
                    SendOrder(marketOrder);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendMarketOrder");
            }
        }

        /// <summary>
        /// Sends market order to the Order Execution Service
        /// </summary>
        /// <param name="orderSide">Order side on which to open position</param>
        /// <param name="symbol">Symbol on which to send order</param>
        /// <param name="price"> </param>
        /// <param name="dateTime"> </param>
        private void SendLimitOrder(string orderSide, string symbol,decimal price ,DateTime dateTime)
        {
            try
            {
                if (!orderSide.Equals(TradeHubConstants.OrderSide.NONE))
                {
                    _entryOrderSent = true;

                    ConsoleWriter.WriteLine(ConsoleColor.Green, "Sending" + orderSide + " limit order.");

                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Sending" + orderSide + " entry order.", _type.FullName, "SendLimtOrder");
                    }

                    // Get new unique ID
                    var id = (_orderId++).ToString("X");

                    Security security = new Security { Symbol = symbol };

                    // Create new Limit Order
                    LimitOrder limitOrder = OrderMessage.GenerateLimitOrder(id, security,
                                                                            TradeHubConstants.OrderSide.BUY, 100, price
                                                                            , _orderExecutionProvider);
                    limitOrder.OrderDateTime = dateTime;

                    // Send Limit Order to OEE
                    SendOrder(limitOrder);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendLimtOrder");
            }
        }

        /// <summary>
        /// Sends enrty order to the Order Execution Service
        /// </summary>
        /// <param name="orderSide">Order side on which to open position</param>
        /// <param name="symbol">Symbol on which to send order</param>
        private void SendBlackwoodEntryOrder(string orderSide, string symbol)
        {
            try
            {
                if (!orderSide.Equals(TradeHubConstants.OrderSide.NONE))
                {
                    _entryOrderSent = true;

                    ConsoleWriter.WriteLine(ConsoleColor.Green, "Sending" + orderSide + " entry order.");

                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Sending" + orderSide + " entry order.", _type.FullName, "SendEntryOrder");
                    }

                    // Get new unique ID
                    var id = (_orderId++).ToString("X");

                    Security security = new Security { Symbol = symbol };

                    // Create new Limit Order
                    MarketOrder marketOrder = OrderMessage.GenerateMarketOrder(id, security, TradeHubConstants.OrderSide.SHORT, 100, _orderExecutionProvider);

                    // Set OPG Venue
                    marketOrder.Exchange = "SDOT";
                    // Set Order TIME_OUT
                    marketOrder.OrderTif = TradeHubConstants.OrderTif.DAY;

                    // Send Market Order to OEE
                    SendOrder(marketOrder);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendEntryOrder");
            }
        }
        
        /// <summary>
        /// Called when Logon is received from live market data service 
        /// </summary>
        /// <param name="marketDataProvider">Name of the market data provider</param>
        public override void OnMarketDataServiceLogonArrived(string marketDataProvider)
        {
            try
            {
                ConsoleWriter.WriteLine(ConsoleColor.Green, "Market Data logon received from: " + marketDataProvider);

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Market Data logon received from: " + marketDataProvider, _type.FullName, "OnMarketDataServiceLogonArrived");
                }

                // NOTE: RUN should always be called from an external call
                // Run();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnMarketDataServiceLogonArrived");
            }
        }

        /// <summary>
        /// Called when Logon is received from live market data service 
        /// </summary>
        /// <param name="historicalDataProvider">Name of the market data provider</param>
        public override void OnHistoricalDataServiceLogonArrived(string historicalDataProvider)
        {
            try
            {
                ConsoleWriter.WriteLine(ConsoleColor.Green, "Market Data logon received from: " + historicalDataProvider);

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Historical Data logon received from: " + historicalDataProvider, _type.FullName, "OnHistoricalDataServiceLogonArrived");
                }

                //SubscribeHistoricalData();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnMarketDataServiceLogonArrived");
            }
        }

        /// <summary>
        /// Called when Logon is received from order data service
        /// </summary>
        /// <param name="orderExecutionProvider">Name of the order execution provider</param>
        public override void OnOrderExecutionServiceLogonArrived(string orderExecutionProvider)
        {
            try
            {
                ConsoleWriter.WriteLine(ConsoleColor.Green, "Order execution logon received from: " + orderExecutionProvider);

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Order execution logon received from: " + orderExecutionProvider, _type.FullName, "OnOrderExecutionServiceLogonArrived");
                }

                // SendEntryOrder(TradeHubConstants.OrderSide.BUY, _symbol[0]);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnOrderExecutionServiceLogonArrived");
            }
        }

        /// <summary>
        /// Called when New Bar is received
        /// </summary>
        /// <param name="bar">TradeHub Bar containing latest info</param>
        public override void OnBarArrived(Bar bar)
        {
            try
            {
                //ConsoleWriter.WriteLine(ConsoleColor.Green, "New bar received : " + bar);

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("New bar received : " + bar, _type.FullName, "OnBarArrived");
                }

                InitiateTrade(bar);

                //Task.Factory.StartNew(() =>
                //    {
                //        InitiateTrade(bar); },TaskCreationOptions.PreferFairness);

                //UnsubscribeLiveBars();

                //ThreadPool.QueueUserWorkItem(new WaitCallback(InitiateTrade),bar);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnBarArrived");
            }
        }

        /// <summary>
        /// Called when new Tick is received
        /// </summary>
        /// <param name="tick">TradeHub Tick containing lastest info</param>
        public override void OnTickArrived(Tick tick)
        {
            try
            {
                base.OnTickArrived(tick);

                //ConsoleWriter.WriteLine(ConsoleColor.Green, "New tick received : " + tick);

                //if (Logger.IsDebugEnabled)
                //{
                //    Logger.Debug("New tick received : " + tick, _type.FullName, "OnTickArrived");
                //}

                //if (_count == 20)
                //{
                //    SendMarketOrder(TradeHubConstants.OrderSide.BUY, "MSFT", tick.DateTime);
                //}

                //else if (_count == 70)
                //{
                //    SendLimitOrder(TradeHubConstants.OrderSide.BUY, "MSFT", tick.LastPrice, tick.DateTime);
                //}

                //_count++;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnTickArrived");
            }
        }

        /// <summary>
        /// Called when Historical data is received
        /// </summary>
        /// <param name="historicBarData">Contains requested historical bars</param>
        public override void OnHistoricalDataArrived(HistoricBarData historicBarData)
        {
            ConsoleWriter.WriteLine(ConsoleColor.Green, "Historical data received : " + historicBarData);
            //ConsoleWriter.WriteLine(ConsoleColor.Green, "Sleepng for 50 ms");

            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Historical data received : " + historicBarData, _type.FullName, "OnTickArrived");
            }
        }

        /// <summary>
        /// Called when order is accepted by the exchange
        /// </summary>
        /// <param name="order">TradeHub Order</param>
        public override void OnNewArrived(Order order)
        {
            try
            {
                ConsoleWriter.WriteLine(ConsoleColor.Green, "Order accepted: " + order);

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Order accepted: " + order, _type.FullName, "OnNewArrived");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnNewArrived");
            }
        }

        /// <summary>
        /// Called when Order Execution is received
        /// </summary>
        /// <param name="execution">TradeHub Execution</param>
        public override void OnExecutionArrived(Execution execution)
        {
            try
            {
                ConsoleWriter.WriteLine(ConsoleColor.Green, "Order Execution received : " + execution);

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Order Execution received : " + execution, _type.FullName, "OnExecutionArrived");
                }

                // Resart trading algo on full fill
                if (execution.Fill.LeavesQuantity.Equals(0))
                {
                    if (_entryOrderSent)
                    {
                        _entryOrderSent = false;

                        // Get new unique ID
                        var id = (_orderId++).ToString("X");

                        Security security = new Security { Symbol = execution.Order.Security.Symbol };

                        var orderSide = execution.Fill.ExecutionSide.Equals(TradeHubConstants.OrderSide.BUY)
                            ? TradeHubConstants.OrderSide.SELL
                            : TradeHubConstants.OrderSide.BUY;

                        // Create Market Order
                        MarketOrder marketOrder = OrderMessage.GenerateMarketOrder(id, security, orderSide, 10,
                            _orderExecutionProvider);

                        // Send Market Order
                        SendOrder(marketOrder);
                    }  
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnExecutionArrived");
            }
        }

        /// <summary>
        /// Called when LocateMessage is received from OEE
        /// </summary>
        /// <param name="locateMessage">TradeHub LimitOrder containing details for the LocateMessage</param>
        public override void OnLocateMessageArrived(LimitOrder locateMessage)
        {
            try
            {
                ConsoleWriter.WriteLine(ConsoleColor.Green, "Locate messaeg received : " + locateMessage);

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Locate messaeg received : " + locateMessage, _type.FullName, "OnLocateMessageArrived");
                }

                // Create LocateRespone object
                LocateResponse locateResponse = new LocateResponse(locateMessage.OrderID, _orderExecutionProvider, true);

                // Sends Locate Response to OEP 
                SendLocateResponse(locateResponse);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnLocateMessageArrived");
            }
        }
    }
}
