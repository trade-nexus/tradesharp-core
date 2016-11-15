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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Spring.Context.Support;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.ValueObjects;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.Inquiry;
using TradeHub.OrderExecutionEngine.Configuration.Service;
using ExchangeType = EasyNetQ.Topology.ExchangeType;

namespace TradeHub.OrderExecutionEngine.ConfigurationTests.Integration
{
    [TestFixture]
    public class MQServerTestCases
    {
        private OrderExecutionMqServer _executionMqServer;
        private IAdvancedBus _advancedBus;
        private IExchange _adminExchange;
        private IQueue _applicationAdminQueue;
        // Native Rabbit MQ Fields
        private ConnectionFactory _rabbitMqOrderBus;
        private IConnection _rabbitMqOrderConnection;
        private IModel _rabbitMqOrderChannel;
        private QueueingBasicConsumer _orderRequestConsumer;

        [SetUp]
        public void SetUp()
        {
            _executionMqServer = ContextRegistry.GetContext()["OrderExecutionMqServer"] as OrderExecutionMqServer;
            if (_executionMqServer != null) _executionMqServer.Connect();

            //// Initialize Advance Bus
            //_advancedBus = RabbitHutch.CreateBus("host=localhost").Advanced;

            //// Create a admin exchange
            //_adminExchange = _advancedBus.ExchangeDeclare("orderexecution_exchange", ExchangeType.Direct, false, true, false);

            // Create Native Rabbit MQ Bus
            _rabbitMqOrderBus = new ConnectionFactory { HostName = "localhost" };

            // Create Native Rabbit MQ Connection
            _rabbitMqOrderConnection = _rabbitMqOrderBus.CreateConnection();

            // Open Native Rabbbit MQ Channel
            _rabbitMqOrderChannel = _rabbitMqOrderConnection.CreateModel();
        }

        [TearDown]
        public void Close()
        {
            _executionMqServer.Disconnect();
        }

        [Test]
        [Category("Integration")]
        public void LoginMessageTestCase()
        {
            bool loginReceived = false;
            var loginEvent = new ManualResetEvent(false);

            _executionMqServer.LogonRequestRecieved += delegate(IMessage<Login> obj)
                {
                    loginReceived = true;
                    loginEvent.Set();
                };

            //using (var channel = _advancedBus.OpenPublishChannel())
            {
                IMessage<Login> message = new Message<Login>(new Login());
                _advancedBus.Publish(_adminExchange, "orderexecution.engine.login", true , true, message);
            }

            loginEvent.WaitOne(10000, false);
            Assert.AreEqual(true, loginReceived, "Login Received");
        }

        [Test]
        [Category("Integration")]
        public void LogoutMessageTestCase()
        {
            bool logoutReceived = false;
            var logoutEvent = new ManualResetEvent(false);

            _executionMqServer.LogoutRequestRecieved += delegate(IMessage<Logout> obj)
            {
                logoutReceived = true;
                logoutEvent.Set();
            };

            //using (var channel = _advancedBus.OpenPublishChannel())
            {
                IMessage<Logout> message = new Message<Logout>(new Logout());
                _advancedBus.Publish(_adminExchange, "orderexecution.engine.logout", true, false, message);
            }

            logoutEvent.WaitOne(10000, false);
            Assert.AreEqual(true, logoutReceived, "Logout Received");
        }

        [Test]
        [Category("Integration")]
        public void InquiryMessageTestCase()
        {
            bool inquiryReceived = false;
            var inquiryEvent = new ManualResetEvent(false);

            _executionMqServer.InquiryRequestReceived += delegate(IMessage<InquiryMessage> obj)
            {
                inquiryReceived = true;
                inquiryEvent.Set();
            };

            //using (var channel = _advancedBus.OpenPublishChannel())
            {
                IMessage<InquiryMessage> message = new Message<InquiryMessage>(new InquiryMessage());
                _advancedBus.Publish(_adminExchange, "orderexecution.engine.inquiry", true, false, message);
            }

            inquiryEvent.WaitOne(10000, false);
            Assert.AreEqual(true, inquiryReceived, "Inquiry Received");
        }

        [Test]
        [Category("Integration")]
        public void AppInfoMessageTestCase()
        {
            bool appInfoReceived = false;
            var appInfoEvent = new ManualResetEvent(false);

            _executionMqServer.AppInfoReceived += delegate(IMessage<Dictionary<string, string>> obj)
            {
                appInfoReceived = true;
                appInfoEvent.Set();
            };

            //using (var channel = _advancedBus.OpenPublishChannel())
            {
                IMessage<Dictionary<string, string>> message = new Message<Dictionary<string, string>>(new Dictionary<string, string>());
                _advancedBus.Publish(_adminExchange, "orderexecution.engine.appinfo", true, false, message);
            }

            appInfoEvent.WaitOne(10000, false);
            Assert.AreEqual(true, appInfoReceived, "App Info Received");
        }

