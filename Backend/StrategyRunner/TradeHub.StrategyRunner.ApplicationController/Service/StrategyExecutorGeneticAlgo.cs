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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Spring.Context.Support;
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.Repositories;
using TradeHub.Common.HistoricalDataProvider.Service;
using TradeHub.Common.HistoricalDataProvider.Utility;
using TradeHub.Optimization.Genetic.Interfaces;
using TradeHub.StrategyEngine.TradeHub;
using TradeHub.StrategyRunner.Infrastructure.Entities;
using TradeHub.StrategyRunner.Infrastructure.Service;

namespace TradeHub.StrategyRunner.ApplicationController.Service
{
    /// <summary>
    /// Mananges User Strategy execution for Genetic Algorithm
    /// </summary>
    public class StrategyExecutorGeneticAlgo : IStrategyExecutor
    {
        private readonly Type _type = typeof (StrategyExecutorGeneticAlgo);

        private AsyncClassLogger _logger;

        /// <summary>
        /// User strategy Instance
        /// </summary>
        private TradeHubStrategy _tradeHubStrategy;

        /// <summary>
        /// User Strategy Type
        /// </summary>
        private Type _strategyType;

        /// <summary>
        /// Constructor Arguments to be used with the strategy instance
        /// </summary>
        private object[] _ctorArguments;

        /// <summary>
        /// Responsible for providing requested data
        /// </summary>
        private DataHandler _dataHandler;

        /// <summary>
        /// Responsible for providing order executions in backtesting
        /// </summary>
        private OrderExecutor _orderExecutor;

        /// <summary>
        /// Manages order requests from strategy in backtesting
        /// </summary>
        private OrderRequestListener _orderRequestListener;

        /// <summary>
        /// Manages market data requests from strategy in backtesting
        /// </summary>
        private MarketRequestListener _marketRequestListener;

        /// <summary>
        /// Save Strategy Execution Statistics
        /// </summary>
        private Statistics _statistics;

        /// <summary>
        /// Used to wait for the strategy iteration to complete
        /// </summary>
        private ManualResetEvent _manualReset;

        private Bar _currentBar;
        private Bar _prevBar;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="strategyType">User Strategy Type</param>
        /// <param name="ctorArguments">Constructor arguments to initialize strategy</param>
        public StrategyExecutorGeneticAlgo(Type strategyType, object[] ctorArguments)
        {
            // Initialize required fields and parameters
            Initialize(strategyType, ctorArguments);
        }

        /// <summary>
        /// Initializes necessary fields and parameters
        /// </summary>
        /// <param name="strategyType">User Strategy Type</param>
        /// <param name="ctorArguments">Constructor arguments to initialize strategy</param>
        private void Initialize(Type strategyType, object[] ctorArguments)
        {
            _manualReset = new ManualResetEvent(false);
            _logger = new AsyncClassLogger("StrategyExecutorGeneticAlgo");

            // Save Strategy Type
            _strategyType = strategyType;

            //Save Arguments
            _ctorArguments = ctorArguments;

            // Set Logging levels
            _logger.SetLoggingLevel();

            // Get new strategy instance
            var strategyInstance = LoadCustomStrategy.CreateStrategyInstance(_strategyType, _ctorArguments);
            
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
        /// Initializes and registers required parametes and delagates
        /// </summary>
        private void InitializeStrategyListeners()
        {
            // Register TradeHUB Strategy's status (Running/Stopped) Event
            _tradeHubStrategy.OnStrategyStatusChanged += OnStrategyStatusChanged;

            // Initialize statistics
            _statistics = new Statistics("A00");

            // Initialze Utility Classes
            _orderExecutor = new OrderExecutor(_logger);
            //_marketDataListener = new MarketDataListener(_asyncClassLogger);
            _orderRequestListener = new OrderRequestListener(_orderExecutor, _logger);

            // Use MarketDataListener.cs as Event Handler to get data from DataHandler.cs
            //_dataHandler = new DataHandler(new IEventHandler<MarketDataObject>[] { _marketDataListener });
            _dataHandler = new DataHandler(true); // Set local persistance to true

            //_marketDataListener.BarSubscriptionList = _dataHandler.BarSubscriptionList;
            //_marketDataListener.TickSubscriptionList = _dataHandler.TickSubscriptionList;

            // Initialize MarketRequestListener.cs to manage incoming market data requests from strategy
            _marketRequestListener = new MarketRequestListener(_dataHandler, _logger);

            // Register Events to receive data
            RegisterMarketDataListenerEvents();
            // Register Events to receive order executions
            RegisterOrderExecutorEvents();
        }

        #region Override Market and Order Calls

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
        }

