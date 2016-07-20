using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Spring.Context.Support;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.Repositories;
using TradeHub.Common.Core.Repositories.Parameters;

namespace TradeHub.ReportingEngine.ProfitLossReporter.Tests.Integration
{
    [TestFixture]
    public class ProfitLossReporterTests
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
        [Category("Integration")]
        public void RequestProfitLossReport_AddTradesToDatabaseForTheIntendedReport_RetrieveResults_Successful_DeleteAddedEntries()
        {
            ITradeRepository tradeRespository = ContextRegistry.GetContext()["TradeRepository"] as ITradeRepository;
            var profitLossReportManager = new ProfitLossReportManager(tradeRespository);

            var dataEventReceived = false;
            var manualDataEvent = new ManualResetEvent(false);

            ProfitLossStats report = null;
            var arguments = new Dictionary<TradeParameters, string>();

            // Contains Trades which are created in the Test Case
            IList<Trade> localTradesList = new List<Trade>();

            #region Add Trades to DB

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Buy, 20, 113, "FilterTestCase", "C00",
                    new Security() {Symbol = "GOOG"}, new DateTime(2015, 01, 21, 18, 20, 57));
                trade.CompletionTime = new DateTime(2015, 01, 21, 18, 20, 58);

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("C00", 20);
                executionDetails.Add("C01", -20);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Sell, 20, 113, "FilterTestCase", "C02",
                    new Security() {Symbol = "GOOG"}, new DateTime(2015, 01, 21, 18, 20, 58));
                trade.CompletionTime = new DateTime(2015, 01, 21, 18, 20, 59);

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("C02", -20);
                executionDetails.Add("C03", 20);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Buy, 20, 113, "FilterTestCase", "C04",
                    new Security() {Symbol = "GOOG"}, new DateTime(2015, 01, 21, 18, 20, 59));
                trade.CompletionTime = new DateTime(2015, 01, 21, 18, 21, 00);

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("C04", 20);
                executionDetails.Add("C05", -20);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Wait for Transactions to complete
            Thread.Sleep(1000);

            #endregion

            // Add Filter Parameters
            arguments.Add(TradeParameters.TradeSide, Convert.ChangeType(TradeSide.Buy, TradeSide.Buy.GetTypeCode()).ToString());
            arguments.Add(TradeParameters.TradeSize, "20");
            arguments.Add(TradeParameters.StartTime, "2015-01-21 18:20:57");
            arguments.Add(TradeParameters.CompletionTime, "2015-01-21 18:21:00");
            arguments.Add(TradeParameters.ExecutionProvider, "FilterTestCase");

            // Hook Data Event
            profitLossReportManager.DataReceived += delegate(ProfitLossStats obj)
            {
                report = obj;
                dataEventReceived = true;
                manualDataEvent.Set();
            };

            // Request Report Data
            profitLossReportManager.RequestReport(arguments);

            manualDataEvent.WaitOne(10000);

            Assert.IsNotNull(report);
            Assert.IsTrue(dataEventReceived, "Report Event");
            Assert.AreEqual(2, report.Trades.Count, "Return Count");

            // Delete Trades Generated for Test Case
            foreach (Trade trade in localTradesList)
            {
                tradeRespository.Delete(trade);
            }
        }

        [Test]
        [Category("Integration")]
        public void RequestProfitLossReportBuySide_AddTradesToDatabaseForTheIntendedReport_RetrieveResults_Successful_DeleteAddedEntries()
        {
            ITradeRepository tradeRespository = ContextRegistry.GetContext()["TradeRepository"] as ITradeRepository;
            var profitLossReportManager = new ProfitLossReportManager(tradeRespository);

            var dataEventReceived = false;
            var manualDataEvent = new ManualResetEvent(false);

            ProfitLossStats report = null;
            var arguments = new Dictionary<TradeParameters, string>();

            // Contains Trades which are created in the Test Case
            IList<Trade> localTradesList = new List<Trade>();

            #region Add Trades to DB

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Buy, 20, 113, "FilterTestCase", "C00",
                    new Security() { Symbol = "GOOG" }, new DateTime(2015, 01, 21, 18, 20, 57));
                trade.CompletionTime = new DateTime(2015, 01, 21, 18, 20, 58);

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("C00", 20);
                executionDetails.Add("C01", -20);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Sell, 20, 113, "FilterTestCase", "C02",
                    new Security() { Symbol = "GOOG" }, new DateTime(2015, 01, 21, 18, 20, 58));
                trade.CompletionTime = new DateTime(2015, 01, 21, 18, 20, 59);

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("C02", -20);
                executionDetails.Add("C03", 20);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Buy, 20, 113, "FilterTestCase", "C04",
                    new Security() { Symbol = "GOOG" }, new DateTime(2015, 01, 21, 18, 20, 59));
                trade.CompletionTime = new DateTime(2015, 01, 21, 18, 21, 00);

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("C04", 20);
                executionDetails.Add("C05", -20);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Wait for Transactions to complete
            Thread.Sleep(1000);

            #endregion

            // Add Filter Parameters
            arguments.Add(TradeParameters.TradeSide, Convert.ChangeType(TradeSide.Buy, TradeSide.Buy.GetTypeCode()).ToString());

            // Hook Data Event
            profitLossReportManager.DataReceived += delegate(ProfitLossStats obj)
            {
                report = obj;
                dataEventReceived = true;
                manualDataEvent.Set();
            };

            // Request Report Data
            profitLossReportManager.RequestReport(arguments);

            manualDataEvent.WaitOne(10000);

            Assert.IsNotNull(report);
            Assert.IsTrue(dataEventReceived, "Report Event");
            Assert.AreEqual(2, report.Trades.Count, "Return Count");

            // Delete Trades Generated for Test Case
            foreach (Trade trade in localTradesList)
            {
                tradeRespository.Delete(trade);
            }
        }

        [Test]
        [Category("Integration")]
        public void RequestProfitLossReportSellSide_AddTradesToDatabaseForTheIntendedReport_RetrieveResults_Successful_DeleteAddedEntries()
        {
            ITradeRepository tradeRespository = ContextRegistry.GetContext()["TradeRepository"] as ITradeRepository;
            var profitLossReportManager = new ProfitLossReportManager(tradeRespository);

            var dataEventReceived = false;
            var manualDataEvent = new ManualResetEvent(false);

            ProfitLossStats report = null;
            var arguments = new Dictionary<TradeParameters, string>();

            // Contains Trades which are created in the Test Case
            IList<Trade> localTradesList = new List<Trade>();

            #region Add Trades to DB

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Buy, 20, 113, "FilterTestCase", "C00",
                    new Security() { Symbol = "GOOG" }, new DateTime(2015, 01, 21, 18, 20, 57));
                trade.CompletionTime = new DateTime(2015, 01, 21, 18, 20, 58);

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("C00", 20);
                executionDetails.Add("C01", -20);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Sell, 20, 113, "FilterTestCase", "C02",
                    new Security() { Symbol = "GOOG" }, new DateTime(2015, 01, 21, 18, 20, 58));
                trade.CompletionTime = new DateTime(2015, 01, 21, 18, 20, 59);

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("C02", -20);
                executionDetails.Add("C03", 20);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Buy, 20, 113, "FilterTestCase", "C04",
                    new Security() { Symbol = "GOOG" }, new DateTime(2015, 01, 21, 18, 20, 59));
                trade.CompletionTime = new DateTime(2015, 01, 21, 18, 21, 00);

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("C04", 20);
                executionDetails.Add("C05", -20);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Wait for Transactions to complete
            Thread.Sleep(1000);

            #endregion

            // Add Filter Parameters
            arguments.Add(TradeParameters.TradeSide, Convert.ChangeType(TradeSide.Sell, TradeSide.Sell.GetTypeCode()).ToString());

            // Hook Data Event
            profitLossReportManager.DataReceived += delegate(ProfitLossStats obj)
            {
                report = obj;
                dataEventReceived = true;
                manualDataEvent.Set();
            };

            // Request Report Data
            profitLossReportManager.RequestReport(arguments);

            manualDataEvent.WaitOne(10000);

            Assert.IsNotNull(report);
            Assert.IsTrue(dataEventReceived, "Report Event");
            Assert.AreEqual(1, report.Trades.Count, "Return Count");

            // Delete Trades Generated for Test Case
            foreach (Trade trade in localTradesList)
            {
                tradeRespository.Delete(trade);
            }
        }

        [Test]
        [Category("Integration")]
        public void RequestProfitLossReportTradeSize_AddTradesToDatabaseForTheIntendedReport_RetrieveResults_Successful_DeleteAddedEntries()
        {
            ITradeRepository tradeRespository = ContextRegistry.GetContext()["TradeRepository"] as ITradeRepository;
            var profitLossReportManager = new ProfitLossReportManager(tradeRespository);

            var dataEventReceived = false;
            var manualDataEvent = new ManualResetEvent(false);

            ProfitLossStats report = null;
            var arguments = new Dictionary<TradeParameters, string>();

            // Contains Trades which are created in the Test Case
            IList<Trade> localTradesList = new List<Trade>();

            #region Add Trades to DB

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Buy, 20, 113, "FilterTestCase", "C00",
                    new Security() { Symbol = "GOOG" }, new DateTime(2015, 01, 21, 18, 20, 57));
                trade.CompletionTime = new DateTime(2015, 01, 21, 18, 20, 58);

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("C00", 20);
                executionDetails.Add("C01", -20);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Sell, 25, 113, "FilterTestCase", "C02",
                    new Security() { Symbol = "GOOG" }, new DateTime(2015, 01, 21, 18, 20, 58));
                trade.CompletionTime = new DateTime(2015, 01, 21, 18, 20, 59);

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("C02", -25);
                executionDetails.Add("C03", 25);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Buy, 30, 113, "FilterTestCase", "C04",
                    new Security() { Symbol = "GOOG" }, new DateTime(2015, 01, 21, 18, 20, 59));
                trade.CompletionTime = new DateTime(2015, 01, 21, 18, 21, 00);

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("C04", 30);
                executionDetails.Add("C05", -30);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Wait for Transactions to complete
            Thread.Sleep(1000);

            #endregion

            // Add Filter Parameters
            arguments.Add(TradeParameters.TradeSize, "20");

            // Hook Data Event
            profitLossReportManager.DataReceived += delegate(ProfitLossStats obj)
            {
                report = obj;
                dataEventReceived = true;
                manualDataEvent.Set();
            };

            // Request Report Data
            profitLossReportManager.RequestReport(arguments);

            manualDataEvent.WaitOne(10000);

            Assert.IsNotNull(report);
            Assert.IsTrue(dataEventReceived, "Report Event");
            Assert.AreEqual(1, report.Trades.Count, "Return Count");

            // Delete Trades Generated for Test Case
            foreach (Trade trade in localTradesList)
            {
                tradeRespository.Delete(trade);
            }
        }

        [Test]
        [Category("Integration")]
        public void RequestProfitLossReportStartTime_AddTradesToDatabaseForTheIntendedReport_RetrieveResults_Successful_DeleteAddedEntries()
        {
            ITradeRepository tradeRespository = ContextRegistry.GetContext()["TradeRepository"] as ITradeRepository;
            var profitLossReportManager = new ProfitLossReportManager(tradeRespository);

            var dataEventReceived = false;
            var manualDataEvent = new ManualResetEvent(false);

            ProfitLossStats report = null;
            var arguments = new Dictionary<TradeParameters, string>();

            // Contains Trades which are created in the Test Case
            IList<Trade> localTradesList = new List<Trade>();

            #region Add Trades to DB

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Buy, 20, 113, "FilterTestCase", "C00",
                    new Security() { Symbol = "GOOG" }, new DateTime(2015, 01, 21, 18, 20, 57));
                trade.CompletionTime = new DateTime(2015, 01, 21, 18, 20, 58);

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("C00", 20);
                executionDetails.Add("C01", -20);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Sell, 25, 113, "FilterTestCase", "C02",
                    new Security() { Symbol = "GOOG" }, new DateTime(2015, 01, 21, 18, 20, 58));
                trade.CompletionTime = new DateTime(2015, 01, 21, 18, 20, 59);

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("C02", -25);
                executionDetails.Add("C03", 25);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Buy, 30, 113, "FilterTestCase", "C04",
                    new Security() { Symbol = "GOOG" }, new DateTime(2015, 01, 21, 18, 20, 59));
                trade.CompletionTime = new DateTime(2015, 01, 21, 18, 21, 00);

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("C04", 30);
                executionDetails.Add("C05", -30);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Wait for Transactions to complete
            Thread.Sleep(1000);

            #endregion

            // Add Filter Parameters
            arguments.Add(TradeParameters.StartTime, "2015-01-21 18:20:57");

            // Hook Data Event
            profitLossReportManager.DataReceived += delegate(ProfitLossStats obj)
            {
                report = obj;
                dataEventReceived = true;
                manualDataEvent.Set();
            };

            // Request Report Data
            profitLossReportManager.RequestReport(arguments);

            manualDataEvent.WaitOne(10000);

            Assert.IsNotNull(report);
            Assert.IsTrue(dataEventReceived, "Report Event");
            Assert.AreEqual(3, report.Trades.Count, "Return Count");

            // Delete Trades Generated for Test Case
            foreach (Trade trade in localTradesList)
            {
                tradeRespository.Delete(trade);
            }
        }

        [Test]
        [Category("Integration")]
        public void RequestProfitLossReportCompletionTime_AddTradesToDatabaseForTheIntendedReport_RetrieveResults_Successful_DeleteAddedEntries()
        {
            ITradeRepository tradeRespository = ContextRegistry.GetContext()["TradeRepository"] as ITradeRepository;
            var profitLossReportManager = new ProfitLossReportManager(tradeRespository);

            var dataEventReceived = false;
            var manualDataEvent = new ManualResetEvent(false);

            ProfitLossStats report = null;
            var arguments = new Dictionary<TradeParameters, string>();

            // Contains Trades which are created in the Test Case
            IList<Trade> localTradesList = new List<Trade>();

            #region Add Trades to DB

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Buy, 20, 113, "FilterTestCase", "C00",
                    new Security() { Symbol = "GOOG" }, new DateTime(2015, 01, 21, 18, 20, 57));
                trade.CompletionTime = new DateTime(2015, 01, 21, 18, 20, 58);

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("C00", 20);
                executionDetails.Add("C01", -20);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Sell, 20, 113, "FilterTestCase", "C02",
                    new Security() { Symbol = "GOOG" }, new DateTime(2015, 01, 21, 18, 20, 58));
                trade.CompletionTime = new DateTime(2015, 01, 21, 18, 20, 59);

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("C02", -20);
                executionDetails.Add("C03", 20);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Buy, 20, 113, "FilterTestCase", "C04",
                    new Security() { Symbol = "GOOG" }, new DateTime(2015, 01, 21, 18, 20, 59));
                trade.CompletionTime = new DateTime(2015, 01, 21, 18, 21, 00);

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("C04", 20);
                executionDetails.Add("C05", -20);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Wait for Transactions to complete
            Thread.Sleep(1000);

            #endregion

            // Add Filter Parameters
            arguments.Add(TradeParameters.CompletionTime, "2015-01-21 18:20:58");

            // Hook Data Event
            profitLossReportManager.DataReceived += delegate(ProfitLossStats obj)
            {
                report = obj;
                dataEventReceived = true;
                manualDataEvent.Set();
            };

            // Request Report Data
            profitLossReportManager.RequestReport(arguments);

            manualDataEvent.WaitOne(10000);

            Assert.IsNotNull(report);
            Assert.IsTrue(dataEventReceived, "Report Event");
            Assert.AreEqual(1, report.Trades.Count, "Return Count");

            // Delete Trades Generated for Test Case
            foreach (Trade trade in localTradesList)
            {
                tradeRespository.Delete(trade);
            }
        }

        [Test]
        [Category("Integration")]
        public void RequestProfitLossReportExecutionProvider_AddTradesToDatabaseForTheIntendedReport_RetrieveResults_Successful_DeleteAddedEntries()
        {
            ITradeRepository tradeRespository = ContextRegistry.GetContext()["TradeRepository"] as ITradeRepository;
            var profitLossReportManager = new ProfitLossReportManager(tradeRespository);

            var dataEventReceived = false;
            var manualDataEvent = new ManualResetEvent(false);

            ProfitLossStats report = null;
            var arguments = new Dictionary<TradeParameters, string>();

            // Contains Trades which are created in the Test Case
            IList<Trade> localTradesList = new List<Trade>();

            #region Add Trades to DB

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Buy, 20, 113, "FilterTestCase", "C00",
                    new Security() { Symbol = "GOOG" }, new DateTime(2015, 01, 21, 18, 20, 57));
                trade.CompletionTime = new DateTime(2015, 01, 21, 18, 20, 58);

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("C00", 20);
                executionDetails.Add("C01", -20);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Sell, 20, 113, "FilterTestCase", "C02",
                    new Security() { Symbol = "GOOG" }, new DateTime(2015, 01, 21, 18, 20, 58));
                trade.CompletionTime = new DateTime(2015, 01, 21, 18, 20, 59);

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("C02", -20);
                executionDetails.Add("C03", 20);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Buy, 20, 113, "FilterTestCaseSelection", "C04",
                    new Security() { Symbol = "GOOG" }, new DateTime(2015, 01, 21, 18, 20, 59));
                trade.CompletionTime = new DateTime(2015, 01, 21, 18, 21, 00);

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("C04", 20);
                executionDetails.Add("C05", -20);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Wait for Transactions to complete
            Thread.Sleep(1000);

            #endregion

            // Add Filter Parameters
            arguments.Add(TradeParameters.ExecutionProvider, "FilterTestCaseSelection");

            // Hook Data Event
            profitLossReportManager.DataReceived += delegate(ProfitLossStats obj)
            {
                report = obj;
                dataEventReceived = true;
                manualDataEvent.Set();
            };

            // Request Report Data
            profitLossReportManager.RequestReport(arguments);

            manualDataEvent.WaitOne(10000);

            Assert.IsNotNull(report);
            Assert.IsTrue(dataEventReceived, "Report Event");
            Assert.AreEqual(1, report.Trades.Count, "Return Count");

            // Delete Trades Generated for Test Case
            foreach (Trade trade in localTradesList)
            {
                tradeRespository.Delete(trade);
            }
        }

        [Test]
        [Category("Integration")]
        public void RequestProfitLossReportSymbol_AddTradesToDatabaseForTheIntendedReport_RetrieveResults_Successful_DeleteAddedEntries()
        {
            ITradeRepository tradeRespository = ContextRegistry.GetContext()["TradeRepository"] as ITradeRepository;
            var profitLossReportManager = new ProfitLossReportManager(tradeRespository);

            var dataEventReceived = false;
            var manualDataEvent = new ManualResetEvent(false);

            ProfitLossStats report = null;
            var arguments = new Dictionary<TradeParameters, string>();

            // Contains Trades which are created in the Test Case
            IList<Trade> localTradesList = new List<Trade>();

            #region Add Trades to DB

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Buy, 20, 113, "FilterTestCase", "C00",
                    new Security() { Symbol = "GOOG" }, new DateTime(2015, 01, 21, 18, 20, 57));
                trade.CompletionTime = new DateTime(2015, 01, 21, 18, 20, 58);

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("C00", 20);
                executionDetails.Add("C01", -20);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Sell, 20, 113, "FilterTestCaseZero", "C02",
                    new Security() { Symbol = "AAPL" }, new DateTime(2015, 01, 21, 18, 20, 58));
                trade.CompletionTime = new DateTime(2015, 01, 21, 18, 20, 59);

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("C02", -20);
                executionDetails.Add("C03", 20);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Buy, 20, 113, "FilterTestCaseOne", "C04",
                    new Security() { Symbol = "IBM" }, new DateTime(2015, 01, 21, 18, 20, 59));
                trade.CompletionTime = new DateTime(2015, 01, 21, 18, 21, 00);

                Dictionary<string, int> executionDetails = new Dictionary<string, int>();
                executionDetails.Add("C04", 20);
                executionDetails.Add("C05", -20);

                trade.ExecutionDetails = executionDetails;

                //add Trade to database
                tradeRespository.AddUpdate(trade);

                // Add to list
                localTradesList.Add(trade);
            }

            // Wait for Transactions to complete
            Thread.Sleep(1000);

            #endregion

            // Add Filter Parameters
            arguments.Add(TradeParameters.Symbol, "GOOG,AAPL");

            // Hook Data Event
            profitLossReportManager.DataReceived += delegate(ProfitLossStats obj)
            {
                report = obj;
                dataEventReceived = true;
                manualDataEvent.Set();
            };

            // Request Report Data
            profitLossReportManager.RequestReport(arguments);

            manualDataEvent.WaitOne(10000);

            Assert.IsNotNull(report);
            Assert.IsTrue(dataEventReceived, "Report Event");
            Assert.AreEqual(2, report.Trades.Count, "Return Count");

            // Delete Trades Generated for Test Case
            foreach (Trade trade in localTradesList)
            {
                tradeRespository.Delete(trade);
            }
        }
    }
}
