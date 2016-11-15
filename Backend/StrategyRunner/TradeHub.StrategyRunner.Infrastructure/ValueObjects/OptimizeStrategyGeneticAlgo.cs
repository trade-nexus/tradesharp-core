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
    /// Contains info for the parameters to be used when initiating optimization
    /// </summary>
    public class OptimizeStrategyGeneticAlgo
    {
        /// <summary>
        /// Constructor arguments to use
        /// </summary>
        private object[] _ctorArgs;

        /// <summary>
        /// Contains info for the parameters to be used for optimization
        /// </summary>
        private SortedDictionary<int,GeneticAlgoParameters> _optimzationParameters;
        
        /// <summary>
        /// Holds reference of user selected custom strategy
        /// </summary>
        private Type _strategyType;


        /// <summary>
        /// Iterations of the GA to be run
        /// </summary>
        private int _iterations;

        //create population size
        private int _populationSize;

        /// <summary>
        /// Argument Constrcutor
        /// </summary>
        /// <param name="strategyType">Type of custom strategy used</param>
        /// <param name="ctorArgs">Constructor arguments to be used for given strategy</param>
        /// <param name="optimzationParameters">Parameters to be used for optimizing the strategy</param>
        public OptimizeStrategyGeneticAlgo(Type strategyType, object[] ctorArgs, SortedDictionary<int, GeneticAlgoParameters> optimzationParameters,int iterations,int populationSize)
        {
            _strategyType = strategyType;
            _ctorArgs = ctorArgs;
            _optimzationParameters = optimzationParameters;
            _iterations = iterations;
            _populationSize = populationSize;
        }

        /// <summary>
        /// Constructor arguments to use
        /// </summary>
        public object[] CtorArgs
        {
            get { return _ctorArgs; }
        }

        /// <summary>
        /// Contains info for the parameters to be used for optimization
        /// </summary>
        public SortedDictionary<int, GeneticAlgoParameters> OptimzationParameters
        {
            get { return _optimzationParameters; }
        }

        /// <summary>
        /// Holds reference of user selected custom strategy
        /// </summary>
        public Type StrategyType
        {
            get { return _strategyType; }
        }

        /// <summary>
        /// Contains info for Population Size
        /// </summary>
        public int PopulationSize
        {
            get { return _populationSize; }
            set { _populationSize = value; }
        }

        /// <summary>
        /// Contains info for Iterations.
        /// </summary>
        public int Iterations
        {
            get { return _iterations; }
            set { _iterations = value; }
        }

    }
}
