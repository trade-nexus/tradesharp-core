using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Spring.Context.Support;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.OrderExecutionEngine.OrderExecutionProviderGateway.Service;
using Constants = TradeHub.Common.Core.Constants;

namespace TradeHub.OrderExecutionEngine.OrderExecutionProviderGatewayTests.Unit
{
    [TestFixture]
    public class OrderExecutionMessageProcessorTests
    {
        private OrderExecutionMessageProcessor _messageProcessor;

        [SetUp]
        public void SetUp()
        {
            _messageProcessor = new OrderExecutionMessageProcessor();
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
                new Login { OrderExecutionProvider = Constants.MarketDataProvider.Blackwood }, "AppID");

            Assert.AreEqual(1, _messageProcessor.ProvidersLoginRequestMap[Constants.OrderExecutionProvider.Blackwood].Count, "Number of Login Requests recieved");
            Assert.AreEqual(1, _messageProcessor.ProvidersMap.Count, "Number of data provider instances loaded");
        }

        [Test]
        [Category("Unit")]
        public void DataProviderLoginTestCase_MultipleLogin()
        {
            _messageProcessor.OnLogonMessageRecieved(new Login() { OrderExecutionProvider = Constants.MarketDataProvider.Blackwood }, "AppID-One");
            _messageProcessor.OnLogonMessageRecieved(new Login() { OrderExecutionProvider = Constants.MarketDataProvider.Blackwood }, "AppID-Two");

            Assert.AreEqual(2, _messageProcessor.ProvidersLoginRequestMap[Constants.OrderExecutionProvider.Blackwood].Count, "Number of Login Requests recieved");
            Assert.AreEqual(1, _messageProcessor.ProvidersMap.Count, "Number of data provider instances loaded");
        }

        [Test]
        [Category("Unit")]
        public void DataProviderLogoutTestCase_SingleProivderLoggedIn()
        {
            _messageProcessor.OnLogonMessageRecieved(new Login() { OrderExecutionProvider = Constants.OrderExecutionProvider.Blackwood }, "AppID");

            Assert.AreEqual(1, _messageProcessor.ProvidersLoginRequestMap[Constants.OrderExecutionProvider.Blackwood].Count, "Number of Login Requests recieved");
            Assert.AreEqual(1, _messageProcessor.ProvidersMap.Count, "Number of data provider instances loaded");

            _messageProcessor.OnLogoutMessageRecieved(new Logout() { OrderExecutionProvider = Constants.OrderExecutionProvider.Blackwood }, "AppID");

            Assert.AreEqual(0, _messageProcessor.ProvidersLoginRequestMap.Count, "Number of Login Requests recieved");
            Assert.AreEqual(0, _messageProcessor.ProvidersMap.Count, "Number of data provider instances loaded");
        }

        [Test]
        [Category("Unit")]
        public void DataProviderLogoutTestCase_MultipleProivderLoggedIn()
        {
            _messageProcessor.OnLogonMessageRecieved(new Login() { OrderExecutionProvider = Constants.OrderExecutionProvider.Blackwood }, "AppID-One");
            _messageProcessor.OnLogonMessageRecieved(new Login() { OrderExecutionProvider = Constants.OrderExecutionProvider.Blackwood }, "AppID-Two");
            Assert.AreEqual(2, _messageProcessor.ProvidersLoginRequestMap[Constants.OrderExecutionProvider.Blackwood].Count, "Number of Login Requests recieved");
            Assert.AreEqual(1, _messageProcessor.ProvidersMap.Count, "Number of data provider instances loaded");

            _messageProcessor.OnLogoutMessageRecieved(new Logout() { OrderExecutionProvider = Constants.OrderExecutionProvider.Blackwood }, "AppID-One");

            Assert.AreEqual(1, _messageProcessor.ProvidersLoginRequestMap[Constants.OrderExecutionProvider.Blackwood].Count, "Number of Login Requests recieved");
            Assert.AreEqual(1, _messageProcessor.ProvidersMap.Count, "Number of data provider instances loaded");
        }

    }
}
