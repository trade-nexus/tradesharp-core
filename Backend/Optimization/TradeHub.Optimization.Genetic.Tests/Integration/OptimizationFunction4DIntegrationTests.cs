/***************************************************************************** 
* Copyright 2016 Aurora Solutions 
* 
*    http://www.aurorasolutions.io 
* 
* Aurora Solutions is an innovative services and product company at 
* the forefront of the software industry, with processes and practices 
* involving Domain Driven Design(DDD), Agile methodologies to build 
* scalable, secure, reliable and high performance products.
* 
* TradeSharp is a C# based data feed and broker neutral Algorithmic 
* Trading Platform that lets trading firms or individuals automate 
* any rules based trading strategies in stocks, forex and ETFs. 
* TradeSharp allows users to connect to providers like Tradier Brokerage, 
* IQFeed, FXCM, Blackwood, Forexware, Integral, HotSpot, Currenex, 
* Interactive Brokers and more. 
* Key features: Place and Manage Orders, Risk Management, 
* Generate Customized Reports etc 
* 
* Licensed under the Apache License, Version 2.0 (the "License"); 
* you may not use this file except in compliance with the License. 
* You may obtain a copy of the License at 
* 
*    http://www.apache.org/licenses/LICENSE-2.0 
* 
* Unless required by applicable law or agreed to in writing, software 
* distributed under the License is distributed on an "AS IS" BASIS, 
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
* See the License for the specific language governing permissions and 
* limitations under the License. 
*****************************************************************************/


﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AForge;
using AForge.Genetic;
using Disruptor;
using NUnit.Framework;
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.HistoricalDataProvider.Service;
using TradeHub.Common.HistoricalDataProvider.Utility;
using TradeHub.Common.HistoricalDataProvider.ValueObjects;
using TradeHub.Optimization.Genetic.FitnessFunction;
using TradeHub.StrategyEngine.TradeHub;
using TradeHub.StrategyEngine.Utlility.Services;

namespace TradeHub.Optimization.Genetic.Tests.Integration
{
    [TestFixture]
    public class OptimizationFunction4DIntegrationTests
    {
        private object[] _ctorArguments;
        private Type _strategyType;

        private Population _population;
        private CustomFitnessFunction _fitnessFunction;

        /// <summary>
        /// Holds reference to the strategy object
        /// </summary>
        private TradeHubStrategy _tradeHubStrategy;

        /// <summary>
        /// Holds reference of the executor for the current strategy
        /// </summary>
        private StrategyExecutor _strategyExecutor;

        [SetUp]
        public void SetUp()
        {
            // Set Logging levels
            Logger.SetLoggingLevel();
            
            // Create Constructor Arguments
            _ctorArguments = new object[]
                {
                    (Int32) 100, (decimal) 1.5, (uint) 40, "ERX", (decimal) 45, (float) 0.2, (decimal) 0.005,
                    (float) 0.005, (decimal) 0.005, "9:30", "9:30", (decimal) 10, (decimal) 0.04, "SDOT", (float) 0.006,
                    (decimal) 0.01, (decimal) 0.01,
                    "SimulatedExchange", "SimulatedExchange"
                };
        }

        [TearDown]
        public void Close()
        {
            if (_strategyExecutor != null)
            {
                _strategyExecutor.StopStrategy();
            }
        }

        /// <summary>
        /// Initializes Optimization parameters and fields
        /// </summary>
        private void InitializeOptimizationParameters()
        {
            // Initialize Fitness function to be used
            _fitnessFunction = new CustomFitnessFunction(_strategyExecutor, new Range(1, 3), new Range(0.001f, 0.01f),
                                                         new Range(0.001f, 0.009f), new Range(0.001f, 0.009f));

            // Set Fitness Mode
            _fitnessFunction.Mode = OptimizationFunction4D.Modes.Maximization;

            // Create genetic population
            _population = new Population(50, new BinaryChromosome(32), _fitnessFunction, new EliteSelection());
        }

