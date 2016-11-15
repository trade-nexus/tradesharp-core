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
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using Disruptor;
using NUnit.Framework;
using Spring.Context.Support;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.FactoryMethods;
using TradeHub.Common.Core.ValueObjects;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.StrategyEngine.Testing.SimpleStrategy.EMA;

namespace TradeHub.StrategyEngine.TradeHub.Tests.Integration
{
    [TestFixture]
    class TradeHubSampleStrategyTests : IEventHandler<RabbitMqRequestMessage>
    {
        //private ApplicationController _applicationController;
        private TradeHubStrategy _tradeHubStrategy;
        private bool _tickArrived;
        private bool _tickRequestArrived;
        private ManualResetEvent _manualTickEvent;
        private ManualResetEvent _manualTickRequestEvent;
        private ManualResetEvent _manualNewOrderEvent;
        private bool _newOrderEventArrived;
        private decimal _count=1;
        private Type _type = typeof (TradeHubSampleStrategyTests);
        private Stopwatch _stopwatch;

        [SetUp]
        public void StartUp()
        {
            //_applicationController = ContextRegistry.GetContext()["ApplicationController"] as ApplicationController;
            //if (_applicationController != null) _applicationController.StartServer();

            _tradeHubStrategy = new EmaStrategy(1, 2, "LAST", "ERX", 2, "TIME", "LAST",
                                                Common.Core.Constants.MarketDataProvider.Simulated,
                                                Common.Core.Constants.OrderExecutionProvider.Simulated);
            _stopwatch = new Stopwatch();
        }

        [TearDown]
        public void Close()
        {
            if(_tradeHubStrategy.IsRunning)
                _tradeHubStrategy.Stop();

            //_applicationController.StopServer();
        }

        [Test]
        [Category("Console")]
        public void OverRideTickEventTestCase()
        {
            _tradeHubStrategy.InitializeMarketDataServiceDisruptor(new IEventHandler<RabbitMqRequestMessage>[] { this });

            _tickArrived = false;

            _manualTickEvent = new ManualResetEvent(false);

            Thread.Sleep(5000);
            _tradeHubStrategy.Run();

            _manualTickEvent.WaitOne(300000, false);

            Assert.AreEqual(true, _tickArrived, "Tick Arrived");
        }

        [Test]
        [Category("Integration")]
        public void OverRideTickRequestTestCase()
        {
            //_tradeHubStrategy.InitializeMarketDataServiceDisruptor(new IEventHandler<RabbitMqMessage>[] { this });
            Subscribe subscribe = SubscriptionMessage.TickSubscription("1", new Security() {Symbol = "AAPL"},
                                                                       "SimulatedExchange");
            _tradeHubStrategy.OverrideTickSubscriptionRequest(TickSubscribedRequestReceived);

            _tickRequestArrived = false;

            _manualTickRequestEvent = new ManualResetEvent(false);

            Thread.Sleep(5000);
            _tradeHubStrategy.Run();

            _manualTickRequestEvent.WaitOne(30000, false);

            Assert.AreEqual(true, _tickRequestArrived, "Tick Request Arrived");
        }

        [Test]
        [Category("Integration")]
        public void OverRideNewOrderEventTestCase()
        {
            _tradeHubStrategy.InitializeOrderExecutionServiceDisruptor(new IEventHandler<RabbitMqRequestMessage>[] {this});

            _newOrderEventArrived = false;

            _manualNewOrderEvent = new ManualResetEvent(false);

            Thread.Sleep(5000);
            _tradeHubStrategy.Run();
            Thread.Sleep(1000);
            Security security = new Security { Symbol = "AAPL" };

            // Create new Market Order
            MarketOrder marketOrder = OrderMessage.GenerateMarketOrder("1", security,
                                                                       Common.Core.Constants.OrderSide.BUY, 100
                                                                       , Common.Core.Constants.OrderExecutionProvider.Simulated);

            // Send Market Order to OEE
            _tradeHubStrategy.SendOrder(marketOrder);

            _manualNewOrderEvent.WaitOne(30000, false);

            Assert.AreEqual(true, _newOrderEventArrived, "New Order Event Arrived");
        }

        [Test]
        [Category("Console")]
        public void OrderIDGeneratorTestCase_ConnectionEstablishedWithServer()
        {
            Thread.Sleep(2000);
            string orderId = _tradeHubStrategy.GetNewOrderId();

            Console.WriteLine("Order ID: " + orderId);

            string appender = orderId.Substring(orderId.Length - 3);

            Assert.IsTrue(appender.Equals("A00"), "Appender: " + appender);
        }

        [Test]
        [Category("Integration")]
        public void OrderIDGeneratorTestCase_ConnectionNotEstablishedWithServer()
        {
            string orderId = _tradeHubStrategy.GetNewOrderId();

            Console.WriteLine("Order ID: " + orderId);

            string appender = orderId.Substring(orderId.Length - 4);

            Assert.IsTrue(appender.Equals("A000"), "Appender: " + appender);
        }

        /// <summary>
        /// Called when new tick subscription request is received
        /// </summary>
        private void TickSubscribedRequestReceived(Subscribe subscribe)
        {
            _tickRequestArrived = true;
            _manualTickRequestEvent.Set();
        }

