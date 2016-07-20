using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Spring.Context.Support;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.OrderExecutionEngine.Client.Service;
using TradeHub.OrderExecutionEngine.Server.Service;
using TradeHubConstants = TradeHub.Common.Core.Constants;

namespace TradeHub.StrategyEngine.OrderExecution.Tests.Integration
{
    [TestFixture]
    public class OrderExecutionServiceTests
    {
        private OrderExecutionService _service;
        private ApplicationController _applicationController;

        [SetUp]
        public void StartUp()
        {
            _applicationController = ContextRegistry.GetContext()["ApplicationController"] as ApplicationController;
            if (_applicationController != null) _applicationController.StartServer();
            _service = ContextRegistry.GetContext()["OrderExecutionService"] as OrderExecutionService;
        }

        [TearDown]
        public void Close()
        {
            _service.StopService();
            _applicationController.StopServer();
        }

        [Test]
        [Category("Integration")]
        public void ConnectivityTestCase()
        {
            bool logonArrived = false;
            bool logoutArrived = false;

            ManualResetEvent manualLogonEvent = new ManualResetEvent(false);
            ManualResetEvent manualLogoutEvent = new ManualResetEvent(false);
            ManualResetEvent manualConnectedEvent = new ManualResetEvent(false);

            _service.Connected += delegate()
            {
                _service.Login(new Login() { OrderExecutionProvider = TradeHubConstants.OrderExecutionProvider.Simulated });
                manualConnectedEvent.Set();
            };

            _service.LogonArrived +=
                    delegate(string obj)
                    {
                        logonArrived = true;
                        _service.Logout(new Logout { OrderExecutionProvider = TradeHubConstants.OrderExecutionProvider.Simulated });
                        manualLogonEvent.Set();
                    };

            _service.LogoutArrived +=
                    delegate(string obj)
                    {
                        logoutArrived = true;
                        manualLogoutEvent.Set();
                    };

            _service.StartService();

            manualConnectedEvent.WaitOne(30000, false);
            manualLogonEvent.WaitOne(30000, false);
            manualLogoutEvent.WaitOne(30000, false);

            Assert.AreEqual(true, logonArrived, "Logon Arrived");
            Assert.AreEqual(true, logoutArrived, "Logout Arrived");
        }

        [Test]
        [Category("Console")]
        public void MarketOrderTestCase()
        {
            bool logonArrived = false;
            bool logoutArrived = false;
            bool newArrived = false;
            bool executionArrived = false;

            ManualResetEvent manualLogonEvent = new ManualResetEvent(false);
            ManualResetEvent manualLogoutEvent = new ManualResetEvent(false);
            ManualResetEvent manualConnectedEvent = new ManualResetEvent(false);
            ManualResetEvent manualNewOrderEvent = new ManualResetEvent(false);
            ManualResetEvent manualExecutionEvent = new ManualResetEvent(false);

            _service.Connected += delegate()
            {
                _service.Login(new Login() { OrderExecutionProvider = TradeHubConstants.OrderExecutionProvider.Simulated });
                manualConnectedEvent.Set();
            };

            _service.LogonArrived +=
                    delegate(string obj)
                    {
                        logonArrived = true;
                        _service.SendOrder(new MarketOrder(TradeHubConstants.OrderExecutionProvider.Simulated)
                            {
                                Security = new Security{Symbol = "AAPL"},
                                OrderSide = TradeHubConstants.OrderSide.BUY,
                                OrderSize = 100,
                                OrderID = "01"
                            });
                        manualLogonEvent.Set();
                    };

            _service.NewArrived +=
            delegate(Order obj)
                {
                    newArrived = true;
                    manualNewOrderEvent.Set();
                };

            _service.ExecutionArrived +=
            delegate(Execution obj)
            {
                executionArrived = true;
                _service.Logout(new Logout { OrderExecutionProvider = TradeHubConstants.OrderExecutionProvider.Simulated });
                manualExecutionEvent.Set();
            };

            _service.LogoutArrived +=
                    delegate(string obj)
                    {
                        logoutArrived = true;
                        manualLogoutEvent.Set();
                    };

            _service.StartService();

            manualConnectedEvent.WaitOne(30000, false);
            manualLogonEvent.WaitOne(30000, false);
            manualNewOrderEvent.WaitOne(30000, false);
            manualExecutionEvent.WaitOne(30000, false);
            manualLogoutEvent.WaitOne(30000, false);

            Assert.AreEqual(true, logonArrived, "Logon Arrived");
            Assert.AreEqual(true, newArrived , "New Arrived");
            Assert.AreEqual(true, executionArrived, "Execution Arrived");
            Assert.AreEqual(true, logoutArrived, "Logout Arrived");
        }

