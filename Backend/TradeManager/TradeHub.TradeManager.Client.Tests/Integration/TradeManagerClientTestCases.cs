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
using TradeHub.TradeManager.Client.Constants;
using TradeHub.TradeManager.Client.Service;
using TradeHub.TradeManager.Client.Utility;
using TradeHub.TradeManager.CommunicationManager.Service;
using TradeHub.TradeManager.Server.Service;

namespace TradeHub.TradeManager.Client.Tests.Integration
{
    [TestFixture]
    public class TradeManagerClientTestCases
    {
        private ApplicationController _applicationController;
        private TradeManagerMqServer _tradeManagerMqServer;

        private TradeManagerClient _tradeManagerClient;

        [SetUp]
        public void Setup()
        {
            _tradeManagerMqServer = new TradeManagerMqServer("TradeManagerMqConfig.xml");

            // Initialize Server Object
            _applicationController = new ApplicationController(_tradeManagerMqServer, new ExecutionHandler());

            // Start Server
            _applicationController.StartCommunicator();

            _tradeManagerClient = ContextRegistry.GetContext()["TradeManagerClient"] as TradeManagerClient;

            // Start Client
            if (_tradeManagerClient != null) 
                _tradeManagerClient.StartCommunicator();
        }

        [TearDown]
        public void TearDown()
        {
            _applicationController.StopCommunicator();
            _tradeManagerClient.StopCommunicator();
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
            fill.ExecutionDateTime = new DateTime(2014, 01, 22, 18, 20, 57);
            // Create Execution Object
            Execution execution = new Execution(fill, order);

            Execution executionReceivedObject = null;

            _tradeManagerMqServer.NewExecutionReceivedEvent += delegate(Execution executionObject)
            {
                executionReceivedObject = executionObject;
                executionReceived = true;
                executionManualResetEvent.Set();
            };

            byte[] message = Encoding.UTF8.GetBytes(execution.DataToPublish());

            var requestMessage = new MessageQueueObject();
            requestMessage.Message = message;

            // Publish Execution to MQ Exchange 
            _tradeManagerClient.SendExecution(requestMessage);

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

        [Test]
        [Category("Integration")]
        public void MultipleExecutions_SendInformationToTradeManagerServer_ExecutionReceivedByServerTradeCompleted()
        {
            Thread.Sleep(2000);

            int executionCount = 0;
            bool executionReceived = false;
            var executionManualResetEvent = new ManualResetEvent(false);

            Execution executionOne = null;
            Execution executionTwo = null;

            MessageQueueObject requestMessage;

            {
                // Create Order Object
                Order order = new Order(OrderExecutionProvider.Simulated)
                {
                    OrderID = "1",
                    OrderSide = OrderSide.BUY,
                    Security = new Security() {Symbol = "GOOG"}
                };

                // Create Fill Object
                Fill fill = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fill.ExecutionId = "1";
                fill.ExecutionSide = OrderSide.BUY;
                fill.ExecutionSize = 40;
                fill.ExecutionDateTime = new DateTime(2014, 01, 22, 18, 20, 57);
                // Create Execution Object
                executionOne = new Execution(fill, order);

                byte[] message = Encoding.UTF8.GetBytes(executionOne.DataToPublish());

                requestMessage = new MessageQueueObject();
                requestMessage.Message = message;
            }

            Execution executionReceivedObjectOne = null;
            Execution executionReceivedObjectTwo = null;

            _tradeManagerMqServer.NewExecutionReceivedEvent += delegate(Execution executionObject)
            {
                executionCount++;

                if (executionCount == 1)
                {
                    executionReceivedObjectOne = executionObject;

                    // Create Order Object
                    Order order = new Order(OrderExecutionProvider.Simulated)
                    {
                        OrderID = "2",
                        OrderSide = OrderSide.BUY,
                        Security = new Security() { Symbol = "GOOG" }
                    };

                    // Create Fill Object
                    Fill fill = new Fill(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated, "2");
                    fill.ExecutionId = "2";
                    fill.ExecutionSide = OrderSide.SELL;
                    fill.ExecutionSize = 40;
                    fill.ExecutionDateTime = new DateTime(2014, 01, 22, 18, 20, 59);
                    // Create Execution Object
                    executionTwo = new Execution(fill, order);

                    byte[] message = Encoding.UTF8.GetBytes(executionTwo.DataToPublish());

                    requestMessage = new MessageQueueObject();
                    requestMessage.Message = message;

                    // Publish Execution to MQ Exchange 
                    _tradeManagerClient.SendExecution(requestMessage);
                }

                if (executionCount == 2)
                {
                    executionReceived = true;
                    executionReceivedObjectTwo = executionObject;

                    //Give Repo time to save data
                    Thread.Sleep(2000);

                    executionManualResetEvent.Set();
                }
            };

            // Publish Execution to MQ Exchange 
            _tradeManagerClient.SendExecution(requestMessage);

            executionManualResetEvent.WaitOne(10000, false);

            Assert.AreEqual(true, executionReceived, "Execution Received");

            Assert.AreEqual(executionOne.Order.OrderExecutionProvider, executionReceivedObjectOne.Order.OrderExecutionProvider, "Order Execution Provider - ONE");
            Assert.AreEqual(executionOne.Order.OrderID, executionReceivedObjectOne.Order.OrderID, "Order ID - ONE");
            Assert.AreEqual(executionOne.Order.Security.Symbol, executionReceivedObjectOne.Order.Security.Symbol, "Order Symbol - ONE");
            Assert.AreEqual(executionOne.Order.OrderSide, executionReceivedObjectOne.Order.OrderSide, "Order Side - ONE");
            Assert.AreEqual(executionOne.Fill.Security.Symbol, executionReceivedObjectOne.Fill.Security.Symbol, "Fill Symbol - ONE");
            Assert.AreEqual(executionOne.Fill.OrderExecutionProvider, executionReceivedObjectOne.Fill.OrderExecutionProvider, "Fill Execution Provider - ONE");
            Assert.AreEqual(executionOne.Fill.ExecutionId, executionReceivedObjectOne.Fill.ExecutionId, "Execution ID - ONE");
            Assert.AreEqual(executionOne.Fill.OrderId, executionReceivedObjectOne.Fill.OrderId, "Order ID in Fill - ONE");
            Assert.AreEqual(executionOne.Fill.ExecutionSide, executionReceivedObjectOne.Fill.ExecutionSide, "Execution Side - ONE");
            Assert.AreEqual(executionOne.Fill.ExecutionSize, executionReceivedObjectOne.Fill.ExecutionSize, "Execution Size - ONE");

            Assert.AreEqual(executionTwo.Order.OrderExecutionProvider, executionReceivedObjectTwo.Order.OrderExecutionProvider, "Order Execution Provider - TWO");
            Assert.AreEqual(executionTwo.Order.OrderID, executionReceivedObjectTwo.Order.OrderID, "Order ID - TWO");
            Assert.AreEqual(executionTwo.Order.Security.Symbol, executionReceivedObjectTwo.Order.Security.Symbol, "Order Symbol - TWO");
            Assert.AreEqual(executionTwo.Order.OrderSide, executionReceivedObjectTwo.Order.OrderSide, "Order Side - TWO");
            Assert.AreEqual(executionTwo.Fill.Security.Symbol, executionReceivedObjectTwo.Fill.Security.Symbol, "Fill Symbol - TWO");
            Assert.AreEqual(executionTwo.Fill.OrderExecutionProvider, executionReceivedObjectTwo.Fill.OrderExecutionProvider, "Fill Execution Provider - TWO");
            Assert.AreEqual(executionTwo.Fill.ExecutionId, executionReceivedObjectTwo.Fill.ExecutionId, "Execution ID - TWO");
            Assert.AreEqual(executionTwo.Fill.OrderId, executionReceivedObjectTwo.Fill.OrderId, "Order ID in Fill - TWO");
            Assert.AreEqual(executionTwo.Fill.ExecutionSide, executionReceivedObjectTwo.Fill.ExecutionSide, "Execution Side - TWO");
            Assert.AreEqual(executionTwo.Fill.ExecutionSize, executionReceivedObjectTwo.Fill.ExecutionSize, "Execution Size - TWO");
        }
    }
}
