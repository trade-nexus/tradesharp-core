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
using TradeHub.Common.Core.ValueObjects.MarketData;

namespace TradeHub.SimulatedExchange.Common.Interfaces
{
    /// <summary>
    /// Interface to be implemented by class responsible for communication with connecting modules
    /// </summary>
    public interface ICommunicationController
    {
        event Action MarketDataLoginRequest;
        event Action MarketDataLogoutRequest;
        event Action OrderExecutionLoginRequest;
        event Action OrderExecutionLogoutRequest;
        event Action<Subscribe> TickDataRequest;
        event Action<BarDataRequest> BarDataRequest;
        event Action<HistoricDataRequest> HistoricDataRequest;
        event Action<MarketOrder> MarketOrderRequest;
        event Action<LimitOrder> LimitOrderRequest;

        /// <summary>
        /// Starts MQ Server
        /// </summary>
        void Connect();

        /// <summary>
        /// Stop MQ Server
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Publishes Admin Level request response for Market Data
        /// </summary>
        void PublishMarketAdminMessageResponse(string response);

        /// <summary>
        /// Publishes Admin Level request response for Order Executions
        /// </summary>
        void PublishOrderAdminMessageResponse(string response);

        /// <summary>
        /// Publishes Tick Data
        /// </summary>
        void PublishTickData(Tick tick);

        /// <summary>
        /// Publishes Bar Data
        /// </summary>
        void PublishBarData(Bar bar);

        /// <summary>
        /// Publishes Bar Data
        /// </summary>
        void PublishHistoricData(HistoricBarData historicBarData);

        /// <summary>
        /// Publishes New Order status message
        /// </summary>
        void PublishNewOrder(Order order);

        /// <summary>
        /// Publishes Order Rejection
        /// </summary>
        void PublishOrderRejection(Rejection rejection);

        /// <summary>
        /// Publishes Order Executions
        /// </summary>
        void PublishExecutionOrder(Execution execution);
    }
}
