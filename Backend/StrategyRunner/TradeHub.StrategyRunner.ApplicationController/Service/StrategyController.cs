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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;
using Microsoft.Practices.Unity;
using Spring.Context.Support;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.Utility;
using TradeHub.StrategyEngine.HistoricalData;
using TradeHub.StrategyEngine.MarketData;
using TradeHub.StrategyEngine.OrderExecution;
using TradeHub.StrategyRunner.Infrastructure.Service;
using TradeHub.StrategyRunner.Infrastructure.ValueObjects;
using TradeHub.StrategyRunner.UserInterface.Common;
using TradeHub.StrategyRunner.UserInterface.SearchModule.Utility;

namespace TradeHub.StrategyRunner.ApplicationController.Service
{
    /// <summary>
    /// Provides backend functionality for the Strategy Runner
    /// </summary>
    public class StrategyController : IEventHandler<Execution>
    {
        private Type _type = typeof(StrategyController);
        private AsyncClassLogger _asyncClassLogger;

        private int _ringSize = 65536;
        private Disruptor<Execution> _disruptor;
        private RingBuffer<Execution> _ringBuffer; 

        /// <summary>
        /// Keeps tracks of all the running strategies
        /// KEY = Unique string to identify each strategy instance
        /// Value = <see cref="StrategyExecutor"/>
        /// </summary>
        private ConcurrentDictionary<string, StrategyExecutor> _strategiesCollection;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public StrategyController()
        {
            //_asyncClassLogger = ContextRegistry.GetContext()["OEEClientLogger"] as AsyncClassLogger;
            _asyncClassLogger = new AsyncClassLogger("StrategyController");
            if (_asyncClassLogger != null)
            {
                _asyncClassLogger.SetLoggingLevel();
                //set logging path
                string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                                  "\\TradeHub Logs\\Client";
                _asyncClassLogger.LogDirectory(path);
            }

            _strategiesCollection = new ConcurrentDictionary<string, StrategyExecutor>();

            _disruptor = new Disruptor<Execution>(() => new Execution(new Fill(new Security(), "", ""), new Order("")),
                                                  _ringSize, TaskScheduler.Default);
            _disruptor.HandleEventsWith(this);
            _ringBuffer = _disruptor.Start();

            #region Event Aggregator

            EventSystem.Subscribe<LoadStrategy>(LoadUserStrategy);
            EventSystem.Subscribe<InitializeStrategy>(InitializeUserStrategy);
            EventSystem.Subscribe<RunStrategy>(RunUserStrategy);
            EventSystem.Subscribe<StopStrategy>(StopUserStrategy);
            EventSystem.Subscribe<string>(ManageUserCommands);
            #endregion
        }

