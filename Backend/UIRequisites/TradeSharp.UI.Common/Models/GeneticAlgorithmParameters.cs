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


using System;
using System.Collections.Generic;
using TradeSharp.UI.Common.ValueObjects;

namespace TradeSharp.UI.Common.Models
{
    /// <summary>
    /// Contains details for all the Parameters to be used for Genetic Optimization
    /// </summary>
    public class GeneticAlgorithmParameters
    {
        /// <summary>
        /// Constructor arguments to use
        /// </summary>
        private object[] _ctorArgs;

        /// <summary>
        /// Contains info for the parameters to be used for optimization
        /// </summary>
        private SortedDictionary<int,OptimizationParameterDetail> _optimzationParameters;
        
        /// <summary>
        /// Holds reference of user selected custom strategy
        /// </summary>
        private Type _strategyType;

        /// <summary>
        /// Iterations of the GA to be run
        /// </summary>
        private int _iterations;

        /// <summary>
        /// Population size to be used in Genetic Algorithm working
        /// </summary>
        private int _populationSize;

        /// <summary>
        /// No. of rounds Genetic Algorithm should run
        /// </summary>
        private int _rounds;

        /// <summary>
        /// Argument Constrcutor
        /// </summary>
        /// <param name="strategyType">Type of custom strategy used</param>
        /// <param name="ctorArgs">Constructor arguments to be used for given strategy</param>
        /// <param name="optimzationParameters">Parameters to be used for optimizing the strategy</param>
        /// <param name="iterations">No. of iterations to be executed</param>
        /// <param name="populationSize">Population size to be used in Genetic Algorithm working</param>
        /// <param name="rounds">No. of rounds Genetic Algorithm should run</param>
        public GeneticAlgorithmParameters(Type strategyType, object[] ctorArgs,
            SortedDictionary<int, OptimizationParameterDetail> optimzationParameters, int iterations,
            int populationSize, int rounds)
        {
            _strategyType = strategyType;
            _ctorArgs = ctorArgs;
            _optimzationParameters = optimzationParameters;
            _iterations = iterations;
            _populationSize = populationSize;
            _rounds = rounds;
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
        public SortedDictionary<int, OptimizationParameterDetail> OptimzationParameters
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
        /// Population size to be used in Genetic Algorithm working
        /// </summary>
        public int PopulationSize
        {
            get { return _populationSize; }
            set { _populationSize = value; }
        }

        /// <summary>
        /// Iterations of the GA to be run
        /// </summary>
        public int Iterations
        {
            get { return _iterations; }
            set { _iterations = value; }
        }

        /// <summary>
        /// No. of rounds Genetic Algorithm should run
        /// </summary>
        public int Rounds
        {
            get { return _rounds; }
            set { _rounds = value; }
        }
    }
}
