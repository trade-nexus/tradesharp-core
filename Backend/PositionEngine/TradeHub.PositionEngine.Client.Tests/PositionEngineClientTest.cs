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
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.PositionEngine.Client.Service;
using TradeHub.PositionEngine.Configuration.Service;
using TradeHub.PositionEngine.ProviderGateway.Service;
using TradeHub.PositionEngine.Service;

namespace TradeHub.PositionEngine.Client.Tests
{
    [TestFixture]
    class PositionEngineClientTest
    {
        private PositionEngineClient _positionEngineClient;
        private ApplicationController _applicationController;

        [SetUp]
        public void Setup()
        {
            _applicationController = new ApplicationController(new PositionEngineMqServer("PEMQConfig.xml"),
                new PositionMessageProcessor());
            _applicationController.StartServer();
            _positionEngineClient=new PositionEngineClient();
        }

        [TearDown]
        public void Close()
        {
            _positionEngineClient.Shutdown();
            _applicationController.StopServer();
        }

        [Test]
        [Category("Integration")]
        public void AppIDTestCase()
        {
            Thread.Sleep(2000);
            _positionEngineClient.Initialize(OrderExecutionProvider.Simulated);
            ManualResetEvent manualAppIDEvent = new ManualResetEvent(false); ;

            manualAppIDEvent.WaitOne(3000, false);

            Assert.NotNull(true, _positionEngineClient.AppId, "App ID");
            Assert.AreEqual("A00", _positionEngineClient.AppId, "App ID Value");
        }

        [Test]
        [Category("Integration")]
        public void PositionTestCase()
        {
            Thread.Sleep(2000);
            bool PositionArrived = false;
            ManualResetEvent manualPositionEvent=new ManualResetEvent(false);
            manualPositionEvent.WaitOne(4000, false);
            //_positionEngineClient.SubscribeProviderPosition();
           _positionEngineClient.PositionArrived += delegate
            {
                PositionArrived = true;
                manualPositionEvent.Set();
            };
            Assert.AreEqual(true,PositionArrived,"PositionArrived");
        }
    }
}
