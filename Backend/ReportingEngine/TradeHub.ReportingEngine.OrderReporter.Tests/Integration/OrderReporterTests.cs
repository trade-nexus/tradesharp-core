using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Spring.Context.Support;
using TradeHub.Common.Core.Repositories;
using TradeHub.Common.Core.Repositories.Parameters;

namespace TradeHub.ReportingEngine.OrderReporter.Tests.Integration
{
    [TestFixture]
    public class OrderReporterTests
    {
        [SetUp]
        public void Setup()
        {
            //var ctx = ContextRegistry.GetContext();
        }

        [TearDown]
        public void TearDown()
        {
            
        }

        [Test]
        [Category("Integration")]
        public void OrderQueryTest()
        {
            IOrderRepository orderRespository = ContextRegistry.GetContext()["OrderRespository"] as IOrderRepository;
            var orderReportManager = new OrderReportManager(orderRespository);

            var dataEventReceived = false;
            var manualDataEvent = new ManualResetEvent(false);

            IList<object[]> report = null;
            var arguments = new Dictionary<OrderParameters, string>();

            // Add Filter Parameters
            arguments.Add(OrderParameters.OrderSide, "BUY");
            //arguments.Add(OrderParameters.OrderStatus, "CANCELLED,OPEN,EXECUTED");
            //arguments.Add(OrderParameters.StartOrderDateTime, "2013-06-20 09:30:00");
            //arguments.Add(OrderParameters.EndOrderDateTime, "2013-06-20 09:31:00");
            //arguments.Add(OrderParameters.OrderSize, "13");

            // Hook Data Event
            orderReportManager.DataReceived += delegate(IList<object[]> obj)
            {
                report = obj;
                dataEventReceived = true;
                manualDataEvent.Set();
            };

            // Request Report Data
            orderReportManager.RequestReport(arguments);

            manualDataEvent.WaitOne(200000);

            Assert.IsNotNull(report);
            Assert.IsTrue(dataEventReceived, "Report Event");
            Assert.Greater(report.Count, 0, "Return Count");
        }

        [Test]
        [Category("Integration")]
        public void OrderSideBuyQueryTest()
        {
            IOrderRepository orderRespository = ContextRegistry.GetContext()["OrderRespository"] as IOrderRepository;
            var orderReportManager = new OrderReportManager(orderRespository);

            var dataEventReceived = false;
            var manualDataEvent = new ManualResetEvent(false);

            IList<object[]> report = null;
            var arguments = new Dictionary<OrderParameters, string>();

            // Add Filter Parameters
            arguments.Add(OrderParameters.OrderSide, "BUY");

            // Hook Data Event
            orderReportManager.DataReceived += delegate(IList<object[]> obj)
            {
                report = obj;
                dataEventReceived = true;
                manualDataEvent.Set();
            };

            // Request Report Data
            orderReportManager.RequestReport(arguments);

            manualDataEvent.WaitOne(2000);

            Assert.IsNotNull(report);
            Assert.IsTrue(dataEventReceived, "Report Event");
            Assert.Greater(report.Count, 0, "Return Count");
        }

        [Test]
        [Category("Integration")]
        public void OrderSideSellQueryTest()
        {
            IOrderRepository orderRespository = ContextRegistry.GetContext()["OrderRespository"] as IOrderRepository;
            var orderReportManager = new OrderReportManager(orderRespository);

            var dataEventReceived = false;
            var manualDataEvent = new ManualResetEvent(false);

            IList<object[]> report = null;
            var arguments = new Dictionary<OrderParameters, string>();

            // Add Filter Parameters
            arguments.Add(OrderParameters.OrderSide, "SELL");

            // Hook Data Event
            orderReportManager.DataReceived += delegate(IList<object[]> obj)
            {
                report = obj;
                dataEventReceived = true;
                manualDataEvent.Set();
            };

            // Request Report Data
            orderReportManager.RequestReport(arguments);

            manualDataEvent.WaitOne(2000);

            Assert.IsNotNull(report);
            Assert.IsTrue(dataEventReceived, "Report Event");
            Assert.Greater(report.Count, 0, "Return Count");
        }