        /// <summary>
        /// Called when new Tick message is received and processed by Disruptor
        /// </summary>
        /// <param name="message"></param>
        private void OnTickDataReceived(string[] message)
        {
            try
            {
                //if (Logger.IsDebugEnabled)
                //{
                //    Logger.Debug("Tick received", "TradeHubSampleStrategyTests", "OnTickDataReceived");
                //}

                Tick entry = new Tick();

                if (ParseToTick(entry, message))
                {
                   // _count++;
                    _tickArrived = true;
                    //if(_count==10)
                    //{
                    //    _manualTickEvent.Set();
                    //}
                    //if (_count == 1)
                    //{
                    //    _stopwatch.Start();
                    //    Logger.Info("First tick arrived " +entry, _type.FullName, "TickArrived");
                    //}
                    //if (_count == 336360)
                    {
                        _stopwatch.Stop();
                        //Logger.Info("1000000 Ticks recevied in " + _stopwatch.ElapsedMilliseconds + " ms", _type.FullName, "TickArrived");
                        //Logger.Info(1000000 / _stopwatch.ElapsedMilliseconds * 1000 + "msg/sec", _type.FullName, "TickArrived");
                        //_marketDataEngineClient.SendLogoutRequest(new Logout { MarketDataProvider = Common.Core.Constants.MarketDataProvider.Simulated });
                        //Close();
                        _manualTickEvent.Set();

                    }

                    _count++;
            

                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "TradeHubSampleStrategyTests", "OnTickDataReceived");
            }
        }

        /// <summary>
        /// Creats tick object from incoming string message
        /// </summary>
        /// <param name="tick">Tick to add values to</param>
        /// <param name="message">Received message</param>
        /// <returns></returns>
        private bool ParseToTick(Tick tick, string[] message)
        {
            try
            {
                // Get Bid Values
                tick.BidPrice = Convert.ToDecimal(message[1]);
                tick.BidSize = Convert.ToDecimal(message[2]);

                // Get Ask Values
                tick.AskPrice = Convert.ToDecimal(message[3]);
                tick.AskSize = Convert.ToDecimal(message[4]);

                // Get Last Values
                tick.LastPrice = Convert.ToDecimal(message[5]);
                tick.LastSize = Convert.ToDecimal(message[6]);

                // Get Symbol
                tick.Security = new Security() { Symbol = message[7] };
                // Get Time Value
                tick.DateTime = DateTime.ParseExact(message[8], "M/d/yyyy h:mm:ss.fff tt", CultureInfo.InvariantCulture);
                // Get Provider name
                tick.MarketDataProvider = message[9];
                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "TradeHubSampleStrategyTests", "ParseToTick");
                return true;
            }
        }

        /// <summary>
        /// Called when Market Order Request is receieved
        /// </summary>
        /// <param name="messageArray"></param>
        private void OnMarketOrderReceived(string[] messageArray)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Market Order request received: " + messageArray[0] + messageArray[2], "TradeHubSampleStrategyTests", "OnMarketOrderReceived");
            }

            MarketOrder marketOrder = new MarketOrder(messageArray[7]);

            if (ParseToMarketOrder(marketOrder, messageArray))
            {
                _newOrderEventArrived = true;
                _manualNewOrderEvent.Set();
            }
        }

        /// <summary>
        /// Creats market order object from incoming string message
        /// </summary>
        /// <param name="marketOrder">Market Order to add values to</param>
        /// <param name="message">Received message</param>
        /// <returns></returns>
        private bool ParseToMarketOrder(MarketOrder marketOrder, string[] message)
        {
            try
            {
                // Get Order ID
                marketOrder.OrderID = message[1];
                // Get Order Side
                marketOrder.OrderSide = message[2];
                // Get Order Size
                marketOrder.OrderSize = Convert.ToInt32(message[3]);
                // Get Order TIF value
                marketOrder.OrderTif = message[4];
                // Get Symbol
                marketOrder.Security = new Security() { Symbol = message[5] };
                // Get Time Value
                marketOrder.OrderDateTime = DateTime.ParseExact(message[6], "M/d/yyyy h:mm:ss.fff tt", CultureInfo.InvariantCulture);

                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "TradeHubSampleStrategyTests", "ParseToMarketOrder");
                return true;
            }
        }

        #region Implementation of IEventHandler<in RabbitMqMessage>

        /// <summary>
        /// Called when a publisher has committed an event to the <see cref="T:Disruptor.RingBuffer`1"/>
        /// </summary>
        /// <param name="data">Data committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="sequence">Sequence number committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="endOfBatch">flag to indicate if this is the last event in a batch from the <see cref="T:Disruptor.RingBuffer`1"/></param>
        public void OnNext(RabbitMqRequestMessage data, long sequence, bool endOfBatch)
        {
            string message = Encoding.UTF8.GetString(data.Message);

            var messageArray = message.Split(',');

            if (messageArray[0].Equals("Market"))
                OnMarketOrderReceived(messageArray);
            if (messageArray[0].Equals("TICK"))
                OnTickDataReceived(messageArray);
        }

        #endregion
    }
}
