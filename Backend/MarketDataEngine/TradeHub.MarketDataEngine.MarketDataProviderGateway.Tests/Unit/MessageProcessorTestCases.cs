using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataEngine.MarketDataProviderGateway.Service;
using Constants = TradeHub.Common.Core.Constants;

namespace TradeHub.MarketDataEngine.MarketDataProviderGateway.Tests.Unit
{
    [TestFixture]
    public class MessageProcessorTestCases
    {
        private MessageProcessor _messageProcessor;

        [SetUp]
        public void SetUp()
        {
            _messageProcessor = new MessageProcessor(new LiveBarGenerator(new BarFactory.Service.BarFactory()));
        }

        [TearDown]
        public void Close()
        {
         _messageProcessor.StopProcessing();   
        }


        [Test]
        [Category("Unit")]
        public void DataProviderLoginTestCase_SingleLogin()
        {
            _messageProcessor.OnLogonMessageRecieved(
                new Login { MarketDataProvider = Constants.MarketDataProvider.Blackwood }, "AppID");

            Assert.AreEqual(1, _messageProcessor.ProvidersLoginRequestMap[Constants.MarketDataProvider.Blackwood].Count, "Number of Login Requests recieved");
            Assert.AreEqual(1, _messageProcessor.ProvidersMap.Count,"Number of data provider instances loaded");
        }

        [Test]
        [Category("Unit")]
        public void DataProviderLoginTestCase_MultipleLogin()
        {
            _messageProcessor.OnLogonMessageRecieved(new Login() { MarketDataProvider = Constants.MarketDataProvider.Blackwood }, "AppID-One");
            _messageProcessor.OnLogonMessageRecieved(new Login() { MarketDataProvider = Constants.MarketDataProvider.Blackwood }, "AppID-Two");

            Assert.AreEqual(2, _messageProcessor.ProvidersLoginRequestMap[Constants.MarketDataProvider.Blackwood].Count, "Number of Login Requests recieved");
            Assert.AreEqual(1, _messageProcessor.ProvidersMap.Count, "Number of data provider instances loaded");
        }

        [Test]
        [Category("Unit")]
        public void DataProviderLogoutTestCase_SingleProivderLoggedIn()
        {
            _messageProcessor.OnLogonMessageRecieved(new Login() { MarketDataProvider = Constants.MarketDataProvider.Blackwood }, "AppID");

            Assert.AreEqual(1, _messageProcessor.ProvidersLoginRequestMap[Constants.MarketDataProvider.Blackwood].Count, "Number of Login Requests recieved");
            Assert.AreEqual(1, _messageProcessor.ProvidersMap.Count, "Number of data provider instances loaded");

            _messageProcessor.OnLogoutMessageRecieved(new Logout() { MarketDataProvider = Constants.MarketDataProvider.Blackwood }, "AppID");

            Assert.AreEqual(0, _messageProcessor.ProvidersLoginRequestMap.Count, "Number of Login Requests recieved");
            Assert.AreEqual(0, _messageProcessor.ProvidersMap.Count, "Number of data provider instances loaded");
        }

        [Test]
        [Category("Unit")]
        public void DataProviderLogoutTestCase_MultipleProivderLoggedIn()
        {
            _messageProcessor.OnLogonMessageRecieved(new Login() { MarketDataProvider = Constants.MarketDataProvider.Blackwood }, "AppID-One");
            _messageProcessor.OnLogonMessageRecieved(new Login() { MarketDataProvider = Constants.MarketDataProvider.Blackwood }, "AppID-Two");
            Assert.AreEqual(2, _messageProcessor.ProvidersLoginRequestMap[Constants.MarketDataProvider.Blackwood].Count, "Number of Login Requests recieved");
            Assert.AreEqual(1, _messageProcessor.ProvidersMap.Count, "Number of data provider instances loaded");

            _messageProcessor.OnLogoutMessageRecieved(new Logout() { MarketDataProvider = Constants.MarketDataProvider.Blackwood }, "AppID-One");

            Assert.AreEqual(1, _messageProcessor.ProvidersLoginRequestMap[Constants.MarketDataProvider.Blackwood].Count, "Number of Login Requests recieved");
            Assert.AreEqual(1, _messageProcessor.ProvidersMap.Count, "Number of data provider instances loaded");
        }