        [Test]
        [Category("Integration")]
        public void OrderOrderSizeQueryTest()
        {
            IOrderRepository orderRespository = ContextRegistry.GetContext()["OrderRespository"] as IOrderRepository;
            var orderReportManager = new OrderReportManager(orderRespository);

            var dataEventReceived = false;
            var manualDataEvent = new ManualResetEvent(false);

            IList<object[]> report = null;
            var arguments = new Dictionary<OrderParameters, string>();

            // Add Filter Parameters
            arguments.Add(OrderParameters.OrderSize, "13");

            // Hook Data Event
            orderReportManager.DataReceived += delegate(IList<object[]> obj)
            {
                report = obj;
                dataEventReceived = true;
                manualDataEvent.Set();
            };

            // Request Report Data
            orderReportManager.RequestReport(arguments);

            manualDataEvent.WaitOne(2000);

            Assert.IsNotNull(report);
            Assert.IsTrue(dataEventReceived, "Report Event");
            Assert.Greater(report.Count, 0, "Return Count");
        }

        [Test]
        [Category("Integration")]
        public void StartOrderTimeQueryTest()
        {
            IOrderRepository orderRespository = ContextRegistry.GetContext()["OrderRespository"] as IOrderRepository;
            var orderReportManager = new OrderReportManager(orderRespository);

            var dataEventReceived = false;
            var manualDataEvent = new ManualResetEvent(false);

            IList<object[]> report = null;
            var arguments = new Dictionary<OrderParameters, string>();

            // Add Filter Parameters
            arguments.Add(OrderParameters.StartOrderDateTime, "2013-06-19 09:30:00");

            // Hook Data Event
            orderReportManager.DataReceived += delegate(IList<object[]> obj)
            {
                report = obj;
                dataEventReceived = true;
                manualDataEvent.Set();
            };

            // Request Report Data
            orderReportManager.RequestReport(arguments);

            manualDataEvent.WaitOne(2000);

            Assert.IsNotNull(report);
            Assert.IsTrue(dataEventReceived, "Report Event");
            Assert.Greater(report.Count, 0, "Return Count");
        }

        [Test]
        [Category("Integration")]
        public void EndOrderTimeQueryTest()
        {
            IOrderRepository orderRespository = ContextRegistry.GetContext()["OrderRespository"] as IOrderRepository;
            var orderReportManager = new OrderReportManager(orderRespository);

            var dataEventReceived = false;
            var manualDataEvent = new ManualResetEvent(false);

            IList<object[]> report = null;
            var arguments = new Dictionary<OrderParameters, string>();

            // Add Filter Parameters
            arguments.Add(OrderParameters.EndOrderDateTime, "2013-06-19 09:31:00");

            // Hook Data Event
            orderReportManager.DataReceived += delegate(IList<object[]> obj)
            {
                report = obj;
                dataEventReceived = true;
                manualDataEvent.Set();
            };

            // Request Report Data
            orderReportManager.RequestReport(arguments);

            manualDataEvent.WaitOne(2000);

            Assert.IsNotNull(report);
            Assert.IsTrue(dataEventReceived, "Report Event");
            Assert.Greater(report.Count, 0, "Return Count");
        }

        [Test]
        [Category("Integration")]
        public void OrderStatusCancelledQueryTest()
        {
            IOrderRepository orderRespository = ContextRegistry.GetContext()["OrderRespository"] as IOrderRepository;
            var orderReportManager = new OrderReportManager(orderRespository);

            var dataEventReceived = false;
            var manualDataEvent = new ManualResetEvent(false);

            IList<object[]> report = null;
            var arguments = new Dictionary<OrderParameters, string>();

            // Add Filter Parameters
            arguments.Add(OrderParameters.OrderStatus, "CANCELLED");

            // Hook Data Event
            orderReportManager.DataReceived += delegate(IList<object[]> obj)
            {
                report = obj;
                dataEventReceived = true;
                manualDataEvent.Set();
            };

            // Request Report Data
            orderReportManager.RequestReport(arguments);

            manualDataEvent.WaitOne(2000);

            Assert.IsNotNull(report);
            Assert.IsTrue(dataEventReceived, "Report Event");
            Assert.Greater(report.Count, 0, "Return Count");
        }

