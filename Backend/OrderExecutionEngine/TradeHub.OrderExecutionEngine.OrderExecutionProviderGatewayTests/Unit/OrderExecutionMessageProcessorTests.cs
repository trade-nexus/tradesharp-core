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
using System.Threading.Tasks;
using NUnit.Framework;
using Spring.Context.Support;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.OrderExecutionEngine.OrderExecutionProviderGateway.Service;
using Constants = TradeHub.Common.Core.Constants;

namespace TradeHub.OrderExecutionEngine.OrderExecutionProviderGatewayTests.Unit
{
    [TestFixture]
    public class OrderExecutionMessageProcessorTests
    {
        private OrderExecutionMessageProcessor _messageProcessor;

        [SetUp]
        public void SetUp()
        {
            _messageProcessor = new OrderExecutionMessageProcessor();
        }

        [TearDown]
        public void Close()
        {
            _messageProcessor.StopProcessing();
        }


        [Test]
        [Category("Unit")]
        public void DataProviderLoginTestCase_SingleLogin()
        {
            _messageProcessor.OnLogonMessageRecieved(
                new Login { OrderExecutionProvider = Constants.MarketDataProvider.Blackwood }, "AppID");

            Assert.AreEqual(1, _messageProcessor.ProvidersLoginRequestMap[Constants.OrderExecutionProvider.Blackwood].Count, "Number of Login Requests recieved");
            Assert.AreEqual(1, _messageProcessor.ProvidersMap.Count, "Number of data provider instances loaded");
        }

        [Test]
        [Category("Unit")]
        public void DataProviderLoginTestCase_MultipleLogin()
        {
            _messageProcessor.OnLogonMessageRecieved(new Login() { OrderExecutionProvider = Constants.MarketDataProvider.Blackwood }, "AppID-One");
            _messageProcessor.OnLogonMessageRecieved(new Login() { OrderExecutionProvider = Constants.MarketDataProvider.Blackwood }, "AppID-Two");

            Assert.AreEqual(2, _messageProcessor.ProvidersLoginRequestMap[Constants.OrderExecutionProvider.Blackwood].Count, "Number of Login Requests recieved");
            Assert.AreEqual(1, _messageProcessor.ProvidersMap.Count, "Number of data provider instances loaded");
        }

        [Test]
        [Category("Unit")]
        public void DataProviderLogoutTestCase_SingleProivderLoggedIn()
        {
            _messageProcessor.OnLogonMessageRecieved(new Login() { OrderExecutionProvider = Constants.OrderExecutionProvider.Blackwood }, "AppID");

            Assert.AreEqual(1, _messageProcessor.ProvidersLoginRequestMap[Constants.OrderExecutionProvider.Blackwood].Count, "Number of Login Requests recieved");
            Assert.AreEqual(1, _messageProcessor.ProvidersMap.Count, "Number of data provider instances loaded");

            _messageProcessor.OnLogoutMessageRecieved(new Logout() { OrderExecutionProvider = Constants.OrderExecutionProvider.Blackwood }, "AppID");

            Assert.AreEqual(0, _messageProcessor.ProvidersLoginRequestMap.Count, "Number of Login Requests recieved");
            Assert.AreEqual(0, _messageProcessor.ProvidersMap.Count, "Number of data provider instances loaded");
        }

        [Test]
        [Category("Unit")]
        public void DataProviderLogoutTestCase_MultipleProivderLoggedIn()
        {
            _messageProcessor.OnLogonMessageRecieved(new Login() { OrderExecutionProvider = Constants.OrderExecutionProvider.Blackwood }, "AppID-One");
            _messageProcessor.OnLogonMessageRecieved(new Login() { OrderExecutionProvider = Constants.OrderExecutionProvider.Blackwood }, "AppID-Two");
            Assert.AreEqual(2, _messageProcessor.ProvidersLoginRequestMap[Constants.OrderExecutionProvider.Blackwood].Count, "Number of Login Requests recieved");
            Assert.AreEqual(1, _messageProcessor.ProvidersMap.Count, "Number of data provider instances loaded");

            _messageProcessor.OnLogoutMessageRecieved(new Logout() { OrderExecutionProvider = Constants.OrderExecutionProvider.Blackwood }, "AppID-One");

            Assert.AreEqual(1, _messageProcessor.ProvidersLoginRequestMap[Constants.OrderExecutionProvider.Blackwood].Count, "Number of Login Requests recieved");
            Assert.AreEqual(1, _messageProcessor.ProvidersMap.Count, "Number of data provider instances loaded");
        }

    }
}
