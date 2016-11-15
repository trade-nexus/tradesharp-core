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


﻿
using System.Threading;
using NUnit.Framework;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;

namespace TradeHub.SimulatedExchange.SimulatorControler.Test.Unit
{
    [TestFixture]
    public class SimulateLimitOrderTest
    {
        [Test]
        [Category("Unit")]
        public void TestLimitOrderLogic()
        {
            var manualBarEvent = new ManualResetEvent(false);
            string executionId = null;
            SimulateLimitOrder simulateLimitOrder=new SimulateLimitOrder();
            decimal executionPrice = 0;
            simulateLimitOrder.NewArrived += delegate(Order order)
                {
                    executionId = order.OrderID;
                };
            simulateLimitOrder.NewExecution += delegate(Execution orderExecution)
                {
                    executionPrice = orderExecution.Fill.ExecutionPrice;
                };
            manualBarEvent.WaitOne(500);
            LimitOrder limitOrder=new LimitOrder("1",OrderSide.SELL,10,OrderTif.DAY,"USD",new Security(){Symbol = "AAPL"},OrderExecutionProvider.SimulatedExchange){LimitPrice = 100};
            Bar bar=new Bar(new Security(){Symbol = "AAPL"},MarketDataProvider.SimulatedExchange,"123"){Low = 120,Close = 130};
            simulateLimitOrder.NewLimitOrderArrived(limitOrder);
            simulateLimitOrder.NewBarArrived(bar);
            manualBarEvent.WaitOne(500);
            Assert.AreEqual("1", executionId);
            Assert.AreEqual(100, executionPrice);

        }

        [Test]
        [Category("Unit")]
        public void TestLimitRejection()
        {
            var manualBarEvent = new ManualResetEvent(false);
            var count = 0;
            SimulateLimitOrder simulateLimitOrder = new SimulateLimitOrder();
            simulateLimitOrder.LimitOrderRejection += delegate(Rejection rejection)
                {
                    count = 1;
                };
            
            LimitOrder limitOrder = new LimitOrder("1", OrderSide.SELL, 10, OrderTif.DAY, "USD", new Security() { Symbol = "AAPL" }, OrderExecutionProvider.SimulatedExchange) { LimitPrice = 0 };
            simulateLimitOrder.NewLimitOrderArrived(limitOrder);
            manualBarEvent.WaitOne(500);
            Assert.AreEqual(1,count);
        }
    }
}
