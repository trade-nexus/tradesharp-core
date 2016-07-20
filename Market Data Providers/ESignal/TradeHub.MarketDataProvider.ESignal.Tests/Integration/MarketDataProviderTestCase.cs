using System.Threading;
using NUnit.Framework;
using Spring.Context.Support;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataProvider.ESignal.Provider;

namespace TradeHub.MarketDataProvider.ESignal.Tests.Integration
{
    [TestFixture]
    public class MarketDataProviderTestCase
    {
        private ESignalMarketDataProvider _marketDataProvider;

        [SetUp]
        public void SetUp()
        {
            _marketDataProvider = ContextRegistry.GetContext()["ESignalMarketDataProvider"] as ESignalMarketDataProvider;
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
    }
}
