using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.FactoryMethods;
using TradeHub.OrderExecutionProvider.Fxcm.Provider;

namespace TradeHub.OrderExecutionProvider.Fxcm.Tests.Integration
{
    [TestFixture]
    class OrderExecutionProviderTestCase
    {
        FxcmOrderExecutionProvider _provider = new FxcmOrderExecutionProvider();

        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        [Category("Integration")]
        public void ConnectionTestCase()
        {
            bool logon = false;
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            _provider.LogonArrived += delegate(string dataProvider)
            {
                logon = true;
                resetEvent.Set();
            };

            //start provider
            _provider.Start();
            resetEvent.WaitOne(10000);
            Assert.True(logon, "Logon Not Arrived");

            if (logon)
            {
                bool logout = false;
                _provider.LogoutArrived += delegate(string dataProvider)
                {
                    logout = true;
                    resetEvent.Set();
                };
                resetEvent.Reset();
                _provider.Stop();
                resetEvent.WaitOne(5000);
                Assert.True(logout);
            }
        }

        [Test]
        [Category("Integration")]
        public void MarketOrderTestCase()
        {
            MarketOrder order = OrderMessage.GenerateMarketOrder(DateTime.Now.ToString("yyMMddHmsfff"),
                new Security() {Symbol = "EUR/USD"}, OrderSide.BUY, 1000,
                "Fxcm");
            order.OrderCurrency = "EUR";
            order.OrderTif = "GTC";

            bool logon = false;
            ManualResetEvent resetEvent = new ManualResetEvent(false);

            _provider.LogonArrived += delegate(string dataProvider)
            {
                logon = true;
                resetEvent.Set();
            };

            //start provider
            _provider.Start();

            resetEvent.WaitOne(5000);

            Assert.True(logon, "Logon Arrived");

            if (logon)
            {
                bool newArrived = false;
                bool executionArrived = false;

                _provider.NewArrived += delegate(Order newOrder)
                {
                    newArrived = true;
                };

                _provider.ExecutionArrived += delegate(Execution execution)
                {
                    executionArrived = true;
                };

                _provider.SendMarketOrder(order);

                resetEvent.Reset();
                resetEvent.WaitOne(10000);

                bool logout = false;

                _provider.LogoutArrived += delegate(string dataProvider)
                {
                    logout = true;
                    resetEvent.Set();
                };

                resetEvent.Reset();

                _provider.Stop();

                resetEvent.WaitOne(5000);

                Assert.True(logout, "Logout Arrived");
                Assert.True(newArrived, "New Arrived");
                Assert.True(executionArrived, "Execution Arrived");
            }
        }

        [Test]
        [Category("Integration")]
        public void CancelOrderTestCase()
        {
            LimitOrder order = OrderMessage.GenerateLimitOrder(DateTime.Now.ToString("yyMMddHmsfff"), new Security() { Symbol = "EUR/USD" }, OrderSide.BUY, 1000, 1.090M,
                "Fxcm");

            order.OrderCurrency = "EUR";
            order.OrderTif = "GTC";
            
            bool logon = false;

            ManualResetEvent resetEvent = new ManualResetEvent(false);
            _provider.LogonArrived += delegate(string dataProvider)
            {
                logon = true;
                resetEvent.Set();
            };
            //start provider
            _provider.Start();
            resetEvent.WaitOne(5000);
            Assert.True(logon);
            if (logon)
            {
                bool newArrived = false;
                bool cancellationArrived = false;
                bool logout = false;

                _provider.NewArrived += delegate(Order newOrder)
                {
                    newArrived = true;
                    _provider.CancelLimitOrder(newOrder);
                };
                
                _provider.CancellationArrived += delegate(Order cancelledOrder)
                {
                    cancellationArrived = true;
                    resetEvent.Set();
                };
               
                _provider.SendLimitOrder(order);
                resetEvent.Reset();
                resetEvent.WaitOne(30000);

                _provider.LogoutArrived += delegate(string dataProvider)
                {
                    logout = true;
                    resetEvent.Set();
                };

                resetEvent.Reset();
                _provider.Stop();
                resetEvent.WaitOne(5000);
                Assert.True(logout, "Logout Arrived");
                Assert.True(newArrived, "New Arrived");
                Assert.True(cancellationArrived, "Cancellation Arrived");
            }
        }
    }
}
