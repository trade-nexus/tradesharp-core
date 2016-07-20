using System;
using System.Threading;
using NUnit.Framework;
using Spring.Context.Support;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataProvider.InteractiveBrokers.Provider;

namespace TradeHub.MarketDataProvider.InteractiveBrokersTests.Integration
{
    [TestFixture]
    public class MarketDataProviderTestCase
    {
        private InteractiveBrokersMarketDataProvider _marketDataProvider;
        [SetUp]
        public void SetUp()
        {
            _marketDataProvider = ContextRegistry.GetContext()["InteractiveBrokersMarketDataProvider"] as InteractiveBrokersMarketDataProvider;
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
                        _marketDataProvider.SubscribeTickData(new Subscribe{Security = new Security{Symbol = "AAPL"},Id = "One"});
                        Console.WriteLine(obj);
                        manualLogonEvent.Set();
                    };

            _marketDataProvider.TickArrived +=
                    delegate(Tick obj)
                    {
                        tickArrived = true;
                        Console.WriteLine(obj);
                        //_marketDataProvider.Stop();
                        //manualTickEvent.Set();
                    };

            _marketDataProvider.Start();
            manualLogonEvent.WaitOne(30000, false);
            manualTickEvent.WaitOne(30000, false);
            Assert.AreEqual(true, isConnected,"Is Market Data Provider connected");
            Assert.AreEqual(true, tickArrived, "Tick arrived");
        }
    }
}