        [Test]
        [Category("Unit")]
        public void DataProviderSubscribeTestCase_SingleRequest()
        {
            _messageProcessor.OnLogonMessageRecieved(new Login() { MarketDataProvider = Constants.MarketDataProvider.Blackwood }, "AppID");
            _messageProcessor.OnTickSubscribeRecieved(new Subscribe() { MarketDataProvider = Constants.MarketDataProvider.Blackwood ,Security = new Security(){Symbol = "IBM"}},"1");

            Assert.AreEqual(1, _messageProcessor.ProvidersLoginRequestMap[Constants.MarketDataProvider.Blackwood].Count, "Number of Login Requests recieved");
            Assert.AreEqual(1, _messageProcessor.ProvidersMap.Count, "Number of data provider instances loaded");

            Assert.AreEqual(1, _messageProcessor.SubscriptionMap.Count, "Number of data providers with subscriptions");
            Assert.AreEqual(1, _messageProcessor.SubscriptionMap[Constants.MarketDataProvider.Blackwood].Count, "Number of subscriptions for current provider");
        
            Dictionary<Security, List<string>> subscrition = _messageProcessor.SubscriptionMap[Constants.MarketDataProvider.Blackwood];

            Assert.AreEqual(1, (subscrition[new Security() { Symbol = "IBM" }]).Count, "Number of subscriptions for current Security");
        }

        [Test]
        [Category("Unit")]
        public void DataProviderSubscribeTestCase_MultipleRequests()
        {
            _messageProcessor.OnLogonMessageRecieved(new Login() { MarketDataProvider = Constants.MarketDataProvider.Blackwood }, "AppID");
            _messageProcessor.OnTickSubscribeRecieved(new Subscribe() { MarketDataProvider = Constants.MarketDataProvider.Blackwood, Security = new Security() { Symbol = "IBM" } }, "1");
            _messageProcessor.OnTickSubscribeRecieved(new Subscribe() { MarketDataProvider = Constants.MarketDataProvider.Blackwood, Security = new Security() { Symbol = "IBM" } }, "2");

            Assert.AreEqual(1, _messageProcessor.ProvidersLoginRequestMap[Constants.MarketDataProvider.Blackwood].Count, "Number of Login Requests recieved");
            Assert.AreEqual(1, _messageProcessor.ProvidersMap.Count, "Number of data provider instances loaded");

            Assert.AreEqual(1, _messageProcessor.SubscriptionMap.Count, "Number of data providers with subscriptions");
            Assert.AreEqual(1, _messageProcessor.SubscriptionMap[Constants.MarketDataProvider.Blackwood].Count, "Number of subscriptions for current provider");

            Dictionary<Security, List<string>> subscrition = _messageProcessor.SubscriptionMap[Constants.MarketDataProvider.Blackwood];

            Assert.AreEqual(2, (subscrition[new Security() { Symbol = "IBM" }]).Count, "Number of subscriptions for current Security");
        }

        [Test]
        [Category("Unit")]
        public void DataProviderUnsubscribeTestCase_WithSingleSubscription()
        {
            _messageProcessor.OnLogonMessageRecieved(new Login() { MarketDataProvider = Constants.MarketDataProvider.Blackwood }, "AppID");
            _messageProcessor.OnTickSubscribeRecieved(new Subscribe() { MarketDataProvider = Constants.MarketDataProvider.Blackwood, Security = new Security() { Symbol = "IBM" } }, "1");

            Assert.AreEqual(1, _messageProcessor.ProvidersLoginRequestMap[Constants.MarketDataProvider.Blackwood].Count, "Number of Login Requests recieved");
            Assert.AreEqual(1, _messageProcessor.ProvidersMap.Count, "Number of data provider instances loaded");

            Assert.AreEqual(1, _messageProcessor.SubscriptionMap.Count, "Number of data providers with subscriptions");
            Assert.AreEqual(1, _messageProcessor.SubscriptionMap[Constants.MarketDataProvider.Blackwood].Count, "Number of subscriptions for current provider");

            Dictionary<Security, List<string>> subscrition = _messageProcessor.SubscriptionMap[Constants.MarketDataProvider.Blackwood];

            Assert.AreEqual(1, (subscrition[new Security() { Symbol = "IBM" }]).Count, "Number of subscriptions for current Security");
        
            _messageProcessor.OnTickUnsubscribeRecieved(new Unsubscribe() { MarketDataProvider = Constants.MarketDataProvider.Blackwood, Security = new Security() { Symbol = "IBM" } },"1");

            Assert.AreEqual(0, _messageProcessor.SubscriptionMap.Count, "Number of data providers with subscriptions");
        }