        /// <summary>
        /// Execute single iteration of the loaded strategy
        /// </summary>
        [Test]
        [Category("Integration")]
        public void ExecuteStrategySingleIteration()
        {
            LoadAssembly(@"StockTraderStrategy\StockTrader.Common.dll");
            
            if (_strategyType != null)
            {
                // Initialize executor
                _strategyExecutor = new StrategyExecutor(_strategyType, _ctorArguments);

                // Execute single iteration of the strategy
                _strategyExecutor.ExecuteStrategy(1.5, 0.006, 0.2, 0.005);
            }
        }

        /// <summary>
        /// Execute strategy with multiple parameters.
        /// </summary>
        [Test]
        [Category("Integration")]
        public void ExecuteStrategy()
        {
            LoadAssembly(@"C:\Users\Muhammad Bilal\Desktop\StockTrader - Copy\StockTrader.Common.dll");
            List<string> data=new List<string>();
            _strategyExecutor = new StrategyExecutor(_strategyType, _ctorArguments);
            string[] file = File.ReadAllLines(@"C:\Users\Muhammad Bilal\Downloads\matlab_singlepoint_data.csv");
            for (int i = 0; i < file.Length; i++)
            {
                string[] param = file[i].Split(',');

                double alpha = double.Parse(param[0]);
                double beta = double.Parse(param[1]);
                double gamma = double.Parse(param[2]);
                double espilon =double.Parse(param[3]);
                //_ctorArguments = new object[]
                //{
                //    // Chelen Len,   ALPHA    , Shares   , Symbol,   EMA ,GAMMA, EPSILON 
                //    (Int32) 100, alpha, (uint) 40, "ERX", (decimal) 45, gamma, espilon,
                    
                //    // Profit Take,  Tolerance,  StartTime, EndTime, HTB Thresh, OPG Thresh   , OPG Venue,   BETA
                //    (float) 0.005, (decimal) 0.005, "9:30", "9:30", (decimal) 10, (decimal) 0.04, "SDOT", beta,

                //    // Entry Slippage, Exit Slippage
                //    (decimal) 0.01, (decimal) 0.01,
                //    "SimulatedExchange", "SimulatedExchange"
                //};
                if (_strategyType != null)
                {
                    // Initialize executor
                    // Execute single iteration of the strategy
                    double risk=_strategyExecutor.ExecuteStrategy(alpha, beta, gamma, espilon);
                    string[] lines=new string[1];
                    lines[0] = string.Format("{0},{1},{2},{3},{4},{5}", param[0], param[1], param[2], param[3], param[4],
                        risk);
                    data.Add(lines[0]);
                    Console.WriteLine(i);
                }
                
            }
            File.WriteAllLines(@"D:\matlabvsSr.csv",data);
            
        }


        /// <summary>
        /// Execute strategy while optimizing 'FOUR' parameters
        /// </summary>
        [Test]
        [Category("Integration")]
        public void ExecuteStrategyFourParameters()
        {
            LoadAssembly(@"StockTraderStrategy\StockTrader.Common.dll");

            if (_strategyType != null)
            {
                // Initialize executor
                _strategyExecutor = new StrategyExecutor(_strategyType, _ctorArguments);

                // Initialize required Optimization parameters
                InitializeOptimizationParameters();

                for (int i = 0; i < 30; i++)
                {
                    _population.RunEpoch();

                    Logger.Info("Iteration Count: " + i, "Optimization", "PopulationIterations");
                }

                Console.WriteLine(_fitnessFunction.Translate(_population.BestChromosome)[0].ToString("F3"));
                Console.WriteLine(_fitnessFunction.Translate(_population.BestChromosome)[1].ToString("F3"));
                Console.WriteLine(_fitnessFunction.Translate(_population.BestChromosome)[2].ToString("F3"));
                Console.WriteLine(_fitnessFunction.Translate(_population.BestChromosome)[3].ToString("F3"));

                Logger.Info(_fitnessFunction.Translate(_population.BestChromosome)[0].ToString("F3"), "Optimization", "BestChromosome");
                Logger.Info(_fitnessFunction.Translate(_population.BestChromosome)[1].ToString("F3"), "Optimization", "BestChromosome");
                Logger.Info(_fitnessFunction.Translate(_population.BestChromosome)[2].ToString("F3"), "Optimization", "BestChromosome");
                Logger.Info(_fitnessFunction.Translate(_population.BestChromosome)[3].ToString("F3"), "Optimization", "BestChromosome");
            }
        }

