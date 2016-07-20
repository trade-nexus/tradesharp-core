using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Disruptor;
using Spring.Context.Support;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.Repositories;
using TradeHub.Common.Core.ValueObjects;
using TradeHub.Common.HistoricalDataProvider.Service;
using TradeHub.Common.HistoricalDataProvider.Utility;
using TradeHub.Common.Persistence;
using TradeHub.StrategyEngine.TradeHub;
using TradeHub.StrategyRunner.Infrastructure.Entities;
using TradeHub.StrategyRunner.Infrastructure.Service;
using TradeHub.StrategyRunner.Infrastructure.ValueObjects;
using TradeHubConstants = TradeHub.Common.Core.Constants;

namespace TradeHub.StrategyRunner.ApplicationController.Service
{
    /// <summary>
    /// Responsibe for handling individual strategy instances
    /// </summary>
    public class StrategyExecutor
    {
        private Type _type = typeof (StrategyExecutor);
        private AsyncClassLogger _asyncClassLogger;

        /// <summary>
        /// Responsible for providing order executions in backtesting
        /// </summary>
        private IOrderExecutor _orderExecutor;
        
        /// <summary>
        /// Manages order requests from strategy in backtesting
        /// </summary>
        private OrderRequestListener _orderRequestListener;

        /// <summary>
        /// Manages market data for backtesting strategy
        /// </summary>
        private MarketDataListener _marketDataListener;

        /// <summary>
        /// Manages market data requests from strategy in backtesting
        /// </summary>
        private MarketRequestListener _marketRequestListener;

        /// <summary>
        /// Responsible for providing requested data
        /// </summary>
        private DataHandler _dataHandler;

        /// <summary>
        /// Unique Key to Identify the Strategy Instance
        /// </summary>
        private string _strategyKey;

        /// <summary>
        /// Indicates whether the strategy is None/Executing/Executed
        /// </summary>
        private string _strategyStatus;

        /// <summary>
        /// Save Custom Strategy Type (C# Class Type which implements TradeHubStrategy.cs)
        /// </summary>
        private Type _strategyType;

        /// <summary>
        /// Holds reference of Strategy Instance
        /// </summary>
        private TradeHubStrategy _tradeHubStrategy;

        /// <summary>
        /// Save Strategy Execution Statistics
        /// </summary>
        private Statistics _statistics;

        private Bar _currentBar;
        private Bar _prevBar;

        /// <summary>
        /// Holds selected ctor arguments to execute strategy
        /// </summary>
        private object[] _ctorArguments;

        #region Events

        // ReSharper disable InconsistentNaming
        private event Action<bool, string> _statusChanged;
        private event Action<Execution> _executionReceived;
        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Raised when custom strategy status changed from Running-to-Stopped and vice versa
        /// </summary>
        public event Action<bool, string> StatusChanged
        {
            add { if (_statusChanged == null) _statusChanged += value; }
            remove { _statusChanged -= value; }
        }

