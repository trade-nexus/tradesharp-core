using System;
using System.Linq;
using NUnit.Framework;
using TradeHub.Common.Core.Constants;

namespace TradeHub.SimulatedExchange.FileReader.Test.Integration
{
    [TestFixture]
    public class IntegrationTest
    {
        [Test]
        public void TestReadMarketData()
        {
            ReadMarketData marketData=new ReadMarketData();
            /*var bars = marketData.ReadBars(new DateTime(2013, 07, 09), new DateTime(2013, 07, 10),
                                           MarketDataProvider.Blackwood,
                                           "AAPL");*/
          /*  Assert.AreEqual(bars.Count(),31);*/
        }
    }
}
