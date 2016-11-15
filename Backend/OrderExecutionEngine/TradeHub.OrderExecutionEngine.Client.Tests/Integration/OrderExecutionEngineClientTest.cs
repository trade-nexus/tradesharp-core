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


﻿using System.Text;
using System.Threading;
using NUnit.Framework;
using Spring.Context.Support;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.ValueObjects;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.OrderExecutionEngine.Client.Service;
using TradeHub.OrderExecutionEngine.Server.Service;
using TradeHubConstants = TradeHub.Common.Core.Constants;

namespace TradeHub.OrderExecutionEngine.Client.Tests.Integration
{
    [TestFixture]
    public class OrderExecutionEngineClientTest
    {
        private OrderExecutionEngineClient _executionEngineClient;
        private ApplicationController _applicationController;
         
        [SetUp]
        public void Setup()
        {
            _applicationController = ContextRegistry.GetContext()["ApplicationController"] as ApplicationController;
            if (_applicationController != null) _applicationController.StartServer();

            Thread.Sleep(5000);

            _executionEngineClient = new OrderExecutionEngineClient();
        }

        [TearDown]
        public void Close()
        {
            _executionEngineClient.Shutdown();
            _applicationController.StopServer();
        }

        [Test]
        [Category("Integration")]
        public void AppIDTestCase()
        {
            Thread.Sleep(2000);
            _executionEngineClient.Start();
            ManualResetEvent manualAppIDEvent = new ManualResetEvent(false); ;

            manualAppIDEvent.WaitOne(3000, false);

            Assert.NotNull(true, _executionEngineClient.AppId, "App ID");
            Assert.AreEqual("A00", _executionEngineClient.AppId, "App ID Value");
        }

        [Test]
        [Category("Integration")]
        public void ConnectivityTestCase()
        {
            
            bool logonArrived = false;
            bool logoutArrived = false;
            bool connected = false;
            ManualResetEvent manualLogonEvent = new ManualResetEvent(false); 
            ManualResetEvent manualLogoutEvent = new ManualResetEvent(false);
            ManualResetEvent manualConnectedEvent = new ManualResetEvent(false); 

            _executionEngineClient.ServerConnected += delegate()
                {
                    connected = true;
                    _executionEngineClient.SendLoginRequest(new Login() { OrderExecutionProvider = TradeHubConstants.OrderExecutionProvider.Simulated });
                    manualConnectedEvent.Set();
                };

            _executionEngineClient.LogonArrived +=
                    delegate(string obj)
                    {
                        logonArrived = true;

                        _executionEngineClient.SendLogoutRequest(new Logout
                            {OrderExecutionProvider = TradeHubConstants.OrderExecutionProvider.Simulated});
                        manualLogonEvent.Set();
                    };
            _executionEngineClient.LogoutArrived +=
                    delegate(string obj)
                    {
                        logoutArrived = true;
                        manualLogoutEvent.Set();
                    };

            _executionEngineClient.Start();

            manualConnectedEvent.WaitOne(30000, false);
            manualLogonEvent.WaitOne(30000, false);
            manualLogoutEvent.WaitOne(30000, false);

            //Thread.Sleep(70000);

            Assert.AreEqual(true, logonArrived, "Logon Arrived");
            Assert.AreEqual(true, logoutArrived, "Logout Arrived");
        }