        /// <summary>
        /// Raised when new execution is received by the custom strategy
        /// </summary>
        public event Action<Execution> ExecutionReceived
        {
            add { if (_executionReceived == null) _executionReceived += value; }
            remove { _executionReceived -= value; }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Unique Key to Identify the Strategy Instance
        /// </summary>
        public string StrategyKey
        {
            get { return _strategyKey; }
            set { _strategyKey = value; }
        }

        /// <summary>
        /// Save Strategy Execution Statistics
        /// </summary>
        public Statistics Statistics
        {
            get { return _statistics; }
            set { _statistics = value; }
        }

        /// <summary>
        /// Holds selected ctor arguments to execute strategy
        /// </summary>
        public object[] CtorArguments
        {
            get { return _ctorArguments; }
            set { _ctorArguments = value; }
        }

        /// <summary>
        /// Indicates whether the strategy is None/Executing/Executed
        /// </summary>
        public string StrategyStatus
        {
            get { return _strategyStatus; }
            set { _strategyStatus = value; }
        }

        #endregion

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="strategyKey">Unique Key to Identify the Strategy Instance</param>
        /// <param name="strategyType">C# Class Type which implements TradeHubStrategy.cs</param>
        /// <param name="ctorArguments">Holds selected ctor arguments to execute strategy</param>
        public StrategyExecutor(string strategyKey, Type strategyType, object[] ctorArguments)
        {
            //_asyncClassLogger = ContextRegistry.GetContext()["StrategyRunnerLogger"] as AsyncClassLogger;
            _asyncClassLogger = new AsyncClassLogger("StrategyExecutor");
            {
                _asyncClassLogger.SetLoggingLevel();
                //set logging path
                string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                                  "\\TradeHub Logs\\Client";
                _asyncClassLogger.LogDirectory(path);
            }

            _tradeHubStrategy = null;
            _strategyKey = strategyKey;
            _strategyType = strategyType;
            _ctorArguments = ctorArguments;
            _strategyStatus = Infrastructure.Constants.StrategyStatus.None;

            // Initialize statistics
            _statistics = new Statistics(_strategyKey);

            // Initialze Utility Classes
            //_orderExecutor = new OrderExecutor(_asyncClassLogger);
            _orderExecutor = new OrderExecutorGeneric();
            _marketDataListener = new MarketDataListener(_asyncClassLogger);
            _orderRequestListener= new OrderRequestListener(_orderExecutor,_asyncClassLogger);

            // Use MarketDataListener.cs as Event Handler to get data from DataHandler.cs
            _dataHandler =
                new DataHandler(
                    new IEventHandler<TradeHub.Common.HistoricalDataProvider.ValueObjects.MarketDataObject>[]
                        {_marketDataListener});

            _marketDataListener.BarSubscriptionList = _dataHandler.BarSubscriptionList;
            _marketDataListener.TickSubscriptionList = _dataHandler.TickSubscriptionList;

            // Initialize MarketRequestListener.cs to manage incoming market data requests from strategy
            _marketRequestListener = new MarketRequestListener(_dataHandler, _asyncClassLogger);

            //Register OrderExecutor Events
            RegisterOrderExecutorEvents();

            //Register Market Data Listener Events
            RegisterMarketDataListenerEvents();
        }

        /// <summary>
        /// Starts custom strategy execution
        /// </summary>
        public void ExecuteStrategy()
        {
            try
            {
                // Verify Strategy Instance
                if (_tradeHubStrategy == null)
                {
                    //create DB strategy 
                    Strategy strategy=new Strategy();
                    strategy.Name = _strategyType.Name;
                    strategy.StartDateTime = DateTime.Now;

                    // Get new strategy instance
                    var strategyInstance = LoadCustomStrategy.CreateStrategyInstance(_strategyType, CtorArguments);

                    if (strategyInstance != null)
                    {
                        // Cast to TradeHubStrategy Instance
                        _tradeHubStrategy = strategyInstance as TradeHubStrategy;
                    }

                    if (_tradeHubStrategy == null)
                    {
                        if (_asyncClassLogger.IsInfoEnabled)
                        {
                            _asyncClassLogger.Info("Unable to initialize Custom Strategy: " + _strategyType.FullName, _type.FullName, "ExecuteStrategy");
                        }

                        // Skip execution of further actions
                        return;
                    }

                    // Set Strategy Name
                    _tradeHubStrategy.StrategyName = LoadCustomStrategy.GetCustomClassSummary(_strategyType);

                    // Register Events
                    _tradeHubStrategy.OnStrategyStatusChanged += OnStrategyStatusChanged;
                    _tradeHubStrategy.OnNewExecutionReceived += OnNewExecutionReceived;
                }

                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Executing user strategy: " + _strategyType.FullName, _type.FullName, "ExecuteStrategy");
                }

                //Overriding if running on simulated exchange
                ManageBackTestingStrategy();

                // Start Executing the strategy
                _tradeHubStrategy.Run();
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "ExecuteStrategy");
            }
        }

