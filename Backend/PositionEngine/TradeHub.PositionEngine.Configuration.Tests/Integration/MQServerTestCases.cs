using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using NUnit.Framework;
using TradeHub.Common.Core.ValueObjects.Inquiry;
using TradeHub.PositionEngine.Configuration.Service;

namespace TradeHub.PositionEngine.Configuration.Tests.Integration
{
    [TestFixture]
    class MQServerTestCases
    {
        private PositionEngineMqServer _positionMqServer;
        private IAdvancedBus _advancedBus;
        private IExchange _adminExchange;
        private IQueue _applicationAdminQueue;
        [SetUp]
        public void SetUp()
        {
            _positionMqServer = new PositionEngineMqServer("PEMQConfig.xml");
            _positionMqServer.Connect();
            // Initialize Advance Bus
            _advancedBus = RabbitHutch.CreateBus("host=localhost").Advanced;

            // Create a admin exchange
            _adminExchange = _advancedBus.ExchangeDeclare("position_exchange", ExchangeType.Direct, true, false, true);
        }

        [TearDown]
        public void Close()
        {
            _positionMqServer.Disconnect();
        }

        [Test]
        [Category("Integration")]
        public void InquiryMessageTestCase()
        {
            bool inquiryReceived = false;
            var inquiryEvent = new ManualResetEvent(false);

            _positionMqServer.InquiryRequestReceived += delegate(IMessage<InquiryMessage> obj)
            {
                inquiryReceived = true;
                inquiryEvent.Set();
            };

           // using (var channel = _advancedBus.OpenPublishChannel())
            {
                IMessage<InquiryMessage> message = new Message<InquiryMessage>(new InquiryMessage());
                _advancedBus.Publish(_adminExchange, "position.engine.inquiry",true,false, message);
            }

            inquiryEvent.WaitOne(10000, false);
            Assert.AreEqual(true, inquiryReceived, "Inquiry Received");
        }

        [Test]
        [Category("Integration")]
        public void AppInfoMessageTestCase()
        {
            bool appInfoReceived = false;
            var appInfoEvent = new ManualResetEvent(false);

            _positionMqServer.AppInfoReceived += delegate(IMessage<Dictionary<string, string>> obj)
            {
                appInfoReceived = true;
                appInfoEvent.Set();
            };

          //  using (var channel = _advancedBus.OpenPublishChannel())
            {
                IMessage<Dictionary<string, string>> message = new Message<Dictionary<string, string>>(new Dictionary<string, string>());
                _advancedBus.Publish(_adminExchange, "position.engine.appinfo",true,false, message);
            }

            appInfoEvent.WaitOne(10000, false);
            Assert.AreEqual(true, appInfoReceived, "App Info Received");
        }


        [Test]
        [Category("Integration")]
        public void ProviderRequestTestCase()
        {
            bool providerRequestReceived = false;
            var providerRequest = new ManualResetEvent(false);

            _positionMqServer.ProviderRequestReceived+= delegate(IMessage<string> obj)
            {
                providerRequestReceived = true;
                providerRequest.Set();
            };

          //  using (var channel = _advancedBus.OpenPublishChannel())
            {
                IMessage<string> message =new Message<string>("");
                _advancedBus.Publish(_adminExchange, "position.engine.provider.request",true,false, message);
            }

            providerRequest.WaitOne(10000, false);
            Assert.AreEqual(true, providerRequestReceived, "Provider Request Received");
        }


    }
}
