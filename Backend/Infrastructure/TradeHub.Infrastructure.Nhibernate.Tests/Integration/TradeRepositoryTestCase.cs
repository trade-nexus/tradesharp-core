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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.Repositories;

namespace TradeHub.Infrastructure.Nhibernate.Tests.Integration
{
    [TestFixture]
    public class TradeRepositoryTestCase
    {
        private ITradeRepository _tradeRespository;
        private IApplicationContext ctx;
        private IPersistRepository<object> _repository;

        [SetUp]
        public void Setup()
        {
            ctx = ContextRegistry.GetContext();
            _tradeRespository = ContextRegistry.GetContext()["TradeRepository"] as ITradeRepository;
            _repository = ContextRegistry.GetContext()["PersistRepository"] as IPersistRepository<object>;
        }

        [TearDown]
        public void TearDown()
        {

        }

        [Test]
        [Category("Integration")]
        public void TradeCruld()
        {
            bool saved = false;
            string id = DateTime.Now.ToString();
            Trade trade = new Trade(TradeSide.Buy, 20, 113, OrderExecutionProvider.Simulated, "A00", new Security(){Symbol = "GOOG"}, new DateTime(2015,01,21,18,20,57));

            Dictionary<string, int> executionDetails = new Dictionary<string, int>();
            executionDetails.Add("A00", 20);
            executionDetails.Add("A01", -20);

            trade.ExecutionDetails = executionDetails;

            var tradeSaved = new ManualResetEvent(false);

            //get Trades
            IList<Trade> getTrade = _tradeRespository.ListAll();
            int initialTradeCount = getTrade.Count;

            //add Trade to database
            _tradeRespository.AddUpdate(trade);

            Thread.Sleep(2000);

            //get Trades
            getTrade = _tradeRespository.ListAll();
            if (getTrade.Count.Equals(initialTradeCount + 1))
            {
                saved = true;
                tradeSaved.Set();
            }

            Assert.IsTrue(getTrade.Last().ExecutionDetails["A00"].Equals(20), "Matching Trade");
            Assert.IsTrue(getTrade.Last().Security.Symbol.Equals("GOOG"), "Matching Trade Symbol");

            tradeSaved.WaitOne(30000);
            //delete the order
            _tradeRespository.Delete(getTrade.Last());

            //get ther order again to verify its deleted or not
            getTrade = _tradeRespository.ListAll();
            Assert.AreEqual(getTrade.Count, initialTradeCount, "Trade Count after Delete");
        }

