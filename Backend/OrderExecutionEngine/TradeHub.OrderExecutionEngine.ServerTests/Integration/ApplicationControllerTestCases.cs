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
using Spring.Context.Support;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.Inquiry;
using TradeHub.OrderExecutionEngine.Server.Service;
using Constants = TradeHub.Common.Core.Constants;

namespace TradeHub.OrderExecutionEngine.ServerTests.Integration
{
    [TestFixture]
    public class ApplicationControllerTestCases
    {
        private ApplicationController _applicationController;
        private IAdvancedBus _advancedBus;
        private IQueue _inquiryQueue;
        private IQueue _adminQueue;
        private IQueue _orderQueue;
        private IQueue _executionQueue;
        private IQueue _rejectionQueue;
        private IQueue _positionQueue;
        private IExchange _adminExchange;
        private Dictionary<string, string> appInfo = new Dictionary<string, string>()
            {
                {"Admin","admin.strategy.key"},{"Order","order.strategy.key"},{"Execution","execution.strategy.key"},{"Rejection","rejection.strategy.key"},{"Locate","locate.strategy.key"}
            };

        [SetUp]
        public void SetUp()
        {
            _applicationController = ContextRegistry.GetContext()["ApplicationController"] as ApplicationController;
            if (_applicationController != null) _applicationController.StartServer();

            // Initialize Advance Bus
            _advancedBus = RabbitHutch.CreateBus("host=localhost").Advanced;

            // Create a Admin exchange
            _adminExchange = _advancedBus.ExchangeDeclare("orderexecution_exchange", ExchangeType.Direct, true, false, true);

            // Create Admin Queue
            _adminQueue = _advancedBus.QueueDeclare("admin_queue", false, false, true, true);

            // Create Inquiry Queue
            _inquiryQueue = _advancedBus.QueueDeclare("inquiry_queue", false, false, true, true);

            // Create Order Queue
            _orderQueue = _advancedBus.QueueDeclare("order_queue", false, false, true, true);

            // Create Execution Queue
            _executionQueue = _advancedBus.QueueDeclare("execution_queue", false, false, true, true);

            // Create Rejection Queue
            _rejectionQueue = _advancedBus.QueueDeclare("rejection_queue", false, false, true, true);

            // Create Poistion Queue
           // _positionQueue = Queue.Declare(false, true, false, "position_queue", null);

            _positionQueue = _advancedBus.QueueDeclare("position_queue", false, false, true, true);

            // Bind Admin Queue to already initialized Exchange with the specified Routing Key
            _advancedBus.Bind(_adminExchange, _adminQueue, "admin.strategy.key");

            // Bind Inquiry Queue to already initialized Exchange with the specified Routing Key
            _advancedBus.Bind(_adminExchange, _inquiryQueue, "inquiry.strategy.key");

            // Bind Order Queue to already initialized Exchange with the specified Routing Key
            _advancedBus.Bind(_adminExchange, _orderQueue, "order.strategy.key");

            // Bind Execution Queue to already initialized Exchange with the specified Routing Key
            _advancedBus.Bind(_adminExchange, _executionQueue, "execution.strategy.key");

            // Bind Rejection Queue to already initialized Exchange with the specified Routing Key
            _advancedBus.Bind(_adminExchange, _rejectionQueue, "rejection.strategy.key");

            // Bind Position Queue to already initialized Exchange with the specified Routing Key
            _advancedBus.Bind(_adminExchange,_positionQueue, "orderexecution.engine.position");

            var appInfoMessage = new Message<Dictionary<string, string>>(appInfo);
            appInfoMessage.Properties.AppId = "ID";
            string routingKey = "orderexecution.engine.appinfo";

            //using (var channel = _advancedBus.OpenPublishChannel())
            {
                _advancedBus.Publish(_adminExchange, routingKey, true, false, appInfoMessage);
            }
        }

        [TearDown]
        public void Close()
        {
            _applicationController.StopServer();
        }

