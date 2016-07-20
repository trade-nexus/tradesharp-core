using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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
using TradeHub.Common.Core.Repositories.Parameters;
using TradeHub.ReportingEngine.CommunicationManager.Service;
using TradeHub.ReportingEngine.ProfitLossReporter;
using TradeHub.ReportingEngine.Server.Service;

namespace TradeHub.ReportingEngine.Server.Tests.Integration
{
    [TestFixture]
    public class ReportingEngineServerTests
    {
        private ApplicationController _applicationController;
        private Communicator _communicator; 

        [SetUp]
        public void Setup()
        {
            _communicator = ContextRegistry.GetContext()["Communicator"] as Communicator;
            _applicationController = ContextRegistry.GetContext()["ApplicationController"] as ApplicationController;
        }

        [TearDown]
        public void TearDown()
        {
            
        }

        [Test]
        [Category("Integration")]
        public void RequestOrderReport_RetrieveResults_Successful()
        {
            IList<object[]> report = null;
            var dataEventReceived = false;

            if (_communicator != null)
            {
                var manualReportEvent = new ManualResetEvent(false);

                // Hook Event
                _communicator.OrderReportReceivedEvent += delegate(IList<object[]> obj)
                {
                    report = obj;
                    dataEventReceived = true;
                    manualReportEvent.Set();
                };

                // Create Parameters for Report
                Dictionary<OrderParameters, string> arguments = new Dictionary<OrderParameters, string>();

                arguments.Add(OrderParameters.OrderSide, "BUY");
                arguments.Add(OrderParameters.OrderStatus, "CANCELLED,OPEN");
                arguments.Add(OrderParameters.OrderSize, "13");

                // Request Order Report
                _communicator.RequestOrderReport(arguments);

                manualReportEvent.WaitOne(3000, false);
            }

            Assert.IsNotNull(report);
            Assert.IsTrue(dataEventReceived, "Report Event");
            Assert.Greater(report.Count, 0, "Return Count");
        }

        [Test]
        [Category("Integration")]
        public void RequestProfitLossReport_AddTradesToDatabaseForTheIntendedReport__RetrieveResults_Successful_DeleteAddedEntries()
        {
            var dataEventReceived = false;

            ProfitLossStats report = null;
            var arguments = new Dictionary<TradeParameters, string>();

            #region Insert Trade Data in DB

            ITradeRepository tradeRespository = ContextRegistry.GetContext()["TradeRepository"] as ITradeRepository;
            var profitLossReportManager = new ProfitLossReportManager(tradeRespository);

            // Contains Trades which are created in the Test Case
            IList<Trade> localTradesList = new List<Trade>();

            // Save New Trade
            {
                Trade trade = new Trade(TradeSide.Buy, 20, 113, "FilterTestCase", "C00", new Security() { Symbol = "GOOG" }, new DateTime(2015, 01, 21, 18, 20, 57));
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
                Trade trade = new Trade(TradeSide.Sell, 20, 113, "FilterTestCase", "C02", new Security() { Symbol = "GOOG" }, new DateTime(2015, 01, 21, 18, 20, 58));
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
                Trade trade = new Trade(TradeSide.Buy, 20, 113, "FilterTestCase", "C04", new Security() { Symbol = "GOOG" }, new DateTime(2015, 01, 21, 18, 20, 59));
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

            // Add Filter Parameters
            arguments.Add(TradeParameters.TradeSide, Convert.ChangeType(TradeSide.Buy, TradeSide.Buy.GetTypeCode()).ToString());
            arguments.Add(TradeParameters.TradeSize, "20");
            arguments.Add(TradeParameters.StartTime, "2015-01-21 18:20:57");
            arguments.Add(TradeParameters.CompletionTime, "2015-01-21 18:21:00");
            arguments.Add(TradeParameters.ExecutionProvider, "FilterTestCase");

            #endregion

            if (_communicator != null)
            {
                var manualReportEvent = new ManualResetEvent(false);

                // Hook Event
                _communicator.ProfitLossReportReceivedEvent += delegate(ProfitLossStats obj)
                {
                    report = obj;
                    dataEventReceived = true;
                    manualReportEvent.Set();
                };

                // Request Profit Loss Report
                _communicator.RequestProfitLossReport(arguments);

                manualReportEvent.WaitOne(3000, false);
            }

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
