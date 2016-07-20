using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using TradeHub.Common.Core.DomainModels;
using TradeHub.MarketDataProvider.Simulator.Service;

namespace TradeHub.MarketDataProvider.Simulator.Tests.Unit
{
    [TestFixture]
    class SimulatedDataProcessorTestCases
    {
        private SimulatedDataProcessor _dataProcessor;

        [SetUp]
        public void SetUp()
        {
            _dataProcessor = new SimulatedDataProcessor();
        }

        [Test]
        [Category("Integration")]
        public void TickTestCase()
        {
            string sampleInput = "Tick IBM Ask 1.22 100 Bid 1.24 98 Last 1.23 99 Depth 1";

            bool tickArrived = false;
            var manualTickEvent = new ManualResetEvent(false);
            _dataProcessor.TickArrived +=
                    delegate(Tick obj)
                    {
                        tickArrived = true;
                        manualTickEvent.Set();
                    };

            _dataProcessor.ProcessIncomingMessage(sampleInput);
            manualTickEvent.WaitOne(3000, false);

            Assert.AreEqual(true, tickArrived, "Tick Recieved from Simulated Data Processor");
        }

        [Test]
        [Category("Integration")]
        public void BarTestCase()
        {
            string sampleInput = "Bar IBM Test 1.22 1.24 1.23 1.10 100";

            bool barArrived = false;
            var manualBarEvent = new ManualResetEvent(false);
            _dataProcessor.LiveBarArrived +=
                    delegate(Bar obj)
                    {
                        barArrived = true;
                        manualBarEvent.Set();
                    };

            _dataProcessor.ProcessIncomingMessage(sampleInput);
            manualBarEvent.WaitOne(3000, false);

            Assert.AreEqual(true, barArrived, "Bar Recieved from Simulated Data Processor");
        }
    }
}