        /// <summary>
        /// Loads and Initializes "TradeHub" strategy from the given assembly
        /// </summary>
        /// <param name="path">Assembly path</param>
        public void LoadAssembly(string path)
        {
            try
            {
                // Load Assembly file from the selected file
                Assembly assembly = Assembly.LoadFrom(path);

                var strategyDetails = StrategyHelper.GetConstructorDetails(path);

                if (strategyDetails == null)
                {
                    Console.WriteLine("NULL value in strategy details");
                    return;
                }

                // Get Strategy Type
                _strategyType = strategyDetails.Item1;

                //// Verify Strategy Instance
                //if (_tradeHubStrategy == null)
                //{
                //    // Get new strategy instance
                //    var strategyInstance = LoadCustomStrategy.CreateStrategyInstance(_strategyType, _ctorArguments);

                //    if (strategyInstance != null)
                //    {
                //        // Cast to TradeHubStrategy Instance
                //        _tradeHubStrategy = strategyInstance as TradeHubStrategy;
                //    }

                //    if (_tradeHubStrategy == null)
                //    {
                //        Console.WriteLine("Unable to initialize Custom Strategy: " + _strategyType.FullName);

                //        // Skip execution of further actions
                //        return;
                //    }
                //}
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }

    /// <summary>
    /// Provide basic fitness function where strategy is executed to find the PnL for the given iteration
    /// </summary>
    public class CustomFitnessFunction : OptimizationFunction4D
    {
        /// <summary>
        /// Holds reference of the strategy to be optimized
        /// </summary>
        private readonly StrategyExecutor _strategyExecutor;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="strategyExecutor">Contains strategy reference to be executed</param>
        /// <param name="rangeW">Specifies W variable's range.</param>
        /// <param name="rangeX">Specifies X variable's range.</param>
        /// <param name="rangeY">Specifies Y variable's range.</param>
        /// <param name="rangeZ">Specifies Z variable's range.</param>
        public CustomFitnessFunction(StrategyExecutor strategyExecutor,
            Range rangeW, Range rangeX, Range rangeY, Range rangeZ)
            : base(rangeW, rangeX, rangeY, rangeZ)
        {
            _strategyExecutor = strategyExecutor;
        }

        #region Overrides of OptimizationFunction4D

        /// <summary>
        /// Function to optimize.
        /// </summary>
        /// <param name="w">Function W input value.</param>
        /// <param name="x">Function X input value.</param>
        /// <param name="y">Function Y input value.</param>
        /// <param name="z">Function Z input value.</param>
        /// <returns>Returns function output value.</returns>
        /// <remarks>The method should be overloaded by inherited class to
        /// specify the optimization function.</remarks>
        public override double OptimizationFunction(double w, double x, double y, double z)
        {
            double result = 0;
            
            // Calculate result
            result = _strategyExecutor.ExecuteStrategy(w, x, y, z);

            Logger.Info("ALPHA:   " + w, "Optimization", "FitnessFunction");
            Logger.Info("BETA:    " + x, "Optimization", "FitnessFunction");
            Logger.Info("GAMMA:   " + y, "Optimization", "FitnessFunction");
            Logger.Info("EPSILON: " + z, "Optimization", "FitnessFunction");
            Logger.Info("PNL:     " + result, "Optimization", "FitnessFunction");

            // Return result
            return result;
        }

        #endregion
    }

    /// <summary>
    /// Responsible for executing the given instance of the strategy 
    /// Calculates the PnL for the executed strategy
    /// </summary>
    public class StrategyExecutor
    {
        private readonly AsyncClassLogger _asyncClassLogger;

        private TradeHubStrategy _tradeHubStrategy;

        private readonly Type _strategyType;
        private readonly object[] _ctorArguments;

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
        /// <param name="ctorArguments"> </param>
        public StrategyExecutor(Type strategyType, object[] ctorArguments)
        {
            _manualReset = new ManualResetEvent(false);
            _asyncClassLogger = new AsyncClassLogger("OptimizationFunctionIntegrationTest");

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
                _tradeHubStrategy = strategyInstance as TradeHubStrategy;

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
            _tradeHubStrategy.OnStrategyStatusChanged += OnStrategyStatusChanged;

            // Initialize statistics
            _statistics = new Statistics("A00");

            // Initialze Utility Classes
            _orderExecutor = new OrderExecutor(_asyncClassLogger);
            //_marketDataListener = new MarketDataListener(_asyncClassLogger);
            _orderRequestListener = new OrderRequestListener(_orderExecutor, _asyncClassLogger);

            // Use MarketDataListener.cs as Event Handler to get data from DataHandler.cs
            //_dataHandler = new DataHandler(new IEventHandler<MarketDataObject>[] { _marketDataListener });
            _dataHandler = new DataHandler();

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
            _tradeHubStrategy.SetParameters(new object[] { alpha, beta, gemma, epsilon });

            // Reset Statistics for current iteration
            _statistics.ResetAllValues();

            // Start Strategy Execution
            _tradeHubStrategy.Run();

            // Wait for the strategy to execute
            _manualReset.WaitOne();

            ////Dispose Object
            //_tradeHubStrategy.Dispose();

            //// Force garbage collector to free memory
            //GC.Collect();
            //GC.WaitForPendingFinalizers();

            //ExecuteStrategy(alpha, beta, gemma, epsilon);

            // return current iterations PnL
            return (double) _statistics.GetRisk();
        }

        /// <summary>
        /// Stop Strategy and disposes the object
        /// </summary>
        public void StopStrategy()
        {
            if (_tradeHubStrategy != null)
            {
                _tradeHubStrategy.Dispose();
            }
        }

        #region Helper Functions

        /// <summary>
        /// Overriders TradeHUB Strategy's Market Data request calls to entertain them locally
        /// </summary>
        private void OverrideMarketRequestCalls()
        {
            //Override Market Data Requests
            _tradeHubStrategy.OverrideBarSubscriptionRequest(_marketRequestListener.SubscribeLiveBars);
            _tradeHubStrategy.OverriderBarUnsubscriptionRequest(_marketRequestListener.UnsubcribeLiveBars);
        }

        /// <summary>
        /// Overrides TradeHUB Strategy's Order request calls to entertain them locally
        /// </summary>
        private void OverrideOrderRequestCalls()
        {
            // Override Order Requests
            _tradeHubStrategy.OverrideMarketOrderRequest(_orderRequestListener.NewMarketOrderRequest);
            _tradeHubStrategy.OverrideLimitOrderRequest(_orderRequestListener.NewLimitOrderRequest);
            _tradeHubStrategy.OverrideCancelOrderRequest(_orderRequestListener.NewCancelOrderRequest);
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
        ///  </summary>
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
            _tradeHubStrategy.OnCancellationArrived(order);
        }

        /// <summary>
        /// Called when Executions received from <see cref="OrderExecutor"/>
        /// </summary>
        /// <param name="execution"></param>
        private void OnOrderExecutorExecutionArrived(Execution execution)
        {
            // Send Execution to strategy
            _tradeHubStrategy.OnExecutionArrived(execution);

            // Update Strategy Statistics
            UpdateStatistics(execution);
        }

        /// <summary>
        /// Called when New order status received from <see cref="OrderExecutor"/>
        /// </summary>
        /// <param name="order"></param>
        private void OnOrderExecutorNewArrived(Order order)
        {
            _tradeHubStrategy.OnNewArrived(order);
        }

        /// <summary>
        /// Called when bar received from <see cref="MarketDataListener"/>
        /// </summary>
        /// <param name="bar"></param>
        private void OnBarArrived(Bar bar)
        {
            _orderExecutor.BarArrived(bar);
            _tradeHubStrategy.OnBarArrived(bar);
        }

        /// <summary>
        /// Called when tick received from <see cref="MarketDataListener"/>
        /// </summary>
        /// <param name="tick"></param>
        private void OnTickArrived(Tick tick)
        {
            _orderExecutor.TickArrived(tick);
            _tradeHubStrategy.OnTickArrived(tick);
        }

        /// <summary>
        /// Called when startegy status changes
        /// </summary>
        /// <param name="status">Indicates strategy status</param>
        private void OnStrategyStatusChanged(bool status)
        {
            if (!status)
            {
                if (_tradeHubStrategy.IsRunning == false)
                    _manualReset.Set();
            }
        }

        /// <summary>
        /// Updates strategy statistics on each execution
        /// </summary>
        /// <param name="execution">Contains Execution Info</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void UpdateStatistics(Execution execution)
        {
            _statistics.UpdateCalulcationsOnExecution(execution);
            //try
            //{
            //    if (Logger.IsDebugEnabled)
            //    {
            //        Logger.Debug("Updating statistics on: " + execution, "IntegrationTest", "UpdateStatistics");
            //    }

            //    // Update statistics on BUY Order
            //    if (execution.Fill.ExecutionSide.Equals(OrderSide.BUY))
            //    {
            //        // Update Avg Buy Price
            //        _statistics.AvgBuyPrice = ((execution.Fill.ExecutionPrice * execution.Fill.ExecutionSize)
            //                                   + (_statistics.AvgBuyPrice * _statistics.SharesBought))
            //                                  / (_statistics.SharesBought + execution.Fill.ExecutionSize);

            //        // Update Size
            //        _statistics.SharesBought += execution.Fill.ExecutionSize;
            //    }
            //    // Update statistics on SELL Order
            //    else if (execution.Fill.ExecutionSide.Equals(OrderSide.SELL) ||
            //        execution.Fill.ExecutionSide.Equals(OrderSide.SHORT))
            //    {
            //        // Update Avg Sell Price
            //        _statistics.AvgSellPrice = ((execution.Fill.ExecutionPrice * execution.Fill.ExecutionSize)
            //                                    + (_statistics.AvgSellPrice * _statistics.SharesSold))
            //                                   / (_statistics.SharesSold + execution.Fill.ExecutionSize);

            //        // Update Size
            //        _statistics.SharesSold += execution.Fill.ExecutionSize;
            //    }
            //    // Update statistics on COVER Order (order used to close the open position)
            //    else if (execution.Fill.ExecutionSide.Equals(OrderSide.COVER))
            //    {
            //        if (_statistics.Position.Contains("Long"))
            //        {
            //            // Update Avg Sell Price
            //            _statistics.AvgSellPrice = ((execution.Fill.ExecutionPrice * execution.Fill.ExecutionSize)
            //                                        + (_statistics.AvgSellPrice * _statistics.SharesSold))
            //                                       / (_statistics.SharesSold + execution.Fill.ExecutionSize);

            //            // Update Size
            //            _statistics.SharesSold += execution.Fill.ExecutionSize;
            //        }
            //        else if (_statistics.Position.Contains("Short"))
            //        {
            //            // Update Avg Buy Price
            //            _statistics.AvgBuyPrice = ((execution.Fill.ExecutionPrice * execution.Fill.ExecutionSize)
            //                                       + (_statistics.AvgBuyPrice * _statistics.SharesBought))
            //                                      / (_statistics.SharesBought + execution.Fill.ExecutionSize);

            //            // Update Size
            //            _statistics.SharesBought += execution.Fill.ExecutionSize;
            //        }
            //    }

            //    // Update Profit and Loss
            //    _statistics.Pnl = (_statistics.AvgSellPrice * _statistics.SharesSold) -
            //                      (_statistics.AvgBuyPrice * _statistics.SharesBought);

            //    if (Logger.IsDebugEnabled)
            //    {
            //        Logger.Debug("Statistics updated: " + _statistics, "IntegrationTest", "UpdateStatistics");
            //    }
            //}
            //catch (Exception exception)
            //{
            //    Logger.Error(exception, "IntegrationTest", "UpdateStatistics");
            //}
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
