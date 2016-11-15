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
using TraceSourceLogger;
using TradeHub.StrategyRunner.Infrastructure.Service;
using TradeHub.StrategyRunner.Infrastructure.ValueObjects;
using TradeHub.StrategyRunner.UserInterface.Common;
using TradeHub.StrategyRunner.UserInterface.ParametersModule.ValueObjects;

namespace TradeHub.StrategyRunner.UserInterface.ParametersModule.ViewModel
{
    /// <summary>
    /// Contains backend functionality for ParametersView.xaml
    /// </summary>
    public class ParametersViewModel : ViewModelBase
    {
        private Type _type = typeof (ParametersViewModel);

        /// <summary>
        /// Holds reference to UI dispatcher
        /// </summary>
        private readonly Dispatcher _currentDispatcher;

        /// <summary>
        /// Contains info to identify strategy functionality
        /// </summary>
        private string _strategyInfo;

        /// <summary>
        /// Saves orignal set of ctor arguments
        /// </summary>
        private object[] _ctorArguments;

        /// <summary>
        /// Holds reference of user selected custom strategy
        /// </summary>
        private Type _strategyType;

        /// <summary>
        /// Save constuctor parameter info for the selected strategy
        /// </summary>
        private System.Reflection.ParameterInfo[] _parmatersDetails;

        /// <summary>
        /// Contains Info for each individual parameter
        /// </summary>
        private ObservableCollection<ParameterInfo> _parameters;

        /// <summary>
        /// Contains info to identify strategy functionality
        /// </summary>
        public string StrategyInfo
        {
            get { return _strategyInfo; }
            set 
            {
                _strategyInfo = value;
                RaisePropertyChanged("StrategyInfo");
            }
        }

        /// <summary>
        /// Command to start User Strategy optimization.
        /// </summary>
        public ICommand RunOptimization { get; set; }

        /// <summary>
        /// Contains Info for each individual parameter
        /// </summary>
        public ObservableCollection<ParameterInfo> Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ParametersViewModel()
        {
            _currentDispatcher = Dispatcher.CurrentDispatcher;
            _parameters = new ObservableCollection<ParameterInfo>();

            // Initialize Command
            RunOptimization = new DelegateCommand(StartOptimization);

            // Event Aggregator Subscriptions
            EventSystem.Subscribe<OptimizationParametersBruteForce>(DisplaySelectedStrategy);
        }

        /// <summary>
        /// Starts Optimization for the selected strategy
        /// </summary>
        private void StartOptimization()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Starting strategy optimization.", _type.FullName, "StartOptimization");
                }

                // Get parameters to be used for optimization
                var parameters = GetOptimizationParameters();

                if (parameters != null)
                {
                    if (parameters.Count > 0)
                    {
                        // Create a new object to be used with Event Aggregator
                        OptimizeStrategyBruteForce optimizationParameters =
                            new OptimizeStrategyBruteForce(_ctorArguments, _strategyType, parameters.ToArray(), _parmatersDetails);

                        // Raise Event to notify listeners
                        EventSystem.Publish<OptimizeStrategyBruteForce>(optimizationParameters);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "StartOptimization");
            }
        }

        /// <summary>
        /// Displays the selected strategy parameters 
        /// </summary>
        /// <param name="optimizeStrategy">Contains info regarding the strategy to be optimized</param>
        private void DisplaySelectedStrategy(OptimizationParametersBruteForce optimizeStrategy)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Displaying the strategy parameters", _type.FullName, "DisplaySelectedStrategy");
                }

                _parameters.Clear();

                // Get Strategy Info
                StrategyInfo = LoadCustomStrategy.GetCustomClassSummary(optimizeStrategy.StrategyType);

                // Save Ctor Arguments
                _ctorArguments = optimizeStrategy.CtorArguments;

                // Save Custom Strategy Type
                _strategyType = optimizeStrategy.StrategyType;

                // Save Parameters Details
                _parmatersDetails = optimizeStrategy.ParameterDetails;

                // Get all parameters
                for (int i = 0; i < optimizeStrategy.CtorArguments.Length; i++)
                {
                    ParameterInfo parameterInfo= new ParameterInfo
                        {
                            Index = i,
                            Parameter = optimizeStrategy.ParameterDetails[i].Name,
                            Value = optimizeStrategy.CtorArguments[i].ToString()
                        };

                    _currentDispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                    {
                        // Add to collection
                        _parameters.Add(parameterInfo);
                    }));
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "DisplaySelectedStrategy");
            }
        }

        /// <summary>
        /// Reads the parameters collection 
        /// Gets the parameters to be used for optimization
        /// </summary>
        private List<Tuple<int, string, string>> GetOptimizationParameters()
        {
            try
            {
                // Create a list to hold all optimization parameters
                var optimizationParameters = new List<Tuple<int, string, string>>();

                // Read info from all parameters
                foreach (ParameterInfo parameterInfo in _parameters)
                {
                    // Check if both End Point and Increment values are added
                    if (! (string.IsNullOrEmpty(parameterInfo.EndPoint) && string.IsNullOrWhiteSpace(parameterInfo.EndPoint)))
                    {
                        if (!(string.IsNullOrEmpty(parameterInfo.Increment) && string.IsNullOrWhiteSpace(parameterInfo.Increment)))
                        {
                            // Add parameter info
                            optimizationParameters.Add(new Tuple<int, string, string>(parameterInfo.Index,
                                                                                      parameterInfo.EndPoint,
                                                                                      parameterInfo.Increment));
                        }
                    }
                }

                return optimizationParameters;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "GetOptimizationParameters");
                return null;
            }
        }
    }
}
