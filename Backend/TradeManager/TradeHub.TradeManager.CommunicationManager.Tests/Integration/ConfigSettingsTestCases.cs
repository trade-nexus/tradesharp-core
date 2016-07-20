using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TradeHub.TradeManager.CommunicationManager.Service;

namespace TradeHub.TradeManager.CommunicationManager.Tests.Integration
{
    [TestFixture]
    public class ConfigSettingsTestCases
    {
        private TradeManagerMqServer _tradeManagerMqServer;

        [SetUp]
        public void Setup()
        {
            _tradeManagerMqServer = new TradeManagerMqServer("TradeManagerMqConfig.xml");
        }

        [TearDown]
        public void TearDown()
        {
            
        }

        [Test]
        [Category("Integration")]
        public void ReadConfigParameters()
        {
            string connectionString = _tradeManagerMqServer.ReadConfigSettings("ConnectionString");
            string exchangeName = _tradeManagerMqServer.ReadConfigSettings("Exchange");
            string queueName = _tradeManagerMqServer.ReadConfigSettings("ExecutionMessageQueue");
            string queueRoutingKey = _tradeManagerMqServer.ReadConfigSettings("ExecutionMessageInquiryRoutingKey");

            Assert.AreEqual("localhost", connectionString, "ConnectionString");
            Assert.AreEqual("trademanager_exchange", exchangeName, "Exchange");
            Assert.AreEqual("trademanager_execution_message_queue", queueName, "Queue Name");
            Assert.AreEqual("trademanager.execution.message", queueRoutingKey, "Queue Routing Key");
        }
    }
}