        [Test]
        [Category("Integration")]
        public void RequestNewStrategyIdTestCase()
        {
            InquiryMessage inquiryMessage = new InquiryMessage() { Type = Constants.InquiryTags.AppID };
            Message<InquiryMessage> message = new Message<InquiryMessage>(inquiryMessage);

            var manualInquiryEvent = new ManualResetEvent(false);

            bool inquiryArrived = false;

            message.Properties.AppId = "ID";
            message.Properties.ReplyTo = "inquiry.strategy.key";

            _advancedBus.Consume<InquiryResponse>(
                _inquiryQueue, (msg, messageReceivedInfo) =>
                          Task.Factory.StartNew(() =>
                          {
                              Console.WriteLine("Received Application ID: " + msg.Body.AppId);
                              inquiryArrived = true;
                              manualInquiryEvent.Set();
                          }));

            //using (var channel = _advancedBus.OpenPublishChannel())
            {
                _advancedBus.Publish(_adminExchange, "orderexecution.engine.inquiry", true, false, message);

            }
            manualInquiryEvent.WaitOne(9000, false);

            Assert.AreEqual(true, inquiryArrived);
        }

        [Test]
        [Category("Integration")]
        public void ConnectSimulatorTestCase()
        {
            Login login = new Login { OrderExecutionProvider = Constants.OrderExecutionProvider.Simulated };
            Message<Login> message = new Message<Login>(login);

            var manualLogonEvent = new ManualResetEvent(false);

            bool logonArrived = false;

            message.Properties.AppId = "ID";
            message.Properties.ReplyTo = "admin.strategy.key";

            _advancedBus.Consume<string>(
                _adminQueue, (msg, messageReceivedInfo) =>
                          Task.Factory.StartNew(() =>
                          {
                              if (msg.Body.Contains("Logon"))
                              {
                                  logonArrived = true;
                                  manualLogonEvent.Set();
                              }
                          }));

            //using (var channel = _advancedBus.OpenPublishChannel())
            {
                _advancedBus.Publish(_adminExchange, "orderexecution.engine.login", true, false, message);
            }
            manualLogonEvent.WaitOne(30000, false);

            Assert.AreEqual(true, logonArrived);
        }

        [Test]
        [Category("Integration")]
        public void DisconnectSimulatorTestCase()
        {
            Login login = new Login { OrderExecutionProvider = Constants.OrderExecutionProvider.Simulated };
            Message<Login> loginMessage = new Message<Login>(login);

            Logout logout = new Logout { OrderExecutionProvider = Constants.OrderExecutionProvider.Simulated };
            Message<Logout> logoutMessage = new Message<Logout>(logout);

            var manualLogonEvent = new ManualResetEvent(false);
            var manualLogoutEvent = new ManualResetEvent(false);

            bool logonArrived = false;
            bool logoutArrived = false;

            loginMessage.Properties.AppId = "ID";
            loginMessage.Properties.ReplyTo = "admin.strategy.key";

            logoutMessage.Properties.AppId = "ID";
            logoutMessage.Properties.ReplyTo = "admin.strategy.key";

            //using (var channel = _advancedBus.OpenPublishChannel())
            {

                _advancedBus.Consume<string>(
                    _adminQueue, (msg, messageReceivedInfo) =>
                              Task.Factory.StartNew(() =>
                              {
                                  if (msg.Body.Contains("Logon"))
                                  {
                                      logonArrived = true;

                                      _advancedBus.Publish(_adminExchange, "orderexecution.engine.logout", true, false, logoutMessage);

                                      manualLogonEvent.Set();
                                  }
                                  else if (msg.Body.Contains("Logout"))
                                  {
                                      logoutArrived = true;
                                      manualLogoutEvent.Set();
                                  }
                              }));


                _advancedBus.Publish(_adminExchange, "orderexecution.engine.login", true, false, loginMessage);

                manualLogonEvent.WaitOne(30000, false);
                manualLogoutEvent.WaitOne(30000, false);
            }
            Assert.AreEqual(true, logonArrived, "Logon Status");
            Assert.AreEqual(true, logoutArrived, "Logout Status");
        }

