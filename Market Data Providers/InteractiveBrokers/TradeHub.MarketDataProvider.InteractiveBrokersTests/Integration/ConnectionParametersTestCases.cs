using NUnit.Framework;
using Spring.Context.Support;
using TradeHub.MarketDataProvider.InteractiveBrokers.Utility;
using TradeHub.MarketDataProvider.InteractiveBrokers.ValueObjects;

namespace TradeHub.MarketDataProvider.InteractiveBrokersTests.Integration
{
    [TestFixture]
    public class ConnectionParametersTestCases
    {
        private ConnectionParametersLoader _parametersLoader;
        private ConnectionParameters _parameters;

        [SetUp]
        public void SetUp()
        {
            _parametersLoader = ContextRegistry.GetContext()["IBConnectionParametersLoader"] as ConnectionParametersLoader;
            if (_parametersLoader != null) _parameters = _parametersLoader.Parameters;
        }

        [Test]
        [Category("Integration")]
        public void ReadParametersTestCase()
        {
            Assert.AreEqual("localhost", _parameters.Host);
            Assert.AreEqual(7496, _parameters.Port);
            Assert.AreEqual(5300, _parameters.ClientId);
        }
    }
}
