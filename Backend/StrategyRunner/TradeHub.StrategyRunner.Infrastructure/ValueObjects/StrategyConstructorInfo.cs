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
    /// Contains Contructor details for the selected user strategy
    /// </summary>
    public class StrategyConstructorInfo
    {
        /// <summary>
        /// Holds parameter details for the custom strategy
        /// </summary>
        private ParameterInfo[] _parameterInfo;

        /// <summary>
        /// Holds reference of the User selected strategy Type(Contains TradeHubStrategy implementation)
        /// </summary>
        private Type _strategyType;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="parameterInfo">Contains Constructor Parameter details</param>
        /// <param name="strategyType">Type from user selected assembly containing TradeHubStrategy implementation</param>
        public StrategyConstructorInfo(ParameterInfo[] parameterInfo, Type strategyType)
        {
            _strategyType = strategyType;
            ParameterInfo = parameterInfo;
        }

        /// <summary>
        /// Gets/Sets parameter details for the custom strategy
        /// </summary>
        public ParameterInfo[] ParameterInfo
        {
            get { return _parameterInfo; }
            set { _parameterInfo = value; }
        }

        /// <summary>
        /// Holds reference of the User selected strategy Type(Contains TradeHubStrategy implementation)
        /// </summary>
        public Type StrategyType
        {
            get { return _strategyType; }
            set { _strategyType = value; }
        }

    }
}
