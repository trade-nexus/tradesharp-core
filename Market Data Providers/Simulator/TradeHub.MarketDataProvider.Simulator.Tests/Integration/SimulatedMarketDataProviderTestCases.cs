﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Spring.Context.Support;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataProvider.Simulator.Provider;

namespace TradeHub.MarketDataProvider.Simulator.Tests.Integration
{
    [TestFixture]
    class SimulatedMarketDataProviderTestCases
    {
        private SimulatedMarketDataProvider _marketDataProvider;
        [SetUp]
        public void SetUp()
        {
            _marketDataProvider = ContextRegistry.GetContext()["SimulatedMarketDataProvider"] as SimulatedMarketDataProvider;
        }

        [Test]
        [Category("Integration")]
        public void ConnectMarketDataProviderTestCase()
        {
            bool isConnected = false;
            var manualLogonEvent = new ManualResetEvent(false);

            _marketDataProvider.LogonArrived +=
                    delegate(string obj)
                    {
                        isConnected = true;
                        manualLogonEvent.Set();
                    };

            _marketDataProvider.Start();
            manualLogonEvent.WaitOne(30000, false);

            Assert.AreEqual(true, isConnected);
        }

        [Test]
        [Category("Integration")]
        public void DisconnectMarketDataProviderTestCase()
        {
            bool isConnected = false;
            var manualLogonEvent = new ManualResetEvent(false);
            _marketDataProvider.LogonArrived +=
                    delegate(string obj)
                    {
                        isConnected = true;
                        _marketDataProvider.Stop();
                        manualLogonEvent.Set();
                    };

            bool isDisconnected = false;
            var manualLogoutEvent = new ManualResetEvent(false);
            _marketDataProvider.LogoutArrived +=
                    delegate(string obj)
                    {
                        isDisconnected = true;
                        manualLogoutEvent.Set();
                    };

            _marketDataProvider.Start();
            manualLogonEvent.WaitOne(30000, false);
            manualLogoutEvent.WaitOne(30000, false);

            Assert.AreEqual(true, isConnected, "Connected");
            Assert.AreEqual(true, isDisconnected, "Disconnected");
        }

        [Test]
        [Category("Console")]
        public void SubscribeMarketDataProviderTestCase()
        {
            bool isConnected = false;
            bool tickArrived = false;
            int count = 0;

            var manualLogonEvent = new ManualResetEvent(false);
            var manualTickEvent = new ManualResetEvent(false);

            _marketDataProvider.LogonArrived +=
                    delegate(string obj)
                    {
                        isConnected = true;
                        _marketDataProvider.SubscribeTickData(new Subscribe() { Security = new Security() { Symbol = "IBM" } });
                        manualLogonEvent.Set();
                    };

            _marketDataProvider.TickArrived +=
                    delegate(Tick obj)
                    {
                        if (count == 10)
                        {
                            tickArrived = true;
                            _marketDataProvider.Stop();
                            manualTickEvent.Set();
                        }
                        count++;
                    };

            _marketDataProvider.Start();
            //manualLogonEvent.WaitOne(30000, false);
            manualTickEvent.WaitOne(300000, false);
            Assert.AreEqual(true, isConnected, "Is Market Data Provider connected");
            Assert.AreEqual(true, tickArrived, "Tick arrived");
            Assert.AreEqual(10, count, "Count");
        }

        [Test]
        [Category("Console")]
        public void BarSubscriptionMarketDataProviderTestCase()
        {
            bool isConnected = false;
            bool barArrived = false;

            BarDataRequest barDataRequest = new BarDataRequest()
            {
                Security = new Security() { Symbol = "IBM" },
                Id = "123456",
                MarketDataProvider = Common.Core.Constants.MarketDataProvider.Simulated,
                BarFormat = Common.Core.Constants.BarFormat.TIME,
                BarLength = 2,
                PipSize = 1.2M,
                BarPriceType = Common.Core.Constants.BarPriceType.ASK
            };

            var manualLogonEvent = new ManualResetEvent(false);
            var manualBarEvent = new ManualResetEvent(false);

            _marketDataProvider.LogonArrived +=
                    delegate(string obj)
                    {
                        isConnected = true;
                        _marketDataProvider.SubscribeBars(barDataRequest);
                        //manualLogonEvent.Set();
                    };

            _marketDataProvider.BarArrived +=
                    delegate(Bar obj, string arg2)
                    {
                        barArrived = true;
                        _marketDataProvider.Stop();
                        manualBarEvent.Set();
                    };

            _marketDataProvider.Start();
            //manualLogonEvent.WaitOne(30000, false);
            manualBarEvent.WaitOne(300000, false);
            Assert.AreEqual(true, isConnected, "Is Market Data Provider connected");
            Assert.AreEqual(true, barArrived, "Bar arrived");
        }
    }
}