        [Test]
        [Category("Console")]
        public void LimitOrderTestCase()
        {
            bool logonArrived = false;
            bool logoutArrived = false;
            bool newArrived = false;
            bool executionArrived = false;

            ManualResetEvent manualLogonEvent = new ManualResetEvent(false);
            ManualResetEvent manualLogoutEvent = new ManualResetEvent(false);
            ManualResetEvent manualConnectedEvent = new ManualResetEvent(false);
            ManualResetEvent manualNewOrderEvent = new ManualResetEvent(false);
            ManualResetEvent manualExecutionEvent = new ManualResetEvent(false);

            _service.Connected += delegate()
            {
                _service.Login(new Login() { OrderExecutionProvider = TradeHubConstants.OrderExecutionProvider.Simulated });
                manualConnectedEvent.Set();
            };

            _service.LogonArrived +=
                    delegate(string obj)
                    {
                        logonArrived = true;
                        _service.SendOrder(new LimitOrder(TradeHubConstants.OrderExecutionProvider.Simulated)
                        {
                            Security = new Security { Symbol = "AAPL" },
                            OrderSide = TradeHubConstants.OrderSide.BUY,
                            OrderSize = 100,
                            LimitPrice = 1.23m,
                            OrderID = "01"
                        });
                        manualLogonEvent.Set();
                    };

            _service.NewArrived +=
            delegate(Order obj)
            {
                newArrived = true;
                manualNewOrderEvent.Set();
            };

            _service.ExecutionArrived +=
            delegate(Execution obj)
            {
                executionArrived = true;
                _service.Logout(new Logout { OrderExecutionProvider = TradeHubConstants.OrderExecutionProvider.Simulated });
                manualExecutionEvent.Set();
            };

            _service.LogoutArrived +=
                    delegate(string obj)
                    {
                        logoutArrived = true;
                        manualLogoutEvent.Set();
                    };

            _service.StartService();

            manualConnectedEvent.WaitOne(30000, false);
            manualLogonEvent.WaitOne(30000, false);
            manualNewOrderEvent.WaitOne(30000, false);
            manualExecutionEvent.WaitOne(30000, false);
            manualLogoutEvent.WaitOne(30000, false);

            Assert.AreEqual(true, logonArrived, "Logon Arrived");
            Assert.AreEqual(true, newArrived, "New Arrived");
            Assert.AreEqual(true, executionArrived, "Execution Arrived");
            Assert.AreEqual(true, logoutArrived, "Logout Arrived");
        }