        [Test]
        [Category("Integration")]
        public void OrderStatusOpenQueryTest()
        {
            IOrderRepository orderRespository = ContextRegistry.GetContext()["OrderRespository"] as IOrderRepository;
            var orderReportManager = new OrderReportManager(orderRespository);

            var dataEventReceived = false;
            var manualDataEvent = new ManualResetEvent(false);

            IList<object[]> report = null;
            var arguments = new Dictionary<OrderParameters, string>();

            // Add Filter Parameters
            arguments.Add(OrderParameters.OrderStatus, "OPEN");

            // Hook Data Event
            orderReportManager.DataReceived += delegate(IList<object[]> obj)
            {
                report = obj;
                dataEventReceived = true;
                manualDataEvent.Set();
            };

            // Request Report Data
            orderReportManager.RequestReport(arguments);

            manualDataEvent.WaitOne(2000);

            Assert.IsNotNull(report);
            Assert.IsTrue(dataEventReceived, "Report Event");
            Assert.Greater(report.Count, 0, "Return Count");
        }

        [Test]
        [Category("Integration")]
        public void OrderStatusExecutedQueryTest()
        {
            IOrderRepository orderRespository = ContextRegistry.GetContext()["OrderRespository"] as IOrderRepository;
            var orderReportManager = new OrderReportManager(orderRespository);

            var dataEventReceived = false;
            var manualDataEvent = new ManualResetEvent(false);

            IList<object[]> report = null;
            var arguments = new Dictionary<OrderParameters, string>();

            // Add Filter Parameters
            arguments.Add(OrderParameters.OrderStatus, "EXECUTED");

            // Hook Data Event
            orderReportManager.DataReceived += delegate(IList<object[]> obj)
            {
                report = obj;
                dataEventReceived = true;
                manualDataEvent.Set();
            };

            // Request Report Data
            orderReportManager.RequestReport(arguments);

            manualDataEvent.WaitOne(2000);

            Assert.IsNotNull(report);
            Assert.IsTrue(dataEventReceived, "Report Event");
            Assert.Greater(report.Count, 0, "Return Count");
        }

        [Test]
        [Category("Integration")]
        public void OrderStrategyIdQueryTest()
        {
            IOrderRepository orderRespository = ContextRegistry.GetContext()["OrderRespository"] as IOrderRepository;
            var orderReportManager = new OrderReportManager(orderRespository);

            var dataEventReceived = false;
            var manualDataEvent = new ManualResetEvent(false);

            IList<object[]> report = null;
            var arguments = new Dictionary<OrderParameters, string>();

            // Add Filter Parameters
            arguments.Add(OrderParameters.StrategyId, "3");

            // Hook Data Event
            orderReportManager.DataReceived += delegate(IList<object[]> obj)
            {
                report = obj;
                dataEventReceived = true;
                manualDataEvent.Set();
            };

            // Request Report Data
            orderReportManager.RequestReport(arguments);

            manualDataEvent.WaitOne(2000);

            Assert.IsNotNull(report);
            Assert.IsTrue(dataEventReceived, "Report Event");
            Assert.Greater(report.Count, 0, "Return Count");
        }

