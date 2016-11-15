/***************************************************************************** 
* Copyright 2016 Aurora Solutions 
* 
*    http://www.aurorasolutions.io 
* 
* Aurora Solutions is an innovative services and product company at 
* the forefront of the software industry, with processes and practices 
* involving Domain Driven Design(DDD), Agile methodologies to build 
* scalable, secure, reliable and high performance products.
* 
* TradeSharp is a C# based data feed and broker neutral Algorithmic 
* Trading Platform that lets trading firms or individuals automate 
* any rules based trading strategies in stocks, forex and ETFs. 
* TradeSharp allows users to connect to providers like Tradier Brokerage, 
* IQFeed, FXCM, Blackwood, Forexware, Integral, HotSpot, Currenex, 
* Interactive Brokers and more. 
* Key features: Place and Manage Orders, Risk Management, 
* Generate Customized Reports etc 
* 
* Licensed under the Apache License, Version 2.0 (the "License"); 
* you may not use this file except in compliance with the License. 
* You may obtain a copy of the License at 
* 
*    http://www.apache.org/licenses/LICENSE-2.0 
* 
* Unless required by applicable law or agreed to in writing, software 
* distributed under the License is distributed on an "AS IS" BASIS, 
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
* See the License for the specific language governing permissions and 
* limitations under the License. 
*****************************************************************************/


﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Spring.Context.Support;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataEngine.Client.Service;
using TradeHub.MarketDataEngine.Server.Service;
using TradeHubConstants = TradeHub.Common.Core.Constants;

namespace TradeHub.StrategyEngine.MarketData.Tests.Integration
{
    [TestFixture]
    public class MarketDataServiceTests
    {
        private MarketDataService _service;
        private ApplicationController _applicationController;

