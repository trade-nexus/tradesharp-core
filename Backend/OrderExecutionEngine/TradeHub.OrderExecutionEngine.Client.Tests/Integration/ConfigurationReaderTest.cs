using NUnit.Framework;
using TraceSourceLogger;
using TradeHub.OrderExecutionEngine.Client.Utility;

namespace TradeHub.OrderExecutionEngine.Client.Tests.Integration
{
    [TestFixture]
    public class ConfigurationReaderTest
    {
        private ConfigurationReader _configurationReader;

        [SetUp]
        public void SetUp()
        {
            _configurationReader = new ConfigurationReader("OEEServer.xml","OEEClientMqConfig.xml", new AsyncClassLogger("ConfigurationReaderTest"));
        }

        [Test]
        [Category("Integration")]
        public void ReadOeeMQConfigSettings()
        {
            var oeeMqParameters = _configurationReader.OeeMqServerparameters;

            Assert.AreEqual("host=localhost", oeeMqParameters["ConnectionString"], "ConnectionString");
            Assert.AreEqual("orderexecution_exchange", oeeMqParameters["Exchange"], "Exchange");
            Assert.AreEqual("orderexecution.engine.limitorder", oeeMqParameters["LimitOrderRoutingKey"], "LimitOrderRoutingKey");
            Assert.AreEqual("orderexecution.engine.marketorder", oeeMqParameters["MarketOrderRoutingKey"], "MarketOrderRoutingKey");
            Assert.AreEqual("orderexecution.engine.stoporder", oeeMqParameters["StopOrderRoutingKey"], "StopOrderRoutingKey");
            Assert.AreEqual("orderexecution.engine.stoplimitorder", oeeMqParameters["StopLimitOrderRoutingKey"], "StopLimitOrderRoutingKey");
            Assert.AreEqual("orderexecution.engine.login", oeeMqParameters["LoginRoutingKey"], "LoginRoutingKey");
            Assert.AreEqual("orderexecution.engine.logout", oeeMqParameters["LogoutRoutingKey"], "LogoutRoutingKey");
            Assert.AreEqual("orderexecution.engine.locateresponse", oeeMqParameters["LocateResponseRoutingKey"], "LogoutRoutingKey");
        }

        [Test]
        [Category("Integration")]
        public void ReadClientMQConfigSettings()
        {
            var clientMqParameters = _configurationReader.ClientMqParameters;

            Assert.AreEqual("host=localhost", clientMqParameters["ConnectionString"], "ConnectionString");
            Assert.AreEqual("orderexecution_exchange", clientMqParameters["Exchange"], "Exchange");

            Assert.AreEqual("orderexecution_client_admin_queue", clientMqParameters["AdminMessageQueue"], "AdminMessageQueue");
            Assert.AreEqual("orderexecution.client.admin", clientMqParameters["AdminMessageRoutingKey"], "AdminMessageRoutingKey");


            Assert.AreEqual("orderexecution_client_order_queue", clientMqParameters["OrderMessageQueue"], "OrderMessageQueue");
            Assert.AreEqual("orderexecution.client.order", clientMqParameters["OrderMessageRoutingKey"], "OrderMessageRoutingKey");

            Assert.AreEqual("orderexecution_client_execution_queue", clientMqParameters["ExecutionMessageQueue"], "ExecutionMessageQueue");
            Assert.AreEqual("orderexecution.client.execution", clientMqParameters["ExecutionMessageRoutingKey"], "ExecutionMessageRoutingKey");

            Assert.AreEqual("orderexecution_client_rejection_queue", clientMqParameters["RejectionMessageQueue"], "RejectionMessageQueue");
            Assert.AreEqual("orderexecution.client.rejection", clientMqParameters["RejectionMessageRoutingKey"], "RejectionMessageRoutingKey");

            Assert.AreEqual("orderexecution_client_locatemessage_queue", clientMqParameters["LocateMessageQueue"], "LocateMessageQueue");
            Assert.AreEqual("orderexecution.locatemessage.order", clientMqParameters["LocateMessageRoutingKey"], "LocateMessageRoutingKey");

            Assert.NotNull(clientMqParameters["InquiryResponseQueue"], "InquiryResponseQueue");
            Assert.NotNull(clientMqParameters["InquiryResponseRoutingKey"], "InquiryResponseRoutingKey");
        }
    }
}
