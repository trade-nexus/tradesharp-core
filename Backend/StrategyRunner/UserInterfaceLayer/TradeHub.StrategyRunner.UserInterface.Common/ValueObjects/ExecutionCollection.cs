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
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.StrategyRunner.UserInterface.Common.Interface;

namespace TradeHub.StrategyRunner.UserInterface.Common.ValueObjects
{
    /// <summary>
    /// Contains order executions
    /// </summary>
    public class ExecutionCollection: IItemsProvider<Execution>
    {
        private IList<Execution> _executionList;

        public ExecutionCollection()
        {
            _executionList = new List<Execution>();
        }

        #region Implementation of IItemsProvider<Execution>

        /// <summary>
        /// Fetches the total number of items available.
        /// </summary>
        /// <returns></returns>
        public int FetchCount()
        {
            return _executionList.Count;
        }

        /// <summary>
        /// Fetches a range of items.
        /// </summary>
        /// <param name="startIndex">The start index.</param>
        /// <param name="count">The number of items to fetch.</param>
        /// <returns></returns>
        public IList<Execution> FetchRange(int startIndex, int count)
        {
            IList<Execution> list = new List<Execution>();
            for (int i = startIndex; i < startIndex + count && i < _executionList.Count; i++)
            {
                list.Add(_executionList[i]);
            }

            return list;
        }

        /// <summary>
        /// Adds new Item to the collection
        /// </summary>
        /// <param name="item"></param>
        public void AddItem(Execution item)
        {
            _executionList.Add(item);
        }

        #endregion
    }
}