        [Test]
        [Category("Integration")]
        public void OrderDiscriminatorQueryTest()
        {
            IOrderRepository orderRespository = ContextRegistry.GetContext()["OrderRespository"] as IOrderRepository;
            var orderReportManager = new OrderReportManager(orderRespository);

            var dataEventReceived = false;
            var manualDataEvent = new ManualResetEvent(false);

            IList<object[]> report = null;
            var arguments = new Dictionary<OrderParameters, string>();

            // Add Filter Parameters
            arguments.Add(OrderParameters.discriminator, "LimitOrder");

            // Hook Data Event
            orderReportManager.DataReceived += delegate(IList<object[]> obj)
            {
                report = obj;
                dataEventReceived = true;
                manualDataEvent.Set();
            };

            // Request Report Data
            orderReportManager.RequestReport(arguments);

            manualDataEvent.WaitOne(2000);

            Assert.IsNotNull(report);
            Assert.IsTrue(dataEventReceived, "Report Event");
            Assert.Greater(report.Count, 0, "Return Count");
        }

        [Test]
        [Category("Integration")]
        public void OrderLimitPriceQueryTest()
        {
            IOrderRepository orderRespository = ContextRegistry.GetContext()["OrderRespository"] as IOrderRepository;
            var orderReportManager = new OrderReportManager(orderRespository);

            var dataEventReceived = false;
            var manualDataEvent = new ManualResetEvent(false);

            IList<object[]> report = null;
            var arguments = new Dictionary<OrderParameters, string>();

            // Add Filter Parameters
            arguments.Add(OrderParameters.LimitPrice, "69.43");

            // Hook Data Event
            orderReportManager.DataReceived += delegate(IList<object[]> obj)
            {
                report = obj;
                dataEventReceived = true;
                manualDataEvent.Set();
            };

            // Request Report Data
            orderReportManager.RequestReport(arguments);

            manualDataEvent.WaitOne(2000);

            Assert.IsNotNull(report);
            Assert.IsTrue(dataEventReceived, "Report Event");
            Assert.Greater(report.Count, 0, "Return Count");
        }

        [Test]
        [Category("Integration")]
        public void OrderFillQueryTest()
        {
            IOrderRepository orderRespository = ContextRegistry.GetContext()["OrderRespository"] as IOrderRepository;
            var orderReportManager = new OrderReportManager(orderRespository);

            var dataEventReceived = false;
            var manualDataEvent = new ManualResetEvent(false);

            IList<object[]> report = null;
            var arguments = new Dictionary<OrderParameters, string>();

            // Add Filter Parameters
            arguments.Add(OrderParameters.OrderSide, "BUY");
            arguments.Add(OrderParameters.StartOrderDateTime, "2013-06-19 09:30:00");
            arguments.Add(OrderParameters.ExecutionSize, "40");

            // Hook Data Event
            orderReportManager.DataReceived += delegate(IList<object[]> obj)
            {
                report = obj;
                dataEventReceived = true;
                manualDataEvent.Set();
            };

            // Request Report Data
            orderReportManager.RequestReport(arguments);

            manualDataEvent.WaitOne(2000);

            Assert.IsNotNull(report);
            Assert.IsTrue(dataEventReceived, "Report Event");
            Assert.Greater(report.Count, 0, "Return Count");
        }

        [Test]
        [Category("Integration")]
        public void OrderFillExecutionSizeQueryTest()
        {
            IOrderRepository orderRespository = ContextRegistry.GetContext()["OrderRespository"] as IOrderRepository;
            var orderReportManager = new OrderReportManager(orderRespository);

            var dataEventReceived = false;
            var manualDataEvent = new ManualResetEvent(false);

            IList<object[]> report = null;
            var arguments = new Dictionary<OrderParameters, string>();

            // Add Filter Parameters
            arguments.Add(OrderParameters.ExecutionSize, "40");

            // Hook Data Event
            orderReportManager.DataReceived += delegate(IList<object[]> obj)
            {
                report = obj;
                dataEventReceived = true;
                manualDataEvent.Set();
            };

            // Request Report Data
            orderReportManager.RequestReport(arguments);

            manualDataEvent.WaitOne(2000);

            Assert.IsNotNull(report);
            Assert.IsTrue(dataEventReceived, "Report Event");
            Assert.Greater(report.Count, 0, "Return Count");
        }

