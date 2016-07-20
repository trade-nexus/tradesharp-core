using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Disruptor;
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.HistoricalDataProvider.Service;
using TradeHub.Common.HistoricalDataProvider.Utility;
using TradeHub.Common.HistoricalDataProvider.ValueObjects;
using TradeHub.StrategyEngine.TradeHub;
using TradeHub.StrategyEngine.Utlility.Services;

namespace TradeHub.Optimization.Genetic.Tests.Application.Utility
{
    /// <summary>
    /// Responsible for executing the given TradeHub Strategy Instance
    /// </summary>
    public class TestStrategyExecutor
    {
        private AsyncClassLogger _asyncClassLogger;

        private TradeHubStrategy _tradeHubStrategy;

        private Type _strategyType;
        private object[] _ctorArguments;

        private ManualResetEvent _manualReset;

        /// <summary>
        /// Responsible for providing order executions in backtesting
        /// </summary>
        private OrderExecutor _orderExecutor;

        /// <summary>
        /// Manages order requests from strategy in backtesting
        /// </summary>
        private OrderRequestListener _orderRequestListener;

        ///// <summary>
        ///// Manages market data for backtesting strategy
        ///// </summary>
        //private MarketDataListener _marketDataListener;

        /// <summary>
        /// Manages market data requests from strategy in backtesting
        /// </summary>
        private MarketRequestListener _marketRequestListener;

        /// <summary>
        /// Responsible for providing requested data
        /// </summary>
        private DataHandler _dataHandler;

        /// <summary>
        /// Save Strategy Execution Statistics
        /// </summary>
        private Statistics _statistics;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="strategyType"></param>
        /// <param name="ctorArguments"></param>
        public TestStrategyExecutor(Type strategyType, object[] ctorArguments)
        {
            // Initialize parameters
            //TestStrategyInitializeParameters(strategyType,ctorArguments);

            _manualReset = new ManualResetEvent(false);
            _asyncClassLogger = new AsyncClassLogger("TestStrategyExecutor");

            // Save Strategy Type
            _strategyType = strategyType;

            //Save Arguments
            _ctorArguments = ctorArguments;

            // Set Logging levels
            _asyncClassLogger.SetLoggingLevel();

            // Get new strategy instance
            var strategyInstance = StrategyHelper.CreateStrategyInstance(_strategyType, _ctorArguments);

            if (strategyInstance != null)
            {
                // Cast to TradeHubStrategy Instance
                TradeHubStrategy = strategyInstance as TradeHubStrategy;

                InitializeStrategyListeners();
                OverrideMarketRequestCalls();
                OverrideOrderRequestCalls();
            }
        }

