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
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.TradeManager.Server.Service;

namespace TradeHub.TradeManager.Server.Tests.Unit
{
    [TestFixture]
    public class ExecutionHandlerTestCases
    {
        [SetUp]
        public void SetUp()
        {
            
        }

        [TearDown]
        public void TearDown()
        {
            
        }

        [Test]
        [Category("Unit")]
        public void NewExecutionSimulated_CreateNewTradeProcessor_OneTradeProcessorInExecutionHandlerForSimulated()
        {
            // Create new Execution Processor Object
            ExecutionHandler executionHandler = new ExecutionHandler();

            // Create Order Object
            Order order = new Order(OrderExecutionProvider.Simulated);

            // Create Fill Object
            Fill fill = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
            fill.ExecutionId = "1";
            fill.ExecutionSide = OrderSide.BUY;
            fill.ExecutionSize = 40;
            // Create Execution Object
            Execution execution = new Execution(fill, order);

            // Add new execution to the Execution Processor
            executionHandler.NewExecutionArrived(execution);

            var tradeFactories = executionHandler.TradeProcessorMap;

            Assert.AreEqual(1, tradeFactories.Count, "Trade Factory Count");
            Assert.AreEqual(OrderExecutionProvider.Simulated, tradeFactories[OrderExecutionProvider.Simulated][new Security() {Symbol = "GOOG"}].ExecutionProvider, "Execution of Trade Factory");
        }


        [Test]
        [Category("Unit")]
        public void NewExecutionSimulatedMultipleSecurities_CreateNewTradeProcessor_OneTradeProcessorInExecutionHandlerForSimulated()
        {
            // Create new Execution Processor Object
            ExecutionHandler executionHandler = new ExecutionHandler();
            
            {
                // Create Order Object
                Order order = new Order(OrderExecutionProvider.Simulated);

                // Create Fill Object
                Fill fill = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fill.ExecutionId = "1";
                fill.ExecutionSide = OrderSide.BUY;
                fill.ExecutionSize = 40;
                // Create Execution Object
                Execution execution = new Execution(fill, order);

                // Add new execution to the Execution Processor
                executionHandler.NewExecutionArrived(execution);
            }

            {
                // Create Order Object
                Order order = new Order(OrderExecutionProvider.Simulated);

                // Create Fill Object
                Fill fill = new Fill(new Security() { Symbol = "AAPL" }, OrderExecutionProvider.Simulated, "1");
                fill.ExecutionId = "1";
                fill.ExecutionSide = OrderSide.BUY;
                fill.ExecutionSize = 40;
                // Create Execution Object
                Execution execution = new Execution(fill, order);

                // Add new execution to the Execution Processor
                executionHandler.NewExecutionArrived(execution);
            }

            var tradeFactories = executionHandler.TradeProcessorMap;

            Assert.AreEqual(1, tradeFactories.Count, "Trade Factory List Count");
            Assert.AreEqual(2, tradeFactories[OrderExecutionProvider.Simulated].Count, "Total Active Trade Factories");
            Assert.AreEqual(OrderExecutionProvider.Simulated, tradeFactories[OrderExecutionProvider.Simulated][new Security() { Symbol = "GOOG" }].ExecutionProvider, "Execution of Trade Factory");
            Assert.AreEqual(OrderExecutionProvider.Simulated, tradeFactories[OrderExecutionProvider.Simulated][new Security() { Symbol = "AAPL" }].ExecutionProvider, "Execution of Trade Factory");
        }

        [Test]
        [Category("Unit")]
        public void MultipleExecutionsSimulated_CreateNewTradeProcessor_OneTradeProcessorInExecutionHandlerForSimulated()
        {
            // Create new Execution Processor Object
            ExecutionHandler executionHandler = new ExecutionHandler();

            // Create Order Object
            Order order = new Order(OrderExecutionProvider.Simulated);

            {
                // Create Fill Object
                Fill fill = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fill.ExecutionId = "1";
                fill.ExecutionSide = OrderSide.BUY;
                fill.ExecutionSize = 40;
                // Create Execution Object
                Execution execution = new Execution(fill, order);

                // Add new execution to the Execution Processor
                executionHandler.NewExecutionArrived(execution);
            }

            {
                // Create Fill Object
                Fill fill = new Fill(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated, "2");
                fill.ExecutionId = "2";
                fill.ExecutionSide = OrderSide.BUY;
                fill.ExecutionSize = 40;
                // Create Execution Object
                Execution execution = new Execution(fill, order);

                // Add new execution to the Execution Processor
                executionHandler.NewExecutionArrived(execution);
            }
            var tradeFactories = executionHandler.TradeProcessorMap;

            Assert.AreEqual(1, tradeFactories.Count, "Trade Factory Count");
            Assert.AreEqual(OrderExecutionProvider.Simulated, tradeFactories[OrderExecutionProvider.Simulated][new Security() { Symbol = "GOOG" }].ExecutionProvider, "Execution of Trade Factory");
        }