        /// <summary>
        /// Stops custom strategy execution
        /// </summary>
        public void StopStrategy()
        {
            try
            {
                // Verify Strategy Instance
                if (_tradeHubStrategy != null)
                {
                    if (_asyncClassLogger.IsInfoEnabled)
                    {
                        _asyncClassLogger.Info("Stopping user strategy execution: " + _strategyType.FullName, _type.FullName, "StopStrategy");
                    }

                    // Start Executing the strategy
                    _tradeHubStrategy.Stop();
                }
                else
                {
                    if (_asyncClassLogger.IsInfoEnabled)
                    {
                        _asyncClassLogger.Info("User strategy not initialized: " + _strategyType.FullName, _type.FullName, "StopStrategy");
                    }
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "StopStrategy");
            }
        }

        #region Manage Back-Testing Strategy (i.e. Provider = SimulatedExchange)

        /// <summary>
        /// Will take appropariate actions to handle a strategy intended to be back tested
        /// </summary>
        private void ManageBackTestingStrategy()
        {
            if (_tradeHubStrategy != null)
            {
                if(_tradeHubStrategy.MarketDataProviderName.Equals(TradeHubConstants.MarketDataProvider.SimulatedExchange))
                    OverrideStrategyDataEvents();
                if(_tradeHubStrategy.OrderExecutionProviderName.Equals(TradeHubConstants.OrderExecutionProvider.SimulatedExchange))
                    OverrideStrategyOrderRequests();
            }
        }

        /// <summary>
        /// Overrides required data events for backtesting strategy
        /// </summary>
        private void OverrideStrategyDataEvents()
        {
            //NOTE: LOCAL Data

            _tradeHubStrategy.OverrideTickSubscriptionRequest(_marketRequestListener.SubscribeTickData);
            _tradeHubStrategy.OverrideTickUnsubscriptionRequest(_marketRequestListener.UnsubscribeTickData);

            _tradeHubStrategy.OverrideBarSubscriptionRequest(_marketRequestListener.SubscribeLiveBars);
            _tradeHubStrategy.OverrideBarSubscriptionRequest(_marketRequestListener.SubscribeMultipleLiveBars);
            _tradeHubStrategy.OverriderBarUnsubscriptionRequest(_marketRequestListener.UnsubcribeLiveBars);

            ////NOTE: SX Data
            //_tradeHubStrategy.InitializeMarketDataServiceDisruptor(new IEventHandler<RabbitMqMessage>[] { _marketDataListener });
        }

        /// <summary>
        /// Overrides backtesting strategy's order requests to manage them inside strategy runner
        /// </summary>
        private void OverrideStrategyOrderRequests()
        {
            _tradeHubStrategy.OverrideMarketOrderRequest(_orderRequestListener.NewMarketOrderRequest);
            _tradeHubStrategy.OverrideLimitOrderRequest(_orderRequestListener.NewLimitOrderRequest);
            _tradeHubStrategy.OverrideCancelOrderRequest(_orderRequestListener.NewCancelOrderRequest);
            //_tradeHubStrategy.OverrideOrderRequest(_orderRequestListener.NewOrderRequest);

            /*_tradeHubStrategy.InitializeOrderExecutionServiceDisruptor(new IEventHandler<RabbitMqMessage>[] {_orderRequestListener});*/
        }

        /// <summary>
        /// Subscribes order events from <see cref="OrderExecutor"/>
        /// </summary>
        private void RegisterOrderExecutorEvents()
        {
            _orderExecutor.NewOrderArrived += OnOrderExecutorNewArrived;
            _orderExecutor.ExecutionArrived += OnOrderExecutorExecutionArrived;
            _orderExecutor.RejectionArrived += OnOrderExecutorRejectionArrived;
            _orderExecutor.CancellationArrived += OnOrderExecutorCancellationArrived;
        }

