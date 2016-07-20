using System;
using System.Threading;
using NUnit.Framework;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataProvider.SimulatedExchange.Provider;

namespace TradeHub.MarketDataProvider.SimulatedExchange.Test.Integration
{
    [TestFixture]
    public class SimulatedExchangeIntegrationTest
    {
        private SimulatedExchangeMarketDataProvider _simulatedExchangeMarketDataProvider;
        [Test]
        public void TestDataSubscribe()
        {

            string value = null;
            ManualResetEvent manualResetEvent = new ManualResetEvent(false);

            _simulatedExchangeMarketDataProvider = new SimulatedExchangeMarketDataProvider();

            _simulatedExchangeMarketDataProvider.BarArrived += delegate(Bar arg1, string arg2)
                {
                    Console.WriteLine(arg1);
                    value = arg2;
                };
            _simulatedExchangeMarketDataProvider.SubscribeBars(new BarDataRequest
                {
                    MarketDataProvider = Common.Core.Constants.MarketDataProvider.SimulatedExchange,
                    Security = new Security {Symbol = "AAPL"},
                    Id = "1"
                });
            manualResetEvent.WaitOne(10000);
            Assert.AreEqual(value,"2");
        }
    }
}