        [SetUp]
        public void StartUp()
        {
            _applicationController = ContextRegistry.GetContext()["ApplicationController"] as ApplicationController;
            if (_applicationController != null) _applicationController.StartServer();
            _service = ContextRegistry.GetContext()["MarketDataService"] as MarketDataService;
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

        [Test]
        [Category("Console")]
        public void TickSubscriptionTestCase_SingleSecurity()
        {

            bool logonArrived = false;
            bool logoutArrived = false;
            bool tickArrived = false;

            ManualResetEvent manualLogonEvent = new ManualResetEvent(false);
            ManualResetEvent manualLogoutEvent = new ManualResetEvent(false);
            ManualResetEvent manualTickEvent = new ManualResetEvent(false);
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
                        _service.Subscribe(new Subscribe()
                            {
                                Security = new Security {Symbol = "AAPL"},
                                MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated
                            });
                        manualLogonEvent.Set();
                    };

            _service.TickArrived +=
                    delegate(Tick obj)
                    {
                        tickArrived = true;
                        _service.Logout(new Logout { MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated });
                        manualTickEvent.Set();
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
            manualTickEvent.WaitOne(30000, false);
            manualLogoutEvent.WaitOne(30000, false);

            Assert.AreEqual(true, logonArrived, "Logon Arrived");
            Assert.AreEqual(true, tickArrived, "Tick Arrived");
            Assert.AreEqual(true, logoutArrived, "Logout Arrived");
            Assert.AreEqual(1, _service.TickSubscriptions[TradeHubConstants.MarketDataProvider.Simulated].Count, "Subscribed securities");
        }

        [Test]
        [Category("Console")]
        public void BarSubscriptionTestCase_SingleSecurity()
        {

            bool logonArrived = false;
            bool logoutArrived = false;
            bool barArrived = false;

            ManualResetEvent manualLogonEvent = new ManualResetEvent(false);
            ManualResetEvent manualLogoutEvent = new ManualResetEvent(false);
            ManualResetEvent manualBarEvent = new ManualResetEvent(false);
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
                        _service.Subscribe(new BarDataRequest()
                        {
                            Id = "A00",
                            Security = new Security { Symbol = "AAPL" },
                            MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated
                        });
                        manualLogonEvent.Set();
                    };

            _service.BarArrived +=
                    delegate(Bar obj)
                    {
                        barArrived = true;
                        _service.Logout(new Logout { MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated });
                        manualBarEvent.Set();
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
            manualBarEvent.WaitOne(30000, false);
            manualLogoutEvent.WaitOne(30000, false);

            Assert.AreEqual(true, logonArrived, "Logon Arrived");
            Assert.AreEqual(true, barArrived, "Bar Arrived");
            Assert.AreEqual(true, logoutArrived, "Logout Arrived");
            Assert.AreEqual(1, _service.BarSubscriptions[TradeHubConstants.MarketDataProvider.Simulated].Count, "Subscribed securities");
        }

        [Test]
        [Category("Console")]
        public void TickSubscriptionTestCase_SingleSecurity_MultipleTimes()
        {

            bool logonArrived = false;
            bool logoutArrived = false;
            bool tickArrived = false;

            ManualResetEvent manualLogonEvent = new ManualResetEvent(false);
            ManualResetEvent manualLogoutEvent = new ManualResetEvent(false);
            ManualResetEvent manualTickEvent = new ManualResetEvent(false);
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
                        _service.Subscribe(new Subscribe()
                        {
                            Security = new Security { Symbol = "AAPL" },
                            MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated
                        });
                        _service.Subscribe(new Subscribe()
                        {
                            Security = new Security { Symbol = "AAPL" },
                            MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated
                        });
                        manualLogonEvent.Set();
                    };

            _service.TickArrived +=
                    delegate(Tick obj)
                    {
                        tickArrived = true;
                        _service.Logout(new Logout { MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated });
                        manualTickEvent.Set();
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
            manualTickEvent.WaitOne(30000, false);
            manualLogoutEvent.WaitOne(30000, false);

            Assert.AreEqual(true, logonArrived, "Logon Arrived");
            Assert.AreEqual(true, tickArrived, "Tick Arrived");
            Assert.AreEqual(true, logoutArrived, "Logout Arrived");
            Assert.AreEqual(1, _service.TickSubscriptions[TradeHubConstants.MarketDataProvider.Simulated].Count, "Subscribed securities");
        }

        [Test]
        [Category("Console")]
        public void TickSubscriptionTestCase_MultipleSecurities()
        {

            bool logonArrived = false;
            bool logoutArrived = false;
            bool tickArrived = false;

            ManualResetEvent manualLogonEvent = new ManualResetEvent(false);
            ManualResetEvent manualLogoutEvent = new ManualResetEvent(false);
            ManualResetEvent manualTickEvent = new ManualResetEvent(false);
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
                        _service.Subscribe(new Subscribe()
                        {
                            Security = new Security { Symbol = "AAPL" },
                            MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated
                        });
                        _service.Subscribe(new Subscribe()
                        {
                            Security = new Security { Symbol = "IBM" },
                            MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated
                        });
                        manualLogonEvent.Set();
                    };

            _service.TickArrived +=
                    delegate(Tick obj)
                    {
                        tickArrived = true;
                        _service.Logout(new Logout { MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated });
                        manualTickEvent.Set();
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
            manualTickEvent.WaitOne(30000, false);
            manualLogoutEvent.WaitOne(30000, false);

            Assert.AreEqual(true, logonArrived, "Logon Arrived");
            Assert.AreEqual(true, tickArrived, "Tick Arrived");
            Assert.AreEqual(true, logoutArrived, "Logout Arrived");
            Assert.AreEqual(2, _service.TickSubscriptions[TradeHubConstants.MarketDataProvider.Simulated].Count, "Subscribed securities");
        }

        [Test]
        [Category("Console")]
        public void TickUnsubscriptionTestCase_SingleSecurity()
        {

            bool logonArrived = false;
            bool logoutArrived = false;
            bool tickArrived = false;

            ManualResetEvent manualLogonEvent = new ManualResetEvent(false);
            ManualResetEvent manualLogoutEvent = new ManualResetEvent(false);
            ManualResetEvent manualTickEvent = new ManualResetEvent(false);
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
                        _service.Subscribe(new Subscribe()
                        {
                            Security = new Security { Symbol = "AAPL" },
                            MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated
                        });
                        manualLogonEvent.Set();
                    };

            _service.TickArrived +=
                    delegate(Tick obj)
                    {
                        tickArrived = true;
                        _service.Unsubscribe(new Unsubscribe()
                        {
                            Security = new Security { Symbol = "AAPL" },
                            MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated
                        });
                        _service.Logout(new Logout { MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated });
                        manualTickEvent.Set();
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
            manualTickEvent.WaitOne(30000, false);
            manualLogoutEvent.WaitOne(30000, false);

            Assert.AreEqual(true, logonArrived, "Logon Arrived");
            Assert.AreEqual(true, tickArrived, "Tick Arrived");
            Assert.AreEqual(true, logoutArrived, "Logout Arrived");
            Assert.AreEqual(0, _service.TickSubscriptions[TradeHubConstants.MarketDataProvider.Simulated].Count, "Subscribed securities");
        }

        [Test]
        [Category("Console")]
        public void TickUnsubscriptionTestCase_MultipleSecurities()
        {

            bool logonArrived = false;
            bool logoutArrived = false;
            bool tickArrived = false;

            ManualResetEvent manualLogonEvent = new ManualResetEvent(false);
            ManualResetEvent manualLogoutEvent = new ManualResetEvent(false);
            ManualResetEvent manualTickEvent = new ManualResetEvent(false);
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
                        _service.Subscribe(new Subscribe()
                        {
                            Security = new Security { Symbol = "AAPL" },
                            MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated
                        });

                        _service.Subscribe(new Subscribe()
                        {
                            Security = new Security { Symbol = "IBM" },
                            MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated
                        });
                        manualLogonEvent.Set();
                    };

            _service.TickArrived +=
                    delegate(Tick obj)
                    {
                        tickArrived = true;
                        _service.Unsubscribe(new Unsubscribe()
                        {
                            Security = new Security { Symbol = "AAPL" },
                            MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated
                        });
                        _service.Logout(new Logout { MarketDataProvider = TradeHubConstants.MarketDataProvider.Simulated });
                        manualTickEvent.Set();
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
            manualTickEvent.WaitOne(30000, false);
            manualLogoutEvent.WaitOne(30000, false);

            Assert.AreEqual(true, logonArrived, "Logon Arrived");
            Assert.AreEqual(true, tickArrived, "Tick Arrived");
            Assert.AreEqual(true, logoutArrived, "Logout Arrived");
            Assert.AreEqual(1, _service.TickSubscriptions[TradeHubConstants.MarketDataProvider.Simulated].Count, "Subscribed securities");
        }

    }
}
