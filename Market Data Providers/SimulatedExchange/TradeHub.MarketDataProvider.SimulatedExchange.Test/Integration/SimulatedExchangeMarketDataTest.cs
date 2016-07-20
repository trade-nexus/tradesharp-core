
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
