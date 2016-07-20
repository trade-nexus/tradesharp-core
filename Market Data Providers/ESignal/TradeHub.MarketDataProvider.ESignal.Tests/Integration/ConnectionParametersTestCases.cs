using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Spring.Context.Support;
using TradeHub.MarketDataProvider.ESignal.Utility;
using TradeHub.MarketDataProvider.ESignal.ValueObjects;

namespace TradeHub.MarketDataProvider.ESignal.Tests.Integration
{
    [TestFixture]
    public class ConnectionParametersTestCases
    {
        private ConnectionParametersLoader _parametersLoader;
        private ConnectionParameters _parameters;

        [SetUp]
        public void SetUp()
        {
            _parametersLoader = ContextRegistry.GetContext()["ESignalConnectionParametersLoader"] as ConnectionParametersLoader;
            if (_parametersLoader != null) _parameters = _parametersLoader.Parameters;
        }

        [Test]
        [Category("Integration")]
        public void ReadParametersTestCase()
        {
            Assert.AreEqual("alpha998", _parameters.UserName);
            Assert.AreEqual("123456", _parameters.Password);
        }
    }
}
