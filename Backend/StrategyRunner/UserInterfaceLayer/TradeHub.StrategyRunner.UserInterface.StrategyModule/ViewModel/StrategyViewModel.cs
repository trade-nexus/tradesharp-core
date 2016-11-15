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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Commands;
using Spring.Context.Support;
using TraceSourceLogger;
using TradeHub.Common.Core.Repositories;
using TradeHub.Common.Persistence;
using TradeHub.StrategyRunner.Infrastructure.ValueObjects;
using TradeHub.StrategyRunner.UserInterface.Common;

namespace TradeHub.StrategyRunner.UserInterface.StrategyModule.ViewModel
{
    /// <summary>
    /// Proivdes backend functionality for the StrategyView.xaml
    /// </summary>
    public class StrategyViewModel : ViewModelBase
    {
        private Type _type = typeof (StrategyViewModel);

        /// <summary>
        /// Holds reference to UI dispatcher
        /// </summary>
        private readonly Dispatcher _currentDispatcher;

        /// <summary>
        /// Contains all the selected strategies
        /// </summary>
        private ObservableCollection<SelectedStrategy> _strategiesCollection;

        /// <summary>
        /// Command to execute strategy
        /// </summary>
        public ICommand RunStrategyCommand { get; set; }

        /// <summary>
        /// Command to stop strategy
        /// </summary>
        public ICommand StopStrategyCommand { get; set; }

        /// <summary>
        /// Contains all the selected strategies
        /// </summary>
        public ObservableCollection<SelectedStrategy> StrategiesCollection
        {
            get { return _strategiesCollection; }
            set { _strategiesCollection = value; }
        }

        public bool EnablePersistence { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public StrategyViewModel()
        {
            _currentDispatcher = Dispatcher.CurrentDispatcher;

            _strategiesCollection = new ObservableCollection<SelectedStrategy>();

            // Setup Commands
            RunStrategyCommand = new DelegateCommand<string>(RunSelectedStrategy);
            StopStrategyCommand = new DelegateCommand<string>(StopSelectedStrategy);

            //default persistence option
            EnablePersistence = false;
            
            #region Event Aggregator

            EventSystem.Subscribe<AddStrategy>(AddSelectedStrategy);
            EventSystem.Subscribe<UpdateStrategy>(UpdateSelectedStrategy);

            #endregion
        }

        /// <summary>
        /// Adds selected strategy to strategy view
        /// </summary>
        /// <param name="addStrategy"></param>
        private void AddSelectedStrategy(AddStrategy addStrategy)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Adding strategy to view: " + addStrategy.SelectedStrategy, _type.FullName,
                                "AddSelectedStrategy");
                }

                // Add to observable collection
                _strategiesCollection.Add(addStrategy.SelectedStrategy);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "AddSelectedStrategy");
            }
        }

        /// <summary>
        /// Updates the strategy in the strategy view
        /// </summary>
        /// <param name="updateStrategy">Contians info for the strategy to be updated</param>
        private void UpdateSelectedStrategy(UpdateStrategy updateStrategy)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Updating strategy: " + updateStrategy.StrategyKey, _type.FullName, "UpdateSelectedStrategy");
                }

                // Get required stratgey to update on UI
                var selectedStrategy = _strategiesCollection.FirstOrDefault(i => i.Key == updateStrategy.StrategyKey);
                if (selectedStrategy != null)
                {
                    lock (_currentDispatcher)
                    {
                        _currentDispatcher.Invoke(DispatcherPriority.Normal, (Action) (() =>
                            {
                                _strategiesCollection.Remove(selectedStrategy);
                                selectedStrategy.IsRunning = updateStrategy.IsRunning;
                                _strategiesCollection.Insert(0, selectedStrategy);
                            }));
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "UpdateSelectedStrategy");
            }
        }

        /// <summary>
        /// Execute selected strategy
        /// </summary>
        private void RunSelectedStrategy(string key)
        {
            try
            {
                IPersistRepository<object> persistRepository=ContextRegistry.GetContext()["PersistRepository"] as IPersistRepository<object>;
                PersistencePublisher.InitializeDisruptor(EnablePersistence,persistRepository);
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Going to execute the selected strategy: ", _type.FullName, "RunSelectedStrategy");
                }

                // Create new instance to pass to event aggregator
                RunStrategy runStrategy = new RunStrategy(key);

                // Publish event to notify listeners
                EventSystem.Publish<RunStrategy>(runStrategy);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "RunSelectedStrategy");
            }
        }

        /// <summary>
        /// Stops the selected strategy
        /// </summary>
        private void StopSelectedStrategy(string key)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Stoping the selected strategy: ", _type.FullName, "StopSelectedStrategy");
                }

                // Create new instance to pass to event aggregator
                StopStrategy stopStrategy = new StopStrategy(key);

                // Publish event to notfiy listeners
                EventSystem.Publish<StopStrategy>(stopStrategy);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "StopSelectedStrategy");
            }
        }
    }
}
