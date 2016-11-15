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

namespace TradeHub.Common.Core.DomainModels.OrderDomain
{
    /// <summary>
    /// Contains PnL information
    /// </summary>
    public class ProfitLossStats
    {
        /// <summary>
        /// Overall Profit and Loss value
        /// </summary>
        private decimal _profitAndLoss = default(decimal);

        /// <summary>
        /// Trades responsible for the PnL value
        /// </summary>
        private IList<Trade> _trades;

        /// <summary>
        /// Overall Profit and Loss value
        /// </summary>
        public decimal ProfitAndLoss
        {
            get { return _profitAndLoss; }
        }

        /// <summary>
        /// Trades responsible for the PnL value
        /// </summary>
        public IList<Trade> Trades
        {
            get { return _trades; }
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="trades">Trades responsible for the PnL value</param>
        public ProfitLossStats(IList<Trade> trades)
        {
            // Save Instance
            _trades = trades;

            // Do calculation on initialization
            CalculatePnl();
        }

        /// <summary>
        /// Calculates Profit and Loss value from the given Trades list
        /// </summary>
        private void CalculatePnl()
        {
            // Traverse each Trade
            foreach (var trade in _trades)
            {
                // Sum PnL for all the individual Trades
                _profitAndLoss += trade.ProfitAndLoss;
            }
        }
    }
}
