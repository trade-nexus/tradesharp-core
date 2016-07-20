
using System.Threading;
using NUnit.Framework;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataEngine.Client.Service;
using TradeHub.OrderExecutionEngine.Client.Service;

namespace TradeHub.SimulatedExchange.SimulatorControler.Test.Integration
{
    [TestFixture]
    public class TestMdeAndOee
    {
        private MarketDataEngineClient _marketDataEngineClient;
        private OrderExecutionEngineClient _orderExecutionEngine;

        [SetUp]
        public void Setup()
        {
            _marketDataEngineClient = new MarketDataEngineClient();
            _orderExecutionEngine = new OrderExecutionEngineClient();
            _marketDataEngineClient.Start();
            _orderExecutionEngine.Start();
        }

        [TearDown]
        public void Close()
        {
            _marketDataEngineClient.Shutdown();
        }

        [Test]
        public void TestWholeExchange()
        {
            int count = 0;
            _orderExecutionEngine.NewArrived += delegate(Order order)
                {
                    count++;
                };
            _orderExecutionEngine.ExecutionArrived += delegate(Execution execution)
                {
                    count++;
                };
            ManualResetEvent manualResetEvent = new ManualResetEvent(false);
            manualResetEvent.WaitOne(1000);
            _orderExecutionEngine.SendLoginRequest(new Login { OrderExecutionProvider = OrderExecutionProvider.SimulatedExchange });
            _marketDataEngineClient.SendLoginRequest(new Login { MarketDataProvider = MarketDataProvider.SimulatedExchange });
            manualResetEvent.WaitOne(3000);
            LimitOrder limitOrder = new LimitOrder("1", OrderSide.SELL, 10, OrderTif.DAY, "USD", new Security() { Symbol = "GOOG" }, OrderExecutionProvider.SimulatedExchange) { LimitPrice = 120 };
            _orderExecutionEngine.SendLimitOrderRequest(limitOrder);
            _marketDataEngineClient.SendLiveBarSubscriptionRequest(new BarDataRequest
            {
                BarFormat = BarFormat.TIME,
                Security = new Security { Symbol = "GOOG" },
                BarLength = 10,
                MarketDataProvider = MarketDataProvider.SimulatedExchange,
                Id = "1",
                BarPriceType = BarPriceType.BID
            });
            manualResetEvent.WaitOne(3000);
            Assert.AreEqual(2,count);
        }
    }
}