        [Test]
        [Category("Unit")]
        public void MultipleExecutionsSimulatedMultipleSecurities_CreateNewTradeProcessor_OneTradeProcessorInExecutionHandlerForSimulated()
        {
            // Create new Execution Processor Object
            ExecutionHandler executionHandler = new ExecutionHandler();

            // Create Order Object
            Order order = new Order(OrderExecutionProvider.Simulated);

            {
                // Create Fill Object
                Fill fill = new Fill(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated, "1");
                fill.ExecutionId = "1";
                fill.ExecutionSide = OrderSide.BUY;
                fill.ExecutionSize = 40;
                // Create Execution Object
                Execution execution = new Execution(fill, order);

                // Add new execution to the Execution Processor
                executionHandler.NewExecutionArrived(execution);
            }

            {
                // Create Fill Object
                Fill fill = new Fill(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated, "2");
                fill.ExecutionId = "2";
                fill.ExecutionSide = OrderSide.BUY;
                fill.ExecutionSize = 40;
                // Create Execution Object
                Execution execution = new Execution(fill, order);

                // Add new execution to the Execution Processor
                executionHandler.NewExecutionArrived(execution);
            }

            {
                // Create Fill Object
                Fill fill = new Fill(new Security() { Symbol = "AAPL" }, OrderExecutionProvider.Simulated, "1");
                fill.ExecutionId = "1";
                fill.ExecutionSide = OrderSide.BUY;
                fill.ExecutionSize = 40;
                // Create Execution Object
                Execution execution = new Execution(fill, order);

                // Add new execution to the Execution Processor
                executionHandler.NewExecutionArrived(execution);
            }

            {
                // Create Fill Object
                Fill fill = new Fill(new Security() { Symbol = "AAPL" }, OrderExecutionProvider.Simulated, "2");
                fill.ExecutionId = "2";
                fill.ExecutionSide = OrderSide.BUY;
                fill.ExecutionSize = 40;
                // Create Execution Object
                Execution execution = new Execution(fill, order);

                // Add new execution to the Execution Processor
                executionHandler.NewExecutionArrived(execution);
            }

            var tradeFactories = executionHandler.TradeProcessorMap;

            Assert.AreEqual(1, tradeFactories.Count, "Trade Factory List Count");
            Assert.AreEqual(2, tradeFactories[OrderExecutionProvider.Simulated].Count, "Total Active Trade Factories");
            Assert.AreEqual(OrderExecutionProvider.Simulated, tradeFactories[OrderExecutionProvider.Simulated][new Security() { Symbol = "GOOG" }].ExecutionProvider, "Execution of Trade Factory");
            Assert.AreEqual(OrderExecutionProvider.Simulated, tradeFactories[OrderExecutionProvider.Simulated][new Security() { Symbol = "AAPL" }].ExecutionProvider, "Execution of Trade Factory");
        }

        [Test]
        [Category("Unit")]
        public void NewExecutionSimulatedAndBlackwood_CreateNewTradeProcessor_TwoTradeProcessorInExecutionProcessorOneForSimulatedOneForBlackwood()
        {
            // Create new Execution Processor Object
            ExecutionHandler executionHandler = new ExecutionHandler();

            {
                // Create Order Object
                Order order = new Order(OrderExecutionProvider.Simulated);

                // Create Fill Object
                Fill fill = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fill.ExecutionId = "1";
                fill.ExecutionSide = OrderSide.BUY;
                fill.ExecutionSize = 40;
                // Create Execution Object
                Execution execution = new Execution(fill, order);
                // Add new execution to the Execution Processor
                executionHandler.NewExecutionArrived(execution);
            }

            {
                // Create Order Object
                Order order = new Order(OrderExecutionProvider.Blackwood);

                // Create Fill Object
                Fill fill = new Fill(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Blackwood, "1");
                fill.ExecutionId = "1";
                fill.ExecutionSide = OrderSide.BUY;
                fill.ExecutionSize = 40;
                // Create Execution Object
                Execution execution = new Execution(fill, order);
                // Add new execution to the Execution Processor
                executionHandler.NewExecutionArrived(execution);
            }

            var tradeProcessorMap = executionHandler.TradeProcessorMap;

            Assert.AreEqual(2, tradeProcessorMap.Count, "Trade Processor Count");
            Assert.AreEqual(OrderExecutionProvider.Simulated, tradeProcessorMap[OrderExecutionProvider.Simulated][new Security() { Symbol = "GOOG" }].ExecutionProvider, "Execution Provider of Trade Processor");
            Assert.AreEqual(OrderExecutionProvider.Blackwood, tradeProcessorMap[OrderExecutionProvider.Blackwood][new Security() { Symbol = "GOOG" }].ExecutionProvider, "Execution Provider of Trade Processor");
        }

