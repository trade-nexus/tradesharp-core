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
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataEngine.Client.Service;
using TradeHub.OrderExecutionEngine.Client.Service;

namespace TradeHub.SimulatedExchange.SimulatorControler.Test.Integration
{
    [TestFixture]
    public class TestMdeAndOee
    {
        private MarketDataEngineClient _marketDataEngineClient;
        private OrderExecutionEngineClient _orderExecutionEngine;

        [SetUp]
        public void Setup()
        {
            _marketDataEngineClient = new MarketDataEngineClient();
            _orderExecutionEngine = new OrderExecutionEngineClient();
            _marketDataEngineClient.Start();
            _orderExecutionEngine.Start();
        }

        [TearDown]
        public void Close()
        {
            _marketDataEngineClient.Shutdown();
        }

        [Test]
        public void TestWholeExchange()
        {
            int count = 0;
            _orderExecutionEngine.NewArrived += delegate(Order order)
                {
                    count++;
                };
            _orderExecutionEngine.ExecutionArrived += delegate(Execution execution)
                {
                    count++;
                };
            ManualResetEvent manualResetEvent = new ManualResetEvent(false);
            manualResetEvent.WaitOne(1000);
            _orderExecutionEngine.SendLoginRequest(new Login { OrderExecutionProvider = OrderExecutionProvider.SimulatedExchange });
            _marketDataEngineClient.SendLoginRequest(new Login { MarketDataProvider = MarketDataProvider.SimulatedExchange });
            manualResetEvent.WaitOne(3000);
            LimitOrder limitOrder = new LimitOrder("1", OrderSide.SELL, 10, OrderTif.DAY, "USD", new Security() { Symbol = "GOOG" }, OrderExecutionProvider.SimulatedExchange) { LimitPrice = 120 };
            _orderExecutionEngine.SendLimitOrderRequest(limitOrder);
            _marketDataEngineClient.SendLiveBarSubscriptionRequest(new BarDataRequest
            {
                BarFormat = BarFormat.TIME,
                Security = new Security { Symbol = "GOOG" },
                BarLength = 10,
                MarketDataProvider = MarketDataProvider.SimulatedExchange,
                Id = "1",
                BarPriceType = BarPriceType.BID
            });
            manualResetEvent.WaitOne(3000);
            Assert.AreEqual(2,count);
        }
    }
}
