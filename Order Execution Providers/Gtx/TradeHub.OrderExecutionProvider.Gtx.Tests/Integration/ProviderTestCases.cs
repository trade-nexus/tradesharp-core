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
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.OrderExecutionProvider.Gtx.Provider;
using Constants = TradeHub.Common.Core.Constants;

namespace TradeHub.OrderExecutionProvider.Gtx.Tests.Integration
{
    [TestFixture]
    public class OrderExecutionProviderTestCases
    {
        private GtxOrderExecutionProvider _executionProvider;

        [SetUp]
        public void SetUp()
        {
            _executionProvider = new GtxOrderExecutionProvider();
        }

        [Test]
        [Category("Integration")]
        public void ConnectOrderExecutionProviderTestCase()
        {
            bool isConnected = false;
            var manualLogonEvent = new ManualResetEvent(false);

            _executionProvider.LogonArrived +=
                    delegate(string obj)
                    {
                        isConnected = true;
                        manualLogonEvent.Set();
                    };

            _executionProvider.Start();
            manualLogonEvent.WaitOne(30000, false);

            Assert.AreEqual(true, isConnected);
        }

        [Test]
        [Category("Integration")]
        public void DisconnectOrderExecutionProviderTestCase()
        {
            bool isConnected = false;
            var manualLogonEvent = new ManualResetEvent(false);
            _executionProvider.LogonArrived +=
                    delegate(string obj)
                    {
                        isConnected = true;
                        _executionProvider.Stop();
                        manualLogonEvent.Set();
                    };

            bool isDisconnected = false;
            var manualLogoutEvent = new ManualResetEvent(false);
            _executionProvider.LogoutArrived +=
                    delegate(string obj)
                    {
                        isDisconnected = true;
                        manualLogoutEvent.Set();
                    };

            _executionProvider.Start();
            manualLogonEvent.WaitOne(30000, false);
            manualLogoutEvent.WaitOne(30000, false);

            Assert.AreEqual(true, isConnected, "Connected");
            Assert.AreEqual(true, isDisconnected, "Disconnected");
        }

        [Test]
        [Category("Integration")]
        public void MarketOrder_OrderExecutionProviderTestCase()
        {
            MarketOrder marketOrder = new MarketOrder(Constants.OrderExecutionProvider.Gtx)
                {
                    OrderID = "5000",
                    Security = new Security{Symbol = "EURUSD"},
                    OrderSide = Constants.OrderSide.BUY,
                    OrderSize = 1000
                };

            bool isConnected = false;
            bool isDisconnected = false;
            bool newArrived = false;
            bool executionArrived = false;

            var manualLogoutEvent = new ManualResetEvent(false);
            var manualLogonEvent = new ManualResetEvent(false);
            var manualNewEvent = new ManualResetEvent(false);
            var manualExecutionEvent = new ManualResetEvent(false);
            
            _executionProvider.LogonArrived +=
                    delegate(string obj)
                    {
                        isConnected = true;
                        _executionProvider.SendMarketOrder(marketOrder);
                        manualLogonEvent.Set();
                    };

            _executionProvider.NewArrived += delegate(Order order)
                {
                    newArrived = true;
                    manualNewEvent.Set();
                };

            _executionProvider.ExecutionArrived += delegate(Execution order)
                {
                    if (order.Fill.LeavesQuantity.Equals(0))
                    {
                        executionArrived = true;
                        _executionProvider.Stop();
                        manualExecutionEvent.Set();   
                    }
                };

            _executionProvider.LogoutArrived +=
                    delegate(string obj)
                    {
                        isDisconnected = true;
                        manualLogoutEvent.Set();
                    };

            _executionProvider.Start();

            manualLogonEvent.WaitOne(30000, false);
            manualNewEvent.WaitOne(30000, false);
            manualExecutionEvent.WaitOne(30000, false);
            manualLogoutEvent.WaitOne(30000, false);

            Assert.AreEqual(true, isConnected, "Connected");
            Assert.AreEqual(true, newArrived, "New Arrived");
            Assert.AreEqual(true, executionArrived, "Execution Arrived");
            Assert.AreEqual(true, isDisconnected, "Disconnected");
        }

        [Test]
        [Category("Integration")]
        public void LimitOrder_OrderExecutionProviderTestCase()
        {
            LimitOrder limitOrder = new LimitOrder(Constants.OrderExecutionProvider.Gtx)
            {
                OrderID = "5000",
                Security = new Security { Symbol = "EURUSD" },
                OrderSide = Constants.OrderSide.SELL,
                OrderSize = 1000,
                LimitPrice = (decimal) 430.10
            };

            bool isConnected = false;
            bool isDisconnected = false;
            bool newArrived = false;
            bool cancellationArrived = false;

            var manualLogoutEvent = new ManualResetEvent(false);
            var manualLogonEvent = new ManualResetEvent(false);
            var manualNewEvent = new ManualResetEvent(false);
            var manualCancellationEvent = new ManualResetEvent(false);

            _executionProvider.LogonArrived +=
                    delegate(string obj)
                    {
                        isConnected = true;
                        _executionProvider.SendLimitOrder(limitOrder);
                        manualLogonEvent.Set();
                    };

            _executionProvider.NewArrived += delegate(Order order)
            {
                newArrived = true;
                limitOrder.BrokerOrderID = order.BrokerOrderID;
                _executionProvider.CancelLimitOrder(limitOrder);
                manualNewEvent.Set();
            };

            _executionProvider.CancellationArrived += delegate(Order order)
            {
                cancellationArrived = true;
                _executionProvider.Stop();
                manualCancellationEvent.Set();
            };
            
            _executionProvider.LogoutArrived +=
                    delegate(string obj)
                    {
                        isDisconnected = true;
                        manualLogoutEvent.Set();
                    };

            _executionProvider.Start();

            manualLogonEvent.WaitOne(30000, false);
            manualNewEvent.WaitOne(30000, false);
            manualCancellationEvent.WaitOne(30000, false);
            manualLogoutEvent.WaitOne(30000, false);

            Assert.AreEqual(true, isConnected, "Connected");
            Assert.AreEqual(true, newArrived, "New Arrived");
            Assert.AreEqual(true, cancellationArrived, "Cancellation Arrived");
            Assert.AreEqual(true, isDisconnected, "Disconnected");
        }
    }
}
