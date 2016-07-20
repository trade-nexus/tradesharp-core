using System.Threading;
using NUnit.Framework;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.OrderExecutionEngine.Client.Service;
using Constants = TradeHub.Common.Core.Constants;

namespace TradeHub.OrderExecutionProvider.SimulatedExchange.Test.Integration
{
    [TestFixture]
    public class SimulatedExchangeOrderExecutionProviderTest
    {

        private SimulatedExchangeOrderExecutionProvider _orderExecutionProvider;
        [SetUp]
        public void SetUp()
        {
            _orderExecutionProvider = new SimulatedExchangeOrderExecutionProvider();
        }

        [TearDown]
        public void Close()
        {
            _orderExecutionProvider.Stop();
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
        [Category("Integration")]
        public void MarketOrderTestCase()
        {
            bool isConnected = false;
            bool newArrived = false;
            bool executionArrived = false;

            var manualLogonEvent = new ManualResetEvent(false);
            var manualNewEvent = new ManualResetEvent(false);
            var manualExecutionEvent = new ManualResetEvent(false);

            MarketOrder marketOrder = new MarketOrder(Constants.OrderExecutionProvider.Simulated);
            marketOrder.OrderID = "AA";
            marketOrder.OrderSize = 100;
            marketOrder.OrderSide = Constants.OrderSide.BUY;

            _orderExecutionProvider.LogonArrived +=
                    delegate(string obj)
                    {
                        isConnected = true;
                        _orderExecutionProvider.SendMarketOrder(marketOrder);
                        manualLogonEvent.Set();
                    };

            _orderExecutionProvider.NewArrived +=
                    delegate(Order obj)
                    {
                        newArrived = true;
                        manualNewEvent.Set();
                    };

            //_orderExecutionProvider.ExecutionArrived +=
            //        delegate(Execution obj)
            //        {
            //            executionArrived = true;
            //            manualExecutionEvent.Set();
            //        };

            _orderExecutionProvider.Start();

            manualLogonEvent.WaitOne(3000, false);
            manualNewEvent.WaitOne(3000, false);
            //manualExecutionEvent.WaitOne(3000, false);

            Assert.AreEqual(true, isConnected, "Is Execution Order Provider connected");
            Assert.AreEqual(true, newArrived, "New arrived");
            //Assert.AreEqual(true, executionArrived, "Execution arrived");
        }

        [Test]
        [Category("Integration")]
        public void LimitOrderTestCase()
        {
            bool isConnected = false;
            bool newArrived = false;
            bool executionArrived = false;

            var manualLogonEvent = new ManualResetEvent(false);
            var manualNewEvent = new ManualResetEvent(false);
            var manualExecutionEvent = new ManualResetEvent(false);

            LimitOrder limitOrder = new LimitOrder(Constants.OrderExecutionProvider.Simulated);
            limitOrder.Security= new Security(){Symbol = "AAPL"};
            limitOrder.OrderID = "AA";
            limitOrder.OrderSide = Constants.OrderSide.BUY;
            limitOrder.OrderSize = 100;
            limitOrder.LimitPrice = 1.34M;

            _orderExecutionProvider.LogonArrived +=
                    delegate(string obj)
                    {
                        isConnected = true;
                        _orderExecutionProvider.SendLimitOrder(limitOrder);
                        manualLogonEvent.Set();
                    };

            _orderExecutionProvider.NewArrived +=
                    delegate(Order obj)
                    {
                        newArrived = true;
                        manualNewEvent.Set();
                    };

            //_orderExecutionProvider.ExecutionArrived +=
            //        delegate(Execution obj)
            //        {
            //            executionArrived = true;
            //            manualExecutionEvent.Set();
            //        };

            _orderExecutionProvider.Start();

            manualLogonEvent.WaitOne(3000, false);
            manualNewEvent.WaitOne(3000, false);
            //manualExecutionEvent.WaitOne(300000, false);

            Assert.AreEqual(true, isConnected, "Is Execution Order Provider connected");
            Assert.AreEqual(true, newArrived, "New arrived");
            //Assert.AreEqual(true, executionArrived, "Execution arrived");
        }

        [Test]
        public void TestOrderExecutionLogin()
        {
            var manualResetEvent = new ManualResetEvent(false);
            string loginMessage = null;
            var executionEngineClient=new OrderExecutionEngineClient();
            executionEngineClient.Start();
            manualResetEvent.WaitOne(5000);
            executionEngineClient.LogoutArrived += delegate(string s)
                {
                    loginMessage = s;
                };
            executionEngineClient.SendLoginRequest(new Login() { OrderExecutionProvider = Common.Core.Constants.OrderExecutionProvider.SimulatedExchange });
            manualResetEvent.WaitOne(5000);
            executionEngineClient.SendMarketOrderRequest(new MarketOrder("1", OrderSide.BUY, 10, "123", "USD",
                                                                         new Security {Symbol = "AAPL"},
                                                                         Common.Core.Constants.OrderExecutionProvider.
                                                                             SimulatedExchange));
            manualResetEvent.WaitOne(5000);
            executionEngineClient.Shutdown();
            System.Console.WriteLine(loginMessage);
            Assert.AreEqual(false,string.IsNullOrEmpty(loginMessage));
        }
    }
}
