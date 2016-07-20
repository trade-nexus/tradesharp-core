using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Spring.Context.Support;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.MarketDataEngine.Client.Service;
using TradeHub.MarketDataEngine.Server.Service;
using TradeHubConstants = TradeHub.Common.Core.Constants;

namespace TradeHub.StrategyEngine.HistoricalData.Tests.Integration
{
    [TestFixture]
    public class HistoricalDataServiceTests
    {
        private HistoricalDataService _service;
        private ApplicationController _applicationController;

        [SetUp]
        public void StartUp()
        {
            _applicationController = ContextRegistry.GetContext()["ApplicationController"] as ApplicationController;
            if (_applicationController != null) _applicationController.StartServer();
            _service = new HistoricalDataService(new MarketDataEngineClient());
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
                _service.Login(new Login() { MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated });
                manualConnectedEvent.Set();
            };

            _service.LogonArrived +=
                    delegate(string obj)
                    {
                        logonArrived = true;
                        _service.Logout(new Logout { MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated });
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
    }
}