        /// <summary>
        /// Loads user strategy and extracts constructor parameters
        /// </summary>
        private void LoadUserStrategy(LoadStrategy loadStrategy)
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Trying to load user defined strategy from: " + 
                        loadStrategy.StrategyAssembly.FullName.Substring(0, loadStrategy.StrategyAssembly.FullName.IndexOf(",", System.StringComparison.Ordinal)),
                        _type.FullName, "LoadUserStrategy");
                }

                var strategyDetails = LoadCustomStrategy.GetConstructorDetails(loadStrategy.StrategyAssembly);

                if (strategyDetails != null)
                {
                    if (_asyncClassLogger.IsInfoEnabled)
                    {
                        _asyncClassLogger.Info("Successfully loaded custom strategy: " + strategyDetails.Item1.Name, _type.Name, "LoadUserStrategy");
                    }

                    // Create new Strategy Constructor Info object 
                    StrategyConstructorInfo strategyConstructorInfo = new StrategyConstructorInfo(
                        strategyDetails.Item2, strategyDetails.Item1);

                    // Publish Event to Notify Listener.
                    EventSystem.Publish<StrategyConstructorInfo>(strategyConstructorInfo);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "LoadUserStrategy");
            }
        }

        /// <summary>
        /// Sets up the strategy to be executed
        /// </summary>
        /// <param name="initializeStrategy">Holds info to initialize the given strategy</param>
        private void InitializeUserStrategy(InitializeStrategy initializeStrategy)
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Setting up user strategy to run: " + initializeStrategy.StrategyType.FullName,
                                _type.FullName, "InitializeUserStrategy");
                }

                // Get new Key.
                string key = ApplicationIdGenerator.NextId();

                // Save Strategy details in new Strategy Executor object
                StrategyExecutor strategyExecutor = new StrategyExecutor(key, initializeStrategy.StrategyType, initializeStrategy.CtorArguments);

                // Add to local map
                _strategiesCollection.AddOrUpdate(key, strategyExecutor, (ky, value) => strategyExecutor);

                //Register Event
                strategyExecutor.StatusChanged += OnStatusChanged;
                strategyExecutor.ExecutionReceived += OnExecutionArrived;

                // Save Brief info of constructor parameters
                StringBuilder briefInfo = new StringBuilder();
                
                // Add Strategy Description
                briefInfo.Append(LoadCustomStrategy.GetCustomClassSummary(initializeStrategy.StrategyType));
                briefInfo.Append(" :: ");

                // Add Parameters Description
                foreach (object ctorArgument in initializeStrategy.CtorArguments)
                {
                    briefInfo.Append(ctorArgument.ToString());
                    briefInfo.Append("|");
                }

                // Create object to add to AddStrattegy.cs object
                SelectedStrategy selectedStrategy = new SelectedStrategy
                    {
                        Key = key,
                        Symbol = initializeStrategy.CtorArguments[3].ToString(),
                        BriefInfo = briefInfo.ToString()
                    };

                // Create object to pass to event aggregator.
                AddStrategy addStrategy = new AddStrategy(selectedStrategy);

                // Publish event to notify listeners.
                EventSystem.Publish<AddStrategy>(addStrategy);
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "InitializeUserStrategy");
            }
        }

        /// <summary>
        /// Manages incoming general user commands
        /// </summary>
        /// <param name="value"></param>
        private void ManageUserCommands(string value)
        {
            if(value.Equals("Close"))
            {
                CloseUserStrategies(value);
            }
        }

        /// <summary>
        /// Starts user strategy execution
        /// </summary>
        /// <param name="runStrategy">Contains info for the strategy to be executed</param>
        private void RunUserStrategy(RunStrategy runStrategy)
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Attempting to execute user strategy: " + runStrategy.Key, _type.FullName, "RunUserStrategy");
                }

                StrategyExecutor strategyExecutor;
                if(_strategiesCollection.TryGetValue(runStrategy.Key, out strategyExecutor))
                {
                    // Execute Strategy
                    strategyExecutor.ExecuteStrategy();
                    //Task.Factory.StartNew(() => strategyExecutor.ExecuteStrategy());
                }
                else
                {
                    if (_asyncClassLogger.IsInfoEnabled)
                    {
                        _asyncClassLogger.Info("User strategy: " + runStrategy.Key + " not found.", _type.FullName, "RunUserStrategy");
                    }    
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "RunUserStrategy");
            }
        }

        /// <summary>
        /// Stops user strategy execution
        /// </summary>
        /// <param name="stopStrategy">Contains info for the strategy to be stoppped</param>
        private void StopUserStrategy(StopStrategy stopStrategy)
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Attempting to stop user strategy: " + stopStrategy.Key, _type.FullName, "StopUserStrategy");
                }

                StrategyExecutor strategyExecutor;
                if (_strategiesCollection.TryGetValue(stopStrategy.Key, out strategyExecutor))
                {
                    // Stop Execution
                    strategyExecutor.StopStrategy();
                }
                else
                {
                    if (_asyncClassLogger.IsInfoEnabled)
                    {
                        _asyncClassLogger.Info("User strategy: " + stopStrategy.Key + " not found.", _type.FullName, "StopUserStrategy");
                    }
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "StopUserStrategy");
            }
        }

        /// <summary>
        /// Close all open strategies and dispose created objects
        /// </summary>
        /// <param name="value"></param>
        private void CloseUserStrategies(string value)
        {
            try
            {
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Disposing all user strategies", _type.FullName, "CloseUserStrategies");
                }

                // Dispose each individual loaded strategy
                foreach (StrategyExecutor strategyExecutor in _strategiesCollection.Values)
                {
                    // Stop Execution
                    strategyExecutor.StopStrategy();
                    // Dispose objects
                    strategyExecutor.Close();
                }

                //// Get running services
                //var context = ContextRegistry.GetContext();
                //var marketDataService = context.GetObject("MarketDataService") as MarketDataService;
                //var historicalDataService = context.GetObject("HistoricalDataService") as HistoricalDataService;
                //var orderExecutionService = context.GetObject("OrderExecutionService") as OrderExecutionService;

                //// Stop Services
                //if (marketDataService != null) marketDataService.StopService();
                //if (historicalDataService != null) historicalDataService.StopService();
                //if (orderExecutionService != null) orderExecutionService.StopService();
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "CloseUserStrategies");
            }
        }

        /// <summary>
        /// Raised when a custom loaded strategy status changes
        /// </summary>
        /// <param name="status">Indicated if the strategy is Running/Stopped</param>
        /// <param name="strategyKey">Unique Key to identify Strategy Instance</param>
        private void OnStatusChanged(bool status, string strategyKey)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Stratgey: " + strategyKey + " is running: " + status, _type.FullName, "OnStatusChanged");
                }

                // Create a new instance to be used with event aggregator
                UpdateStrategy updateStrategy = new UpdateStrategy(strategyKey, status);
                
                // Publish event to notify listeners
                EventSystem.Publish<UpdateStrategy>(updateStrategy);
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnStatusChanged");
            }
        }

        /// <summary>
        /// Raised when new order execution is recieved from the running strategies
        /// </summary>
        /// <param name="execution">Contains execution info</param>
        private void OnExecutionArrived(Execution execution)
        {
            try
            {
                // Get Next Sequence number
                long sequenceNumber = _ringBuffer.Next();
                // Get new entry value
                var entry = _ringBuffer[sequenceNumber];

                // Update values
                entry.Fill = execution.Fill;
                entry.Order = execution.Order;
                //entry.BarClose = execution.BarClose;

                // Publish updated entry
                _ringBuffer.Publish(sequenceNumber);

                //if (_asyncClassLogger.IsDebugEnabled)
                //{
                //    _asyncClassLogger.Debug("Execution arrived for: " + execution.Order.OrderID, _type.FullName, "OnExecutionArrived");
                //}

                //// Create new object to be used with Event Aggregator
                //UpdateStats updateStats= new UpdateStats(execution);

                ////Raise event to notify listeners
                //EventSystem.Publish<UpdateStats>(updateStats);
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnExecutionArrived");
            }
        }

        #region Implementation of IEventHandler<in Execution>

        /// <summary>
        /// Called when a publisher has committed an event to the <see cref="T:Disruptor.RingBuffer`1"/>
        /// </summary>
        /// <param name="data">Data committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="sequence">Sequence number committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="endOfBatch">flag to indicate if this is the last event in a batch from the <see cref="T:Disruptor.RingBuffer`1"/></param>
        public void OnNext(Execution data, long sequence, bool endOfBatch)
        {
            if (_asyncClassLogger.IsDebugEnabled)
            {
                _asyncClassLogger.Debug("Execution arrived for: " + data.Order.OrderID, _type.FullName, "OnNext");
            }

            // Create new object to be used with Event Aggregator
            UpdateStats updateStats = new UpdateStats(data);

            //Raise event to notify listeners
            EventSystem.Publish<UpdateStats>(updateStats);
        }

        #endregion
    }
}