        [Test]
        [Category("Unit")]
        public void NewExecutionSimulatedAndBlackwoodMultipleSecurities_CreateNewTradeProcessor_TwoTradeProcessorInExecutionProcessorOneForSimulatedOneForBlackwood()
        {
            // Create new Execution Processor Object
            ExecutionHandler executionHandler = new ExecutionHandler();

            {
                // Create Order Object
                Order order = new Order(OrderExecutionProvider.Simulated);

                // Create Fill Object
                Fill fill = new Fill(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated, "1");
                fill.ExecutionId = "1";
                fill.ExecutionSide = OrderSide.BUY;
                fill.ExecutionSize = 40;
                // Create Execution Object
                Execution execution = new Execution(fill, order);
                // Add new execution to the Execution Processor
                executionHandler.NewExecutionArrived(execution);
            }

            {
                // Create Order Object
                Order order = new Order(OrderExecutionProvider.Simulated);

                // Create Fill Object
                Fill fill = new Fill(new Security() { Symbol = "AAPL" }, OrderExecutionProvider.Simulated, "1");
                fill.ExecutionId = "1";
                fill.ExecutionSide = OrderSide.BUY;
                fill.ExecutionSize = 40;
                // Create Execution Object
                Execution execution = new Execution(fill, order);
                // Add new execution to the Execution Processor
                executionHandler.NewExecutionArrived(execution);
            }

            {
                // Create Order Object
                Order order = new Order(OrderExecutionProvider.Blackwood);

                // Create Fill Object
                Fill fill = new Fill(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Blackwood, "1");
                fill.ExecutionId = "1";
                fill.ExecutionSide = OrderSide.BUY;
                fill.ExecutionSize = 40;
                // Create Execution Object
                Execution execution = new Execution(fill, order);
                // Add new execution to the Execution Processor
                executionHandler.NewExecutionArrived(execution);
            }

            {
                // Create Order Object
                Order order = new Order(OrderExecutionProvider.Blackwood);

                // Create Fill Object
                Fill fill = new Fill(new Security() { Symbol = "AAPL" }, OrderExecutionProvider.Blackwood, "1");
                fill.ExecutionId = "1";
                fill.ExecutionSide = OrderSide.BUY;
                fill.ExecutionSize = 40;
                // Create Execution Object
                Execution execution = new Execution(fill, order);
                // Add new execution to the Execution Processor
                executionHandler.NewExecutionArrived(execution);
            }

            var tradeProcessorMap = executionHandler.TradeProcessorMap;

            Assert.AreEqual(2, tradeProcessorMap.Count, "Trade Processor List Count");
            Assert.AreEqual(2, tradeProcessorMap[OrderExecutionProvider.Simulated].Count, "Total Active Trade Factories for Simulated");
            Assert.AreEqual(2, tradeProcessorMap[OrderExecutionProvider.Blackwood].Count, "Total Active Trade Factories for Simulated");
            Assert.AreEqual(OrderExecutionProvider.Simulated, tradeProcessorMap[OrderExecutionProvider.Simulated][new Security() { Symbol = "GOOG" }].ExecutionProvider, "Execution Provider of Trade Processor");
            Assert.AreEqual(OrderExecutionProvider.Simulated, tradeProcessorMap[OrderExecutionProvider.Simulated][new Security() { Symbol = "AAPL" }].ExecutionProvider, "Execution Provider of Trade Processor");
            Assert.AreEqual(OrderExecutionProvider.Blackwood, tradeProcessorMap[OrderExecutionProvider.Blackwood][new Security() { Symbol = "GOOG" }].ExecutionProvider, "Execution Provider of Trade Processor");
            Assert.AreEqual(OrderExecutionProvider.Blackwood, tradeProcessorMap[OrderExecutionProvider.Blackwood][new Security() { Symbol = "AAPL" }].ExecutionProvider, "Execution Provider of Trade Processor");
        }
    }
}