        [Test]
        [Category("Console")]
        public void CancelOrderTestCase()
        {
            bool logonArrived = false;
            bool logoutArrived = false;
            bool newArrived = false;
            bool cancellationArrived = false;

            ManualResetEvent manualLogonEvent = new ManualResetEvent(false);
            ManualResetEvent manualLogoutEvent = new ManualResetEvent(false);
            ManualResetEvent manualConnectedEvent = new ManualResetEvent(false);
            ManualResetEvent manualNewOrderEvent = new ManualResetEvent(false);
            ManualResetEvent manualCancellationEvent = new ManualResetEvent(false);

            _service.Connected += delegate()
            {
                _service.Login(new Login() { OrderExecutionProvider = TradeHubConstants.OrderExecutionProvider.Simulated });
                manualConnectedEvent.Set();
            };

            _service.LogonArrived +=
                    delegate(string obj)
                    {
                        logonArrived = true;
                        _service.SendOrder(new LimitOrder(TradeHubConstants.OrderExecutionProvider.Simulated)
                        {
                            Security = new Security { Symbol = "AAPL" },
                            OrderSide = TradeHubConstants.OrderSide.BUY,
                            OrderSize = 100,
                            LimitPrice = 1.23m,
                            OrderID = "01"
                        });
                        manualLogonEvent.Set();
                    };

            _service.NewArrived +=
            delegate(Order obj)
            {
                newArrived = true;
                _service.CancelOrder("01");
                manualNewOrderEvent.Set();
            };

            _service.CancellationArrived +=
            delegate(Order obj)
            {
                cancellationArrived = true;
                _service.Logout(new Logout { OrderExecutionProvider = TradeHubConstants.OrderExecutionProvider.Simulated });
                manualCancellationEvent.Set();
            };

            _service.LogoutArrived +=
                    delegate(string obj)
                    {
                        logoutArrived = true;
                        manualLogoutEvent.Set();
                    };

            _service.StartService();

            manualConnectedEvent.WaitOne(30000, false);
            manualLogonEvent.WaitOne(30000, false);
            manualNewOrderEvent.WaitOne(30000, false);
            manualCancellationEvent.WaitOne(30000, false);
            manualLogoutEvent.WaitOne(30000, false);

            Assert.AreEqual(true, logonArrived, "Logon Arrived");
            Assert.AreEqual(true, newArrived, "New Arrived");
            Assert.AreEqual(true, cancellationArrived, "Cancellation Arrived");
            Assert.AreEqual(true, logoutArrived, "Logout Arrived");
        }

        [Test]
        [Category("Console")]
        public void CancelMultipleOrderTestCases()
        {
            bool logonArrived = false;
            bool logoutArrived = false;
            bool newArrived = false;
            bool cancellationArrived = false;

            int newCount = 0;
            int cancellationCount = 0;
            string oep = TradeHubConstants.OrderExecutionProvider.Simulated;
            Security security = new Security {Symbol = "AAPL"};

            LimitOrder orderOne = new LimitOrder(oep)
                {
                    Security = security,
                    OrderSide = TradeHubConstants.OrderSide.BUY,
                    OrderSize = 100,
                    LimitPrice = 1.23m,
                    OrderID = "01"
                };

            LimitOrder orderTwo = new LimitOrder(oep)
                {
                    Security = security,
                    OrderSide = TradeHubConstants.OrderSide.BUY,
                    OrderSize = 100,
                    LimitPrice = 1.23m,
                    OrderID = "02"
                };

            LimitOrder orderThree = new LimitOrder(oep)
                {
                    Security = security,
                    OrderSide = TradeHubConstants.OrderSide.BUY,
                    OrderSize = 100,
                    LimitPrice = 1.23m,
                    OrderID = "03"
                };

            ManualResetEvent manualLogonEvent = new ManualResetEvent(false);
            ManualResetEvent manualLogoutEvent = new ManualResetEvent(false);
            ManualResetEvent manualConnectedEvent = new ManualResetEvent(false);
            ManualResetEvent manualNewOrderEvent = new ManualResetEvent(false);
            ManualResetEvent manualCancellationEvent = new ManualResetEvent(false);

            _service.Connected += delegate()
            {
                _service.Login(new Login() { OrderExecutionProvider = TradeHubConstants.OrderExecutionProvider.Simulated });
                manualConnectedEvent.Set();
            };

            _service.LogonArrived +=
                    delegate(string obj)
                    {
                        logonArrived = true;

                        _service.SendOrder(orderOne);
                        _service.SendOrder(orderTwo);
                        _service.SendOrder(orderThree);
                        
                        manualLogonEvent.Set();
                    };

            _service.NewArrived +=
                delegate(Order obj)
                    {
                        if (++newCount == 3)
                        {
                            newArrived = true;

                            _service.CancelOrder("01");
                            _service.CancelOrder("02");
                            _service.CancelOrder("03");
                            
                            manualNewOrderEvent.Set();
                        }
                    };

            _service.CancellationArrived +=
                delegate(Order obj)
                    {
                        if (++cancellationCount == 3)
                        {
                            cancellationArrived = true;
                            _service.Logout(new Logout
                                {OrderExecutionProvider = TradeHubConstants.OrderExecutionProvider.Simulated});
                            manualCancellationEvent.Set();
                        }
                    };

            _service.LogoutArrived +=
                    delegate(string obj)
                    {
                        logoutArrived = true;
                        manualLogoutEvent.Set();
                    };

            _service.StartService();

            manualConnectedEvent.WaitOne(30000, false);
            manualLogonEvent.WaitOne(30000, false);
            manualNewOrderEvent.WaitOne(30000, false);
            manualCancellationEvent.WaitOne(30000, false);
            manualLogoutEvent.WaitOne(30000, false);

            Assert.AreEqual(true, logonArrived, "Logon Arrived");
            
            Assert.AreEqual(true, newArrived, "New Arrived");
            Assert.AreEqual(3, newCount, "Count of accepted orders");

            Assert.AreEqual(true, cancellationArrived, "Cancellation Arrived");
            Assert.AreEqual(3, cancellationCount, "Count of cancelled orders");

            Assert.AreEqual(true, logoutArrived, "Logout Arrived");
        }