        [Test]
        [Category("Console")]
        public void MarketOrderTestCase()
        {
            MarketOrder marketOrder = new MarketOrder(TradeHubConstants.OrderExecutionProvider.Simulated)
            {
                OrderID = "A00",
                Security = new Security { Symbol = "AAPL" },
                OrderSide = TradeHubConstants.OrderSide.SELL,
                OrderSize = 100
            };
            Execution execution = null;

            bool logonArrived = false;
            bool logoutArrived = false;
            bool connected = false;
            bool newArrived = false;
            bool executionArrived = false;

            ManualResetEvent manualLogonEvent = new ManualResetEvent(false);
            ManualResetEvent manualLogoutEvent = new ManualResetEvent(false);
            ManualResetEvent manualConnectedEvent = new ManualResetEvent(false);
            ManualResetEvent manualNewEvent = new ManualResetEvent(false);
            ManualResetEvent manualExecutionEvent = new ManualResetEvent(false);

            _executionEngineClient.ServerConnected += delegate()
            {
                connected = true;
                _executionEngineClient.SendLoginRequest(new Login() { OrderExecutionProvider = TradeHubConstants.OrderExecutionProvider.Simulated });
                manualConnectedEvent.Set();
            };

            _executionEngineClient.LogonArrived +=
                    delegate(string obj)
                    {
                        logonArrived = true;

                        //_executionEngineClient.SendMarketOrderRequest(marketOrder);
                        RabbitMqRequestMessage entryMessage = new RabbitMqRequestMessage();
                        var messageBytes = Encoding.UTF8.GetBytes(marketOrder.DataToPublish());

                        // Initialize property
                        entryMessage.Message = new byte[messageBytes.Length];
                        // Update object values
                        messageBytes.CopyTo(entryMessage.Message, 0);

                        _executionEngineClient.SendOrderRequests(entryMessage);

                        manualLogonEvent.Set();
                    };

            _executionEngineClient.NewArrived +=
                    delegate(Order obj)
                    {
                        newArrived = true;
                        manualNewEvent.Set();
                    };

            _executionEngineClient.ExecutionArrived +=
                    delegate(Execution obj)
                    {
                        if (obj.Fill.LeavesQuantity.Equals(0))
                        {
                            executionArrived = true;
                            execution = obj;
                            _executionEngineClient.SendLogoutRequest(new Logout { OrderExecutionProvider = TradeHubConstants.OrderExecutionProvider.Simulated });
                            manualExecutionEvent.Set();
                        }
                    };

            _executionEngineClient.LogoutArrived +=
                    delegate(string obj)
                    {
                        logoutArrived = true;
                        manualLogoutEvent.Set();
                    };

            _executionEngineClient.Start();

            manualConnectedEvent.WaitOne(30000, false);
            manualLogonEvent.WaitOne(30000, false);
            manualNewEvent.WaitOne(30000, false);
            manualExecutionEvent.WaitOne(30000, false);
            manualLogoutEvent.WaitOne(30000, false);

            Thread.Sleep(1000);

            Assert.AreEqual(true, logonArrived, "Logon Arrived");
            Assert.AreEqual(true, newArrived, "New Arrived");
            Assert.AreEqual(true, executionArrived, "Execution Arrived");
            Assert.AreEqual(true, execution.Fill.ExecutionSide.Equals(TradeHubConstants.OrderSide.SELL), "Execution Side");
            Assert.AreEqual(true, logoutArrived, "Logout Arrived");
        }

        [Test]
        [Category("Console")]
        public void LocateMessageTestCase()
        {
            bool logonArrived = false;
            bool logoutArrived = false;
            bool connected = false;
            bool locateArrived = false;

            ManualResetEvent manualLogonEvent = new ManualResetEvent(false);
            ManualResetEvent manualLogoutEvent = new ManualResetEvent(false);
            ManualResetEvent manualConnectedEvent = new ManualResetEvent(false);
            ManualResetEvent manualLocateEvent = new ManualResetEvent(false);

            _executionEngineClient.ServerConnected += delegate()
            {
                connected = true;
                _executionEngineClient.SendLoginRequest(new Login() { OrderExecutionProvider = TradeHubConstants.OrderExecutionProvider.Simulated });
                manualConnectedEvent.Set();
            };

            _executionEngineClient.LogonArrived +=
                    delegate(string obj)
                    {
                        logonArrived = true;
                        manualLogonEvent.Set();
                    };

            _executionEngineClient.LocateMessageArrived +=
                    delegate(LimitOrder obj)
                    {
                        locateArrived = true;

                        LocateResponse locateResponse = new LocateResponse(obj.OrderID, TradeHubConstants.OrderExecutionProvider.Simulated, true);

                        _executionEngineClient.SendLocateResponse(locateResponse);
                        _executionEngineClient.SendLogoutRequest(new Logout { OrderExecutionProvider = TradeHubConstants.OrderExecutionProvider.Simulated });
                        manualLocateEvent.Set();
                    };

            _executionEngineClient.LogoutArrived +=
                    delegate(string obj)
                    {
                        logoutArrived = true;
                        manualLogoutEvent.Set();
                    };

            _executionEngineClient.Start();

            manualConnectedEvent.WaitOne(30000, false);
            manualLogonEvent.WaitOne(30000, false);
            manualLocateEvent.WaitOne(30000, false);
            manualLogoutEvent.WaitOne(30000, false);

            Thread.Sleep(1000);

            Assert.AreEqual(true, logonArrived, "Logon Arrived");
            Assert.AreEqual(true, locateArrived, "Locate Arrived");
            Assert.AreEqual(true, logoutArrived, "Logout Arrived");
        }
    }
}