        /// <summary>
        /// Subscribes Tick and Bars events from <see cref="MarketDataListener"/>
        ///  </summary>
        private void RegisterMarketDataListenerEvents()
        {
            _marketDataListener.TickArrived += OnTickArrived;
            _marketDataListener.BarArrived += OnBarArrived;
        }

        /// <summary>
        /// Called when Cancellation received from <see cref="OrderExecutor"/>
        /// </summary>
        /// <param name="order"></param>
        private void OnOrderExecutorCancellationArrived(Order order)
        {
            _tradeHubStrategy.CancellationArrived(order);
            PersistencePublisher.PublishDataForPersistence(order);
        }

        /// <summary>
        /// Called when Rejection received from <see cref="OrderExecutor"/>
        /// </summary>
        /// <param name="rejection"></param>
        private void OnOrderExecutorRejectionArrived(Rejection rejection)
        {
            _tradeHubStrategy.RejectionArrived(rejection);
        }

        /// <summary>
        /// Called when Executions received from <see cref="OrderExecutor"/>
        /// </summary>
        /// <param name="execution"></param>
        private void OnOrderExecutorExecutionArrived(Execution execution)
        {
            _tradeHubStrategy.ExecutionArrived(execution);
            PersistencePublisher.PublishDataForPersistence(execution.Fill);
            PersistencePublisher.PublishDataForPersistence(execution.Order);
        }

        /// <summary>
        /// Called when New order status received from <see cref="OrderExecutor"/>
        /// </summary>
        /// <param name="order"></param>
        private void OnOrderExecutorNewArrived(Order order)
        {
            _tradeHubStrategy.NewArrived(order);
            PersistencePublisher.PublishDataForPersistence(order);
        }

        /// <summary>
        /// Called when bar received from <see cref="MarketDataListener"/>
        /// </summary>
        /// <param name="bar"></param>
        private void OnBarArrived(Common.Core.DomainModels.Bar bar)
        {
            if (_asyncClassLogger.IsDebugEnabled)
            {
                _asyncClassLogger.Debug(bar.ToString(), _type.FullName, "OnBarArrived");
            }
            //_statistics.Flag = false;
            _statistics.UpdateBar(bar);
            //_prevBar = _currentBar;
            //_currentBar = bar;
            _orderExecutor.BarArrived(bar);
            _tradeHubStrategy.OnBarArrived(bar);
            //Check();
            _statistics.CalculatePnlAfterBar();
        }

        private void Check()
        {
            if (!_statistics.Flag)
            {
                decimal perpnl = 0;
                if (_statistics.Pos > 0)
                {
                    perpnl = (_currentBar.Close - _prevBar.Close)*_statistics.Pos;
                    _statistics.UpdatePnl(perpnl);
                }
                else if (_statistics.Pos < 0)
                {
                    perpnl = (_currentBar.Close - _prevBar.Close)*_statistics.Pos;
                    _statistics.UpdatePnl(perpnl);
                }
            }
        }

        /// <summary>
        /// Called when tick received from <see cref="MarketDataListener"/>
        /// </summary>
        /// <param name="tick"></param>
        private void OnTickArrived(Common.Core.DomainModels.Tick tick)
        {
            if (_asyncClassLogger.IsDebugEnabled)
            {
                _asyncClassLogger.Debug(tick.ToString(), _type.FullName, "OnTickArrived");
            }

            _orderExecutor.TickArrived(tick);
            _tradeHubStrategy.OnTickArrived(tick);
        }

        #endregion

        /// <summary>
        /// Called when Custom Strategy Running status changes
        /// </summary>
        /// <param name="status">Indicate whether the strategy is running or nor</param>
        private void OnStrategyStatusChanged(bool status)
        {
            if (status)
            {
                _strategyStatus = Infrastructure.Constants.StrategyStatus.Executing;
            }
            else
            {
                _strategyStatus = Infrastructure.Constants.StrategyStatus.Executed;
                var risk=_statistics.GetRisk();
                _asyncClassLogger.Error("RISK="+risk,"a","b");
            }

            if (_statusChanged != null)
            {
                _statusChanged(status, _strategyKey);
            }
        }

