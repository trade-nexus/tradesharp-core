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


using TradeSharp.UI.Common.Constants;

namespace TradeSharp.UI.Common.ValueObjects
{
    /// <summary>
    /// Contains conditions which need to be met to generate price alerts
    /// </summary>
    public class PriceAlertCondition
    {
        /// <summary>
        /// Contains condition operator which needs to be applied in calculation
        /// </summary>
        private ConditionOperator _conditionOperator;

        /// <summary>
        /// Contains the price value at which the alert should be triggered
        /// </summary>
        private decimal _conditionPrice;

        /// <summary>
        /// Contains condition operator which needs to be applied in calculation
        /// </summary>
        public ConditionOperator ConditionOperator
        {
            get { return _conditionOperator; }
        }

        /// <summary>
        /// Contains the price value at which the alert should be triggered
        /// </summary>
        public decimal ConditionPrice
        {
            get { return _conditionPrice; }
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="conditionOperator">Contains condition operator which needs to be applied in calculation</param>
        /// <param name="conditionPrice">Contains the price value at which the alert should be triggered</param>
        public PriceAlertCondition(ConditionOperator conditionOperator, decimal conditionPrice)
        {
            _conditionOperator = conditionOperator;
            _conditionPrice = conditionPrice;
        }

        /// <summary>
        /// Evaluates the specified condition
        /// </summary>
        /// <param name="currentValue">Current value of the property for which the condition is specified</param>
        /// <returns></returns>
        public bool Evaluate(decimal currentValue)
        {
            return ApplyCondition(currentValue);
        }

        /// <summary>
        /// Translates the specified condition and applies to the current value
        /// </summary>
        /// <param name="currentValue">Current value of the property for which the condition is specified</param>
        /// <returns></returns>
        private bool ApplyCondition(decimal currentValue)
        {
            switch (_conditionOperator)
            {
                case ConditionOperator.Equals:
                    if (currentValue.Equals(_conditionPrice))
                        return true;
                    return false;
                case ConditionOperator.Greater:
                    if (currentValue >_conditionPrice)
                        return true;
                    return false;
                case ConditionOperator.Less:
                    if (currentValue < _conditionPrice)
                        return true;
                    return false;
                default:
                    return false;
            }
        }
    }
}
