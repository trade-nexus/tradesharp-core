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

namespace TradeSharp.UI.Common.ValueObjects
{
    public class BruteForceParameterDetail : ParameterDetail
    {
        /// <summary>
        /// Parameter descirption
        /// </summary>
        private string _description;

        /// <summary>
        /// Range end point to corresponde with the parameter value
        /// </summary>
        private object _endValue;

        /// <summary>
        /// Increment to be used to move from parameter value to end value
        /// </summary>
        private double _increment;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="description">Parameter descirption</param>
        /// <param name="parameterType">Type of the parameter i.e. Int32, Decimal, String</param>
        /// <param name="parameterValue">Value for the given parameter value</param>
        /// <param name="endValue">Range end point to corresponde with the parameter value</param>
        /// <param name="increment">Increment to be used to move from parameter value to end value</param>
        public BruteForceParameterDetail(string description, Type parameterType, object parameterValue, object endValue, double increment)
            : base(parameterType, parameterValue)
        {
            _description = description;
            _endValue = endValue;
            _increment = increment;
        }

        /// <summary>
        /// Range end point to corresponde with the parameter value
        /// </summary>
        public object EndValue
        {
            get { return _endValue; }
            set { _endValue = value; }
        }

        /// <summary>
        /// Increment to be used to move from parameter value to end value
        /// </summary>
        public double Increment
        {
            get { return _increment; }
            set { _increment = value; }
        }

        /// <summary>
        /// Parameter descirption
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
