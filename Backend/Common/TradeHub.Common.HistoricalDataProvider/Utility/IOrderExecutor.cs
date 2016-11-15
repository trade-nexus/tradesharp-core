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
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;

namespace TradeHub.Common.HistoricalDataProvider.Utility
{
    /// <summary>
    /// Blue print for creating an Order Executor to be used with Backtesting
    /// </summary>
    public interface IOrderExecutor
    {
        event Action<Order> NewOrderArrived;
        event Action<Execution> ExecutionArrived;
        event Action<Order> CancellationArrived;
        event Action<Rejection> RejectionArrived;

        /// <summary>
        /// Called when new tick is recieved
        /// </summary>
        /// <param name="tick">TradeHub Tick</param>
        void TickArrived(Tick tick);

        /// <summary>
        /// Called when new Bar is received
        /// </summary>
        /// <param name="bar">TradeHub Bar</param>
        void BarArrived(Bar bar);

        /// <summary>
        /// Called when new market order is received
        /// </summary>
        /// <param name="marketOrder">TardeHub MarketOrder</param>
        void NewMarketOrderArrived(MarketOrder marketOrder);

        /// <summary>
        /// Called when new limit order is recieved
        /// </summary>
        /// <param name="limitOrder">TradeHub LimitOrder</param>
        void NewLimitOrderArrived(LimitOrder limitOrder);

        /// <summary>
        /// Called when new cancel order request is recieved
        /// </summary>
        /// <param name="cancelOrder"></param>
        void CancelOrderArrived(Order cancelOrder);

        /// <summary>
        /// Clear necessary resources
        /// </summary>
        void Clear();
    }
}