        [Test]
        [Category("Integration")]
        public void LimitOrderMessageTestCase()
        {
            Thread.Sleep(1000);
            bool limitOrderReceived = false;
            var limitOrderEvent = new ManualResetEvent(false);

            _executionMqServer.LimitOrderRequestRecieved += delegate(LimitOrder obj, string arg2)
            {
                limitOrderReceived = true;
                limitOrderEvent.Set();
                Logger.Info("Limit Order Request receievd: " + obj, "MQServerTestCases", "ConsumeOrderRequestQueue");
            };

            LimitOrder limitOrder = new LimitOrder("01", "BUY", 10, "GTC", "EUR", new Security() { Symbol = "TEST" }, "TestCase");
            limitOrder.LimitPrice = 100.0M;

            byte[] message = Encoding.UTF8.GetBytes("A00," + limitOrder.DataToPublish());

            string corrId = Guid.NewGuid().ToString();
            IBasicProperties replyProps = _rabbitMqOrderChannel.CreateBasicProperties();
            replyProps.CorrelationId = corrId;

            // Publish Order Reqeusts to MQ Exchange 
            _rabbitMqOrderChannel.BasicPublish("orderexecution_exchange", "orderexecution.engine.orderrequest", replyProps, message);

            //IMessage<LimitOrder> message = new Message<LimitOrder>(new LimitOrder(""));
            //_advancedBus.Publish(_adminExchange, "orderexecution.engine.limitorder", true, false, message);

            limitOrderEvent.WaitOne(10000, false);
            Assert.AreEqual(true, limitOrderReceived, "Limit Order Received");
        }

        [Test]
        [Category("Integration")]
        public void MarketOrderMessageTestCase()
        {
            Thread.Sleep(1000);
            bool marketOrderReceived = false;
            var marketOrderEvent = new ManualResetEvent(false);

            _executionMqServer.MarketOrderRequestRecieved += delegate(MarketOrder obj, string arg2)
            {
                marketOrderReceived = true;
                marketOrderEvent.Set();
                Logger.Info("Market Order Request receievd: " + obj, "MQServerTestCases", "ConsumeOrderRequestQueue");
            };

            MarketOrder marketOrder = new MarketOrder("01", "BUY", 10, "GTC", "EUR", new Security() {Symbol = "TEST"}, "TestCase");
            byte[] message = Encoding.UTF8.GetBytes("A00," + marketOrder.DataToPublish());

            string corrId = Guid.NewGuid().ToString();
            IBasicProperties replyProps = _rabbitMqOrderChannel.CreateBasicProperties();
            replyProps.CorrelationId = corrId;

            // Publish Order Reqeusts to MQ Exchange 
            _rabbitMqOrderChannel.BasicPublish("orderexecution_exchange", "orderexecution.engine.orderrequest", replyProps, message);

            //IMessage<MarketOrder> message = new Message<MarketOrder>(new MarketOrder(""));
            //_advancedBus.Publish(_adminExchange, "orderexecution.engine.marketorder", true, false, message);

            marketOrderEvent.WaitOne(50000, false);
            Assert.AreEqual(true, marketOrderReceived, "Market Order Received");
        }

        [Test]
        [Category("Integration")]
        public void StopLimitOrderMessageTestCase()
        {
            bool stopLimitOrderReceived = false;
            var stopLimitOrderEvent = new ManualResetEvent(false);

            _executionMqServer.StopLimitOrderRequestRecieved += delegate(IMessage<StopLimitOrder> obj)
            {
                stopLimitOrderReceived = true;
                stopLimitOrderEvent.Set();
            };

            //using (var channel = _advancedBus.OpenPublishChannel())
            {
                IMessage<StopLimitOrder> message = new Message<StopLimitOrder>(new StopLimitOrder(""));
                _advancedBus.Publish(_adminExchange, "orderexecution.engine.stoplimitorder", true, false, message);
            }

            stopLimitOrderEvent.WaitOne(10000, false);
            Assert.AreEqual(true, stopLimitOrderReceived, "Stop Limit Order Received");
        }

        [Test]
        [Category("Integration")]
        public void StopOrderMessageTestCase()
        {
            bool stopOrderReceived = false;
            var stopOrderEvent = new ManualResetEvent(false);

            _executionMqServer.StopOrderRequestRecieved += delegate(IMessage<StopOrder> obj)
            {
                stopOrderReceived = true;
                stopOrderEvent.Set();
            };

            //using (var channel = _advancedBus.OpenPublishChannel())
            {
                IMessage<StopOrder> message = new Message<StopOrder>(new StopOrder(""));
                _advancedBus.Publish(_adminExchange, "orderexecution.engine.stoporder", true, false, message);
            }

            stopOrderEvent.WaitOne(10000, false);
            Assert.AreEqual(true, stopOrderReceived, "Stop Order Received");
        }

        [Test]
        [Category("Integration")]
        public void CancelOrderMessageTestCase()
        {
            Thread.Sleep(1000);
            bool cancelOrderReceived = false;
            var cancelOrderEvent = new ManualResetEvent(false);

            _executionMqServer.CancelOrderRequestRecieved += delegate(Order obj, string arg2)
            {
                cancelOrderReceived = true;
                cancelOrderEvent.Set();
                Logger.Info("Cancel Order Request receievd: " + obj, "MQServerTestCases", "ConsumeOrderRequestQueue");
            };

            Order marketOrder = new Order("01", "BUY", 10, "GTC", "EUR", new Security() { Symbol = "TEST" }, "TestCase");
            byte[] message = Encoding.UTF8.GetBytes("A00," + marketOrder.DataToPublish("Cancel"));

            string corrId = Guid.NewGuid().ToString();
            IBasicProperties replyProps = _rabbitMqOrderChannel.CreateBasicProperties();
            replyProps.CorrelationId = corrId;

            // Publish Order Reqeusts to MQ Exchange 
            _rabbitMqOrderChannel.BasicPublish("orderexecution_exchange", "orderexecution.engine.orderrequest", replyProps, message);

            //IMessage<MarketOrder> message = new Message<MarketOrder>(new MarketOrder(""));
            //_advancedBus.Publish(_adminExchange, "orderexecution.engine.marketorder", true, false, message);

            cancelOrderEvent.WaitOne(50000, false);
            Assert.AreEqual(true, cancelOrderReceived, "Cancel Order Received");
        }
    }
}