        [Test]
        [Category("Unit")]
        public void DataProviderUnsubscribeTestCase_WithMultipleSubscriptions()
        {
            _messageProcessor.OnLogonMessageRecieved(new Login() { MarketDataProvider = Constants.MarketDataProvider.Blackwood }, "AppID");
            _messageProcessor.OnTickSubscribeRecieved(new Subscribe() { MarketDataProvider = Constants.MarketDataProvider.Blackwood, Security = new Security() { Symbol = "IBM" } },"1");
            _messageProcessor.OnTickSubscribeRecieved(new Subscribe() { MarketDataProvider = Constants.MarketDataProvider.Blackwood, Security = new Security() { Symbol = "IBM" } },"2");

            Assert.AreEqual(1, _messageProcessor.ProvidersLoginRequestMap[Constants.MarketDataProvider.Blackwood].Count, "Number of Login Requests recieved");
            Assert.AreEqual(1, _messageProcessor.ProvidersMap.Count, "Number of data provider instances loaded");

            Assert.AreEqual(1, _messageProcessor.SubscriptionMap.Count, "Number of data providers with subscriptions");
            Assert.AreEqual(1, _messageProcessor.SubscriptionMap[Constants.MarketDataProvider.Blackwood].Count, "Number of subscriptions for current provider");

            Dictionary<Security, List<string>> subscrition = _messageProcessor.SubscriptionMap[Constants.MarketDataProvider.Blackwood];

            Assert.AreEqual(2, (subscrition[new Security() { Symbol = "IBM" }]).Count, "Number of subscriptions for current Security");

            _messageProcessor.OnTickUnsubscribeRecieved(new Unsubscribe() { MarketDataProvider = Constants.MarketDataProvider.Blackwood, Security = new Security() { Symbol = "IBM" } },"1");
            subscrition = _messageProcessor.SubscriptionMap[Constants.MarketDataProvider.Blackwood];

            Assert.AreEqual(1, (subscrition[new Security() { Symbol = "IBM" }]).Count, "Number of subscriptions for current Security");
        
        }

        [Test]
        [Category("Unit")]
        public void DataProviderHistoricBarDataRequestTestCase_SingleRequest()
        {
            _messageProcessor.OnLogonMessageRecieved(new Login() { MarketDataProvider = Constants.MarketDataProvider.Blackwood }, "AppID");

            HistoricDataRequest historicDataRequest = new HistoricDataRequest()
                {
                    BarType = BarType.INTRADAY,
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now.AddHours(1),
                    Interval = 100,
                    MarketDataProvider = Constants.MarketDataProvider.Blackwood,
                    Security = new Security(){Symbol = "IBM"},
                    Id = "5000",
                };

            _messageProcessor.OnHistoricBarDataRequest(historicDataRequest, "1");

            Assert.AreEqual(1, _messageProcessor.ProvidersLoginRequestMap[Constants.MarketDataProvider.Blackwood].Count, "Number of Login Requests recieved");
            Assert.AreEqual(1, _messageProcessor.ProvidersMap.Count, "Number of data provider instances loaded");

            Assert.AreEqual(1, _messageProcessor.HistoricBarRequestsMap.Count, "Number of data providers with historic data request");
            Assert.AreEqual(1, _messageProcessor.HistoricBarRequestsMap[Constants.MarketDataProvider.Blackwood].Count, "Number of historic requests for current provider");

            Dictionary<string, string> idsDic = _messageProcessor.HistoricBarRequestsMap[Constants.MarketDataProvider.Blackwood];

            Assert.AreEqual("1", idsDic["5000"], "Strategy ID which requested the Historic data");

        }