        #endregion

        #region Register Order Executor Events

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
        /// Called when Cancellation received from <see cref="OrderExecutor"/>
        /// </summary>
        /// <param name="order"></param>
        private void OnOrderExecutorCancellationArrived(Order order)
        {
            _tradeHubStrategy.CancellationArrived(order);
        }

        /// <summary>
        /// Called when Executions received from <see cref="OrderExecutor"/>
        /// </summary>
        /// <param name="execution"></param>
        private void OnOrderExecutorExecutionArrived(Execution execution)
        {
            // Send Execution to strategy
            _tradeHubStrategy.ExecutionArrived(execution);
        }

        /// <summary>
        /// Called when New order status received from <see cref="OrderExecutor"/>
        /// </summary>
        /// <param name="order"></param>
        private void OnOrderExecutorNewArrived(Order order)
        {
            _tradeHubStrategy.NewArrived(order);
        }

        #endregion

        #region Register Data Events

        /// <summary>
        /// Subscribes Tick and Bars events from <see cref="MarketDataListener"/>
        /// </summary>
        private void RegisterMarketDataListenerEvents()
        {
            _dataHandler.TickReceived += OnTickArrived;
            _dataHandler.BarReceived += OnBarArrived;
        }

        /// <summary>
        /// Called when bar received from <see cref="MarketDataListener"/>
        /// </summary>
        /// <param name="bar"></param>
        private void OnBarArrived(Bar bar)
        {
            _statistics.UpdateBar(bar);
            _statistics.CalculatePnlAfterBar();
            _orderExecutor.BarArrived(bar);
            _tradeHubStrategy.OnBarArrived(bar);
            //_statistics.CalculatePnlAfterBar();
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

        #endregion

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

        
        #region Implementation of IStrategyExecutor

        public double evaluateFunction(double[] x)
        {
            return ExecuteStrategy(x);
        }
        /// <summary>
        /// Execute Strategy iteration to calculate Fitness
        /// </summary>
        /// <returns>Return Strategy's Fitness for current execution</returns>
        public double ExecuteStrategy(double[] values)
        {
            try
            {
                _manualReset = new ManualResetEvent(false);

                //convert all parameters to object.
                object[] array=new object[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    array[i] = values[i];
                }
                // Update Strategy Parameters
                _tradeHubStrategy.SetParameters(array);

                // Reset Statistics for current iteration
                _statistics.ResetAllValues();
                _orderExecutor.Clear();

                // Start Strategy Execution
                _tradeHubStrategy.Run();

                // Wait for the strategy to execute
                _manualReset.WaitOne();

                // Clear subscription maps
                _dataHandler.ClearMaps();
                _tradeHubStrategy.ClearOrderMap();

                // return current iterations risk
                return -1*(double)_tradeHubStrategy.GetObjectiveFunctionValue();
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "ExecuteStrategy");
                return default(double);
            }
        }

        #endregion

        /// <summary>
        /// Stop Strategy and disposes the object
        /// </summary>
        public void StopStrategy()
        {
            if (_dataHandler != null)
            {
                _dataHandler.Shutdown();
            } 
            if (_tradeHubStrategy != null)
            {
                _tradeHubStrategy.Dispose();
            }
        }

    }
}