        public TradeHubStrategy TradeHubStrategy
        {
            get { return _tradeHubStrategy; }
            set { _tradeHubStrategy = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strategyType"></param>
        /// <param name="ctorArguments"></param>
        public void TestStrategyInitializeParameters(Type strategyType, object[] ctorArguments)
        {
            _manualReset = new ManualResetEvent(false);
            _asyncClassLogger = new AsyncClassLogger("TestStrategyExecutor");

            // Save Strategy Type
            _strategyType = strategyType;

            //Save Arguments
            _ctorArguments = ctorArguments;

            // Set Logging levels
            _asyncClassLogger.SetLoggingLevel();

            // Get new strategy instance
            var strategyInstance = StrategyHelper.CreateStrategyInstance(_strategyType, _ctorArguments);

            if (strategyInstance != null)
            {
                // Cast to TradeHubStrategy Instance
                TradeHubStrategy = strategyInstance as TradeHubStrategy;

                InitializeStrategyListeners();
                OverrideMarketRequestCalls();
                OverrideOrderRequestCalls();
            }
        }

        /// <summary>
        /// Sets required strategy parameters
        /// </summary>
        private void SetStrategyParameters(double alpha, double beta, double gamma, double epsilon)
        {
            _ctorArguments[1] = (decimal) alpha;
            _ctorArguments[5] = (float) gamma;
            _ctorArguments[6] = (decimal) epsilon;
            _ctorArguments[14] = (float) beta;
        }

        /// <summary>
        /// Initializes and registers required parametes and delagates
        /// </summary>
        private void InitializeStrategyListeners()
        {
            // Register TradeHUB Strategy's status (Running/Stopped) Event
            TradeHubStrategy.OnStrategyStatusChanged += OnStrategyStatusChanged;

            // Initialize statistics
            _statistics = new Statistics("A00");

            // Initialze Utility Classes
            _orderExecutor = new OrderExecutor(_asyncClassLogger);
            //_marketDataListener = new MarketDataListener(_asyncClassLogger);
            _orderRequestListener = new OrderRequestListener(_orderExecutor, _asyncClassLogger);

            // Use MarketDataListener.cs as Event Handler to get data from DataHandler.cs
            //_dataHandler = new DataHandler(new IEventHandler<MarketDataObject>[] { _marketDataListener });
            _dataHandler = new DataHandler(true); // Set local persistance to true

            //_marketDataListener.BarSubscriptionList = _dataHandler.BarSubscriptionList;
            //_marketDataListener.TickSubscriptionList = _dataHandler.TickSubscriptionList;

            // Initialize MarketRequestListener.cs to manage incoming market data requests from strategy
            _marketRequestListener = new MarketRequestListener(_dataHandler, _asyncClassLogger);

            // Register Events to receive data
            RegisterMarketDataListenerEvents();
            // Register Events to receive order executions
            RegisterOrderExecutorEvents();
        }

        /// <summary>
        /// Executes single iteration of the given strategy instance
        /// </summary>
        public double ExecuteStrategy(double alpha, double beta, double gemma, double epsilon)
        {
            //SetStrategyParameters(alpha, beta, gemma, epsilon);

            _manualReset = new ManualResetEvent(false);

            // Update Strategy Parameters
            TradeHubStrategy.SetParameters(new object[] { alpha, beta, gemma, epsilon });

            // Reset Statistics for current iteration
            _statistics.ResetValues();
            _orderExecutor.Clear();

            // Start Strategy Execution
            TradeHubStrategy.Run();

            // Wait for the strategy to execute
            _manualReset.WaitOne();

            // Clear subscription maps
            _dataHandler.ClearMaps();

            ////Dispose Object
            //_tradeHubStrategy.Dispose();

            //// Force garbage collector to free memory
            //GC.Collect();
            //GC.WaitForPendingFinalizers();

            // return current iterations PnL
            return (double) _statistics.Pnl;
        }

        /// <summary>
        /// Stop Strategy and disposes the object
        /// </summary>
        public void StopStrategy()
        {
            if (TradeHubStrategy != null)
            {
                TradeHubStrategy.Dispose();
            }
        }

        #region Helper Functions

        /// <summary>
        /// Overriders TradeHUB Strategy's Market Data request calls to entertain them locally
        /// </summary>
        private void OverrideMarketRequestCalls()
        {
            //Override Market Data Requests
            TradeHubStrategy.OverrideBarSubscriptionRequest(_marketRequestListener.SubscribeLiveBars);
            TradeHubStrategy.OverriderBarUnsubscriptionRequest(_marketRequestListener.UnsubcribeLiveBars);
        }

        /// <summary>
        /// Overrides TradeHUB Strategy's Order request calls to entertain them locally
        /// </summary>
        private void OverrideOrderRequestCalls()
        {
            // Override Order Requests
            TradeHubStrategy.OverrideMarketOrderRequest(_orderRequestListener.NewMarketOrderRequest);
            TradeHubStrategy.OverrideLimitOrderRequest(_orderRequestListener.NewLimitOrderRequest);
            TradeHubStrategy.OverrideCancelOrderRequest(_orderRequestListener.NewCancelOrderRequest);
            //_tradeHubStrategy.OverrideOrderRequest(_orderRequestListener.NewOrderRequest);
        }

        /// <summary>
        /// Subscribes order events from <see cref="OrderExecutor"/>
        /// </summary>
        private void RegisterOrderExecutorEvents()
        {
            _orderExecutor.NewOrderArrived += OnOrderExecutorNewArrived;
            _orderExecutor.ExecutionArrived += OnOrderExecutorExecutionArrived;
            _orderExecutor.CancellationArrived += OnOrderExecutorCancellationArrived;
        }

        /// <summary>
        /// Subscribes Tick and Bars events from <see cref="MarketDataListener"/>
        /// </summary>
        private void RegisterMarketDataListenerEvents()
        {
            //_marketDataListener.TickArrived += OnTickArrived;
            //_marketDataListener.BarArrived += OnBarArrived;
            _dataHandler.TickReceived += OnTickArrived;
            _dataHandler.BarReceived += OnBarArrived;
        }

        /// <summary>
        /// Called when Cancellation received from <see cref="OrderExecutor"/>
        /// </summary>
        /// <param name="order"></param>
        private void OnOrderExecutorCancellationArrived(Order order)
        {
            TradeHubStrategy.OnCancellationArrived(order);
        }

        /// <summary>
        /// Called when Executions received from <see cref="OrderExecutor"/>
        /// </summary>
        /// <param name="execution"></param>
        private void OnOrderExecutorExecutionArrived(Execution execution)
        {
            // Send Execution to strategy
            TradeHubStrategy.OnExecutionArrived(execution);

            // Update Strategy Statistics
            UpdateStatistics(execution);
        }

        /// <summary>
        /// Called when New order status received from <see cref="OrderExecutor"/>
        /// </summary>
        /// <param name="order"></param>
        private void OnOrderExecutorNewArrived(Order order)
        {
            TradeHubStrategy.OnNewArrived(order);
        }

        /// <summary>
        /// Called when bar received from <see cref="MarketDataListener"/>
        /// </summary>
        /// <param name="bar"></param>
        private void OnBarArrived(Bar bar)
        {
            _orderExecutor.BarArrived(bar);
            TradeHubStrategy.OnBarArrived(bar);
        }

        /// <summary>
        /// Called when tick received from <see cref="MarketDataListener"/>
        /// </summary>
        /// <param name="tick"></param>
        private void OnTickArrived(Tick tick)
        {
            _orderExecutor.TickArrived(tick);
            TradeHubStrategy.OnTickArrived(tick);
        }

        /// <summary>
        /// Called when startegy status changes
        /// </summary>
        /// <param name="status">Indicates strategy status</param>
        private void OnStrategyStatusChanged(bool status)
        {
            if (!status)
            {
                if (TradeHubStrategy.IsRunning == false)
                    _manualReset.Set();
            }
        }

        /// <summary>
        /// Updates strategy statistics on each execution
        /// </summary>
        /// <param name="execution">Contains Execution Info</param>
        private void UpdateStatistics(Execution execution)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Updating statistics on: " + execution, "IntegrationTest", "UpdateStatistics");
                }

                // Update statistics on BUY Order
                if (execution.Fill.ExecutionSide.Equals(OrderSide.BUY))
                {
                    // Update Avg Buy Price
                    _statistics.AvgBuyPrice = ((execution.Fill.ExecutionPrice * execution.Fill.ExecutionSize)
                                               + (_statistics.AvgBuyPrice * _statistics.SharesBought))
                                              / (_statistics.SharesBought + execution.Fill.ExecutionSize);

                    // Update Size
                    _statistics.SharesBought += execution.Fill.ExecutionSize;
                }
                // Update statistics on SELL Order
                else if (execution.Fill.ExecutionSide.Equals(OrderSide.SELL) ||
                    execution.Fill.ExecutionSide.Equals(OrderSide.SHORT))
                {
                    // Update Avg Sell Price
                    _statistics.AvgSellPrice = ((execution.Fill.ExecutionPrice * execution.Fill.ExecutionSize)
                                                + (_statistics.AvgSellPrice * _statistics.SharesSold))
                                               / (_statistics.SharesSold + execution.Fill.ExecutionSize);

                    // Update Size
                    _statistics.SharesSold += execution.Fill.ExecutionSize;
                }
                // Update statistics on COVER Order (order used to close the open position)
                else if (execution.Fill.ExecutionSide.Equals(OrderSide.COVER))
                {
                    if (_statistics.Position.Contains("Long"))
                    {
                        // Update Avg Sell Price
                        _statistics.AvgSellPrice = ((execution.Fill.ExecutionPrice * execution.Fill.ExecutionSize)
                                                    + (_statistics.AvgSellPrice * _statistics.SharesSold))
                                                   / (_statistics.SharesSold + execution.Fill.ExecutionSize);

                        // Update Size
                        _statistics.SharesSold += execution.Fill.ExecutionSize;
                    }
                    else if (_statistics.Position.Contains("Short"))
                    {
                        // Update Avg Buy Price
                        _statistics.AvgBuyPrice = ((execution.Fill.ExecutionPrice * execution.Fill.ExecutionSize)
                                                   + (_statistics.AvgBuyPrice * _statistics.SharesBought))
                                                  / (_statistics.SharesBought + execution.Fill.ExecutionSize);

                        // Update Size
                        _statistics.SharesBought += execution.Fill.ExecutionSize;
                    }
                }

                // Update Profit and Loss
                _statistics.Pnl = (_statistics.AvgSellPrice * _statistics.SharesSold) -
                                  (_statistics.AvgBuyPrice * _statistics.SharesBought);

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Statistics updated: " + _statistics, "IntegrationTest", "UpdateStatistics");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "IntegrationTest", "UpdateStatistics");
            }
        }