        [Test]
        [Category("Console")]
        public void MarketOrderSimulatorTestCase()
        {
            Login login = new Login { OrderExecutionProvider = Constants.OrderExecutionProvider.Simulated };
            Message<Login> message = new Message<Login>(login);
            message.Properties.AppId = "ID";
            message.Properties.ReplyTo = "admin.strategy.key";

            MarketOrder marketOrder = new MarketOrder(Constants.OrderExecutionProvider.Simulated);
            marketOrder.OrderID = "AA";
            Message<MarketOrder> marketOrderMessage = new Message<MarketOrder>(marketOrder);
            marketOrderMessage.Properties.AppId = "ID";

            var manualLogonEvent = new ManualResetEvent(false);
            var manualNewEvent = new ManualResetEvent(false);
            var manualExecutionEvent = new ManualResetEvent(false);

            bool logonArrived = false;
            bool newArrived = false;
            bool executionArrived = false;

            //using (var channel = _advancedBus.OpenPublishChannel())
            {
                _advancedBus.Consume<string>(
                    _adminQueue, (msg, messageReceivedInfo) =>
                                 Task.Factory.StartNew(() =>
                                     {
                                         if (msg.Body.Contains("Logon"))
                                         {
                                             logonArrived = true;
                                             _advancedBus.Publish(_adminExchange, "orderexecution.engine.marketorder", true, false, marketOrderMessage);
                                             manualLogonEvent.Set();
                                         }
                                     }));

                _advancedBus.Consume<Order>(
                    _orderQueue, (msg, messageReceivedInfo) =>
                                 Task.Factory.StartNew(() =>
                                     {
                                         if (msg.Body.OrderStatus.Equals(Constants.OrderStatus.SUBMITTED))
                                         {
                                             newArrived = true;
                                             manualNewEvent.Set();
                                         }
                                     }));

                _advancedBus.Consume<Execution>(
                    _executionQueue, (msg, messageReceivedInfo) =>
                                     Task.Factory.StartNew(() =>
                                         {
                                             if (msg.Body.Fill.ExecutionPrice == marketOrder.OrderSize)
                                             {
                                                 executionArrived = true;
                                                 manualExecutionEvent.Set();
                                             }
                                         }));

                _advancedBus.Publish(_adminExchange, "orderexecution.engine.login", true, false, message);

                manualLogonEvent.WaitOne(30000, false);
                manualNewEvent.WaitOne(300000, false);
                manualExecutionEvent.WaitOne(300000, false);
            }


            Assert.AreEqual(true, logonArrived, "Logon arrived");
            Assert.AreEqual(true, newArrived, "New arrived");
            Assert.AreEqual(true, executionArrived, "Execution arrived");
        }

