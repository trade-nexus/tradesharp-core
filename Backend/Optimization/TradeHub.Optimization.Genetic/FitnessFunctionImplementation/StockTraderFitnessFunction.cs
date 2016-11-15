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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge;
using TradeHub.Optimization.Genetic.FitnessFunction;
using TradeHub.Optimization.Genetic.HelperFunctions;
using TradeHub.Optimization.Genetic.Interfaces;

namespace TradeHub.Optimization.Genetic.FitnessFunctionImplementation
{
    /// <summary>
    /// Fitness function required for optimizing "StockTrader" Strategy
    /// </summary>
    public class StockTraderFitnessFunction : GeneticOptimization
    {
        /// <summary>
        /// Holds reference of the Strategy Executor to optimize user strategy
        /// </summary>
        private readonly IStrategyExecutor _strategyExecutor;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="strategyExecutor"> </param>
        public StockTraderFitnessFunction(IStrategyExecutor strategyExecutor)
        {
            _strategyExecutor = strategyExecutor;
        }

        #region Overrides of OptimizationFunction4D

        /// <summary>
        /// Function to optimize.
        /// </summary>
        /// <remarks>The method should be overloaded by inherited class to
        /// specify the optimization function.</remarks>
        public override double OptimizationFunction(double[] values)
        {
            double result = 0;
            // Calculate result
            result = _strategyExecutor.ExecuteStrategy(values);
            // Return result
            return result;
        }

        #endregion
    }
}
