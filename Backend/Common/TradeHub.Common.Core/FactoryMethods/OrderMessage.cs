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
using TradeHub.Common.Core.Assertions;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;

namespace TradeHub.Common.Core.FactoryMethods
{
    /// <summary>
    /// Returns desired Order Objects
    /// </summary>
    public static class OrderMessage
    {
        /// <summary>
        /// Creates new Market Order
        /// </summary>
        /// <param name="orderId">Unqiue Order ID</param>
        /// <param name="security">Contains symbol info on which to trade</param>
        /// <param name="orderSide">Order side</param>
        /// <param name="orderSize">Size of the given order</param>
        /// <param name="orderExecutionProvider">Name of the ordeer execution provider</param>
        /// <returns>TradeHub MarketOrder Object</returns>
        public static MarketOrder GenerateMarketOrder(string orderId, Security security, string orderSide, int orderSize, string orderExecutionProvider)
        {
            AssertionConcern.AssertNullOrEmptyString(orderId,"OrderId cannot be null or empty");
            ValidateBasicOrderParameters(security,orderSide,orderSize,orderExecutionProvider);
            MarketOrder marketOrder = new MarketOrder(orderExecutionProvider)
                {
                    OrderID = orderId,
                    Security = security,
                    OrderSize = orderSize,
                    OrderSide = orderSide
                };

            return marketOrder;
        }

        /// <summary>
        /// Creates new Market Order including unique OrderId
        /// </summary>
        /// <param name="security">Contains symbol info on which to trade</param>
        /// <param name="orderSide">Order side</param>
        /// <param name="orderSize">Size of the given order</param>
        /// <param name="orderExecutionProvider">Name of the ordeer execution provider</param>
        /// <returns>TradeHub MarketOrder Object</returns>
        public static MarketOrder GenerateMarketOrder(Security security, string orderSide, int orderSize, string orderExecutionProvider)
        {
            ValidateBasicOrderParameters(security, orderSide, orderSize, orderExecutionProvider);
            MarketOrder marketOrder = new MarketOrder(orderExecutionProvider)
            {
                OrderID = Guid.NewGuid().ToString(),
                Security = security,
                OrderSize = orderSize,
                OrderSide = orderSide
            };

            return marketOrder;
        }

        /// <summary>
        /// Creates new Limit Order
        /// </summary>
        /// <param name="orderId">Unqiue Order ID</param>
        /// <param name="security">Contains symbol info on which to trade</param>
        /// <param name="orderSide">Order side</param>
        /// <param name="orderSize">Size of the given order</param>
        /// <param name="limitPrice">Limit price for the given order</param>
        /// <param name="orderExecutionProvider">Name of the ordeer execution provider</param>
        /// <returns>TradeHub LimitOrder Object</returns>
        public static LimitOrder GenerateLimitOrder(string orderId, Security security, string orderSide, int orderSize, decimal limitPrice, string orderExecutionProvider)
        {
            AssertionConcern.AssertNullOrEmptyString(orderId, "OrderId cannot be null or empty");
            ValidateBasicOrderParameters(security, orderSide, orderSize, orderExecutionProvider);
            ValidateLimitOrderPrice(limitPrice);
            LimitOrder limitOrder = new LimitOrder(orderExecutionProvider)
            {
                OrderID = orderId,
                Security = security,
                OrderSize = orderSize,
                OrderSide = orderSide,
                LimitPrice = limitPrice
            };

            return limitOrder;
        }

        /// <summary>
        /// Creates new Limit Order including unique OrderID
        /// </summary>
        /// <param name="security">Contains symbol info on which to trade</param>
        /// <param name="orderSide">Order side</param>
        /// <param name="orderSize">Size of the given order</param>
        /// <param name="limitPrice">Limit price for the given order</param>
        /// <param name="orderExecutionProvider">Name of the ordeer execution provider</param>
        /// <returns>TradeHub LimitOrder Object</returns>
        public static LimitOrder GenerateLimitOrder(Security security, string orderSide, int orderSize, decimal limitPrice, string orderExecutionProvider)
        {
            ValidateBasicOrderParameters(security, orderSide, orderSize, orderExecutionProvider);
            ValidateLimitOrderPrice(limitPrice);
            LimitOrder limitOrder = new LimitOrder(orderExecutionProvider)
            {
                OrderID = Guid.NewGuid().ToString(),
                Security = security,
                OrderSize = orderSize,
                OrderSide = orderSide,
                LimitPrice = limitPrice
            };

            return limitOrder;
        }

        
        /// <summary>
        /// validate limit order price
        /// </summary>
        private static void ValidateLimitOrderPrice(decimal limitPrice)
        {
            AssertionConcern.AssertGreaterThanZero(limitPrice,"Limit Price should be greater than 0");
        }

        /// <summary>
        /// validate basic order parameters
        /// </summary>
        private static void ValidateBasicOrderParameters(Security security, string orderSide, int orderSize, string orderExecutionProvider)
        {
            AssertionConcern.AssertArgumentNotNull(security,"Security cannot be null");
            AssertionConcern.AssertNullOrEmptyString(orderSide,"Order Side cannot be null or empty");
            AssertionConcern.AssertGreaterThanZero(orderSize,"Order Size must be greater than 0");
            AssertionConcern.AssertNullOrEmptyString(orderExecutionProvider, "Order Execution Provider cannot be null or empty");
        }
    }
}
