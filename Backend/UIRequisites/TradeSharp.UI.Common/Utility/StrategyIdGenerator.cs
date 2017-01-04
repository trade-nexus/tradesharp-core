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


using System.Collections.Generic;

namespace TradeSharp.UI.Common.Utility
{
    /// <summary>
    /// Creates Unique IDs for Strategy and Strategy Instances for a given session
    /// </summary>
    public static class StrategyIdGenerator
    {
        private const int MinStrategyValue = 0xAA;
        private const int MaxStrategyValue = 0xFF;
        private static int _strategyIdValue = MinStrategyValue - 1;

        private const int MinInstanceValue = 0xA000;
        private const int MaxInstanceValue = 0xF999;

        /// <summary>
        /// Contains mapping for Strategy ID to Instance IDs
        /// KEY = Strategy ID
        /// Value = Instance ID
        /// </summary>
        private static Dictionary<string, int> _strategyIdMap = new Dictionary<string, int>();

        /// <summary>
        /// Returns New Strategy ID
        /// </summary>
        /// <returns></returns>
        public static string GetStrategyKey()
        {
            // Create new Strategy ID
            if (_strategyIdValue < MaxStrategyValue)
            {
                _strategyIdValue++;
            }
            else
            {
                _strategyIdValue = MinStrategyValue;
            }

            // Convert to String representation
            string idGenerated = _strategyIdValue.ToString("X");

            // Add New Id to local Map
            _strategyIdMap.Add(idGenerated, MinInstanceValue);

            // Return new ID generated
            return idGenerated;
        }

        /// <summary>
        /// Returns New Strategy Instance ID for the given strategy
        /// </summary>
        /// <param name="strategyKey"></param>
        /// <returns></returns>
        public static string GetInstanceKey(string strategyKey)
        {
            int currentValue;

            // Get current value in use for the given strategy
            if (_strategyIdMap.TryGetValue(strategyKey, out currentValue))
            {
                // Create new Strategy Instance ID
                if (currentValue < MaxInstanceValue)
                {
                    currentValue++;
                }
                else
                {
                    currentValue = MinInstanceValue;
                }

                // Update Value in local Map
                _strategyIdMap[strategyKey] = currentValue;

                // Return new ID generated with the combination of Parent Strategy ID
                return strategyKey + "-" + currentValue.ToString("X");
            }

            // Empty String as ID should stop the further processes for Strategy Instance
            return string.Empty;
        }
    }
}
