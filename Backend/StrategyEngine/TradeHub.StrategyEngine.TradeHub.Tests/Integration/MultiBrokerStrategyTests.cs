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
using TradeHub.StrategyEngine.MultiBrokerStrategy;
using TradeHub.StrategyEngine.Testing.SimpleStrategy;

namespace TradeHub.StrategyEngine.TradeHub.Tests.Integration
{
    [TestFixture]
    public class MultiBrokerStrategyTests
    {
        private MultiBrokerStrategy.MultiBrokerTestStrategy _multiBrokerHubStrategy;

        [SetUp]
        public void StartUp()
        {
            //_multiBrokerHubStrategy = new MultiBrokerStrategy(new[] {MarketDataProvider.Fxcm, MarketDataProvider.Simulated},null, null);
        }

        [TearDown]
        public void Close()
        {
            if (_multiBrokerHubStrategy != null)
            {
                _multiBrokerHubStrategy.Stop();
                _multiBrokerHubStrategy.Dispose();
            }
        }

        [Test]
        [Category("Integration")]
        public void MultiMarketDataLoginTest()
        {
            string providerOne = MarketDataProvider.Fxcm;
            string providerTwo = MarketDataProvider.Simulated;

            _multiBrokerHubStrategy = new MultiBrokerTestStrategy("EUR/USD", "ERX", providerOne, providerTwo, providerOne, providerTwo);

            var logonEvent = new ManualResetEvent(false);

            int loginCount = 0;

            _multiBrokerHubStrategy.MarketDataLogonArrived += delegate(string marketDataProvider)
            {
                loginCount++;
                if (loginCount==2)
                {
                    logonEvent.Set();
                }
            };

            logonEvent.WaitOne(14000, false);

            Assert.AreEqual(2, loginCount, "Login Count");
        }

        [Test]
        [Category("Integration")]
        public void MultiMarketDataTickTest()
        {
            string providerOne = MarketDataProvider.Fxcm;
            string providerTwo = MarketDataProvider.Blackwood;

            _multiBrokerHubStrategy = new MultiBrokerTestStrategy("EUR/USD", "ERX", providerOne, providerTwo, providerOne, providerTwo);

            var logonEvent = new ManualResetEvent(false);
            var tickEvent = new ManualResetEvent(false);

            int loginCount = 0;

            bool providerOneTickArrived = false;
            bool providerTwoTickArrived = false;

            _multiBrokerHubStrategy.MarketDataLogonArrived += delegate(string marketDataProvider)
            {
                loginCount++;
                if (loginCount == 2)
                {
                    logonEvent.Set();
                }
            };

            _multiBrokerHubStrategy.TickArrived += delegate(Tick tick)
            {
                if (tick.MarketDataProvider.Equals(providerOne))
                    providerOneTickArrived = true;
                else
                    providerTwoTickArrived = true;

                Console.WriteLine(tick);

                if (providerOneTickArrived && providerTwoTickArrived)
                {
                    tickEvent.Set();
                }
            };

            logonEvent.WaitOne(14000, false);
            tickEvent.WaitOne(14000, false);

            Assert.AreEqual(2, loginCount, "Login Count");
            Assert.IsTrue(providerOneTickArrived, "Provider One Tick Arrived");
            Assert.IsTrue(providerTwoTickArrived, "Provider Two Tick Arrived");
        }

        [Test]
        [Category("Integration")]
        public void MultiOrderExecutionLoginTest()
        {
            string providerOne = MarketDataProvider.Fxcm;
            string providerTwo = MarketDataProvider.Blackwood;

            _multiBrokerHubStrategy = new MultiBrokerTestStrategy("EUR/USD", "ERX", providerOne, providerTwo, providerOne, providerTwo);

            var logonEvent = new ManualResetEvent(false);

            int loginCount = 0;

            _multiBrokerHubStrategy.OrderExecutionLogonArrived += delegate(string marketDataProvider)
            {
                loginCount++;
                if (loginCount == 2)
                {
                    logonEvent.Set();
                }
            };

            logonEvent.WaitOne(14000, false);

            Assert.AreEqual(2, loginCount, "Login Count");
        }

        [Test]
        [Category("Integration")]
        public void MultiOrderExecutionOrderTest()
        {
            string providerOne = MarketDataProvider.Fxcm;
            string providerTwo = MarketDataProvider.Blackwood;

            _multiBrokerHubStrategy = new MultiBrokerTestStrategy("EUR/USD", "ERX", providerOne, providerTwo, providerOne, providerTwo);

            var logonEvent = new ManualResetEvent(false);
            var orderEvent = new ManualResetEvent(false);

            int loginCount = 0;

            bool providerOneOrderExecuted = false;
            bool providerTwoOrderExecuted = false;

            _multiBrokerHubStrategy.MarketDataLogonArrived += delegate(string marketDataProvider)
            {
                loginCount++;
                if (loginCount == 2)
                {
                    logonEvent.Set();
                }
            };

            _multiBrokerHubStrategy.OnNewExecutionReceived += delegate(Execution execution)
            {
                if (execution.OrderExecutionProvider.Equals(providerOne))
                    providerOneOrderExecuted = true;
                else
                    providerTwoOrderExecuted = true;

                Console.WriteLine(execution);

                if (providerOneOrderExecuted && providerTwoOrderExecuted)
                {
                    orderEvent.Set();
                }
            };

            logonEvent.WaitOne(14000, false);
            orderEvent.WaitOne(14000, false);

            Assert.AreEqual(2, loginCount, "Login Count");
            Assert.IsTrue(providerOneOrderExecuted, "Provider One Order Executed");
            Assert.IsTrue(providerTwoOrderExecuted, "Provider Two Order Executed");
        }
    }
}