        #endregion
    }

    public class Statistics
    {
        /// <summary>
        /// Strategy ID for which the stats are calculated
        /// </summary>
        private string _id;

        /// <summary>
        /// Strategy Profit/Loss
        /// </summary>
        private decimal _pnl;

        /// <summary>
        /// Total number of shares bought 
        /// </summary>
        private int _sharesBought;

        /// <summary>
        /// Total number of shares sold
        /// </summary>
        private int _sharesSold;

        /// <summary>
        /// Average Buy Price 
        /// </summary>
        private decimal _avgBuyPrice;

        /// <summary>
        /// Average Sell Price 
        /// </summary>
        private decimal _avgSellPrice;

        private decimal _pos = 0;
        private bool _flag = false;

        public bool Flag
        {
            get { return _flag; }
            set { _flag = value; }
        }

        /// <summary>
        /// Current Position
        /// </summary>
        public decimal Pos
        {
            get { return _pos; }
            set { _pos = value; }
        }

        #region Properties

        /// <summary>
        /// Strategy ID for which the stats are calculated
        /// </summary>
        public string ID
        {
            get { return _id; }
            set { _id = value; }
        }
        List<string> _pnlList = new List<string>();

        /// <summary>
        /// Gets Over all strategy position (Long/Short)
        /// </summary>
        public string Position
        {
            get
            {
                int temp = _sharesBought + (-_sharesSold);
                if (temp > 0)
                {
                    return "Long " + temp;
                }
                else if (temp < 0)
                {
                    return "Short " + Math.Abs(temp);
                }
                return "NONE";
            }
        }

        /// <summary>
        /// Strategy Profit/Loss
        /// </summary>
        public decimal Pnl
        {
            get { return _pnl; }
            set { _pnl = value; }
        }

        /// <summary>
        /// Total number of shares bought 
        /// </summary>
        public int SharesBought
        {
            get { return _sharesBought; }
            set { _sharesBought = value; }
        }

        /// <summary>
        /// Total number of shares sold
        /// </summary>
        public int SharesSold
        {
            get { return _sharesSold; }
            set { _sharesSold = value; }
        }

        /// <summary>
        /// Average Buy Price 
        /// </summary>
        public decimal AvgBuyPrice
        {
            get { return _avgBuyPrice; }
            set { _avgBuyPrice = value; }
        }

        /// <summary>
        /// Average Sell Price 
        /// </summary>
        public decimal AvgSellPrice
        {
            get { return _avgSellPrice; }
            set { _avgSellPrice = value; }
        }

        #endregion

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="id">Strategy id for which to calculate the statistics</param>
        public Statistics(string id)
        {
            _id = id;
            _pnl = default(decimal);
            _sharesBought = default(int);
            _sharesSold = default(int);
            _avgBuyPrice = default(decimal);
            _avgSellPrice = default(decimal);
        }

        //[MethodImpl(MethodImplOptions.Synchronized)]
        public void UpdatePnl(decimal pnl)
        {
            _pnlList.Add(pnl.ToString());
            var perpnl = pnl * 15;
            if (perpnl < 0)
            {
                _negCount++;
                _sigmaTop += perpnl * perpnl;
            }
            else if (perpnl > 0)
            {
                _posCount++;
                _sigmaBottom += perpnl * perpnl;
            }
            _pnlSum += perpnl;
        }

        /// <summary>
        /// Resets fields to default values
        /// </summary>
        public void ResetValues()
        {
            _pnl = default(decimal);
            _sharesBought = default(int);
            _sharesSold = default(int);
            _avgBuyPrice = default(decimal);
            _avgSellPrice = default(decimal);
        }
        /// <summary>
        /// Update the PNL after receicing the execution
        /// </summary>
        /// <param name="execution"></param>
        public void UpdateCalulcationsOnExecutionMatlab(Execution execution)
        {
            // Update statistics on BUY Order
            if (execution.Fill.ExecutionSide.Equals(OrderSide.BUY))
            {
                //// Update Avg Buy Price
                //AvgBuyPrice = ((execution.Fill.ExecutionPrice * execution.Fill.ExecutionSize)
                //                           + (AvgBuyPrice * SharesBought))
                //                          / (SharesBought + execution.Fill.ExecutionSize);

                //// Update Size
                //SharesBought += execution.Fill.ExecutionSize;
                //Pnl = execution.Fill.ExecutionPrice - execution.BarClose;
            }
            // Update statistics on SELL Order
            else if (execution.Fill.ExecutionSide.Equals(OrderSide.SELL) ||
                execution.Fill.ExecutionSide.Equals(OrderSide.SHORT))
            {
                //// Update Avg Sell Price
                //AvgSellPrice = ((execution.Fill.ExecutionPrice * execution.Fill.ExecutionSize)
                //                            + (AvgSellPrice * SharesSold))
                //                           / (SharesSold + execution.Fill.ExecutionSize);

                //// Update Size
                //SharesSold += execution.Fill.ExecutionSize;
                //Pnl = execution.BarClose-execution.Fill.ExecutionPrice;
            }
            // Update statistics on COVER Order (order used to close the open position)
            else if (execution.Fill.ExecutionSide.Equals(OrderSide.COVER))
            {
                if (Position.Contains("Long"))
                {
                    //// Update Avg Sell Price
                    //AvgSellPrice = ((execution.Fill.ExecutionPrice * execution.Fill.ExecutionSize)
                    //                            + (AvgSellPrice * SharesSold))
                    //                           / (SharesSold + execution.Fill.ExecutionSize);

                    //// Update Size
                    //SharesSold += execution.Fill.ExecutionSize;
                    //Pnl = execution.Fill.ExecutionPrice - execution.BarClose;
                }
                else if (Position.Contains("Short"))
                {
                    //// Update Avg Buy Price
                    //AvgBuyPrice = ((execution.Fill.ExecutionPrice * execution.Fill.ExecutionSize)
                    //                           + (AvgBuyPrice * SharesBought))
                    //                          / (SharesBought + execution.Fill.ExecutionSize);

                    //// Update Size
                    //SharesBought += execution.Fill.ExecutionSize;
                    //Pnl = execution.BarClose - execution.Fill.ExecutionPrice;
                }
            }
            Pnl = Pnl * 15;

            // Update Profit and Loss
            //Pnl = (AvgSellPrice * SharesSold) -
            //                  (AvgBuyPrice * SharesBought);
            UpdateCalcualtions();
        }
        /// <summary>
        /// Update the PNL after receicing the execution
        /// </summary>
        /// <param name="execution"></param>
        public void UpdateCalulcationsOnExecution(Execution execution)
        {
            // Update statistics on BUY Order
            if (execution.Fill.ExecutionSide.Equals(OrderSide.BUY))
            {
                // Update Avg Buy Price
                AvgBuyPrice = ((execution.Fill.ExecutionPrice * execution.Fill.ExecutionSize)
                                           + (AvgBuyPrice * SharesBought))
                                          / (SharesBought + execution.Fill.ExecutionSize);

                // Update Size
                SharesBought += execution.Fill.ExecutionSize;
            }
            // Update statistics on SELL Order
            else if (execution.Fill.ExecutionSide.Equals(OrderSide.SELL) ||
                execution.Fill.ExecutionSide.Equals(OrderSide.SHORT))
            {
                // Update Avg Sell Price
                AvgSellPrice = ((execution.Fill.ExecutionPrice * execution.Fill.ExecutionSize)
                                            + (AvgSellPrice * SharesSold))
                                           / (SharesSold + execution.Fill.ExecutionSize);

                // Update Size
                SharesSold += execution.Fill.ExecutionSize;
            }
            // Update statistics on COVER Order (order used to close the open position)
            else if (execution.Fill.ExecutionSide.Equals(OrderSide.COVER))
            {
                if (Position.Contains("Long"))
                {
                    // Update Avg Sell Price
                    AvgSellPrice = ((execution.Fill.ExecutionPrice * execution.Fill.ExecutionSize)
                                                + (AvgSellPrice * SharesSold))
                                               / (SharesSold + execution.Fill.ExecutionSize);

                    // Update Size
                    SharesSold += execution.Fill.ExecutionSize;
                }
                else if (Position.Contains("Short"))
                {
                    // Update Avg Buy Price
                    AvgBuyPrice = ((execution.Fill.ExecutionPrice * execution.Fill.ExecutionSize)
                                               + (AvgBuyPrice * SharesBought))
                                              / (SharesBought + execution.Fill.ExecutionSize);

                    // Update Size
                    SharesBought += execution.Fill.ExecutionSize;
                }
            }

            // Update Profit and Loss
            Pnl = (AvgSellPrice * SharesSold) -
                              (AvgBuyPrice * SharesBought);
            UpdateCalcualtions();
        }

        #region New Enhancements in Utility Funtion

        private decimal _sigmaTop = 0;
        private decimal _sigmaBottom = 0;
        private decimal _pnlSum = 0;
        private decimal _risk = 0.5m;
        private int _negCount = 0;
        private int _posCount = 0;
        private Bar _currentBar;
        private Bar _prevBar;

        public void UpdateBar(Bar bar)
        {
            //Flag = false;
            _prevBar = _currentBar;
            _currentBar = bar;
        }

        public void CalculatePnlAfterBar()
        {
            Flag = false;
            if (!Flag)
            {
                decimal perpnl = 0;
                if (Pos > 0)
                {
                    perpnl = (_currentBar.Close - _prevBar.Close) * Pos;
                    UpdatePnl(perpnl);
                }
                else if (Pos < 0)
                {
                    perpnl = (_currentBar.Close - _prevBar.Close) * Pos;
                    UpdatePnl(perpnl);
                }
            }
        }
        //[MethodImpl(MethodImplOptions.Synchronized)]
        public void MatlabStatisticsFunction(Execution execution)
        {
            decimal perpnl = 0;
            if (Pos == 0)
            {
                if (execution.Fill.ExecutionSide.Equals(Common.Core.Constants.OrderSide.SELL) ||
                    execution.Fill.ExecutionSide.Equals(Common.Core.Constants.OrderSide.SHORT))
                {
                    Pos = -1;
                    perpnl = (execution.Fill.ExecutionPrice - _currentBar.Close) * -1 * Pos;
                    Flag = true;
                }
                else if (execution.Fill.ExecutionSide.Equals(Common.Core.Constants.OrderSide.BUY))
                {
                    Pos = 1;
                    perpnl = (_currentBar.Close - execution.Fill.ExecutionPrice) * Pos;
                    Flag = true;
                }
            }
            if (!Flag)
            {
                if (Pos < 0)
                {
                    if (execution.Order.Remarks.Contains("PT-3"))
                    {
                        perpnl = (_prevBar.Close - execution.Fill.ExecutionPrice) * -1 * Pos;
                        Pos = 0;
                    }
                    else if (execution.Order.Remarks.Contains("PT"))
                    {
                        Pos += 0.33m;
                        var gl = _prevBar.Close - execution.Fill.ExecutionPrice;
                        perpnl = gl - (_currentBar.Close - _prevBar.Close) * -1 * Pos;
                    }
                    else
                    {
                        perpnl = (_prevBar.Close - execution.Fill.ExecutionPrice) * -1 * Pos;
                        Pos = 0;
                    }
                }
                else if (Pos > 0)
                {
                    if (execution.Order.Remarks.Contains("PT-3"))
                    {
                        perpnl = (execution.Fill.ExecutionPrice - _prevBar.Close) * Pos;
                        Pos = 0;
                    }
                    else if (execution.Order.Remarks.Contains("PT"))
                    {
                        Pos -= 0.33m;
                        var gl = execution.Fill.ExecutionPrice - _prevBar.Close;
                        perpnl = gl + (_currentBar.Close - _prevBar.Close) * Pos;
                    }
                    else
                    {
                        perpnl = (execution.Fill.ExecutionPrice - _prevBar.Close) * Pos;
                        Pos = 0;
                    }
                }
            }
            UpdatePnl(perpnl);
        }
        /// <summary>
        /// Update calcualtions
        /// </summary>
        public void UpdateCalcualtions()
        {
            //Check if the position becomes flat
            if (Position.Contains("NONE"))
            {
                if (Pnl < 0)
                {
                    _negCount++;
                    _sigmaTop += _pnl * _pnl;
                }
                else if (Pnl > 0)
                {
                    _posCount++;
                    _sigmaBottom += _pnl * _pnl;
                }
                _pnlSum += Pnl;
                //reset values as the position is zero now.
                ResetValues();
            }

        }

        /// <summary>
        /// Get Risk after the iteration
        /// </summary>
        /// <returns></returns>
        public decimal GetRisk()
        {
            decimal risk = 0;
            decimal sigma = 0;
            //decimal temp = 0;
            if (_sigmaTop != 0 && _sigmaBottom != 0)
            {
                sigma = _sigmaTop / _sigmaBottom;
            }
            //temp = _pnlSum/(_negCount + _posCount);
            //Logger.Error("Stats, Sigma Top="+_sigmaTop+", Sigma Bottom="+_sigmaBottom+", Sigma="+sigma+", Pnl Sum="+_pnlSum,"Statistics","GetRisk");
            risk = +1 * (((1 - _risk) * _pnlSum) - _risk * sigma);
            _pnlList.Add("R_Hat=" + _pnlSum);
            _pnlList.Add("utility=" + risk);
            File.WriteAllLines("pnlList.txt", _pnlList);
            return risk;
        }

        /// <summary>
        /// Reset all and new values
        /// </summary>
        public void ResetAllValues()
        {
            ResetValues();
            _sigmaTop = 0;
            _sigmaBottom = 0;
            _pnlSum = 0;
            _risk = 0.5m;
            _negCount = 0;
            _posCount = 0;
            _pos = 0;
            _flag = false;
        }

        #endregion

        /// <summary>
        /// ToString override for Statistics.cs
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("Statistics :: ");
            stringBuilder.Append("ID: " + _id);
            stringBuilder.Append(" | Position: " + Position);
            stringBuilder.Append(" | PNL: " + _pnl);
            stringBuilder.Append(" | Bought: " + _sharesBought);
            stringBuilder.Append(" | Sold: " + _sharesSold);
            stringBuilder.Append(" | Avg Buy Price: " + _avgBuyPrice);
            stringBuilder.Append(" | Avg Sell Price: " + _avgSellPrice);

            return stringBuilder.ToString();
        }
    }
}
