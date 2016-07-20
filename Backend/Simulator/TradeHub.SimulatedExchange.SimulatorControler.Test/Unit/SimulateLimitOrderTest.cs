
using System.Threading;
using NUnit.Framework;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;

namespace TradeHub.SimulatedExchange.SimulatorControler.Test.Unit
{
    [TestFixture]
    public class SimulateLimitOrderTest
    {
        [Test]
        [Category("Unit")]
        public void TestLimitOrderLogic()
        {
            var manualBarEvent = new ManualResetEvent(false);
            string executionId = null;
            SimulateLimitOrder simulateLimitOrder=new SimulateLimitOrder();
            decimal executionPrice = 0;
            simulateLimitOrder.NewArrived += delegate(Order order)
                {
                    executionId = order.OrderID;
                };
            simulateLimitOrder.NewExecution += delegate(Execution orderExecution)
                {
                    executionPrice = orderExecution.Fill.ExecutionPrice;
                };
            manualBarEvent.WaitOne(500);
            LimitOrder limitOrder=new LimitOrder("1",OrderSide.SELL,10,OrderTif.DAY,"USD",new Security(){Symbol = "AAPL"},OrderExecutionProvider.SimulatedExchange){LimitPrice = 100};
            Bar bar=new Bar(new Security(){Symbol = "AAPL"},MarketDataProvider.SimulatedExchange,"123"){Low = 120,Close = 130};
            simulateLimitOrder.NewLimitOrderArrived(limitOrder);
            simulateLimitOrder.NewBarArrived(bar);
            manualBarEvent.WaitOne(500);
            Assert.AreEqual("1", executionId);
            Assert.AreEqual(100, executionPrice);

        }

        [Test]
        [Category("Unit")]
        public void TestLimitRejection()
        {
            var manualBarEvent = new ManualResetEvent(false);
            var count = 0;
            SimulateLimitOrder simulateLimitOrder = new SimulateLimitOrder();
            simulateLimitOrder.LimitOrderRejection += delegate(Rejection rejection)
                {
                    count = 1;
                };
            
            LimitOrder limitOrder = new LimitOrder("1", OrderSide.SELL, 10, OrderTif.DAY, "USD", new Security() { Symbol = "AAPL" }, OrderExecutionProvider.SimulatedExchange) { LimitPrice = 0 };
            simulateLimitOrder.NewLimitOrderArrived(limitOrder);
            manualBarEvent.WaitOne(500);
            Assert.AreEqual(1,count);
        }
    }
}
