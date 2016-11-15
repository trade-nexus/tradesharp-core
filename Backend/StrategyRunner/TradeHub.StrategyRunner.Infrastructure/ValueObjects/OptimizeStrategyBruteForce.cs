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
    /// Contains info for the parameters to be used when initiating optimization
    /// </summary>
    public class OptimizeStrategyBruteForce
    {
        /// <summary>
        /// Constructor arguments to use
        /// </summary>
        private object[] _ctorArgs;

        /// <summary>
        /// Parameters to use for creating iterations
        /// </summary>
        private Tuple<int, string, string>[] _conditionalParameters;

        /// <summary>
        /// Save constuctor parameter details for the selected strategy
        /// </summary>
        private readonly ParameterInfo[] _parmatersDetails;

        /// <summary>
        /// Holds reference of user selected custom strategy
        /// </summary>
        private Type _strategyType;

        /// <summary>
        /// Constructor arguments to use
        /// </summary>
        public object[] CtorArgs
        {
            get { return _ctorArgs; }
            set { _ctorArgs = value; }
        }

        /// <summary>
        /// Parameters to use for creating iterations
        /// </summary>
        public Tuple<int, string, string>[] ConditionalParameters
        {
            get { return _conditionalParameters; }
            set { _conditionalParameters = value; }
        }

        /// <summary>
        /// Holds reference of user selected custom strategy
        /// </summary>
        public Type StrategyType
        {
            get { return _strategyType; }
            set { _strategyType = value; }
        }

        /// <summary>
        /// Save constuctor parameter details for the selected strategy
        /// </summary>
        public ParameterInfo[] ParmatersDetails
        {
            get { return _parmatersDetails; }
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="ctorArgs">Constructor arguments to use</param>
        /// <param name="strategyType">Reference of user selected custom strategy</param>
        /// <param name="conditionalParameters">Parameters to use for creating iterations</param>
        /// <param name="parmatersDetails">Save constuctor parameter details for the selected strategy</param>
        public OptimizeStrategyBruteForce(object[] ctorArgs, Type strategyType, Tuple<int, string, string>[] conditionalParameters, ParameterInfo[] parmatersDetails)
        {
            _ctorArgs = ctorArgs;
            _strategyType = strategyType;
            _conditionalParameters = conditionalParameters;
            _parmatersDetails = parmatersDetails;
        }
    }
}
