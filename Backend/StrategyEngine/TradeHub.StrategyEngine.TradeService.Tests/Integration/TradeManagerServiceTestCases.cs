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
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.ValueObjects;
using TradeHub.TradeManager.CommunicationManager.Service;
using TradeHub.TradeManager.Server.Service;

namespace TradeHub.StrategyEngine.TradeService.Tests.Integration
{
    [TestFixture]
    public class TradeManagerServiceTestCases
    {
        private ApplicationController _applicationController;
        private TradeManagerMqServer _tradeManagerMqServer;

        private TradeManagerService _tradeManagerService;

        [SetUp]
        public void Setup()
        {
            _tradeManagerMqServer = new TradeManagerMqServer("TradeManagerMqConfig.xml");

            // Initialize Server Object
            _applicationController = new ApplicationController(_tradeManagerMqServer, new ExecutionHandler());

            // Start Server
            _applicationController.StartCommunicator();

            // Initialize Service
            _tradeManagerService = ContextRegistry.GetContext()["TradeManagerService"] as TradeManagerService;

            if (_tradeManagerService != null)
                _tradeManagerService.StartService();
        }

        [TearDown]
        public void TearDown()
        {
            _applicationController.StopCommunicator();
            _tradeManagerService.StopService();
        }

        [Test]
        [Category("Integration")]
        public void NewExecution_SendInformationToTradeManagerServer_ExecutionReceivedByServer()
        {
            Thread.Sleep(2000);

            bool executionReceived = false;
            var executionManualResetEvent = new ManualResetEvent(false);

            // Create Order Object
            Order order = new Order(OrderExecutionProvider.Simulated)
            {
                OrderID = "1",
                OrderSide = OrderSide.BUY,
                Security = new Security() { Symbol = "GOOG" }
            };

            // Create Fill Object
            Fill fill = new Fill(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated, "1");
            fill.ExecutionId = "1";
            fill.ExecutionSide = OrderSide.BUY;
            fill.ExecutionSize = 40;
            // Create Execution Object
            Execution execution = new Execution(fill, order);

            Execution executionReceivedObject = null;

            _tradeManagerMqServer.NewExecutionReceivedEvent += delegate(Execution executionObject)
            {
                executionReceivedObject = executionObject;
                executionReceived = true;
                executionManualResetEvent.Set();
            };

            // Send Execution 
            _tradeManagerService.SendExecution(execution);

            executionManualResetEvent.WaitOne(10000, false);

            Assert.AreEqual(true, executionReceived, "Execution Received");
            Assert.AreEqual(execution.Order.OrderExecutionProvider, executionReceivedObject.Order.OrderExecutionProvider, "Order Execution Provider");
            Assert.AreEqual(execution.Order.OrderID, executionReceivedObject.Order.OrderID, "Order ID");
            Assert.AreEqual(execution.Order.Security.Symbol, executionReceivedObject.Order.Security.Symbol, "Order Symbol");
            Assert.AreEqual(execution.Order.OrderSide, executionReceivedObject.Order.OrderSide, "Order Side");
            Assert.AreEqual(execution.Fill.Security.Symbol, executionReceivedObject.Fill.Security.Symbol, "Fill Symbol");
            Assert.AreEqual(execution.Fill.OrderExecutionProvider, executionReceivedObject.Fill.OrderExecutionProvider, "Fill Execution Provider");
            Assert.AreEqual(execution.Fill.ExecutionId, executionReceivedObject.Fill.ExecutionId, "Execution ID");
            Assert.AreEqual(execution.Fill.OrderId, executionReceivedObject.Fill.OrderId, "Order ID in Fill");
            Assert.AreEqual(execution.Fill.ExecutionSide, executionReceivedObject.Fill.ExecutionSide, "Execution Side");
            Assert.AreEqual(execution.Fill.ExecutionSize, executionReceivedObject.Fill.ExecutionSize, "Execution Size");
        }
    }
}
