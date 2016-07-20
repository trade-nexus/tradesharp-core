using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.SimulatedExchange.Common.Interfaces;

namespace TradeHub.SimulatedExchange.SimulatorControler.Test.Unit
{
    [TestFixture]
    class ControllerTests
    {
        // Class Objects
        private FetchData _fetchData;
        private MarketDataControler _marketDataControler;
        private SimulatedOrderController _simulatedOrderController;
        private SimulateMarketOrder _simulateMarketOrder;
        private SimulateLimitOrder _simulateLimitOrder;
        private BarDataRequest _barDataRequest;

        // Interfaces
        private IReadMarketData _readMarketData;
        private ICommunicationController _communicationController;
        
        // Mock Objects
        private Mock<IReadMarketData> _moqReadMarketData;
        private Mock<ICommunicationController> _moqCommunicationController;

        [SetUp]
        public void StartUp()
        {
            Logger.SetLoggingLevel();

            // Create Mocks
            _moqReadMarketData = new Mock<IReadMarketData>();
            _moqCommunicationController = new Mock<ICommunicationController>();

            // Initialize dependency objects
            _communicationController = _moqCommunicationController.Object;
            _fetchData = new FetchData(_moqReadMarketData.Object);

            //// Initialize controllers
            //_marketDataControler = new MarketDataControler(_fetchData, _communicationController);
            //_simulatedOrderController = new SimulatedOrderController(_communicationController, _simulateMarketOrder,
            //                                                         _simulateLimitOrder);

            // Create sample bar data
            PopulateBarData();
        }

        [TearDown]
        public void Close()
        {
            
        }

        [Test]
        [Category("Unit")]
        public void GetLiveBars()
        {
            var watch = Stopwatch.StartNew();
            
            _fetchData.ReadData(_barDataRequest);

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            Logger.Debug("Time consumer: " + elapsedMs, "SimulatorControler.Test.Unit", "GetLiveBars");
        }

        [Test]
        [Category("Unit")]
        public void GetLiveBarsWithEvents()
        {
            _simulateMarketOrder = new SimulateMarketOrder();
            _simulateLimitOrder = new SimulateLimitOrder();

            // Initialize Controller
            _marketDataControler= new MarketDataControler(_fetchData, _communicationController);
            _simulatedOrderController= new SimulatedOrderController(_communicationController,_simulateMarketOrder, _simulateLimitOrder);

            // Add Controllers as Parameters to FetchData.cs
            _fetchData.MarketDataControler = _marketDataControler;
            _fetchData.SimulatedOrderController = _simulatedOrderController;

            var watch = Stopwatch.StartNew();

            _fetchData.ReadData(_barDataRequest);

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            Logger.Debug("Time consumer: " + elapsedMs, "SimulatorControler.Test.Unit", "GetLiveBarsWithEvents");
        }

        public void PopulateBarData()
        {
            DateTime currentTime = new DateTime(2013, 8, 1);
            var barsArray = new Bar[50000]; // 200000 Ticks + 50000 Bars

            // Populate values.
            for (int i = 0; i < 50000; i++)
            {
                Bar bar = new Bar("TestRequest");
                bar.Security = new Security() { Symbol = "AAPL" };
                bar.MarketDataProvider = MarketDataProvider.SimulatedExchange;
                bar.Open = 1.22M;
                bar.High = 1.22M;
                bar.Low = 1.22M;
                bar.Close = 1.22M;

                bar.DateTime = currentTime.AddMinutes(i);
                barsArray[i] = bar;
            }

            // Enumerable to be used in the function call.
            IEnumerable<Bar> barsList = barsArray;

            // Request to be used for testing.
            _barDataRequest = new BarDataRequest { Security = new Security { Symbol = "AAPL" }, Id = "TestRequest" };

            // Time value to be used with Moq Object. 
            var startTime = new DateTime(2013, 06, 17);
            var endTime = new DateTime(2013, 06, 18);

            // Mock Read Bars Response.
            _moqReadMarketData.Setup(rqd => rqd.
                ReadBars(startTime, endTime, "Blackwood", _barDataRequest)).Returns(barsList);

            // Mock Publish methods.
            _moqCommunicationController.Setup(cc => cc.PublishBarData(It.IsAny<Bar>()));
            _moqCommunicationController.Setup(cc => cc.PublishTickData(It.IsAny<Tick>()));
        }
    }
}
