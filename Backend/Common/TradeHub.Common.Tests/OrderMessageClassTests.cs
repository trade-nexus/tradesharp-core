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
using NUnit.Framework;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.FactoryMethods;

namespace TradeHub.Common.Tests
{
    [TestFixture]
    public class OrderMessageClassTests
    {
        [Test]
        [Category("Unit")]
        public void CreateMarketOrder_IfAllParametersAreOk_VerifyMarketOrderIsCreated()
        {
            MarketOrder marketOrder=OrderMessage.GenerateMarketOrder(new Security() {Symbol = "AAPL"}, OrderSide.BUY, 10,
                OrderExecutionProvider.SimulatedExchange);
            Assert.NotNull(marketOrder);
            Assert.IsNotNullOrEmpty(marketOrder.OrderID);
            Assert.AreEqual(marketOrder.OrderSide,OrderSide.BUY);
            Assert.AreEqual(marketOrder.OrderSize,10);
            Assert.AreEqual(marketOrder.OrderExecutionProvider,OrderExecutionProvider.SimulatedExchange);
        }

        [Test]
        [Category("Unit")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateMarketOrder_IfSizeIsZero_ExceptionWillBeThrown()
        {
            MarketOrder marketOrder = OrderMessage.GenerateMarketOrder(new Security() { Symbol = "AAPL" }, OrderSide.BUY, 0,
                OrderExecutionProvider.SimulatedExchange);
        }

        [Test]
        [Category("Unit")]
        public void CreateLimitOrder_IfAllParametersAreOk_VerifyLimitOrderIsCreated()
        {
            LimitOrder limitOrder = OrderMessage.GenerateLimitOrder(new Security() { Symbol = "AAPL" }, OrderSide.BUY, 10,100,
                OrderExecutionProvider.SimulatedExchange);
            Assert.NotNull(limitOrder);
            Assert.IsNotNullOrEmpty(limitOrder.OrderID);
            Assert.AreEqual(limitOrder.OrderSide, OrderSide.BUY);
            Assert.AreEqual(limitOrder.OrderSize, 10);
            Assert.AreEqual(limitOrder.LimitPrice,100);
            Assert.AreEqual(limitOrder.OrderExecutionProvider, OrderExecutionProvider.SimulatedExchange);
        }
        [Test]
        [Category("Unit")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateLimitOrder_IfPriceIsZero_ExceptionWillBeThrown()
        {
            LimitOrder limitOrder = OrderMessage.GenerateLimitOrder(new Security() { Symbol = "AAPL" }, OrderSide.BUY, 10, 0,
                OrderExecutionProvider.SimulatedExchange);
        }

    }
}
