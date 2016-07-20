using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataProvider.IqFeed.Provider;

namespace TradeHub.MarketDataProvider.IqFeedTests.Integration
{
    public class MarketDataTestCase
    {
        private IqFeedMarketDataProvider _marketDataProvider;

        [SetUp]
        public void SetUp()
        {
            _marketDataProvider = new IqFeedMarketDataProvider();
        }

        [TearDown]
        public void TearDown()
        {
            _marketDataProvider.Stop();
        }

        [Test]
        [Category("Integration")]
        public void Logon_SendRequestToServer_ReceiveLogonArrived()
        {
            bool logonReceived = false;

            var logonManualResetEvent = new ManualResetEvent(false);

            _marketDataProvider.LogonArrived += delegate(string providerName)
            {
                logonReceived = true;
                logonManualResetEvent.Set();
            };

            _marketDataProvider.Start();

            logonManualResetEvent.WaitOne(10000, false);

            Assert.AreEqual(true, logonReceived, "Logon Received");
        }

        [Test]
        [Category("Integration")]
        public void NewSubscription_SendRequestToServer_ReceiveQuoteStreamByServer()
        {
            bool logonReceived = false;
            bool tickReceived = false;

            var logonManualResetEvent = new ManualResetEvent(false);
            var tickManualResetEvent = new ManualResetEvent(false);

            _marketDataProvider.LogonArrived += delegate(string providerName)
            {
                logonReceived = true;
                logonManualResetEvent.Set();

                _marketDataProvider.SubscribeTickData(new Subscribe() { Security = new Security() { Symbol = "AAPL" } });
            };

            _marketDataProvider.TickArrived += delegate(Tick tick)
            {
                tickReceived = true;
                //tickManualResetEvent.Set();
                Console.WriteLine(tick);
            };

            _marketDataProvider.Start();

            logonManualResetEvent.WaitOne(10000, false);
            tickManualResetEvent.WaitOne(10000, false);

            Assert.AreEqual(true, logonReceived, "Logon Received");
            Assert.AreEqual(true, tickReceived, "Tick Received");
        }

        [Test]
        [Category("Integration")]
        public void BarData_SendRequestToServer_ReceiveBarDataFromServer()
        {
            bool logonReceived = false;
            bool dataReceived = false;

            var logonManualResetEvent = new ManualResetEvent(false);
            var dataManualResetEvent = new ManualResetEvent(false);

            var dataRequestMessage = new BarDataRequest() { Security = new Security() { Symbol = "AAPL" } };
            dataRequestMessage.Id = "AAOOA";
            dataRequestMessage.BarPriceType= BarPriceType.MEAN;
            dataRequestMessage.BarLength = 60;
            dataRequestMessage.BarFormat = BarFormat.TIME;
            dataRequestMessage.BarSeed = 0;
            dataRequestMessage.PipSize = 1;

            _marketDataProvider.LogonArrived += delegate(string providerName)
            {
                logonReceived = true;
                logonManualResetEvent.Set();

                _marketDataProvider.SubscribeBars(dataRequestMessage);

            };

            _marketDataProvider.BarArrived += delegate(Bar data, string key)
            {
                dataReceived = true;
                dataManualResetEvent.Set();
                Console.WriteLine(key + data);
            };

            _marketDataProvider.Start();

            logonManualResetEvent.WaitOne(10000, false);
            dataManualResetEvent.WaitOne(220000, false);

            Assert.AreEqual(true, logonReceived, "Logon Received");
            Assert.AreEqual(true, dataReceived, "Data Received");
        }

        [Test]
        [Category("Integration")]
        public void HistoricalData_SendRequestToServer_ReceiveHistoricalDataFromServer()
        {
            bool logonReceived = false;
            bool dataReceived = false;

            var logonManualResetEvent = new ManualResetEvent(false);
            var dataManualResetEvent = new ManualResetEvent(false);

            var dataRequestMessage = new HistoricDataRequest() { Security = new Security() { Symbol = "AAPL" } };
            dataRequestMessage.Id = "AAOOA";
            dataRequestMessage.BarType = BarType.INTRADAY;
            dataRequestMessage.Interval = 60;
            dataRequestMessage.StartTime = new DateTime(2015, 2, 1);
            dataRequestMessage.EndTime = DateTime.Now;

            _marketDataProvider.LogonArrived += delegate(string providerName)
            {
                logonReceived = true;
                logonManualResetEvent.Set();

                _marketDataProvider.HistoricBarDataRequest(dataRequestMessage);

            };

            _marketDataProvider.HistoricBarDataArrived += delegate(HistoricBarData data)
            {
                dataReceived = true;
                dataManualResetEvent.Set();
                Console.WriteLine(data.Security.Symbol);
            };

            _marketDataProvider.Start();

            logonManualResetEvent.WaitOne(10000, false);
            dataManualResetEvent.WaitOne(20000, false);

            Assert.AreEqual(true, logonReceived, "Logon Received");
            Assert.AreEqual(true, dataReceived, "Data Received");
        }
    }
}
