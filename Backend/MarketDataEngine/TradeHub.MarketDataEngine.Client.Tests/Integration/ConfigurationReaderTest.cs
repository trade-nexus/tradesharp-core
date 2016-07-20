using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Spring.Context.Support;
using TradeHub.MarketDataEngine.Client.Utility;

namespace TradeHub.MarketDataEngine.Client.Tests.Integration
{
    [TestFixture]
    public class ConfigurationReaderTest
    {
        private ConfigurationReader _configurationReader;

        [SetUp]
        public void SetUp()
        {
            _configurationReader = ContextRegistry.GetContext()["ConfigurationReader"] as ConfigurationReader;
        }

        [Test]
        [Category("Integration")]
        public void ReadMdeMQConfigSettings()
        {
            var mdeMqParameters = _configurationReader.MdeMqServerparameters;

            Assert.AreEqual("host=localhost", mdeMqParameters["ConnectionString"], "ConnectionString");
            Assert.AreEqual("marketdata_exchange", mdeMqParameters["Exchange"], "Exchange");
            Assert.AreEqual("marketdata.engine.subscribe", mdeMqParameters["SubscribeRoutingKey"], "SubscribeRoutingKey");
            Assert.AreEqual("marketdata.engine.unsubscribe", mdeMqParameters["UnsubscribeRoutingKey"], "UnsubscribeRoutingKey");
            Assert.AreEqual("marketdata.engine.historicbar", mdeMqParameters["HistoricBarDataRoutingKey"], "HistoricBarDataRoutingKey");
            Assert.AreEqual("marketdata.engine.login", mdeMqParameters["LoginRoutingKey"], "LoginRoutingKey");
            Assert.AreEqual("marketdata.engine.logout", mdeMqParameters["LogoutRoutingKey"], "LogoutRoutingKey");
        }

        [Test]
        [Category("Integration")]
        public void ReadClientMQConfigSettings()
        {
            var clientMqParameters = _configurationReader.ClientMqParameters;

            Assert.AreEqual("host=localhost", clientMqParameters["ConnectionString"], "ConnectionString");
            Assert.AreEqual("marketdata_exchange", clientMqParameters["Exchange"], "Exchange");

            Assert.AreEqual("marketdata_client_admin_queue", clientMqParameters["AdminMessageQueue"], "AdminMessageQueue");
            Assert.AreEqual("marketdata.client.admin", clientMqParameters["AdminMessageRoutingKey"], "AdminMessageRoutingKey");


            Assert.AreEqual("marketdata_client_tickdata_queue", clientMqParameters["TickDataQueue"], "TickDataQueue");
            Assert.AreEqual("marketdata.client.tickdata", clientMqParameters["TickDataRoutingKey"], "TickDataRoutingKey");

            Assert.AreEqual("marketdata_client_historicbar_queue", clientMqParameters["HistoricBarDataQueue"], "HistoricBarDataQueue");
            Assert.AreEqual("marketdata.client.historicbar", clientMqParameters["HistoricBarDataRoutingKey"], "HistoricBarDataRoutingKey");

            Assert.NotNull(clientMqParameters["InquiryResponseQueue"], "InquiryResponseQueue");
            Assert.NotNull(clientMqParameters["InquiryResponseRoutingKey"], "InquiryResponseRoutingKey");
        }
    }
}
