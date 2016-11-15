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
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.StrategyRunner.Infrastructure.ValueObjects
{
    /// <summary>
    /// Contains info for the Strategy to be optimized using Genetic Algorithm
    /// </summary>
    public class OptimizationParametersGeneticAlgo
    {
        /// <summary>
        /// Constructor arguments to be used for the given custom strategy
        /// </summary>
        private object[] _ctorArguments;

        /// <summary>
        /// Holds reference for user selected custom strategy
        /// </summary>
        private Type _strategyType;

        /// <summary>
        /// Contains info for the parameters to be used for Genetic Optimization
        /// </summary>
        private Dictionary<int, Tuple<string, Type>> _geneticAlgoParameters;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="ctorArguments">Constructor Arguments to be used</param>
        /// <param name="strategyType">Type of custom strategy</param>
        /// <param name="geneticAlgoParameters">Optimization parameters for Genetic Algo</param>
        public OptimizationParametersGeneticAlgo(object[] ctorArguments, Type strategyType, Dictionary<int, Tuple<string, Type>> geneticAlgoParameters)
        {
            _ctorArguments = ctorArguments;
            _strategyType = strategyType;
            _geneticAlgoParameters = geneticAlgoParameters;
        }

        /// <summary>
        /// Constructor arguments to be used for the given custom strategy
        /// </summary>
        public object[] CtorArguments
        {
            get { return _ctorArguments; }
        }

        /// <summary>
        /// Holds reference for user selected custom strategy
        /// </summary>
        public Type StrategyType
        {
            get { return _strategyType; }
        }

        /// <summary>
        /// Contains info for the parameters to be used for Genetic Optimization
        /// </summary>
        public Dictionary<int, Tuple<string, Type>> GeneticAlgoParameters
        {
            get { return _geneticAlgoParameters; }
        }
    }
}