        [Test]
        [Category("Integration")]
        public void OrderFillExecutionPriceQueryTest()
        {
            IOrderRepository orderRespository = ContextRegistry.GetContext()["OrderRespository"] as IOrderRepository;
            var orderReportManager = new OrderReportManager(orderRespository);

            var dataEventReceived = false;
            var manualDataEvent = new ManualResetEvent(false);

            IList<object[]> report = null;
            var arguments = new Dictionary<OrderParameters, string>();

            // Add Filter Parameters
            arguments.Add(OrderParameters.ExecutionPrice, "65");

            // Hook Data Event
            orderReportManager.DataReceived += delegate(IList<object[]> obj)
            {
                report = obj;
                dataEventReceived = true;
                manualDataEvent.Set();
            };

            // Request Report Data
            orderReportManager.RequestReport(arguments);

            manualDataEvent.WaitOne(2000);

            Assert.IsNotNull(report);
            Assert.IsTrue(dataEventReceived, "Report Event");
            Assert.Greater(report.Count, 0, "Return Count");
        }

        [Test]
        [Category("Integration")]
        public void OrderParametersTest()
        {
            OrderParameters flagValue = OrderParameters.ExecutionId;

            OrderParameters orderId = OrderParameters.OrderId;
            OrderParameters orderSide = OrderParameters.OrderSide;
            OrderParameters orderSize = OrderParameters.OrderSize;
            OrderParameters triggerPrice = OrderParameters.TriggerPrice;
            OrderParameters limitPrice = OrderParameters.LimitPrice;
            OrderParameters symbol = OrderParameters.Symbol;
            OrderParameters orderStatus = OrderParameters.OrderStatus;
            OrderParameters orderDateTime = OrderParameters.OrderDateTime;
            OrderParameters startOrderDateTime = OrderParameters.StartOrderDateTime;
            OrderParameters endOrderDateTime = OrderParameters.EndOrderDateTime;
            OrderParameters startegyId = OrderParameters.StrategyId;
            OrderParameters discriminator = OrderParameters.discriminator;
            OrderParameters orderExecutionProvider = OrderParameters.OrderExecutionProvider;

            OrderParameters executionId = OrderParameters.ExecutionId;
            OrderParameters executionPrice = OrderParameters.ExecutionPrice;
            OrderParameters executionSize = OrderParameters.ExecutionSize;

            Assert.IsTrue(!orderId.HasFlag(flagValue), "Order ID");
            Assert.IsTrue(!orderSide.HasFlag(flagValue), "Order Side");
            Assert.IsTrue(!orderSize.HasFlag(flagValue), "Order Size");
            Assert.IsTrue(!triggerPrice.HasFlag(flagValue), "Trigger Price");
            Assert.IsTrue(!limitPrice.HasFlag(flagValue), "Limit Price");
            Assert.IsTrue(!symbol.HasFlag(flagValue), "Symbol");
            Assert.IsTrue(!orderStatus.HasFlag(flagValue), "Order Status");
            Assert.IsTrue(!orderDateTime.HasFlag(flagValue), "Order Date Time");
            Assert.IsTrue(!startOrderDateTime.HasFlag(flagValue), "Start Order Date Time");
            Assert.IsTrue(!endOrderDateTime.HasFlag(flagValue), "End Order Date Time");
            Assert.IsTrue(!startegyId.HasFlag(flagValue), "Strategy ID");
            Assert.IsTrue(!discriminator.HasFlag(flagValue), "Discriminator");
            Assert.IsTrue(!orderExecutionProvider.HasFlag(flagValue), "Order Execution Provider");
            
            Assert.IsTrue(executionId.HasFlag(flagValue), "Execution ID");
            Assert.IsTrue(executionPrice.HasFlag(flagValue), "Execution Price");
            Assert.IsTrue(executionSize.HasFlag(flagValue), "Execution Size");
        }
    }
}
