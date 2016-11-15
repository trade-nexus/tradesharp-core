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

namespace TradeHub.StrategyRunner.UserInterface.ParametersModule.ValueObjects
{
    /// <summary>
    /// Contains info for individual ctor arguments parameters
    /// </summary>
    public class ParameterInfo
    {
        /// <summary>
        /// Name of the parameter
        /// </summary>
        private string _parameter;

        /// <summary>
        /// Parameter value
        /// </summary>
        private string _value;

        /// <summary>
        /// Location in Ctor arguments array
        /// </summary>
        private int _index;

        /// <summary>
        /// End Point for the range defined if the parameter is to be used in optimization iterations
        /// </summary>
        private string _endPoint;

        /// <summary>
        /// Increment value to be used to get the to end point starting from the actual value
        /// </summary>
        private string _increment;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ParameterInfo()
        {
            _parameter = string.Empty;
            _value = string.Empty;
            _index = 0;
            _endPoint = string.Empty;
            _increment = string.Empty;
        }

        /// <summary>
        /// Name of the parameter
        /// </summary>
        public string Parameter
        {
            get { return _parameter; }
            set { _parameter = value; }
        }

        /// <summary>
        /// Parameter value
        /// </summary>
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// Location in Ctor arguments array
        /// </summary>
        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        /// <summary>
        /// End Point for the range defined if the parameter is to be used in optimization iterations
        /// </summary>
        public string EndPoint
        {
            get { return _endPoint; }
            set { _endPoint = value; }
        }

        /// <summary>
        /// Increment value to be used to get the to end point starting from the actual value
        /// </summary>
        public string Increment
        {
            get { return _increment; }
            set { _increment = value; }
        }
    }
}
