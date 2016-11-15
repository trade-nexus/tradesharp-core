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
using System;
using System.Threading;
using NUnit.Framework;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataEngine.Client.Service;
using TradeHub.MarketDataProvider.SimulatedExchange.Provider;

namespace TradeHub.MarketDataProvider.SimulatedExchange.Test.Integration
{

    [TestFixture]
    public class SimulatedExchangeMarketDataTest
    {
        private Type _type = typeof(SimulatedExchangeMarketDataTest);

        private MarketDataEngineClient _marketDataEngineClient;
        private SimulatedExchangeMarketDataProvider _marketDataProvider;

        [SetUp]
        public void Setup()
        {
            _marketDataProvider= new SimulatedExchangeMarketDataProvider();
            //_marketDataEngineClient = new MarketDataEngineClient();
            //_marketDataEngineClient.Initialize();
        }

        [TearDown]
        public void Close()
        {
            _marketDataProvider.Stop();
            //_marketDataEngineClient.Shutdown();
        }

        //[Test]
        public void LoginTestCase()
        {
            var manualBarEvent = new ManualResetEvent(false);
            _marketDataEngineClient.LiveBarArrived += Console.WriteLine;
            manualBarEvent.WaitOne(1000);

            _marketDataEngineClient.SendLoginRequest(new Login {MarketDataProvider = Common.Core.Constants.MarketDataProvider.SimulatedExchange});
           // Assert.AreEqual(true, logoutArrived, "Logout Arrived");
            manualBarEvent.WaitOne(3000);
            _marketDataEngineClient.SendLiveBarSubscriptionRequest(new BarDataRequest
                {
                    BarFormat = Common.Core.Constants.BarFormat.TIME,
                    Security = new Security {Symbol = "GOOG"},
                    BarLength = 10,
                    MarketDataProvider = Common.Core.Constants.MarketDataProvider.SimulatedExchange,
                    Id = "1",
                    BarPriceType = Common.Core.Constants.BarPriceType.BID
                });

            manualBarEvent.WaitOne(10000);
        }


        [Test]
        [Category("Integration")]
        public void SubscribeMarketDataProviderTestCase()
        {
            bool isConnected = false;
            bool tickArrived = false;

            var manualLogonEvent = new ManualResetEvent(false);
            var manualTickEvent = new ManualResetEvent(false);

            _marketDataProvider.LogonArrived +=
                    delegate(string obj)
                    {
                        isConnected = true;
                        _marketDataProvider.SubscribeTickData(new Subscribe() { Security = new Security() { Symbol = "AAPL" } });
                        manualLogonEvent.Set();
                    };

            _marketDataProvider.TickArrived +=
                    delegate(Tick obj)
                    {
                        tickArrived = true;

                        Logger.Debug(obj.ToString(), _type.FullName, "SubscribeMarketDataProviderTestCase");
                        //_marketDataProvider.Stop();
                       //manualTickEvent.Set();
                    };

            _marketDataProvider.Start();
            //manualLogonEvent.WaitOne(30000, false);
            manualTickEvent.WaitOne(300000, false);
            Assert.AreEqual(true, isConnected, "Is Market Data Provider connected");
            Assert.AreEqual(true, tickArrived, "Tick arrived");
        }

        [Test]
        [Category("Integration")]
        public void SubscribeTickAndBarTestCase()
        {
            bool isConnected = false;
            bool tickArrived = false;
            bool barArrived = false;

            var manualLogonEvent = new ManualResetEvent(false);
            var manualTickEvent = new ManualResetEvent(false);
            var manualBarEvent = new ManualResetEvent(false);

            BarDataRequest barDataRequest = new BarDataRequest()
            {
                Security = new Security() { Symbol = "GOOG" },
                Id = "123456",
                MarketDataProvider = Common.Core.Constants.MarketDataProvider.Simulated,
                BarFormat = Common.Core.Constants.BarFormat.TIME,
                BarLength = 60,
                PipSize = 1.2M,
                BarPriceType = Common.Core.Constants.BarPriceType.LAST
            };

            _marketDataProvider.LogonArrived +=
                    delegate(string obj)
                    {
                        isConnected = true;
                        _marketDataProvider.SubscribeBars(barDataRequest);
                        _marketDataProvider.SubscribeTickData(new Subscribe() { Security = new Security() { Symbol = "GOOG" } });
                        manualLogonEvent.Set();
                    };

            _marketDataProvider.BarArrived +=
                    delegate(Bar obj, string arg2)
                    {
                        barArrived = true;

                        Logger.Debug(obj.ToString(), "SimulatedExchangeMarketDataTest", "Bar");
                        //_marketDataProvider.Stop();
                        //manualTickEvent.Set();
                    };

            _marketDataProvider.TickArrived +=
                    delegate(Tick obj)
                    {
                        tickArrived = true;

                        Logger.Debug(obj.ToString(), "SimulatedExchangeMarketDataTest", "Tick");
                        //_marketDataProvider.Stop();
                        //manualTickEvent.Set();
                    };

            _marketDataProvider.Start();
            //manualLogonEvent.WaitOne(30000, false);
            manualBarEvent.WaitOne(30000, false);
            manualTickEvent.WaitOne(30000, false);
            Assert.AreEqual(true, isConnected, "Is Market Data Provider connected");
            Assert.AreEqual(true, barArrived, "Bar arrived");
            Assert.AreEqual(true, tickArrived, "Tick arrived");
        }
    }
}
