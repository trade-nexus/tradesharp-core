using System.Threading;
using NUnit.Framework;
using Spring.Context.Support;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataProvider.Blackwood.Provider;
using System;

namespace TradeHub.MarketDataProvider.Blackwood.Tests.Integration
{
    [TestFixture]
    public class MarketDataProviderTestCase
    {
        private BlackwoodMarketDataProvider _marketDataProvider;
        [SetUp]
        public void SetUp()
        {
            _marketDataProvider = ContextRegistry.GetContext()["BlackwoodMarketDataProvider"] as BlackwoodMarketDataProvider;
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
                        _marketDataProvider.SubscribeTickData(new Subscribe(){Security = new Security(){Symbol = "AGQ"}});
                        manualLogonEvent.Set();
                    };

            _marketDataProvider.TickArrived +=
                    delegate(Tick obj)
                    {
                        tickArrived = true;
                        _marketDataProvider.Stop();
                        manualTickEvent.Set();
                    };

            _marketDataProvider.Start();
            manualLogonEvent.WaitOne(30000, false);
            manualTickEvent.WaitOne(30000, false);
            Assert.AreEqual(true, isConnected,"Is Market Data Provider connected");
            Assert.AreEqual(true, tickArrived, "Tick arrived");
        }


        [Test]
        [Category("Integration")]
        public void HistoricalData_SendRequestToServer_ReceiveHistoricalDataFromServer()
        {
            bool logonReceived = false;
            bool dataReceived = false;

            var logonManualResetEvent = new ManualResetEvent(false);
            var dataManualResetEvent = new ManualResetEvent(false);

            var dataRequestMessage = new HistoricDataRequest() { Security = new Security() { Symbol = "TNA" } };
            dataRequestMessage.Id = "AAOOA";
            dataRequestMessage.BarType = BarType.INTRADAY;
            dataRequestMessage.Interval = 60;
            dataRequestMessage.StartTime = new DateTime(2015, 7, 15);
            dataRequestMessage.EndTime = new DateTime(2015, 7, 26);

            _marketDataProvider.LogonArrived += delegate (string providerName)
            {
                logonReceived = true;
                logonManualResetEvent.Set();

                _marketDataProvider.HistoricBarDataRequest(dataRequestMessage);

            };

            _marketDataProvider.HistoricBarDataArrived += delegate (HistoricBarData data)
            {
                dataReceived = true;
                dataManualResetEvent.Set();
                //Console.WriteLine(data.Security.Symbol);
            };

            _marketDataProvider.Start();

            logonManualResetEvent.WaitOne(10000, false);
            dataManualResetEvent.WaitOne(160000, false);

            Assert.AreEqual(true, logonReceived, "Logon Received");
            Assert.AreEqual(true, dataReceived, "Data Received");
        }
    }
}

