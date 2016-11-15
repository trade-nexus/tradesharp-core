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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.StrategyRunner.Infrastructure.ValueObjects
{
    /// <summary>
    /// Contains info for the strategy to be optimized using Brute Force
    /// </summary>
    public class OptimizationParametersBruteForce
    {
        /// <summary>
        /// Holds constructor arguments. 
        /// Required to successfully initialize the given strategy assembly.
        /// </summary>
        private object[] _ctorArguments;

        /// <summary>
        /// Holds reference of user selected custom strategy
        /// </summary>
        private Type _strategyType;

        /// <summary>
        /// Contains info regarding the parameters
        /// </summary>
        private ParameterInfo[] _parameterDetails;

        /// <summary>
        /// Holds reference of user selected custom strategy
        /// </summary>
        public Type StrategyType
        {
            get { return _strategyType; }
        }

        /// <summary>
        /// Holds constructor arguments. 
        /// Required to successfully initialize the given strategy assembly.
        /// </summary>
        public object[] CtorArguments
        {
            get { return _ctorArguments; }
        }

        /// <summary>
        /// Contains info regarding the parameters
        /// </summary>
        public ParameterInfo[] ParameterDetails
        {
            get { return _parameterDetails; }
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="strategyType">User defined custom strategy</param>
        /// <param name="ctorArguments">Constructor arguments required to initialize given strategy</param>
        /// <param name="parameterDetails">Contains info regarding the parameters</param>
        public OptimizationParametersBruteForce(Type strategyType, object[] ctorArguments, ParameterInfo[] parameterDetails)
        {
            _strategyType = strategyType;
            _ctorArguments = ctorArguments;
            _parameterDetails = parameterDetails;
        }
    }
}