        [Test]
        [Category("Integration")]
        public void OrderIDGeneratorTestCase_ConnectionEstablishedWithServer()
        {
            bool logonArrived = false;
            bool logoutArrived = false;

            ManualResetEvent manualLogonEvent = new ManualResetEvent(false);
            ManualResetEvent manualLogoutEvent = new ManualResetEvent(false);
            ManualResetEvent manualConnectedEvent = new ManualResetEvent(false);

            _service.Connected += delegate()
            {
                _service.Login(new Login() { OrderExecutionProvider = TradeHubConstants.OrderExecutionProvider.Simulated });
                manualConnectedEvent.Set();
            };

            _service.LogonArrived +=
                    delegate(string obj)
                    {
                        logonArrived = true;
                        _service.Logout(new Logout { OrderExecutionProvider = TradeHubConstants.OrderExecutionProvider.Simulated });
                        manualLogonEvent.Set();
                    };

            _service.LogoutArrived +=
                    delegate(string obj)
                    {
                        logoutArrived = true;
                        manualLogoutEvent.Set();
                    };

            _service.StartService();

            manualConnectedEvent.WaitOne(30000, false);
            manualLogonEvent.WaitOne(30000, false);
            manualLogoutEvent.WaitOne(30000, false);

            string orderId = _service.GetOrderId();
            
            Console.WriteLine("Order ID: " + orderId);

            string appender = orderId.Substring(orderId.Length - 3);

            Assert.AreEqual(true, logonArrived, "Logon Arrived");
            Assert.AreEqual(true, logoutArrived, "Logout Arrived");
            Assert.IsTrue(appender.Equals("A00"), "Appender: " + appender);
        }


        [Test]
        [Category("Integration")]
        public void OrderIDGeneratorTestCase_ConnectionNotEstablishedWithServer()
        {
            string orderId = _service.GetOrderId();

            Console.WriteLine("Order ID: " + orderId);
            
            string appender = orderId.Substring(orderId.Length - 4);

            Assert.IsTrue(appender.Equals("A000"), "Appender: " + appender);
        }
    }
}
