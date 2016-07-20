using System;
using System.Collections.Generic;
using System.Threading;
using NHibernate;
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.FactoryMethods;
using TradeHub.Common.Core.Repositories;
using TradeHub.Infrastructure.Nhibernate.Repositories;

namespace TradeHub.Infrastructure.Nhibernate.Tests.Integration
{
    [TestFixture]
    public class OrderRepositoryTestCases
    {
        private IOrderRepository _orderRespository;
        private IApplicationContext ctx;
        private ISessionFactory sessionFactory;
        private IPersistRepository<object> _repository;

       
        [SetUp]
        public void Setup()
        {
            ctx = ContextRegistry.GetContext();
            //_orderRespository=new OrderRespository();
            sessionFactory = ContextRegistry.GetContext()["NHibernateSessionFactory"] as ISessionFactory;
           _orderRespository = ContextRegistry.GetContext()["OrderRespository"] as IOrderRepository;
           _repository = ContextRegistry.GetContext()["PersistRepository"] as IPersistRepository<object>;
          
        }

        [TearDown]
        public void TearDown()
        {
            
        }

        [Test]
        public void LimitOrderCruld()
        {
            bool saved = false;
            string id = DateTime.Now.ToString();
            LimitOrder limitOrder = OrderMessage.GenerateLimitOrder(id,
                new Security() {Isin = "123", Symbol = "ERX" }, OrderSide.BUY, 100, 500.50m,
                OrderExecutionProvider.Blackwood);
            var ordersaved = new ManualResetEvent(false);

            //add limit order to database
            _orderRespository.AddUpdate(limitOrder);

            //get the same order
            LimitOrder getLimitOrder = _orderRespository.FindBy(id) as LimitOrder;
            if (getLimitOrder.OrderID.Equals(id) && getLimitOrder.LimitPrice == 500.50m)
            {
                saved = true;
                ordersaved.Set();
            }
            Assert.AreEqual(getLimitOrder.Security.Symbol, "ERX");

            ordersaved.WaitOne(30000);
            //delete the order
            _orderRespository.Delete(getLimitOrder);

            //get ther order again to verify its deleted or not
            getLimitOrder = _orderRespository.FindBy(id) as LimitOrder;
            Assert.AreEqual(true, saved, "LimitOrderNotSaved");
            Assert.IsNull(getLimitOrder, "Not deleted");


        }
        [Test]
        public void MarketOrderCruld()
        {
            var ordersaved = new ManualResetEvent(false);
            bool saved = false;
            string id = DateTime.Now.ToString();
            MarketOrder marketOrder = OrderMessage.GenerateMarketOrder(id,
                new Security() {Isin = "123", Symbol = "AAPL"}, OrderSide.SELL, 50, OrderExecutionProvider.Blackwood);
            
            //adding order fills
            Fill fill = new Fill(new Security() { Isin = "123", Symbol = "AAPL" }, "BlackWood", id);
            fill.ExecutionPrice = 100;
            fill.CummalativeQuantity = 100;
            fill.LeavesQuantity = 100;
            fill.ExecutionSize = 100;
            fill.ExecutionId = "asdfgfcx";
            fill.OrderId = id;
            fill.ExecutionType=ExecutionType.Fill;
            List<Fill> fills = new List<Fill>();
            fills.Add(fill);

            Fill fill1 = new Fill(new Security() { Isin = "123", Symbol = "AAPL" }, "BlackWood", id);
            fill1.ExecutionPrice = 100;
            fill1.CummalativeQuantity = 100;
            fill1.LeavesQuantity = 100;
            fill1.ExecutionSize = 100;
            fill1.ExecutionId = "asdf";
            fill1.OrderId = id;
            fill1.ExecutionType = ExecutionType.Partial;
            fills.Add(fill1);

            marketOrder.Fills = fills;

            //add market order to database
            _orderRespository.AddUpdate(marketOrder);
            
            //get the same order
            MarketOrder getMarketOrder = _orderRespository.FindBy(id) as MarketOrder;
            if (getMarketOrder.OrderID.Equals(id) && getMarketOrder.OrderSize == 50 && getMarketOrder.Fills.Count==2)
            {
                saved = true;
                ordersaved.Set();
            }

            ordersaved.WaitOne(30000);
            //delete the order
            _orderRespository.Delete(getMarketOrder);

            //get ther order again to verify its deleted or not
            getMarketOrder = _orderRespository.FindBy(id) as MarketOrder;
            Assert.AreEqual(true, saved, "MarketOrderNotSaved");
            Assert.IsNull(getMarketOrder, "Not deleted");
        }

        /// <summary>
        /// Test filter by methods
        /// </summary>
        [Test]
        public void FilterByTestCase()
        {
            bool saved = false;
            var ordersaved = new ManualResetEvent(false);
            string id = DateTime.Now.ToString();
            LimitOrder limitOrder = OrderMessage.GenerateLimitOrder(id,
                new Security() { Isin = "124", Symbol = "GOOG" }, OrderSide.BUY, 100, 500.50m,
                OrderExecutionProvider.Blackwood);

            //add limit order to database
            _orderRespository.AddUpdate(limitOrder);

            //get the order filter by security
            IList<Order> orders = _orderRespository.FilterBySecurity(new Security(){Symbol = "GOOG"});
            if (orders != null)
            {
                foreach (var order in orders)
                {
                    LimitOrder getLimitOrder = order as LimitOrder;
                    if (getLimitOrder.OrderID.Equals(id) && getLimitOrder.LimitPrice == 500.50m)
                    {
                        saved = true;
                        ordersaved.Set();
                        break;

                    }
                    
                }
                
            }
            ordersaved.WaitOne(30000);
            Assert.AreEqual(true, saved, "LimitOrderNotSaved");
          
            
        }

        [Test]
        public void InsertTestCase()
        {
            bool saved = false;
            string id = DateTime.Now.ToString();
            LimitOrder limitOrder = OrderMessage.GenerateLimitOrder(id,
                new Security() { Isin = "124", Symbol = "GOOG" }, OrderSide.BUY, 100, 500.50m,
                OrderExecutionProvider.Blackwood);
            var ordersaved = new ManualResetEvent(false);

            //add limit order to database
            _orderRespository.AddUpdate(limitOrder);

            //get the same order
            Order getLimitOrder = _orderRespository.FindBy(id) as Order;
            if (getLimitOrder.OrderID.Equals(id) )
            {
                saved = true;
                ordersaved.Set();

            }
            ordersaved.WaitOne(30000);
            Assert.AreEqual(true, saved, "LimitOrderNotSaved");
        }


    }
}
