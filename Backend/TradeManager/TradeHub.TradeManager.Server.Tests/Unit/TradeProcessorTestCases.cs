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
using TradeHub.TradeManager.Server.Domain;

namespace TradeHub.TradeManager.Server.Tests.Unit
{
    [TestFixture]
    public class TradeProcessorTestCases
    {
        [SetUp]
        public void Setup()
        {
            
        }

        [TearDown]
        public void TearDown()
        {
            
        }

        [Test]
        [Category("Unit")]
        public void NewExecutionBuySide_OpenNewTrade_OverallPositionIsPositive()
        {
            // Create Trade Processor
            TradeProcessor tradeProcessor = new TradeProcessor(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated);

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
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(execution);
            }

            // Get Trades Map
            var tradeMap = tradeProcessor.OpenTrades;

            // Get Open Trade
            Trade trade = tradeMap.Values.First();

            Assert.AreEqual(40, tradeProcessor.Position, "Overall Position");
            Assert.AreEqual(TradeSide.Buy, trade.TradeSide, "Trade Side");
            Assert.AreEqual(40, trade.TradeSize, "Trade Size");
            Assert.AreEqual(false, trade.IsComplete(), "Trade.IsComplete");
        }

        [Test]
        [Category("Unit")]
        public void NewExecutionBuySide_UpdateTrade_OverallPositionIsPositive()
        {
            // Create Trade Processor
            TradeProcessor tradeProcessor = new TradeProcessor(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated);

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
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(execution);
            }

            {
                // Create Fill Object
                Fill fillTwo = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fillTwo.ExecutionId = "2";
                fillTwo.ExecutionSide = OrderSide.SELL;
                fillTwo.ExecutionSize = 20;
                // Create Execution Object
                Execution executionTwo = new Execution(fillTwo, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionTwo);
            }

            // Get Trades Map
            var tradeMap = tradeProcessor.OpenTrades;

            // Get Open Trade
            Trade trade = tradeMap.Values.First();