        [Test]
        [Category("Unit")]
        public void DataProviderHistoricBarDataRequestTestCase_MultipleRequest()
        {
            _messageProcessor.OnLogonMessageRecieved(new Login() { MarketDataProvider = Constants.MarketDataProvider.Blackwood }, "AppID");

            HistoricDataRequest historicDataRequestOne = new HistoricDataRequest()
            {
                BarType = BarType.INTRADAY,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(1),
                Interval = 100,
                MarketDataProvider = Constants.MarketDataProvider.Blackwood,
                Security = new Security() { Symbol = "IBM" },
                Id = "5000",
            };

            HistoricDataRequest historicDataRequestTwo = new HistoricDataRequest()
            {
                BarType = BarType.INTRADAY,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(1),
                Interval = 100,
                MarketDataProvider = Constants.MarketDataProvider.Blackwood,
                Security = new Security() { Symbol = "IBM" },
                Id = "5001",
            };

            _messageProcessor.OnHistoricBarDataRequest(historicDataRequestOne, "1");
            _messageProcessor.OnHistoricBarDataRequest(historicDataRequestTwo, "1");

            Assert.AreEqual(1, _messageProcessor.ProvidersLoginRequestMap[Constants.MarketDataProvider.Blackwood].Count, "Number of Login Requests recieved");
            Assert.AreEqual(1, _messageProcessor.ProvidersMap.Count, "Number of data provider instances loaded");

            Assert.AreEqual(1, _messageProcessor.HistoricBarRequestsMap.Count, "Number of data providers with historic data request");
            Assert.AreEqual(2, _messageProcessor.HistoricBarRequestsMap[Constants.MarketDataProvider.Blackwood].Count, "Number of historic requests for current provider");

            Dictionary<string, string> idsDic = _messageProcessor.HistoricBarRequestsMap[Constants.MarketDataProvider.Blackwood];

            Assert.AreEqual("1", idsDic["5000"], "Strategy ID which requested the Historic data");
            Assert.AreEqual("1", idsDic["5001"], "Strategy ID which requested the Historic data");
        }

        [Test]
        [Category("Unit")]
        public void DataProviderLiveBarDataRequestTestCase_SingleRequest()
        {
            _messageProcessor.OnLogonMessageRecieved(new Login() { MarketDataProvider = Constants.MarketDataProvider.Blackwood }, "AppID");

            BarDataRequest barDataRequest = new BarDataRequest()
                {
                    Security = new Security() {Symbol = "IBM"},
                    Id = "123456",
                    MarketDataProvider = Constants.MarketDataProvider.Blackwood,
                    BarFormat = Constants.BarFormat.TIME,
                    BarLength = 2,
                    PipSize = 1.2M,
                    BarPriceType = Constants.BarPriceType.ASK
                };

            _messageProcessor.OnLiveBarSubscribeRequest(barDataRequest, "1");

            Assert.AreEqual(1, _messageProcessor.ProvidersLoginRequestMap[Constants.MarketDataProvider.Blackwood].Count, "Number of Login Requests recieved");
            Assert.AreEqual(1, _messageProcessor.ProvidersMap.Count, "Number of data provider instances loaded");

            Assert.AreEqual(1, _messageProcessor.LiveBarRequestsMap.Count, "Number of data providers with Live data request");

            Assert.AreEqual(1, _messageProcessor.SubscriptionMap.Count, "Number of data providers with subscriptions");
            Assert.AreEqual(1, _messageProcessor.SubscriptionMap[Constants.MarketDataProvider.Blackwood].Count, "Number of subscriptions for current provider");

            Dictionary<Security, List<string>> subscrition = _messageProcessor.SubscriptionMap[Constants.MarketDataProvider.Blackwood];

            Assert.AreEqual(1, (subscrition[new Security() { Symbol = "IBM" }]).Count, "Number of subscriptions for current Security");
        }

        [Test]
        [Category("Unit")]
        public void DataProviderLiveBarDataRequestTestCase_MultipleRequest()
        {
            _messageProcessor.OnLogonMessageRecieved(new Login() { MarketDataProvider = Constants.MarketDataProvider.Blackwood }, "AppID");

            BarDataRequest barDataRequest = new BarDataRequest()
            {
                Security = new Security() { Symbol = "IBM" },
                Id = "123456",
                MarketDataProvider = Constants.MarketDataProvider.Blackwood,
                BarFormat = Constants.BarFormat.TIME,
                BarLength = 2,
                PipSize = 1.2M,
                BarPriceType = Constants.BarPriceType.ASK
            };

            _messageProcessor.OnLiveBarSubscribeRequest(barDataRequest, "1");
            _messageProcessor.OnLiveBarSubscribeRequest(barDataRequest, "2");

            Assert.AreEqual(1, _messageProcessor.ProvidersLoginRequestMap[Constants.MarketDataProvider.Blackwood].Count, "Number of Login Requests recieved");
            Assert.AreEqual(1, _messageProcessor.ProvidersMap.Count, "Number of data provider instances loaded");

            Assert.AreEqual(1, _messageProcessor.LiveBarRequestsMap.Count, "Number of data providers with Live data request");

            Assert.AreEqual(1, _messageProcessor.SubscriptionMap.Count, "Number of data providers with subscriptions");
            Assert.AreEqual(1, _messageProcessor.SubscriptionMap[Constants.MarketDataProvider.Blackwood].Count, "Number of subscriptions for current provider");

            Dictionary<Security, List<string>> subscrition = _messageProcessor.SubscriptionMap[Constants.MarketDataProvider.Blackwood];

            Assert.AreEqual(1, (subscrition[new Security() { Symbol = "IBM" }]).Count, "Number of subscriptions for current Security");
        }

