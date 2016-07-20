using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using QuickFix.Fields;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.FactoryMethods;
using TradeHub.OrderExecutionProvider.WldxFix.Provider;

namespace TradeHub.OrderExecutionProvider.WldxFix.Tests
{
    [TestFixture]
    public class WldxFixProviderTests
    {
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
        public void LogonLogoutTest()
        {
            bool logon = false;
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            WldxFixOrderExecutionProvider provider=new WldxFixOrderExecutionProvider();
            provider.LogonArrived += delegate(string dataProvider)
            {
                logon = true;
                resetEvent.Set();
            };
            //start provider
            provider.Start();
            resetEvent.WaitOne(10000);
            Assert.True(logon,"Logon Not Arrived");
            if (logon)
            {
                bool logout = false;
                provider.LogoutArrived += delegate(string dataProvider)
                {
                    logout = true;
                    resetEvent.Set();
                };
                resetEvent.Reset();
                provider.Stop();
                resetEvent.WaitOne(5000);
                Assert.True(logout);
            }
        }

        [Test]
        [Category("Integration")]
        public void SendOrderTest()
        {
            MarketOrder order = OrderMessage.GenerateMarketOrder(DateTime.Now.ToString("yyMMddHmsfff"),new Security() { Symbol = "TNA" }, OrderSide.BUY, 3,
                "WldxFix");
            order.Exchange = "SMARTEDGEP";
            bool logon = false;
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            WldxFixOrderExecutionProvider provider = new WldxFixOrderExecutionProvider();
            provider.LogonArrived += delegate(string dataProvider)
            {
                logon = true;
                resetEvent.Set();
            };
            //start provider
            provider.Start();
            resetEvent.WaitOne(5000);
            Assert.True(logon);
            if (logon)
            {
                bool newArrived = false;
                bool executionArrived = false;
                provider.NewArrived += delegate(Order newOrder)
                {
                    newArrived = true;
                };

                provider.ExecutionArrived += delegate(Execution execution)
                {
                    executionArrived = true;
                };
                provider.SendMarketOrder(order);
                resetEvent.Reset();
                resetEvent.WaitOne(10000);

                bool logout = false;
                provider.LogoutArrived += delegate(string dataProvider)
                {
                    logout = true;
                    resetEvent.Set();
                };
                resetEvent.Reset();
                provider.Stop();
                resetEvent.WaitOne(5000);
                Assert.True(logout);
                Assert.True(newArrived);
                Assert.True(executionArrived);
            }
        }

        [Test]
        [Category("Integration")]
        public void CancelOrderTest()
        {
            LimitOrder order = OrderMessage.GenerateLimitOrder(DateTime.Now.ToString("yyMMddHmsfff"),new Security() { Symbol = "TNA" }, OrderSide.SELL, 3, 105,
                "WldxFix");
            order.Exchange = "SMARTEDGEP";
            bool logon = false;
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            WldxFixOrderExecutionProvider provider = new WldxFixOrderExecutionProvider();
            provider.LogonArrived += delegate(string dataProvider)
            {
                logon = true;
                resetEvent.Set();
            };
            //start provider
            provider.Start();
            resetEvent.WaitOne(5000);
            Assert.True(logon);
            if (logon)
            {
                bool newArrived = false;
                bool cancellationArrived = false;

                provider.NewArrived += delegate(Order newOrder)
                {
                    newArrived = true;
                    provider.CancelLimitOrder(order);
                };
                provider.CancellationArrived += delegate(Order cancelledOrder)
                {
                    cancellationArrived = true;
                    resetEvent.Set();
                };
                provider.SendLimitOrder(order);
                resetEvent.Reset();
                resetEvent.WaitOne(30000);

                bool logout = false;
                provider.LogoutArrived += delegate(string dataProvider)
                {
                    logout = true;
                    resetEvent.Set();
                };
                resetEvent.Reset();
                provider.Stop();
                resetEvent.WaitOne(5000);
                Assert.True(logout);
                Assert.True(newArrived);
                Assert.True(cancellationArrived);
            }
        }
    }
}
