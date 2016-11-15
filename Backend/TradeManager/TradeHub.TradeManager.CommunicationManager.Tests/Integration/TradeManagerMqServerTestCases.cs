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
using RabbitMQ.Client;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.TradeManager.CommunicationManager.Service;

namespace TradeHub.TradeManager.CommunicationManager.Tests.Integration
{
    [TestFixture]
    public class TradeManagerMqServerTestCases
    {
        private TradeManagerMqServer _tradeManagerMqServer;

        // Native Rabbit MQ Fields
        private ConnectionFactory _rabbitMqBus;
        private IConnection _rabbitMqConnection;
        private IModel _rabbitMqChannel;

        [SetUp]
        public void SetUp()
        {
            _tradeManagerMqServer = new TradeManagerMqServer("TradeManagerMqConfig.xml");

            _tradeManagerMqServer.Connect();

            // Create Native Rabbit MQ Bus
            _rabbitMqBus = new ConnectionFactory { HostName = "localhost" };

            // Create Native Rabbit MQ Connection
            _rabbitMqConnection = _rabbitMqBus.CreateConnection();

            // Open Native Rabbbit MQ Channel
            _rabbitMqChannel = _rabbitMqConnection.CreateModel();
        }

        [TearDown]
        public void TearDown()
        {
            _rabbitMqChannel.Close();
            _rabbitMqConnection.Close();

            _tradeManagerMqServer.Disconnect();
        }

        [Test]
        [Category("Integration")]
        public void NewExecution_SendExecutionToServer_ExecutionReceivedByServer()
        {
            Thread.Sleep(5000);

            bool executionReceived = false;
            var executionManualResetEvent = new ManualResetEvent(false);

            // Create Order Object
            Order order = new Order(OrderExecutionProvider.Simulated)
            {
                Security = new Security() { Symbol = "GOOG" }
            };

            // Create Fill Object
            Fill fill = new Fill(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated, "1");
            fill.ExecutionId = "1";
            fill.ExecutionSide = OrderSide.BUY;
            fill.ExecutionSize = 40;
            // Create Execution Object
            Execution execution = new Execution(fill, order);

            _tradeManagerMqServer.NewExecutionReceivedEvent += delegate(Execution executionObject)
            {
                executionReceived = true;
                executionManualResetEvent.Set();
            };

            byte[] message = Encoding.UTF8.GetBytes(execution.DataToPublish());

            string corrId = Guid.NewGuid().ToString();
            IBasicProperties replyProps = _rabbitMqChannel.CreateBasicProperties();
            replyProps.CorrelationId = corrId;

            // Publish Execution to MQ Exchange 
            _rabbitMqChannel.BasicPublish("trademanager_exchange", "trademanager.execution.message", replyProps, message);

            executionManualResetEvent.WaitOne(10000, false);

            Assert.AreEqual(true, executionReceived, "Execution Received");
        }
    }
}
