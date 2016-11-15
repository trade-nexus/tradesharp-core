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


﻿using NUnit.Framework;
using TraceSourceLogger;
using TradeHub.OrderExecutionEngine.Client.Utility;

namespace TradeHub.OrderExecutionEngine.Client.Tests.Integration
{
    [TestFixture]
    public class ConfigurationReaderTest
    {
        private ConfigurationReader _configurationReader;

        [SetUp]
        public void SetUp()
        {
            _configurationReader = new ConfigurationReader("OEEServer.xml","OEEClientMqConfig.xml", new AsyncClassLogger("ConfigurationReaderTest"));
        }

        [Test]
        [Category("Integration")]
        public void ReadOeeMQConfigSettings()
        {
            var oeeMqParameters = _configurationReader.OeeMqServerparameters;

            Assert.AreEqual("host=localhost", oeeMqParameters["ConnectionString"], "ConnectionString");
            Assert.AreEqual("orderexecution_exchange", oeeMqParameters["Exchange"], "Exchange");
            Assert.AreEqual("orderexecution.engine.limitorder", oeeMqParameters["LimitOrderRoutingKey"], "LimitOrderRoutingKey");
            Assert.AreEqual("orderexecution.engine.marketorder", oeeMqParameters["MarketOrderRoutingKey"], "MarketOrderRoutingKey");
            Assert.AreEqual("orderexecution.engine.stoporder", oeeMqParameters["StopOrderRoutingKey"], "StopOrderRoutingKey");
            Assert.AreEqual("orderexecution.engine.stoplimitorder", oeeMqParameters["StopLimitOrderRoutingKey"], "StopLimitOrderRoutingKey");
            Assert.AreEqual("orderexecution.engine.login", oeeMqParameters["LoginRoutingKey"], "LoginRoutingKey");
            Assert.AreEqual("orderexecution.engine.logout", oeeMqParameters["LogoutRoutingKey"], "LogoutRoutingKey");
            Assert.AreEqual("orderexecution.engine.locateresponse", oeeMqParameters["LocateResponseRoutingKey"], "LogoutRoutingKey");
        }

        [Test]
        [Category("Integration")]
        public void ReadClientMQConfigSettings()
        {
            var clientMqParameters = _configurationReader.ClientMqParameters;

            Assert.AreEqual("host=localhost", clientMqParameters["ConnectionString"], "ConnectionString");
            Assert.AreEqual("orderexecution_exchange", clientMqParameters["Exchange"], "Exchange");

            Assert.AreEqual("orderexecution_client_admin_queue", clientMqParameters["AdminMessageQueue"], "AdminMessageQueue");
            Assert.AreEqual("orderexecution.client.admin", clientMqParameters["AdminMessageRoutingKey"], "AdminMessageRoutingKey");


            Assert.AreEqual("orderexecution_client_order_queue", clientMqParameters["OrderMessageQueue"], "OrderMessageQueue");
            Assert.AreEqual("orderexecution.client.order", clientMqParameters["OrderMessageRoutingKey"], "OrderMessageRoutingKey");

            Assert.AreEqual("orderexecution_client_execution_queue", clientMqParameters["ExecutionMessageQueue"], "ExecutionMessageQueue");
            Assert.AreEqual("orderexecution.client.execution", clientMqParameters["ExecutionMessageRoutingKey"], "ExecutionMessageRoutingKey");

            Assert.AreEqual("orderexecution_client_rejection_queue", clientMqParameters["RejectionMessageQueue"], "RejectionMessageQueue");
            Assert.AreEqual("orderexecution.client.rejection", clientMqParameters["RejectionMessageRoutingKey"], "RejectionMessageRoutingKey");

            Assert.AreEqual("orderexecution_client_locatemessage_queue", clientMqParameters["LocateMessageQueue"], "LocateMessageQueue");
            Assert.AreEqual("orderexecution.locatemessage.order", clientMqParameters["LocateMessageRoutingKey"], "LocateMessageRoutingKey");

            Assert.NotNull(clientMqParameters["InquiryResponseQueue"], "InquiryResponseQueue");
            Assert.NotNull(clientMqParameters["InquiryResponseRoutingKey"], "InquiryResponseRoutingKey");
        }
    }
}