        [Test]
        [Category("Console")]
        public void LimitOrderSimulatorTestCase()
        {
            Login login = new Login { OrderExecutionProvider = Constants.OrderExecutionProvider.Simulated };
            Message<Login> message = new Message<Login>(login);
            message.Properties.AppId = "ID";
            message.Properties.ReplyTo = "admin.strategy.key";

            LimitOrder limitOrder = new LimitOrder(Constants.OrderExecutionProvider.Simulated);
            limitOrder.OrderID = "AA";
            Message<LimitOrder> limitOrderMessage = new Message<LimitOrder>(limitOrder);
            limitOrderMessage.Properties.AppId = "ID";

            var manualLogonEvent = new ManualResetEvent(false);
            var manualNewEvent = new ManualResetEvent(false);
            var manualExecutionEvent = new ManualResetEvent(false);

            bool logonArrived = false;
            bool newArrived = false;
            bool executionArrived = false;


            //using (var channel = _advancedBus.OpenPublishChannel())
            {
                _advancedBus.Consume<string>(
                    _adminQueue, (msg, messageReceivedInfo) =>
                                 Task.Factory.StartNew(() =>
                                 {
                                     if (msg.Body.Contains("Logon"))
                                     {
                                         logonArrived = true;
                                         _advancedBus.Publish(_adminExchange, "orderexecution.engine.limitorder", true, false, limitOrderMessage);
                                         manualLogonEvent.Set();
                                     }
                                 }));

                _advancedBus.Consume<Order>(
                    _orderQueue, (msg, messageReceivedInfo) =>
                                 Task.Factory.StartNew(() =>
                                 {
                                     if (msg.Body.OrderStatus.Equals(Constants.OrderStatus.SUBMITTED))
                                     {
                                         newArrived = true;
                                         manualNewEvent.Set();
                                     }
                                 }));

                _advancedBus.Consume<Execution>(
                    _executionQueue, (msg, messageReceivedInfo) =>
                                     Task.Factory.StartNew(() =>
                                     {
                                         executionArrived = true;
                                         manualExecutionEvent.Set();
                                     }));

                _advancedBus.Publish(_adminExchange, "orderexecution.engine.login", true, false, message);

                manualLogonEvent.WaitOne(30000, false);
                manualNewEvent.WaitOne(300000, false);
                manualExecutionEvent.WaitOne(300000, false);
            }


            Assert.AreEqual(true, logonArrived, "Logon arrived");
            Assert.AreEqual(true, newArrived, "New arrived");
            Assert.AreEqual(true, executionArrived, "Execution arrived");
        }

        [Test]
        [Category("Console")]
        public void CancelOrderSimulatorTestCase()
        {
            Login login = new Login { OrderExecutionProvider = Constants.OrderExecutionProvider.Simulated };
            Message<Login> message = new Message<Login>(login);
            message.Properties.AppId = "ID";
            message.Properties.ReplyTo = "admin.strategy.key";

            LimitOrder limitOrder = new LimitOrder(Constants.OrderExecutionProvider.Simulated);
            limitOrder.OrderID = "AA";
            Message<LimitOrder> limitOrderMessage = new Message<LimitOrder>(limitOrder);
            limitOrderMessage.Properties.AppId = "ID";

            var manualLogonEvent = new ManualResetEvent(false);
            var manualNewEvent = new ManualResetEvent(false);
            var manualCancellationEvent = new ManualResetEvent(false);

            bool logonArrived = false;
            bool newArrived = false;
            bool cancellationArrived = false;

            //using (var channel = _advancedBus.OpenPublishChannel())
            {
                _advancedBus.Consume<string>(
                    _adminQueue, (msg, messageReceivedInfo) =>
                                 Task.Factory.StartNew(() =>
                                 {
                                     if (msg.Body.Contains("Logon"))
                                     {
                                         logonArrived = true;
                                         _advancedBus.Publish(_adminExchange, "orderexecution.engine.limitorder", true, false, limitOrderMessage);
                                         manualLogonEvent.Set();
                                     }
                                 }));

                _advancedBus.Consume<Order>(
                    _orderQueue, (msg, messageReceivedInfo) =>
                                 Task.Factory.StartNew(() =>
                                 {
                                     if (msg.Body.OrderStatus.Equals(Constants.OrderStatus.SUBMITTED))
                                     {
                                         newArrived = true;
                                         _advancedBus.Publish(_adminExchange, "orderexecution.engine.cancelorder", true, false, limitOrderMessage);
                                         manualNewEvent.Set();
                                     }
                                     else if (msg.Body.OrderStatus.Equals(Constants.OrderStatus.CANCELLED))
                                     {
                                         cancellationArrived = true;
                                         manualCancellationEvent.Set();
                                     }
                                 }));

                _advancedBus.Publish(_adminExchange, "orderexecution.engine.login", true, false, message);

                manualLogonEvent.WaitOne(30000, false);
                manualNewEvent.WaitOne(300000, false);
                manualCancellationEvent.WaitOne(300000, false);
            }


            Assert.AreEqual(true, logonArrived, "Logon arrived");
            Assert.AreEqual(true, newArrived, "New arrived");
            Assert.AreEqual(true, cancellationArrived, "Cancellation arrived");
        }

