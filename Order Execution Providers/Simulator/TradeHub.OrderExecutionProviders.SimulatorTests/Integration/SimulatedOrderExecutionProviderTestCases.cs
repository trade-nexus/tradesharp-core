using System;
using System.Threading;
using NUnit.Framework;
using Spring.Context.Support;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.OrderExecutionProviders.Simulator.Provider;
using Constants = TradeHub.Common.Core.Constants;

namespace TradeHub.OrderExecutionProviders.SimulatorTests.Integration
{
    [TestFixture]
    class SimulatedOrderExecutionProviderTestCases
    {
        private SimulatedOrderExecutionProvider _orderExecutionProvider;
        [SetUp]
        public void SetUp()
        {
            _orderExecutionProvider = ContextRegistry.GetContext()["SimulatedOrderExecutionProvider"] as SimulatedOrderExecutionProvider;
        }

        [Test]
        [Category("Integration")]
        public void ConnectOrderExecutionProviderTestCase()
        {
            bool isConnected = false;
            var manualLogonEvent = new ManualResetEvent(false);

            _orderExecutionProvider.LogonArrived +=
                    delegate(string obj)
                    {
                        isConnected = true;
                        manualLogonEvent.Set();
                    };

            _orderExecutionProvider.Start();
            manualLogonEvent.WaitOne(30000, false);

            Assert.AreEqual(true, isConnected);
        }

        [Test]
        [Category("Integration")]
        public void DisconnectOrderExecutionProviderTestCase()
        {
            bool isConnected = false;
            var manualLogonEvent = new ManualResetEvent(false);
            _orderExecutionProvider.LogonArrived +=
                    delegate(string obj)
                    {
                        isConnected = true;
                        _orderExecutionProvider.Stop();
                        manualLogonEvent.Set();
                    };

            bool isDisconnected = false;
            var manualLogoutEvent = new ManualResetEvent(false);
            _orderExecutionProvider.LogoutArrived +=
                    delegate(string obj)
                    {
                        isDisconnected = true;
                        manualLogoutEvent.Set();
                    };

            _orderExecutionProvider.Start();
            manualLogonEvent.WaitOne(30000, false);
            manualLogoutEvent.WaitOne(30000, false);

            Assert.AreEqual(true, isConnected, "Connected");
            Assert.AreEqual(true, isDisconnected, "Disconnected");
        }

        [Test]
        [Category("Console")]
        public void MarketOrderTestCase()
        {
            bool isConnected = false;
            bool newArrived = false;
            bool executionArrived = false;

            var manualLogonEvent = new ManualResetEvent(false);
            var manualNewEvent = new ManualResetEvent(false);
            var manualExecutionEvent = new ManualResetEvent(false);

            MarketOrder marketOrder= new MarketOrder(Constants.OrderExecutionProvider.Simulated);
            marketOrder.OrderID = "AA";

            _orderExecutionProvider.LogonArrived +=
                    delegate(string obj)
                    {
                        isConnected = true;
                        Console.WriteLine("Logon Received");
                        _orderExecutionProvider.SendMarketOrder(marketOrder);
                        manualLogonEvent.Set();
                    };

            _orderExecutionProvider.NewArrived +=
                    delegate(Order obj)
                    {
                        newArrived = true;
                        Console.WriteLine("New Received");
                        manualNewEvent.Set();
                    };

            _orderExecutionProvider.ExecutionArrived +=
                    delegate(Execution obj)
                    {
                        executionArrived = true;
                        Console.WriteLine("Execution Received");
                        manualExecutionEvent.Set();
                    };

            _orderExecutionProvider.Start();

            manualLogonEvent.WaitOne(30000, false);
            manualNewEvent.WaitOne(300000, false);
            manualExecutionEvent.WaitOne(300000, false);

            Assert.AreEqual(true, isConnected, "Is Execution Order Provider connected");
            Assert.AreEqual(true, newArrived, "New arrived");
            Assert.AreEqual(true, executionArrived, "Execution arrived");
        }

