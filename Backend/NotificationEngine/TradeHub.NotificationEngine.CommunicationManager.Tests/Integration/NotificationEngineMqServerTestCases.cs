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
using EasyNetQ;
using EasyNetQ.Topology;
using NUnit.Framework;
using RabbitMQ.Client;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.NotificationEngine.Common.Constants;
using TradeHub.NotificationEngine.Common.ValueObject;
using TradeHub.NotificationEngine.CommunicationManager.Service;
using ExchangeType = EasyNetQ.Topology.ExchangeType;

namespace TradeHub.NotificationEngine.CommunicationManager.Tests.Integration
{
    [TestFixture]
    public class NotificationEngineMqServerTestCases
    {
        private NotificationEngineMqServer _notificationEngineMqServer;

        // MQ Fields
        private IAdvancedBus _advancedBus;
        private IExchange _adminExchange;

        [SetUp]
        public void SetUp()
        {
            _notificationEngineMqServer = new NotificationEngineMqServer("NotificationEngineMqConfig.xml");

            _notificationEngineMqServer.Connect();

            // Initialize Advance Bus
            _advancedBus = RabbitHutch.CreateBus("host=localhost").Advanced;

            // Create a admin exchange
            _adminExchange = _advancedBus.ExchangeDeclare("notificationengine_exchange", ExchangeType.Direct, false, true, false);
        }

        [TearDown]
        public void TearDown()
        {
            _advancedBus.Dispose();
            _notificationEngineMqServer.Disconnect();
        }

        [Test]
        [Category("Integration")]
        public void NewNotification_SendNotificationToServer_NotificationReceivedByServer()
        {
            Thread.Sleep(5000);

            bool notificationReceived = false;
            var notificationManualResetEvent = new ManualResetEvent(false);

            // Create Order Object
            OrderNotification orderNotification = new OrderNotification(NotificationType.Email, OrderNotificationType.Accepted);

            _notificationEngineMqServer.OrderNotificationEvent += delegate(OrderNotification notificationObject)
            {
                notificationReceived = true;
                notificationManualResetEvent.Set();
            };

            IMessage<OrderNotification> message = new Message<OrderNotification>(orderNotification);
            _advancedBus.Publish(_adminExchange, "notificationengine.order.message", true, false, message);

            notificationManualResetEvent.WaitOne(10000, false);

            Assert.AreEqual(true, notificationReceived, "Notification Received");
        }
    }
}
