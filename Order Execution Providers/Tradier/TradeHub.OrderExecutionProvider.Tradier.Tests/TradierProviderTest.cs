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
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.FactoryMethods;
using TradeHub.OrderExecutionProvider.Tradier.Provider;
using TradeHub.OrderExecutionProvider.Tradier.Utility;

namespace TradeHub.OrderExecutionProvider.Tradier.Tests
{
    [TestFixture]
    public class TradierProviderTest
    {
        private TradierOrderExecutionProvider _executionProvider;

        [SetUp]
        public void Setup()
        {
            _executionProvider = new TradierOrderExecutionProvider();
            _executionProvider.Start();
        }

        [TearDown]
        public void TearDown()
        {
            _executionProvider.Stop();
        }

        [Test]
        [Category("Integration")]
        public void SendLimitOrderAndCancelItEnsureItGetsCancelled()
        {
            bool cancellationArrived = false;
            Order cancelledOrder = null;
            LimitOrder limitOrder = OrderMessage.GenerateLimitOrder(new Security() { Symbol = "MSFT" }, OrderSide.BUY, 1,
                10, "Tradier");
            limitOrder.OrderTif = OrderTif.DAY;
            _executionProvider.SendLimitOrder(limitOrder);
            _executionProvider.CancellationArrived += delegate(Order order)
            {
                cancellationArrived = true;
                cancelledOrder = order;
            };
            _executionProvider.CancelLimitOrder(limitOrder);
            Assert.True(cancellationArrived);
            Assert.AreEqual(cancelledOrder.OrderID, limitOrder.OrderID);
        }

        [Test]
        [Category("Integration")]
        public void SendMarketOrderAndEnsureItGetsFilled()
        {
            ManualResetEvent resetEvent=new ManualResetEvent(false);
            bool executionArrived = false;
            Execution receivedExecution = null;
            MarketOrder marketOrder = OrderMessage.GenerateMarketOrder(new Security() { Symbol = "MSFT" }, OrderSide.BUY, 1,"Tradier");
            marketOrder.OrderTif = OrderTif.DAY;
            _executionProvider.ExecutionArrived += delegate(Execution execution)
            {
                executionArrived = true;
                receivedExecution = execution;
                resetEvent.Set();

            };
            _executionProvider.SendMarketOrder(marketOrder);
            resetEvent.WaitOne(10000);
            Assert.True(executionArrived);
            Assert.IsNotNull(receivedExecution);
            Assert.Greater(receivedExecution.Fill.AverageExecutionPrice,0);
            Assert.Greater(receivedExecution.Fill.ExecutionPrice, 0);
            Assert.AreEqual(receivedExecution.Fill.ExecutionSize, 1);
            Assert.AreEqual(ExecutionType.Fill, receivedExecution.Fill.ExecutionType);
        }
    }
}
