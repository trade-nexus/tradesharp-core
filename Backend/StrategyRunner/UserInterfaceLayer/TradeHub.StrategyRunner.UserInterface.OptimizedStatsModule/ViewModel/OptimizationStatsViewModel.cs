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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using TraceSourceLogger;
using TradeHub.StrategyRunner.Infrastructure.Entities;
using TradeHub.StrategyRunner.UserInterface.Common;

namespace TradeHub.StrategyRunner.UserInterface.OptimizedStatsModule.ViewModel
{
    /// <summary>
    /// Provides backend fucntionality for OptimizationStatsView.xaml
    /// </summary>
    public class OptimizationStatsViewModel : ViewModelBase
    {
        private Type _type = typeof (OptimizationStatsViewModel);

        /// <summary>
        /// Holds reference to UI dispatcher
        /// </summary>
        private readonly Dispatcher _currentDispatcher;

        /// <summary>
        /// Contains statistics for all the optimization iterations
        /// </summary>
        private ObservableCollection<OptimizationStatistics> _statisticsCollection;

        /// <summary>
        /// Contains statistics for all the optimization iterations
        /// </summary>
        public ObservableCollection<OptimizationStatistics> StatisticsCollection
        {
            get { return _statisticsCollection; }
            set { _statisticsCollection = value; }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public OptimizationStatsViewModel()
        {
            _currentDispatcher = Dispatcher.CurrentDispatcher;
            _statisticsCollection= new ObservableCollection<OptimizationStatistics>();

            // Event Aggregator
            EventSystem.Subscribe<OptimizationStatistics>(DisplayOptimizationStatistics);
        }

        /// <summary>
        /// Displays the optimization statistics from each iteration on UI
        /// </summary>
        /// <param name="optimizationStatistics">Contains info for the given iterations execution statistics</param>
        [MethodImpl(MethodImplOptions.Synchronized)] 
        private void DisplayOptimizationStatistics(OptimizationStatistics optimizationStatistics)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Displaying optimization statistics", _type.FullName, "DisplayOptimizationStatistics");
                }

                // Display on UI
                _currentDispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => StatisticsCollection.Add(optimizationStatistics)));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "DisplayOptimizationStatistics");
            }
        }
    }
}
