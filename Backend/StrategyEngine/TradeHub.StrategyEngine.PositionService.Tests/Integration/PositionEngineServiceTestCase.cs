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
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.PositionEngine.Client.Service;
using TradeHub.PositionEngine.Configuration.Service;
using TradeHub.PositionEngine.ProviderGateway.Service;
using TradeHub.PositionEngine.Service;
using TradeHub.StrategyEngine.OrderExecution;
using OeApplicationController = TradeHub.OrderExecutionEngine.Server.Service.ApplicationController;

namespace TradeHub.StrategyEngine.PositionService.Tests.Integration
{
    [TestFixture]
    public class PositionEngineServiceTestCase
    {
        private ApplicationController _applicationController;
        private OeApplicationController _oEapplicationController;
        private PositionEngineService _positionEngineService;
        private OrderExecutionService _orderExecutionService;

        [SetUp]
        public void Setup()
        {
            // Initialize and Start Order Execution Engine Server
            _oEapplicationController = ContextRegistry.GetContext()["ApplicationController"] as OeApplicationController;
            if (_oEapplicationController != null) _oEapplicationController.StartServer();
            
            Thread.Sleep(1000);
            
            // Initialize and Start Position Engine Server
            _applicationController = new ApplicationController(new PositionEngineMqServer("PEMQConfig.xml"), new PositionMessageProcessor());
            if (_applicationController != null) _applicationController.StartServer();

            // Initialize Position Engine Service
            _positionEngineService = ContextRegistry.GetContext()["PositionEngineService"] as PositionEngineService;

            // Initialize Order Execution Service
            _orderExecutionService = ContextRegistry.GetContext()["OrderExecutionService"] as OrderExecutionService;
        }

        [TearDown]
        public void Close()
        {
            // Stop Position Engine Service
            _positionEngineService.StopService();

            // Stop Order Execution Service
            _orderExecutionService.StopService();

            // Stop Position Engine Server
            _applicationController.StopServer();

            // Stop Order Execution Engine Server
            _oEapplicationController.StopServer();
        }

        [Test]
        [Category("Integration")]
        public void ConnectionTestCase()
        {
            Thread.Sleep(5000);
            _positionEngineService.StartService();

            var connected = false;
            var manualConnectedEvent = new ManualResetEvent(false);

            _positionEngineService.Connected += delegate()
            {
                connected = true;
                manualConnectedEvent.Set();
            };

            manualConnectedEvent.WaitOne(3000, false);

            Assert.IsTrue(connected, "Connected");
        }

        [Test]
        [Category("Integration")]
        public void PositionTestCase()
        {
            ConnectSimulatedOrderExexcutionProvider();
         
            Thread.Sleep(5000);
            _positionEngineService.StartService();

            var connected = false;
            var positionArrived = false;

            var manualConnectedEvent = new ManualResetEvent(false);
            var manualPositionEvent = new ManualResetEvent(false);

            _positionEngineService.Connected += delegate()
            {
                connected = true;
                _positionEngineService.Subscribe(OrderExecutionProvider.Simulated);
                manualConnectedEvent.Set();
            };

            _positionEngineService.PositionArrived += delegate
            {
                positionArrived = true;
                manualPositionEvent.Set();
            };

            manualConnectedEvent.WaitOne(3000, false);
            manualPositionEvent.WaitOne(10000, false);

            Assert.IsTrue(connected, "Connected");
            Assert.IsTrue(positionArrived, "Position Arrived");
        }

        public void ConnectSimulatedOrderExexcutionProvider()
        {
            var logonArrived = false;

            ManualResetEvent manualLogonEvent = new ManualResetEvent(false);
            ManualResetEvent manualConnectedEvent = new ManualResetEvent(false);

            _orderExecutionService.Connected += delegate()
            {
                _orderExecutionService.Login(new Login() { OrderExecutionProvider = OrderExecutionProvider.Simulated });
                manualConnectedEvent.Set();
            };

            _orderExecutionService.LogonArrived +=
                delegate(string obj)
                {
                    logonArrived = true;
                    manualLogonEvent.Set();
                };

            _orderExecutionService.StartService();

            manualConnectedEvent.WaitOne(30000, false);
            manualLogonEvent.WaitOne(30000, false);
        }
    }
}