        /// <summary>
        /// Called when Custom Strategy receives new execution message
        /// </summary>
        /// <param name="execution">Contains Execution Info</param>
        private void OnNewExecutionReceived(Execution execution)
        {
            if (_executionReceived != null)
            {
                _executionReceived(execution);
                //Task.Factory.StartNew(() => _executionReceived(execution));
            }

            // Update Stats
            //UpdateStatistics(execution);
            //_statistics.MatlabStatisticsFunction(execution);
            UpdateStatistics(execution);
        }

        private void ManageStatistics(Execution execution)
        {
            if (execution.Fill.ExecutionSide.Equals(TradeHubConstants.OrderSide.BUY))
            {
                if (execution.Fill.ExecutionSize==13)
                {
                    _statistics.Pos = -1;
                    if (_statistics.SharesSold==40&&_statistics.SharesBought==0)
                    {
                        _statistics.Pos += 0.33m;
                    }
                    else
                    {
                        _statistics.Pos += 0.66m;
                    }
                    var gl = _prevBar.Close - execution.Fill.ExecutionPrice;
                    var pnl = gl - ((_currentBar.Close - _prevBar.Close) * -1 * (_statistics.Pos));
                }
                else
                {
                    var perpnl = (_currentBar.Close - execution.Fill.ExecutionPrice);
                }
                _statistics.SharesBought += execution.Fill.ExecutionSize;
            }
            if (execution.Fill.ExecutionSide.Equals(TradeHubConstants.OrderSide.SELL)||
                execution.Fill.ExecutionSide.Equals(TradeHubConstants.OrderSide.SHORT))
            {
                if (execution.Fill.ExecutionSize == 13)
                {
                    _statistics.Pos = 1;
                    if (_statistics.SharesBought == 40 && _statistics.SharesSold == 0)
                    {
                        _statistics.Pos -= 0.33m;
                    }
                    else
                    {
                        _statistics.Pos -= 0.33m;
                    }
                    var gl = execution.Fill.ExecutionPrice - _prevBar.Close;
                    var pnl = gl + ((_currentBar.Close - _prevBar.Close) * (_statistics.Pos));
                }
                else
                {
                    var perpnl = (execution.Fill.ExecutionPrice - _currentBar.Close);
                }
                _statistics.SharesSold += execution.Fill.ExecutionSize;
            }

            //manage take profits:
            if (execution.Fill.ExecutionSide.Equals(TradeHubConstants.OrderSide.COVER))
            {
                if (_statistics.Position.Contains("Long"))
                {
                    //PerPnL(1,i) = (BuyPrice-data(i-1,4)-tcost) * -position
                    var panl = (execution.Fill.ExecutionPrice - _prevBar.Close) * _statistics.Pos;
                    _statistics.SharesSold += execution.Fill.ExecutionSize;
                    _statistics.Pos = 0;
                }
                else if(_statistics.Position.Contains("Short"))
                {
                    //PerPnL(1,i) = (data(i-1,4)-S_Price-tcost) * -position
                    var pnl = -1*(_prevBar.Close - execution.Fill.ExecutionPrice) * _statistics.Pos;
                    _statistics.SharesBought += execution.Fill.ExecutionSize;
                    _statistics.Pos = 0;
                }
            }
        }