        [Test]
        [Category("Console")]
        public void LimitOrderTestCase()
        {
            bool isConnected = false;
            bool newArrived = false;
            bool executionArrived = false;

            var manualLogonEvent = new ManualResetEvent(false);
            var manualNewEvent = new ManualResetEvent(false);
            var manualExecutionEvent = new ManualResetEvent(false);

            LimitOrder limitOrder = new LimitOrder(Constants.OrderExecutionProvider.Simulated);
            limitOrder.OrderID = "AA";

            _orderExecutionProvider.LogonArrived +=
                    delegate(string obj)
                    {
                        isConnected = true;
                        Console.WriteLine("Logon Received");
                        _orderExecutionProvider.SendLimitOrder(limitOrder);
                        manualLogonEvent.Set();
                    };

            _orderExecutionProvider.NewArrived +=
                    delegate(Order obj)
                    {
                        newArrived = true;
                        Console.WriteLine("New Received");
                        manualNewEvent.Set();
                    };

            _orderExecutionProvider.ExecutionArrived +=
                    delegate(Execution obj)
                    {
                        executionArrived = true;
                        Console.WriteLine("Execution Received");
                        manualExecutionEvent.Set();
                    };

            _orderExecutionProvider.Start();

            manualLogonEvent.WaitOne(30000, false);
            manualNewEvent.WaitOne(300000, false);
            manualExecutionEvent.WaitOne(300000, false);

            Assert.AreEqual(true, isConnected, "Is Execution Order Provider connected");
            Assert.AreEqual(true, newArrived, "New arrived");
            Assert.AreEqual(true, executionArrived, "Execution arrived");
        }

        [Test]
        [Category("Console")]
        public void CancelOrderTestCase()
        {
            bool isConnected = false;
            bool newArrived = false;
            bool cancellationArrived = false;

            var manualLogonEvent = new ManualResetEvent(false);
            var manualNewEvent = new ManualResetEvent(false);
            var manualCancellaionEvent = new ManualResetEvent(false);

            LimitOrder limitOrder = new LimitOrder(Constants.OrderExecutionProvider.Simulated);
            limitOrder.OrderID = "AA";

            _orderExecutionProvider.LogonArrived +=
                    delegate(string obj)
                    {
                        isConnected = true;
                        Console.WriteLine("Logon Received");
                        _orderExecutionProvider.SendLimitOrder(limitOrder);
                        manualLogonEvent.Set();
                    };

            _orderExecutionProvider.NewArrived +=
                    delegate(Order obj)
                    {
                        newArrived = true;
                        Console.WriteLine("New Received");
                        _orderExecutionProvider.CancelLimitOrder(limitOrder);
                        manualNewEvent.Set();
                    };

            _orderExecutionProvider.CancellationArrived +=
                    delegate(Order obj)
                    {
                        cancellationArrived = true;
                        Console.WriteLine("Cancellation Received");
                        manualCancellaionEvent.Set();
                    };

            _orderExecutionProvider.Start();

            manualLogonEvent.WaitOne(30000, false);
            manualNewEvent.WaitOne(300000, false);
            manualCancellaionEvent.WaitOne(300000, false);

            Assert.AreEqual(true, isConnected, "Is Execution Order Provider connected");
            Assert.AreEqual(true, newArrived, "New arrived");
            Assert.AreEqual(true, cancellationArrived, "Cancellation arrived");
        }

        [Test]
        [Category("Console")]
        public void LocateMessageTestCase()
        {
            bool isConnected = false;
            bool locateArrived = false;

            var manualLogonEvent = new ManualResetEvent(false);
            var manualLocateEvent = new ManualResetEvent(false);

            _orderExecutionProvider.LogonArrived +=
                    delegate(string obj)
                    {
                        isConnected = true;
                        Console.WriteLine("Logon Received");
                        manualLogonEvent.Set();
                    };

            _orderExecutionProvider.OnLocateMessage +=
                    delegate(LimitOrder obj)
                    {
                        locateArrived = true;
                        Console.WriteLine("Locate Received");
                        manualLocateEvent.Set();
                    };

            _orderExecutionProvider.Start();

            manualLogonEvent.WaitOne(30000, false);
            manualLocateEvent.WaitOne(300000, false);

            Assert.AreEqual(true, isConnected, "Is Execution Order Provider connected");
            Assert.AreEqual(true, locateArrived, "Locate arrived");
        }

        [Test]
        [Category("Console")]
        public void PositionTestCase()
        {
            bool isConnected = false;
            bool positionArrived = false;

            var manualLogonEvent = new ManualResetEvent(false);
            var manualPositionEvent = new ManualResetEvent(false);

            _orderExecutionProvider.LogonArrived +=
                    delegate(string obj)
                    {
                        isConnected = true;
                        Console.WriteLine("Logon Received");
                        manualLogonEvent.Set();
                    };

            _orderExecutionProvider.OnPositionMessage += delegate(Position position)
            {
                positionArrived = true;
                Console.WriteLine("Position Received");
                manualPositionEvent.Set();
            };
            _orderExecutionProvider.Start();

            manualLogonEvent.WaitOne(30000, false);
            manualPositionEvent.WaitOne(30000, false);
            Assert.AreEqual(true, isConnected, "Is Execution Order Provider connected");
            Assert.AreEqual(true, positionArrived, "Position arrived");

        }
    }
}
