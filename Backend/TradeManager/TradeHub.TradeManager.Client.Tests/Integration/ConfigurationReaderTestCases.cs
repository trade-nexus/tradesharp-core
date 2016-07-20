using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TradeHub.TradeManager.Client.Constants;
using TradeHub.TradeManager.Client.Utility;

namespace TradeHub.TradeManager.Client.Tests.Integration
{
    [TestFixture]
    public class ConfigurationReaderTestCases
    {
        private MqConfigurationReader _configurationReader;

        [SetUp]
        public void Setup()
        {
            _configurationReader = new MqConfigurationReader("TradeManagerServerMqInfo.xml", "TradeManagerClientMqConfig.xml");
        }

        [TearDown]
        public void TearDown()
        {
            
        }

        [Test]
        [Category("Integration")]
        public void ReadServerMqParameters_Successful()
        {
            string connectionString;
            _configurationReader.ServerMqParameters.TryGetValue(MqParameters.TradeManagerServer.ConnectionString, out connectionString);

            string exchangeName;
            _configurationReader.ServerMqParameters.TryGetValue(MqParameters.TradeManagerServer.Exchange, out exchangeName);

            string queueRoutingKey;
            _configurationReader.ServerMqParameters.TryGetValue(MqParameters.TradeManagerServer.ExecutionMessageRoutingKey, out queueRoutingKey);

            Assert.AreEqual("localhost", connectionString, "ConnectionString");
            Assert.AreEqual("trademanager_exchange", exchangeName, "Exchange");
            Assert.AreEqual("trademanager.execution.message", queueRoutingKey, "Queue Routing Key");
        }

        [Test]
        [Category("Integration")]
        public void ReadClientMqParameters_Successful()
        {
            string connectionString;
            _configurationReader.ClientMqParameters.TryGetValue(MqParameters.TradeManagerClient.ConnectionString, out connectionString);

            string exchangeName;
            _configurationReader.ClientMqParameters.TryGetValue(MqParameters.TradeManagerClient.Exchange, out exchangeName);

            Assert.AreEqual("localhost", connectionString, "ConnectionString");
            Assert.AreEqual("trademanager_exchange", exchangeName, "Exchange");
        }
    }
}