            Assert.AreEqual(20, tradeProcessor.Position, "Overall Position");
            Assert.AreEqual(TradeSide.Buy, trade.TradeSide, "Trade Side");
            Assert.AreEqual(-20, trade.ExecutionDetails["2"], "Execution Size");
            Assert.AreEqual(false, trade.IsComplete(), "Trade.IsComplete");
        }

        [Test]
        [Category("Unit")]
        public void NewExecutionBuySide_UpdateTrade_OverallPositionBalanced()
        {
            // Create Trade Processor
            TradeProcessor tradeProcessor = new TradeProcessor(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated);

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
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(execution);
            }

            {
                // Create Fill Object
                Fill fillTwo = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fillTwo.ExecutionId = "2";
                fillTwo.ExecutionSide = OrderSide.SELL;
                fillTwo.ExecutionSize = 20;
                // Create Execution Object
                Execution executionTwo = new Execution(fillTwo, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionTwo);
            }

            {
                // Create Fill Object
                Fill fillThree = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fillThree.ExecutionId = "3";
                fillThree.ExecutionSide = OrderSide.SELL;
                fillThree.ExecutionSize = 20;
                // Create Execution Object
                Execution executionThree = new Execution(fillThree, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionThree);
            }

            // Get Trades Map
            var tradeMapCount = tradeProcessor.OpenTrades.Count;

            Assert.AreEqual(0, tradeProcessor.Position, "Overall Position");
            Assert.AreEqual(0, tradeMapCount, "Trade MAP Count");
        }

        [Test]
        [Category("Unit")]
        public void NewExecutionSellSide_OpenNewTrade_OverallPositionIsNegative()
        {
            // Create Trade Processor
            TradeProcessor tradeProcessor = new TradeProcessor(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated);

            // Create Order Object
            Order order = new Order(OrderExecutionProvider.Simulated);

            {
                // Create Fill Object
                Fill fill = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fill.ExecutionId = "1";
                fill.ExecutionSide = OrderSide.SELL;
                fill.ExecutionSize = 40;
                // Create Execution Object
                Execution execution = new Execution(fill, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(execution);
            }

            // Get Trades Map
            var tradeMap = tradeProcessor.OpenTrades;

            // Get Open Trade
            Trade trade = tradeMap.Values.First();

            Assert.AreEqual(-40, tradeProcessor.Position, "Overall Position");
            Assert.AreEqual(TradeSide.Sell, trade.TradeSide, "Trade Side");
            Assert.AreEqual(-40, trade.TradeSize, "Trade Size");
            Assert.AreEqual(false, trade.IsComplete(), "Trade.IsComplete");
        }

        [Test]
        [Category("Unit")]
        public void NewExecutionSellSide_UpdateTrade_OverallPositionIsNegative()
        {
            // Create Trade Processor
            TradeProcessor tradeProcessor = new TradeProcessor(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated);

            // Create Order Object
            Order order = new Order(OrderExecutionProvider.Simulated);

            {
                // Create Fill Object
                Fill fill = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fill.ExecutionId = "1";
                fill.ExecutionSide = OrderSide.SELL;
                fill.ExecutionSize = 40;
                // Create Execution Object
                Execution execution = new Execution(fill, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(execution);
            }

            {
                // Create Fill Object
                Fill fillTwo = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fillTwo.ExecutionId = "2";
                fillTwo.ExecutionSide = OrderSide.BUY;
                fillTwo.ExecutionSize = 20;
                // Create Execution Object
                Execution executionTwo = new Execution(fillTwo, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionTwo);
            }

            // Get Trades Map
            var tradeMap = tradeProcessor.OpenTrades;

            // Get Open Trade
            Trade trade = tradeMap.Values.First();

            Assert.AreEqual(-20, tradeProcessor.Position, "Overall Position");
            Assert.AreEqual(TradeSide.Sell, trade.TradeSide, "Trade Side");
            Assert.AreEqual(20, trade.ExecutionDetails["2"], "Execution Size");
            Assert.AreEqual(false, trade.IsComplete(), "Trade.IsComplete");
        }

        [Test]
        [Category("Unit")]
        public void NewExecutionSellSide_UpdateTrade_OverallPositionBalanced()
        {
            // Create Trade Processor
            TradeProcessor tradeProcessor = new TradeProcessor(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated);

            // Create Order Object
            Order order = new Order(OrderExecutionProvider.Simulated);

            {
                // Create Fill Object
                Fill fill = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fill.ExecutionId = "1";
                fill.ExecutionSide = OrderSide.SELL;
                fill.ExecutionSize = 40;
                // Create Execution Object
                Execution execution = new Execution(fill, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(execution);
            }

            {
                // Create Fill Object
                Fill fillTwo = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fillTwo.ExecutionId = "2";
                fillTwo.ExecutionSide = OrderSide.BUY;
                fillTwo.ExecutionSize = 20;
                // Create Execution Object
                Execution executionTwo = new Execution(fillTwo, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionTwo);
            }

            {
                // Create Fill Object
                Fill fillThree = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fillThree.ExecutionId = "3";
                fillThree.ExecutionSide = OrderSide.BUY;
                fillThree.ExecutionSize = 20;
                // Create Execution Object
                Execution executionThree = new Execution(fillThree, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionThree);
            }

            // Get Trades Map
            var tradeMapCount = tradeProcessor.OpenTrades.Count;

            Assert.AreEqual(0, tradeProcessor.Position, "Overall Position");
            Assert.AreEqual(0, tradeMapCount, "Trade MAP Count");
        }

        [Test]
        [Category("Unit")]
        public void NewExecutionBuySideMultiple_OpenNewTradeMultiple_OverallPositionIsPositive()
        {
            // Create Trade Processor
            TradeProcessor tradeProcessor = new TradeProcessor(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated);

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
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(execution);
            }

            {
                // Create Fill Object
                Fill fillTwo = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fillTwo.ExecutionId = "2";
                fillTwo.ExecutionSide = OrderSide.BUY;
                fillTwo.ExecutionSize = 40;
                // Create Execution Object
                Execution executionTwo = new Execution(fillTwo, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionTwo);
            }

            // Get Trades Map
            var tradeMap = tradeProcessor.OpenTrades;

            // Open Trade Count
            var openTradeCount = tradeMap.Count;

            // Get Open Trade Size
            int tradeOneSize = tradeMap[1].TradeSize;
            int tradeTwoSize = tradeMap[2].TradeSize;

            Assert.AreEqual(80, tradeProcessor.Position, "Overall Position");
            Assert.AreEqual(2, openTradeCount, "Open Trade Count");

            Assert.AreEqual(TradeSide.Buy, tradeMap[1].TradeSide, "Trade One Side");
            Assert.AreEqual(TradeSide.Buy, tradeMap[2].TradeSide, "Trade Two Side");
            
            Assert.AreEqual(40, tradeOneSize, "Trade One Size");
            Assert.AreEqual(40, tradeTwoSize, "Trade Two Size");
            
            Assert.AreEqual(false, tradeMap[1].IsComplete(), "Trade One Completion Status");
            Assert.AreEqual(false, tradeMap[2].IsComplete(), "Trade Two Completion Status");
        }

        [Test]
        [Category("Unit")]
        public void NewExecutionBuySideMultiple_OpenNewTradeMultipleAndUpdateTrade_OverallPositionIsPositiveWithSingleOpenOrder()
        {
            // Create Trade Processor
            TradeProcessor tradeProcessor = new TradeProcessor(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated);

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
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(execution);
            }

            {
                // Create Fill Object
                Fill fillTwo = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fillTwo.ExecutionId = "2";
                fillTwo.ExecutionSide = OrderSide.BUY;
                fillTwo.ExecutionSize = 50;
                // Create Execution Object
                Execution executionTwo = new Execution(fillTwo, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionTwo);
            }

            {
                // Create Fill Object
                Fill fillThree = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fillThree.ExecutionId = "3";
                fillThree.ExecutionSide = OrderSide.SELL;
                fillThree.ExecutionSize = 40;
                // Create Execution Object
                Execution executionThree = new Execution(fillThree, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionThree);
            }

            // Get Trades Map
            var tradeMap = tradeProcessor.OpenTrades;

            // Open Trade Count
            var openTradeCount = tradeMap.Count;

            // Get Open Trade Size
            int tradeTwoSize = tradeMap[2].TradeSize;

            Assert.AreEqual(50, tradeProcessor.Position, "Overall Position");
            Assert.AreEqual(1, openTradeCount, "Open Trade Count");
            Assert.AreEqual(TradeSide.Buy, tradeMap[2].TradeSide, "Trade Two Side");
            Assert.AreEqual(50, tradeTwoSize, "Trade Two Size");
            Assert.AreEqual(false, tradeMap[2].IsComplete(), "Trade Two Completion Status");
        }

        [Test]
        [Category("Unit")]
        public void NewExecutionBuySideMultiple_OpenNewTradeMultipleAndUpdateTrade_OverallPositionIsPositiveWithSingleOpenOrder_ExtraExecutionSize()
        {
            // Create Trade Processor
            TradeProcessor tradeProcessor = new TradeProcessor(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated);

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
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(execution);
            }

            {
                // Create Fill Object
                Fill fillTwo = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fillTwo.ExecutionId = "2";
                fillTwo.ExecutionSide = OrderSide.BUY;
                fillTwo.ExecutionSize = 50;
                // Create Execution Object
                Execution executionTwo = new Execution(fillTwo, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionTwo);
            }

            {
                // Create Fill Object
                Fill fillThree = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fillThree.ExecutionId = "3";
                fillThree.ExecutionSide = OrderSide.SELL;
                fillThree.ExecutionSize = 45;
                // Create Execution Object
                Execution executionThree = new Execution(fillThree, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionThree);
            }

            // Get Trades Map
            var tradeMap = tradeProcessor.OpenTrades;

            // Open Trade Count
            var openTradeCount = tradeMap.Count;

            // Get Open Trade Size
            int tradeTwoSize = tradeMap[2].TradeSize;

            Assert.AreEqual(45, tradeProcessor.Position, "Overall Position");
            Assert.AreEqual(1, openTradeCount, "Open Trade Count");
            Assert.AreEqual(TradeSide.Buy, tradeMap[2].TradeSide, "Trade Two Side");
            Assert.AreEqual(50, tradeTwoSize, "Trade Two Size");
            Assert.AreEqual(-5, tradeMap[2].ExecutionDetails["3"], "Trade Two Execution Size for closing order");
            Assert.AreEqual(false, tradeMap[2].IsComplete(), "Trade Two Completion Status");
        }

        [Test]
        [Category("Unit")]
        public void NewExecutionBuySideMultiple_OpenNewTradeMultipleAndUpdateTrade_OverallPositionIsBalanced()
        {
            // Create Trade Processor
            TradeProcessor tradeProcessor = new TradeProcessor(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated);

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
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(execution);
            }

            {
                // Create Fill Object
                Fill fillTwo = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fillTwo.ExecutionId = "2";
                fillTwo.ExecutionSide = OrderSide.BUY;
                fillTwo.ExecutionSize = 50;
                // Create Execution Object
                Execution executionTwo = new Execution(fillTwo, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionTwo);
            }

            {
                // Create Fill Object
                Fill fillThree = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fillThree.ExecutionId = "3";
                fillThree.ExecutionSide = OrderSide.SELL;
                fillThree.ExecutionSize = 90;
                // Create Execution Object
                Execution executionThree = new Execution(fillThree, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionThree);
            }

            // Get Trades Map
            var tradeMap = tradeProcessor.OpenTrades;

            // Open Trade Count
            var openTradeCount = tradeMap.Count;

            Assert.AreEqual(0, tradeProcessor.Position, "Overall Position");
            Assert.AreEqual(0, openTradeCount, "Open Trade Count");
        }

        [Test]
        [Category("Unit")]
        public void NewExecutionSellSideMultiple_OpenNewTradeMultiple_OverallPositionIsNegative()
        {
            // Create Trade Processor
            TradeProcessor tradeProcessor = new TradeProcessor(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated);

            // Create Order Object
            Order order = new Order(OrderExecutionProvider.Simulated);

            {
                // Create Fill Object
                Fill fill = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fill.ExecutionId = "1";
                fill.ExecutionSide = OrderSide.SELL;
                fill.ExecutionSize = 40;
                // Create Execution Object
                Execution execution = new Execution(fill, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(execution);
            }

            {
                // Create Fill Object
                Fill fillTwo = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fillTwo.ExecutionId = "2";
                fillTwo.ExecutionSide = OrderSide.SELL;
                fillTwo.ExecutionSize = 40;
                // Create Execution Object
                Execution executionTwo = new Execution(fillTwo, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionTwo);
            }

            // Get Trades Map
            var tradeMap = tradeProcessor.OpenTrades;

            // Open Trade Count
            var openTradeCount = tradeMap.Count;

            // Get Open Trade Size
            int tradeOneSize = tradeMap[1].TradeSize;
            int tradeTwoSize = tradeMap[2].TradeSize;

            Assert.AreEqual(-80, tradeProcessor.Position, "Overall Position");
            Assert.AreEqual(2, openTradeCount, "Open Trade Count");

            Assert.AreEqual(TradeSide.Sell, tradeMap[1].TradeSide, "Trade One Side");
            Assert.AreEqual(TradeSide.Sell, tradeMap[2].TradeSide, "Trade Two Side");

            Assert.AreEqual(-40, tradeOneSize, "Trade One Size");
            Assert.AreEqual(-40, tradeTwoSize, "Trade Two Size");

            Assert.AreEqual(false, tradeMap[1].IsComplete(), "Trade One Completion Status");
            Assert.AreEqual(false, tradeMap[2].IsComplete(), "Trade Two Completion Status");
        }

        [Test]
        [Category("Unit")]
        public void NewExecutionSellSideMultiple_OpenNewTradeMultipleAndUpdateTrade_OverallPositionIsNegativeWithSingleOpenOrder()
        {
            // Create Trade Processor
            TradeProcessor tradeProcessor = new TradeProcessor(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated);

            // Create Order Object
            Order order = new Order(OrderExecutionProvider.Simulated);

            {
                // Create Fill Object
                Fill fill = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fill.ExecutionId = "1";
                fill.ExecutionSide = OrderSide.SELL;
                fill.ExecutionSize = 40;
                // Create Execution Object
                Execution execution = new Execution(fill, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(execution);
            }

            {
                // Create Fill Object
                Fill fillTwo = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fillTwo.ExecutionId = "2";
                fillTwo.ExecutionSide = OrderSide.SELL;
                fillTwo.ExecutionSize = 50;
                // Create Execution Object
                Execution executionTwo = new Execution(fillTwo, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionTwo);
            }

            {
                // Create Fill Object
                Fill fillThree = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fillThree.ExecutionId = "3";
                fillThree.ExecutionSide = OrderSide.BUY;
                fillThree.ExecutionSize = 40;
                // Create Execution Object
                Execution executionThree = new Execution(fillThree, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionThree);
            }

            // Get Trades Map
            var tradeMap = tradeProcessor.OpenTrades;

            // Open Trade Count
            var openTradeCount = tradeMap.Count;

            // Get Open Trade Size
            int tradeTwoSize = tradeMap[2].TradeSize;

            Assert.AreEqual(-50, tradeProcessor.Position, "Overall Position");
            Assert.AreEqual(1, openTradeCount, "Open Trade Count");
            Assert.AreEqual(TradeSide.Sell, tradeMap[2].TradeSide, "Trade Two Side");
            Assert.AreEqual(-50, tradeTwoSize, "Trade Two Size");
            Assert.AreEqual(false, tradeMap[2].IsComplete(), "Trade Two Completion Status");
        }

        [Test]
        [Category("Unit")]
        public void NewExecutionSellSideMultiple_OpenNewTradeMultipleAndUpdateTrade_OverallPositionIsNegativeWithSingleOpenOrder_ExtraExecutionSize()
        {
            // Create Trade Processor
            TradeProcessor tradeProcessor = new TradeProcessor(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated);

            // Create Order Object
            Order order = new Order(OrderExecutionProvider.Simulated);

            {
                // Create Fill Object
                Fill fill = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fill.ExecutionId = "1";
                fill.ExecutionSide = OrderSide.SELL;
                fill.ExecutionSize = 40;
                // Create Execution Object
                Execution execution = new Execution(fill, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(execution);
            }

            {
                // Create Fill Object
                Fill fillTwo = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fillTwo.ExecutionId = "2";
                fillTwo.ExecutionSide = OrderSide.SELL;
                fillTwo.ExecutionSize = 50;
                // Create Execution Object
                Execution executionTwo = new Execution(fillTwo, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionTwo);
            }

            {
                // Create Fill Object
                Fill fillThree = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fillThree.ExecutionId = "3";
                fillThree.ExecutionSide = OrderSide.BUY;
                fillThree.ExecutionSize = 45;
                // Create Execution Object
                Execution executionThree = new Execution(fillThree, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionThree);
            }

            // Get Trades Map
            var tradeMap = tradeProcessor.OpenTrades;

            // Open Trade Count
            var openTradeCount = tradeMap.Count;

            // Get Open Trade Size
            int tradeTwoSize = tradeMap[2].TradeSize;

            Assert.AreEqual(-45, tradeProcessor.Position, "Overall Position");
            Assert.AreEqual(1, openTradeCount, "Open Trade Count");
            Assert.AreEqual(TradeSide.Sell, tradeMap[2].TradeSide, "Trade Two Side");
            Assert.AreEqual(-50, tradeTwoSize, "Trade Two Size");
            Assert.AreEqual(5, tradeMap[2].ExecutionDetails["3"], "Trade Two Execution Size for closing order");
            Assert.AreEqual(false, tradeMap[2].IsComplete(), "Trade Two Completion Status");
        }

        [Test]
        [Category("Unit")]
        public void NewExecutionSellSideMultiple_OpenNewTradeMultipleAndUpdateTrade_OverallPositionIsBalanced()
        {
            // Create Trade Processor
            TradeProcessor tradeProcessor = new TradeProcessor(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated);

            // Create Order Object
            Order order = new Order(OrderExecutionProvider.Simulated);

            {
                // Create Fill Object
                Fill fill = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fill.ExecutionId = "1";
                fill.ExecutionSide = OrderSide.SELL;
                fill.ExecutionSize = 40;
                // Create Execution Object
                Execution execution = new Execution(fill, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(execution);
            }

            {
                // Create Fill Object
                Fill fillTwo = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fillTwo.ExecutionId = "2";
                fillTwo.ExecutionSide = OrderSide.SELL;
                fillTwo.ExecutionSize = 50;
                // Create Execution Object
                Execution executionTwo = new Execution(fillTwo, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionTwo);
            }

            {
                // Create Fill Object
                Fill fillThree = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fillThree.ExecutionId = "3";
                fillThree.ExecutionSide = OrderSide.BUY;
                fillThree.ExecutionSize = 90;
                // Create Execution Object
                Execution executionThree = new Execution(fillThree, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionThree);
            }

            // Get Trades Map
            var tradeMap = tradeProcessor.OpenTrades;

            // Open Trade Count
            var openTradeCount = tradeMap.Count;

            Assert.AreEqual(0, tradeProcessor.Position, "Overall Position");
            Assert.AreEqual(0, openTradeCount, "Open Trade Count");
        }

        [Test]
        [Category("Unit")]
        public void NewExecutionBuySideMultiple_OpenNewTradeMultipleAndUpdateTrade_OverallPositionIsNegativeWithSingleOpenOrderOnSellSide()
        {
            // Create Trade Processor
            TradeProcessor tradeProcessor = new TradeProcessor(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated);

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
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(execution);
            }

            {
                // Create Fill Object
                Fill fillTwo = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fillTwo.ExecutionId = "2";
                fillTwo.ExecutionSide = OrderSide.BUY;
                fillTwo.ExecutionSize = 30;
                // Create Execution Object
                Execution executionTwo = new Execution(fillTwo, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionTwo);
            }

            {
                // Create Fill Object
                Fill fillThree = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fillThree.ExecutionId = "3";
                fillThree.ExecutionSide = OrderSide.SELL;
                fillThree.ExecutionSize = 80;
                // Create Execution Object
                Execution executionThree = new Execution(fillThree, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionThree);
            }

            // Get Trades Map
            var tradeMap = tradeProcessor.OpenTrades;

            // Open Trade Count
            var openTradeCount = tradeMap.Count;

            // Get Open Trade Size
            int tradeThreeSize = tradeMap[3].TradeSize;

            Assert.AreEqual(-10, tradeProcessor.Position, "Overall Position");
            Assert.AreEqual(1, openTradeCount, "Open Trade Count");
            Assert.AreEqual(TradeSide.Sell, tradeMap[3].TradeSide, "Trade Three Side");
            Assert.AreEqual(-10, tradeThreeSize, "Trade Three Size");
            Assert.AreEqual(false, tradeMap[3].IsComplete(), "Trade Three Completion Status");
        }

        [Test]
        [Category("Unit")]
        public void NewExecutionBuySideMultiple_OpenNewTradeMultipleAndUpdateTrade_OverallPositionGoesFromPositiveToNegativeThenBalanced()
        {
            // Create Trade Processor
            TradeProcessor tradeProcessor = new TradeProcessor(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated);

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
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(execution);
            }

            {
                // Create Fill Object
                Fill fillTwo = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fillTwo.ExecutionId = "2";
                fillTwo.ExecutionSide = OrderSide.BUY;
                fillTwo.ExecutionSize = 30;
                // Create Execution Object
                Execution executionTwo = new Execution(fillTwo, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionTwo);
            }

            {
                // Create Fill Object
                Fill fillThree = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fillThree.ExecutionId = "3";
                fillThree.ExecutionSide = OrderSide.SELL;
                fillThree.ExecutionSize = 80;
                // Create Execution Object
                Execution executionThree = new Execution(fillThree, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionThree);
            }

            {
                // Create Fill Object
                Fill fillFour = new Fill(new Security() {Symbol = "GOOG"}, OrderExecutionProvider.Simulated, "1");
                fillFour.ExecutionId = "4";
                fillFour.ExecutionSide = OrderSide.BUY;
                fillFour.ExecutionSize = 10;
                // Create Execution Object
                Execution executionFour = new Execution(fillFour, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionFour);
            }

            // Get Trades Map
            var tradeMap = tradeProcessor.OpenTrades;

            // Open Trade Count
            var openTradeCount = tradeMap.Count;
            
            Assert.AreEqual(0, tradeProcessor.Position, "Overall Position");
            Assert.AreEqual(0, openTradeCount, "Open Trade Count");
        }

        [Test]
        [Category("Unit")]
        public void NewExecutionSellSideMultiple_OpenNewTradeMultipleAndUpdateTrade_OverallPositionIsPositiveWithSingleOpenOrderOnBuySide()
        {
            // Create Trade Processor
            TradeProcessor tradeProcessor = new TradeProcessor(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated);

            // Create Order Object
            Order order = new Order(OrderExecutionProvider.Simulated);

            {
                // Create Fill Object
                Fill fill = new Fill(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated, "1");
                fill.ExecutionId = "1";
                fill.ExecutionSide = OrderSide.SELL;
                fill.ExecutionSize = 40;
                // Create Execution Object
                Execution execution = new Execution(fill, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(execution);
            }

            {
                // Create Fill Object
                Fill fillTwo = new Fill(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated, "1");
                fillTwo.ExecutionId = "2";
                fillTwo.ExecutionSide = OrderSide.SELL;
                fillTwo.ExecutionSize = 30;
                // Create Execution Object
                Execution executionTwo = new Execution(fillTwo, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionTwo);
            }

            {
                // Create Fill Object
                Fill fillThree = new Fill(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated, "1");
                fillThree.ExecutionId = "3";
                fillThree.ExecutionSide = OrderSide.BUY;
                fillThree.ExecutionSize = 80;
                // Create Execution Object
                Execution executionThree = new Execution(fillThree, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionThree);
            }

            // Get Trades Map
            var tradeMap = tradeProcessor.OpenTrades;

            // Open Trade Count
            var openTradeCount = tradeMap.Count;

            // Get Open Trade Size
            int tradeThreeSize = tradeMap[3].TradeSize;

            Assert.AreEqual(10, tradeProcessor.Position, "Overall Position");
            Assert.AreEqual(1, openTradeCount, "Open Trade Count");
            Assert.AreEqual(TradeSide.Buy, tradeMap[3].TradeSide, "Trade Three Side");
            Assert.AreEqual(10, tradeThreeSize, "Trade Three Size");
            Assert.AreEqual(false, tradeMap[3].IsComplete(), "Trade Three Completion Status");
        }

        [Test]
        [Category("Unit")]
        public void NewExecutionSellSideMultiple_OpenNewTradeMultipleAndUpdateTrade_OverallPositionGoesFromNegativeToPositiveThenBalanced()
        {
            // Create Trade Processor
            TradeProcessor tradeProcessor = new TradeProcessor(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated);

            // Create Order Object
            Order order = new Order(OrderExecutionProvider.Simulated);

            {
                // Create Fill Object
                Fill fill = new Fill(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated, "1");
                fill.ExecutionId = "1";
                fill.ExecutionSide = OrderSide.SELL;
                fill.ExecutionSize = 40;
                // Create Execution Object
                Execution execution = new Execution(fill, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(execution);
            }

            {
                // Create Fill Object
                Fill fillTwo = new Fill(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated, "1");
                fillTwo.ExecutionId = "2";
                fillTwo.ExecutionSide = OrderSide.SELL;
                fillTwo.ExecutionSize = 30;
                // Create Execution Object
                Execution executionTwo = new Execution(fillTwo, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionTwo);
            }

            {
                // Create Fill Object
                Fill fillThree = new Fill(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated, "1");
                fillThree.ExecutionId = "3";
                fillThree.ExecutionSide = OrderSide.BUY;
                fillThree.ExecutionSize = 80;
                // Create Execution Object
                Execution executionThree = new Execution(fillThree, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionThree);
            }

            {
                // Create Fill Object
                Fill fillFour = new Fill(new Security() { Symbol = "GOOG" }, OrderExecutionProvider.Simulated, "1");
                fillFour.ExecutionId = "4";
                fillFour.ExecutionSide = OrderSide.SELL;
                fillFour.ExecutionSize = 10;
                // Create Execution Object
                Execution executionFour = new Execution(fillFour, order);
                // Update Trade Processor
                tradeProcessor.NewExecutionArrived(executionFour);
            }

            // Get Trades Map
            var tradeMap = tradeProcessor.OpenTrades;

            // Open Trade Count
            var openTradeCount = tradeMap.Count;

            Assert.AreEqual(0, tradeProcessor.Position, "Overall Position");
            Assert.AreEqual(0, openTradeCount, "Open Trade Count");
        }
    }
}
