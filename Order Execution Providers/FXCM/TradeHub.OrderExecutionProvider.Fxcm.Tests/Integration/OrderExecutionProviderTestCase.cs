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
using TradeHub.OrderExecutionProvider.Fxcm.Provider;

namespace TradeHub.OrderExecutionProvider.Fxcm.Tests.Integration
{
    [TestFixture]
    class OrderExecutionProviderTestCase
    {
        FxcmOrderExecutionProvider _provider = new FxcmOrderExecutionProvider();

        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        [Category("Integration")]
        public void ConnectionTestCase()
        {
            bool logon = false;
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            _provider.LogonArrived += delegate(string dataProvider)
            {
                logon = true;
                resetEvent.Set();
            };

            //start provider
            _provider.Start();
            resetEvent.WaitOne(10000);
            Assert.True(logon, "Logon Not Arrived");

            if (logon)
            {
                bool logout = false;
                _provider.LogoutArrived += delegate(string dataProvider)
                {
                    logout = true;
                    resetEvent.Set();
                };
                resetEvent.Reset();
                _provider.Stop();
                resetEvent.WaitOne(5000);
                Assert.True(logout);
            }
        }

        [Test]
        [Category("Integration")]
        public void MarketOrderTestCase()
        {
            MarketOrder order = OrderMessage.GenerateMarketOrder(DateTime.Now.ToString("yyMMddHmsfff"),
                new Security() {Symbol = "EUR/USD"}, OrderSide.BUY, 1000,
                "Fxcm");
            order.OrderCurrency = "EUR";
            order.OrderTif = "GTC";

            bool logon = false;
            ManualResetEvent resetEvent = new ManualResetEvent(false);

            _provider.LogonArrived += delegate(string dataProvider)
            {
                logon = true;
                resetEvent.Set();
            };

            //start provider
            _provider.Start();

            resetEvent.WaitOne(5000);

            Assert.True(logon, "Logon Arrived");

            if (logon)
            {
                bool newArrived = false;
                bool executionArrived = false;

                _provider.NewArrived += delegate(Order newOrder)
                {
                    newArrived = true;
                };

                _provider.ExecutionArrived += delegate(Execution execution)
                {
                    executionArrived = true;
                };

                _provider.SendMarketOrder(order);

                resetEvent.Reset();
                resetEvent.WaitOne(10000);

                bool logout = false;

                _provider.LogoutArrived += delegate(string dataProvider)
                {
                    logout = true;
                    resetEvent.Set();
                };

                resetEvent.Reset();

                _provider.Stop();

                resetEvent.WaitOne(5000);

                Assert.True(logout, "Logout Arrived");
                Assert.True(newArrived, "New Arrived");
                Assert.True(executionArrived, "Execution Arrived");
            }
        }

        [Test]
        [Category("Integration")]
        public void CancelOrderTestCase()
        {
            LimitOrder order = OrderMessage.GenerateLimitOrder(DateTime.Now.ToString("yyMMddHmsfff"), new Security() { Symbol = "EUR/USD" }, OrderSide.BUY, 1000, 1.090M,
                "Fxcm");

            order.OrderCurrency = "EUR";
            order.OrderTif = "GTC";
            
            bool logon = false;

            ManualResetEvent resetEvent = new ManualResetEvent(false);
            _provider.LogonArrived += delegate(string dataProvider)
            {
                logon = true;
                resetEvent.Set();
            };
            //start provider
            _provider.Start();
            resetEvent.WaitOne(5000);
            Assert.True(logon);
            if (logon)
            {
                bool newArrived = false;
                bool cancellationArrived = false;
                bool logout = false;

                _provider.NewArrived += delegate(Order newOrder)
                {
                    newArrived = true;
                    _provider.CancelLimitOrder(newOrder);
                };
                
                _provider.CancellationArrived += delegate(Order cancelledOrder)
                {
                    cancellationArrived = true;
                    resetEvent.Set();
                };
               
                _provider.SendLimitOrder(order);
                resetEvent.Reset();
                resetEvent.WaitOne(30000);

                _provider.LogoutArrived += delegate(string dataProvider)
                {
                    logout = true;
                    resetEvent.Set();
                };

                resetEvent.Reset();
                _provider.Stop();
                resetEvent.WaitOne(5000);
                Assert.True(logout, "Logout Arrived");
                Assert.True(newArrived, "New Arrived");
                Assert.True(cancellationArrived, "Cancellation Arrived");
            }
        }
    }
}