        [Test]
        [Category("Integration")]
        public void ExecutionTestCase()
        {
            Fill fill = new Fill(new Security { Symbol = "AAPL" }, "TEST", "AOO")
            {
                ExecutionDateTime = DateTime.Now,
                ExecutionType = Constants.ExecutionType.Fill,
                ExecutionId = "",
                ExecutionPrice = 100,
                ExecutionSize = 100,
                ExecutionSide = "BUY",
                AverageExecutionPrice = 100,
                LeavesQuantity = 0,
                CummalativeQuantity = 100
            };

            Order order = new Order("TEST")
            {
                OrderID = "5000",
                BrokerOrderID = "5000",
                OrderSide = "BUY",
                OrderSize = 100
            };

            Execution execution = new Execution(fill, order);

            Message<Execution> message = new Message<Execution>(execution);

            var manualExecutionEvent = new ManualResetEvent(false);

            bool executionArrived = false;

            message.Properties.AppId = "ID";
            message.Properties.ReplyTo = "execution.strategy.key";

            _advancedBus.Consume<Execution>(
                _executionQueue, (msg, messageReceivedInfo) =>
                          Task.Factory.StartNew(() =>
                          {
                              Console.WriteLine(msg.Body);
                                  executionArrived = true;
                                  manualExecutionEvent.Set();
                          }));

            //using (var channel = _advancedBus.OpenPublishChannel())
            {
                _advancedBus.Publish(_adminExchange, "execution.strategy.key", true, false, message);
            }
            manualExecutionEvent.WaitOne(30000, false);

            Assert.AreEqual(true, executionArrived);
        }


        // sample input for test case is 
        // P AAPL 1000 10000 400 400 400
        [Test]
        [Category("Console")]
        public void PositionMessageTestCase()
        {
            Login login = new Login {OrderExecutionProvider = Constants.OrderExecutionProvider.Simulated};
            Message<Login> message = new Message<Login>(login);
            var manualLogonEvent = new ManualResetEvent(false);
            var manualPositionEvent = new ManualResetEvent(false);
            bool logonArrived = false;
            bool positionMessageArrived = false;
            
            message.Properties.AppId = "ID";
            message.Properties.ReplyTo = "admin.strategy.key";

          //  using (var channel = _advancedBus.OpenPublishChannel())
            {
                _advancedBus.Consume<string>(
                    _adminQueue, (msg, messageReceivedInfo) =>
                                 Task.Factory.StartNew(() =>
                                 {
                                     if (msg.Body.Contains("Logon"))
                                     {
                                         logonArrived = true;
                                         manualLogonEvent.Set();
                                     }
                                 }));

                _advancedBus.Consume<Position>(
                    _positionQueue, (msg, messageReceivedInfo) =>
                                 Task.Factory.StartNew(() =>
                                 {
                                     if (msg.Body.Provider.Equals(Constants.OrderExecutionProvider.Simulated) &&
                                         msg.Body.Quantity==1000 &&
                                         msg.Body.Security.Symbol=="AAPL" &&
                                         msg.Body.ExitValue==10000 &&
                                         msg.Body.AvgBuyPrice==400 &&
                                         msg.Body.AvgSellPrice==400 &&
                                         msg.Body.Price==400
                                         )
                                     {
                                         positionMessageArrived = true;
                                         
                                         manualPositionEvent.Set();
                                     }
                                     
                                 }));

                _advancedBus.Publish(_adminExchange, "orderexecution.engine.login",true,false, message);

                manualLogonEvent.WaitOne(30000, false);
                manualPositionEvent.WaitOne(300000, false);
               
            }


            Assert.AreEqual(true, logonArrived, "Logon arrived");
            Assert.AreEqual(true, positionMessageArrived, "Position Message arrived");
           


        }
    }
}
