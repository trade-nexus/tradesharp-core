using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using TradeHub.Common.Core.DomainModels;
using TradeHub.DataDownloader.ApplicationCenter;
using TradeHub.DataDownloader.Common.ConcreteImplementation;
using TradeHub.DataDownloader.Common.Interfaces;

namespace TradeHub.DataDownloader.Application.Tests.Unit
{
    [TestFixture]
    public class UnitTest
    {
        [Test]
        public void TestHandleRequestToken()
        {
            var writerCsv = new Mock<IWriter>();
            var writerBin = new Mock<IWriter>();
            IList<IWriter> writers=new List<IWriter>();
            writers.Add(writerBin.Object);
            writers.Add(writerCsv.Object);
            var responseHandler = new MarketDataResponseHandler(writers);
        }
    }
}
