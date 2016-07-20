using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.FactoryMethods;

namespace TradeHub.Common.Tests
{
    [TestFixture]
    public class OrderMessageClassTests
    {
        [Test]
        [Category("Unit")]
        public void CreateMarketOrder_IfAllParametersAreOk_VerifyMarketOrderIsCreated()
        {
            MarketOrder marketOrder=OrderMessage.GenerateMarketOrder(new Security() {Symbol = "AAPL"}, OrderSide.BUY, 10,
                OrderExecutionProvider.SimulatedExchange);
            Assert.NotNull(marketOrder);
            Assert.IsNotNullOrEmpty(marketOrder.OrderID);
            Assert.AreEqual(marketOrder.OrderSide,OrderSide.BUY);
            Assert.AreEqual(marketOrder.OrderSize,10);
            Assert.AreEqual(marketOrder.OrderExecutionProvider,OrderExecutionProvider.SimulatedExchange);
        }

        [Test]
        [Category("Unit")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateMarketOrder_IfSizeIsZero_ExceptionWillBeThrown()
        {
            MarketOrder marketOrder = OrderMessage.GenerateMarketOrder(new Security() { Symbol = "AAPL" }, OrderSide.BUY, 0,
                OrderExecutionProvider.SimulatedExchange);
        }

        [Test]
        [Category("Unit")]
        public void CreateLimitOrder_IfAllParametersAreOk_VerifyLimitOrderIsCreated()
        {
            LimitOrder limitOrder = OrderMessage.GenerateLimitOrder(new Security() { Symbol = "AAPL" }, OrderSide.BUY, 10,100,
                OrderExecutionProvider.SimulatedExchange);
            Assert.NotNull(limitOrder);
            Assert.IsNotNullOrEmpty(limitOrder.OrderID);
            Assert.AreEqual(limitOrder.OrderSide, OrderSide.BUY);
            Assert.AreEqual(limitOrder.OrderSize, 10);
            Assert.AreEqual(limitOrder.LimitPrice,100);
            Assert.AreEqual(limitOrder.OrderExecutionProvider, OrderExecutionProvider.SimulatedExchange);
        }
        [Test]
        [Category("Unit")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateLimitOrder_IfPriceIsZero_ExceptionWillBeThrown()
        {
            LimitOrder limitOrder = OrderMessage.GenerateLimitOrder(new Security() { Symbol = "AAPL" }, OrderSide.BUY, 10, 0,
                OrderExecutionProvider.SimulatedExchange);
        }

    }
}