        [Test]
        [Category("Integration")]
        public void FilterByExecutionProviderRequest_RetrieveResults_Successful_DeleteAddedEntries()
        {
            // Contains Trades which are created in the Test Case
            IList<Trade> localTradesList= new List<Trade>();

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Buy, 20, 113, "FilterTestCase", "B00", new Security() { Symbol = "GOOG" }, new DateTime(2015, 01, 21, 18, 20, 57));

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("B00", 20);
                executionDetails.Add("B01", -20);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                _tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Buy, 20, 113, OrderExecutionProvider.Simulated, "B02", new Security() { Symbol = "GOOG" }, new DateTime(2015, 01, 21, 18, 20, 57));

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("B02", 20);
                executionDetails.Add("B03", -20);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                _tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Buy, 20, 113, "FilterTestCase", "B04", new Security() { Symbol = "GOOG" }, new DateTime(2015, 01, 21, 18, 20, 57));

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("B04", 20);
                executionDetails.Add("B05", -20);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                _tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Wait for Transactions to complete
            Thread.Sleep(1000);

            //get the Trades filtered by Execution Provider
            IList<Trade> tradesReturned = _tradeRespository.FilterByExecutionProvider("FilterTestCase");

            Assert.IsNotNull(tradesReturned, "Values Retrieved");
            Assert.AreEqual(localTradesList.Count - 1, tradesReturned.Count, "Values Retrieved");

            Assert.IsTrue(tradesReturned[0].ExecutionDetails["B00"].Equals(localTradesList[0].ExecutionDetails["B00"]), "Matching 1st Trade 1st Execution");
            Assert.IsTrue(tradesReturned[0].ExecutionDetails["B01"].Equals(localTradesList[0].ExecutionDetails["B01"]), "Matching 1st Trade 2st Execution");

            Assert.IsTrue(tradesReturned[1].ExecutionDetails["B04"].Equals(localTradesList[2].ExecutionDetails["B04"]), "Matching 2nd Trade 1st Execution");
            Assert.IsTrue(tradesReturned[1].ExecutionDetails["B05"].Equals(localTradesList[2].ExecutionDetails["B05"]), "Matching 2nd Trade 2st Execution");

            // Delete Trades Generated for Test Case
            foreach (Trade trade in localTradesList)
            {
                _tradeRespository.Delete(trade);
            }
        }

        [Test]
        [Category("Integration")]
        public void FilterByTradeSideRequest_RetrieveResults_Successful_DeleteAddedEntries()
        {
            // Contains Trades which are created in the Test Case
            IList<Trade> localTradesList = new List<Trade>();

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Buy, 113, 20, "FilterTestCase", "C00", new Security() { Symbol = "GOOG" }, new DateTime(2015, 01, 21, 18, 20, 57));

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("C00", 20);
                executionDetails.Add("C01", -20);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                _tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Sell, 20, 113, "FilterTestCase", "C02", new Security() { Symbol = "GOOG" }, new DateTime(2015, 01, 21, 18, 20, 57));

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("C02", -20);
                executionDetails.Add("C03", 20);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                _tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Buy, 20, 113, "FilterTestCase", "C04", new Security() { Symbol = "GOOG" }, new DateTime(2015, 01, 21, 18, 20, 57));

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("C04", 20);
                executionDetails.Add("C05", -20);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                _tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Wait for Transactions to complete
            Thread.Sleep(1000);

            //get the Trades filtered by Execution Provider
            IList<Trade> tradesReturned = _tradeRespository.FilterByTradeSide(TradeSide.Buy);

            Assert.IsNotNull(tradesReturned, "Values Retrieved");
            Assert.AreEqual(localTradesList.Count - 1, tradesReturned.Count, "Values Retrieved");

            Assert.IsTrue(tradesReturned[0].ExecutionDetails["C00"].Equals(localTradesList[0].ExecutionDetails["C00"]), "Matching 1st Trade 1st Execution");
            Assert.IsTrue(tradesReturned[0].ExecutionDetails["C01"].Equals(localTradesList[0].ExecutionDetails["C01"]), "Matching 1st Trade 2st Execution");

            Assert.IsTrue(tradesReturned[1].ExecutionDetails["C04"].Equals(localTradesList[2].ExecutionDetails["C04"]), "Matching 2nd Trade 1st Execution");
            Assert.IsTrue(tradesReturned[1].ExecutionDetails["C05"].Equals(localTradesList[2].ExecutionDetails["C05"]), "Matching 2nd Trade 2st Execution");

            // Delete Trades Generated for Test Case
            foreach (Trade trade in localTradesList)
            {
                _tradeRespository.Delete(trade);
            }
        }

        [Test]
        [Category("Integration")]
        public void FilterBySecurityRequest_RetrieveResults_Successful_DeleteAddedEntries()
        {
            // Contains Trades which are created in the Test Case
            IList<Trade> localTradesList = new List<Trade>();

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Buy, 20, 113, "FilterTestCase", "D00", new Security() { Symbol = "TestSymbol" }, new DateTime(2015, 01, 21, 18, 20, 57));

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("D00", 20);
                executionDetails.Add("D01", -20);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                _tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Sell, 20, 113, "FilterTestCase", "D02", new Security() { Symbol = "GOOG" }, new DateTime(2015, 01, 21, 18, 20, 57));

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("D02", -20);
                executionDetails.Add("D03", 20);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                _tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Buy, 20, 113, "FilterTestCase", "D04", new Security() { Symbol = "TestSymbol" }, new DateTime(2015, 01, 21, 18, 20, 57));

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("D04", 20);
                executionDetails.Add("D05", -20);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                _tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Wait for Transactions to complete
            Thread.Sleep(1000);

            //get the Trades filtered by Execution Provider
            IList<Trade> tradesReturned = _tradeRespository.FilterBySecurity(new Security() { Symbol = "TestSymbol" });

            Assert.IsNotNull(tradesReturned, "Values Retrieved");
            Assert.AreEqual(localTradesList.Count - 1, tradesReturned.Count, "Values Retrieved");

            Assert.IsTrue(tradesReturned[0].ExecutionDetails["D00"].Equals(localTradesList[0].ExecutionDetails["D00"]), "Matching 1st Trade 1st Execution");
            Assert.IsTrue(tradesReturned[0].ExecutionDetails["D01"].Equals(localTradesList[0].ExecutionDetails["D01"]), "Matching 1st Trade 2st Execution");

            Assert.IsTrue(tradesReturned[1].ExecutionDetails["D04"].Equals(localTradesList[2].ExecutionDetails["D04"]), "Matching 2nd Trade 1st Execution");
            Assert.IsTrue(tradesReturned[1].ExecutionDetails["D05"].Equals(localTradesList[2].ExecutionDetails["D05"]), "Matching 2nd Trade 2st Execution");

            // Delete Trades Generated for Test Case
            foreach (Trade trade in localTradesList)
            {
                _tradeRespository.Delete(trade);
            }
        }
    }
}
