using EasyNetQ;
using NUnit.Framework;
using TradeHub.MarketDataEngine.Configuration.Service;

namespace TradeHub.MarketDataEngine.Configuration.Tests.Unit
{
    /// <summary>
    /// Contains Test Cases for the Project Initialization classes
    /// </summary>
    [TestFixture]
    class InitializationTestCase
    {
        private MqServer _server;
        [SetUp]
        public void SetUp()
        {
            //_server = new MqServer("RabbitMQ.xml", 10000, 120000);
        }

        [Test]
        [Category("Unit")]
        public void ConnectMqServer_ReadConfiSettings()
        {
            _server = new MqServer("RabbitMQ.xml", 10000, 120000);
            string result = _server.ReadConfigSettings("ConnectionString");
            Assert.AreEqual("host=localhost", result);
        }

        [Test]
        [Category("Unit")]
        public void ConnectMqServer_InitializeRabbitHutch()
        {
            string settings = _server.ReadConfigSettings("ConnectionString");
            _server.InitializeRabbitHutch(settings);

            Assert.IsNotNull(_server.IsConnected());
        }
    }
}