        [Test]
        [Category("Unit")]
        public void DataProviderLiveBarUnsubscribeRequestTestCase_SingleRequest()
        {
            _messageProcessor.OnLogonMessageRecieved(new Login() { MarketDataProvider = Constants.MarketDataProvider.Blackwood }, "AppID");

            BarDataRequest barDataRequest = new BarDataRequest()
            {
                Security = new Security() { Symbol = "IBM" },
                Id = "123456",
                MarketDataProvider = Constants.MarketDataProvider.Blackwood,
                BarFormat = Constants.BarFormat.TIME,
                BarLength = 2,
                PipSize = 1.2M,
                BarPriceType = Constants.BarPriceType.ASK
            };

            _messageProcessor.OnLiveBarSubscribeRequest(barDataRequest, "1");

            Assert.AreEqual(1, _messageProcessor.ProvidersLoginRequestMap[Constants.MarketDataProvider.Blackwood].Count, "Number of Login Requests recieved");
            Assert.AreEqual(1, _messageProcessor.ProvidersMap.Count, "Number of data provider instances loaded");

            Assert.AreEqual(1, _messageProcessor.LiveBarRequestsMap.Count, "Number of data providers with Live data request");

            Assert.AreEqual(1, _messageProcessor.SubscriptionMap.Count, "Number of data providers with subscriptions");
            Assert.AreEqual(1, _messageProcessor.SubscriptionMap[Constants.MarketDataProvider.Blackwood].Count, "Number of subscriptions for current provider");

            Dictionary<Security, List<string>> subscrition = _messageProcessor.SubscriptionMap[Constants.MarketDataProvider.Blackwood];

            Assert.AreEqual(1, (subscrition[new Security() { Symbol = "IBM" }]).Count, "Number of subscriptions for current Security");

            _messageProcessor.OnLiveBarUnsubscribeRequest(barDataRequest, "1");

            Assert.AreEqual(0, _messageProcessor.SubscriptionMap.Count, "Number of data providers with subscriptions");
        }

        [Test]
        [Category("Unit")]
        public void DataProviderLiveBarUnsubscribeRequestTestCase_MultipleRequest()
        {
            _messageProcessor.OnLogonMessageRecieved(new Login() { MarketDataProvider = Constants.MarketDataProvider.Blackwood }, "AppID");

            BarDataRequest barDataRequest = new BarDataRequest()
            {
                Security = new Security() { Symbol = "IBM" },
                Id = "123456",
                MarketDataProvider = Constants.MarketDataProvider.Blackwood,
                BarFormat = Constants.BarFormat.TIME,
                BarLength = 2,
                PipSize = 1.2M,
                BarPriceType = Constants.BarPriceType.ASK
            };

            _messageProcessor.OnLiveBarSubscribeRequest(barDataRequest, "1");
            _messageProcessor.OnLiveBarSubscribeRequest(barDataRequest, "2");

            Assert.AreEqual(1, _messageProcessor.ProvidersLoginRequestMap[Constants.MarketDataProvider.Blackwood].Count, "Number of Login Requests recieved");
            Assert.AreEqual(1, _messageProcessor.ProvidersMap.Count, "Number of data provider instances loaded");

            Assert.AreEqual(1, _messageProcessor.LiveBarRequestsMap.Count, "Number of data providers with Live data request");

            Assert.AreEqual(1, _messageProcessor.SubscriptionMap.Count, "Number of data providers with subscriptions");
            Assert.AreEqual(1, _messageProcessor.SubscriptionMap[Constants.MarketDataProvider.Blackwood].Count, "Number of subscriptions for current provider");

            Dictionary<Security, List<string>> subscrition = _messageProcessor.SubscriptionMap[Constants.MarketDataProvider.Blackwood];

            Assert.AreEqual(1, (subscrition[new Security() { Symbol = "IBM" }]).Count, "Number of subscriptions for current Security");

            _messageProcessor.OnLiveBarUnsubscribeRequest(barDataRequest, "1");

            Assert.AreEqual(1, _messageProcessor.SubscriptionMap.Count, "Number of data providers with subscriptions");
        }

    }
}