        private void MatlabStatisticsFunction(Execution execution)
        {
            decimal perpnl=0;
            if (_statistics.Pos == 0)
            {
                if (execution.Fill.ExecutionSide.Equals(TradeHubConstants.OrderSide.SELL) ||
                    execution.Fill.ExecutionSide.Equals(TradeHubConstants.OrderSide.SHORT))
                {
                    _statistics.Pos = -1;
                    perpnl = (execution.Fill.ExecutionPrice - _currentBar.Close)*-1*_statistics.Pos;
                    _statistics.Flag = true;
                }
                else if (execution.Fill.ExecutionSide.Equals(TradeHubConstants.OrderSide.BUY))
                {
                    _statistics.Pos = 1;
                    perpnl = (_currentBar.Close - execution.Fill.ExecutionPrice)*_statistics.Pos;
                    _statistics.Flag = true;
                }
            }
            if (!_statistics.Flag)
            {
                if (_statistics.Pos < 0)
                {
                    if (execution.Order.Remarks.Contains("PT"))
                    {
                        _statistics.Pos += 0.33m;
                        var gl = _prevBar.Close - execution.Fill.ExecutionPrice;
                        perpnl = gl - (_currentBar.Close - _prevBar.Close)*-1*_statistics.Pos;
                    }
                    else
                    {
                        perpnl = (_prevBar.Close - execution.Fill.ExecutionPrice)*-1*_statistics.Pos;
                        _statistics.Pos = 0;
                    }
                }
                else if (_statistics.Pos > 0)
                {
                    if (execution.Order.Remarks.Contains("PT"))
                    {
                        _statistics.Pos -= 0.33m;
                        var gl = execution.Fill.ExecutionPrice - _prevBar.Close;
                        perpnl = gl + (_currentBar.Close - _prevBar.Close) * _statistics.Pos;
                    }
                    else
                    {
                        perpnl = (execution.Fill.ExecutionPrice-_prevBar.Close) * _statistics.Pos;
                        _statistics.Pos = 0;
                    }
                }
            }
            _statistics.UpdatePnl(perpnl);
        }
        
        /// <summary>
        /// Updates strategy statistics on each execution
        /// </summary>
        /// <param name="execution">Contains Execution Info</param>
        [MethodImpl(MethodImplOptions.Synchronized)] 
        private void UpdateStatistics(Execution execution)
        {
            //_statistics.UpdateCalulcationsOnExecution(execution);
            _statistics.MatlabStatisticsFunction(execution);
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Updating statistics on: " + execution, _type.FullName, "UpdateStatistics");
                }

                // Update statistics on BUY Order
                if (execution.Fill.ExecutionSide.Equals(TradeHubConstants.OrderSide.BUY))
                {
                    // Update Avg Buy Price
                    _statistics.AvgBuyPrice = ((execution.Fill.ExecutionPrice * execution.Fill.ExecutionSize)
                                               + (_statistics.AvgBuyPrice * _statistics.SharesBought))
                                              / (_statistics.SharesBought + execution.Fill.ExecutionSize);

                    // Update Size
                    _statistics.SharesBought += execution.Fill.ExecutionSize;
                }
                // Update statistics on SELL Order
                else if (execution.Fill.ExecutionSide.Equals(TradeHubConstants.OrderSide.SELL) ||
                    execution.Fill.ExecutionSide.Equals(TradeHubConstants.OrderSide.SHORT))
                {
                    // Update Avg Sell Price
                    _statistics.AvgSellPrice = ((execution.Fill.ExecutionPrice * execution.Fill.ExecutionSize)
                                                + (_statistics.AvgSellPrice * _statistics.SharesSold))
                                               / (_statistics.SharesSold + execution.Fill.ExecutionSize);

                    // Update Size
                    _statistics.SharesSold += execution.Fill.ExecutionSize;
                }
                // Update statistics on COVER Order (order used to close the open position)
                else if (execution.Fill.ExecutionSide.Equals(TradeHubConstants.OrderSide.COVER))
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

                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Statistics updated: " + _statistics, _type.FullName, "UpdateStatistics");
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "UpdateStatistics");
            }
        }

        /// <summary>
        /// Disposes strategy objects
        /// </summary>
        public void Close()
        {
            try
            {
                if (_tradeHubStrategy != null)
                {
                    _dataHandler.Shutdown();
                    _tradeHubStrategy.Dispose();
                    _tradeHubStrategy = null;
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "Close");
            }
        }
    }
}
