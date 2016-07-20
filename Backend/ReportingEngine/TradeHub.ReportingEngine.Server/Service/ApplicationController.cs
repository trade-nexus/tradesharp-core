using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.Repositories.Parameters;
using TradeHub.ReportingEngine.CommunicationManager.Service;
using TradeHub.ReportingEngine.OrderReporter;
using TradeHub.ReportingEngine.ProfitLossReporter;

namespace TradeHub.ReportingEngine.Server.Service
{
    /// <summary>
    /// Startup class which handles all the "ReportingEngine.Server" activities
    /// </summary>
    public class ApplicationController
    {
        private Type _type = typeof (ApplicationController);

        /// <summary>
        /// Provides Order Reporting Data
        /// </summary>
        private readonly OrderReportManager _orderReportManager;

        /// <summary>
        /// Provides Profit Loss Reprot Data
        /// </summary>
        private readonly ProfitLossReportManager _profitLossReportManager;

        /// <summary>
        /// Handles incoming requests and outgoing data
        /// </summary>
        private readonly ICommunicator _communicator;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="orderReportManager">Provides Order Reporting Data</param>
        /// <param name="profitLossReportManager">Provides Profit Loss Reprot Data</param>
        /// <param name="communicator">Handles incoming requests and outgoing data</param>
        public ApplicationController(OrderReportManager orderReportManager, ProfitLossReportManager profitLossReportManager, ICommunicator communicator)
        {
            // Save References
            _communicator = communicator;
            _orderReportManager = orderReportManager;
            _profitLossReportManager = profitLossReportManager;

            // Register Communicator Events
            RegisterCommunicatorEvents();
            
            // Register Report Manager Events
            RegisterReportManagerEvents();
        }

        #region Register Events

        /// <summary>
        /// Hooks Report Manager Events to access requested data
        /// </summary>
        private void RegisterReportManagerEvents()
        {
            // Makes sure events are only hooked once
            UnregisterReportManagerEvents();

            // Subscribe Events
            _orderReportManager.DataReceived += OnOrderReportReceived;
            _profitLossReportManager.DataReceived += OnProfitLossReportReceived;
        }

        /// <summary>
        /// Unhooks Report Manager Events
        /// </summary>
        private void UnregisterReportManagerEvents()
        {
            // Un-Subscribe Events
            _orderReportManager.DataReceived -= OnOrderReportReceived;
            _profitLossReportManager.DataReceived -= OnProfitLossReportReceived;
        }

        /// <summary>
        /// Hooks Communicator Events to provide access to other Modules
        /// </summary>
        private void RegisterCommunicatorEvents()
        {
            // Makes sure events are not hooked multiple times
            UnregisterCommunicatorEvents();

            // Subscribe Events
            _communicator.RequestOrderReportEvent += FetchOrderReportData;
            _communicator.RequestProfitLossReportEvent += FetchProfitLossReportData;
        }

        /// <summary>
        /// Unhooks Communicator Events
        /// </summary>
        private void UnregisterCommunicatorEvents()
        {
            // Un-Subscribe Events
            _communicator.RequestOrderReportEvent -= FetchOrderReportData;
            _communicator.RequestProfitLossReportEvent -= FetchProfitLossReportData;
        }

        #endregion

        #region Report Manager Events

        /// <summary>
        /// Called when requested Order Report Data is received
        /// </summary>
        /// <param name="report">Items list containing requested information</param>
        private void OnOrderReportReceived(IList<object[]> report)
        {
            // Forward report to communicator
            _communicator.OrderReportReceived(report);
        }

        /// <summary>
        /// Called when requested Profit Loss Report Data is received
        /// </summary>
        /// <param name="report">Contains requested report information</param>
        private void OnProfitLossReportReceived(ProfitLossStats report)
        {
            // Forward report to communicator
            _communicator.ProfitLossReportReceived(report);
        }

        #endregion

        #region Incoming Reporting Requests

        /// <summary>
        /// Handles incoming Order Report Request
        /// </summary>
        /// <param name="parameters">Search parameters to be used for report</param>
        private void FetchOrderReportData(Dictionary<OrderParameters, string> parameters)
        {
            // Forward Request to Order Report Manager
            _orderReportManager.RequestReport(parameters);
        }

        /// <summary>
        /// Handles incoming Profit Loss Report Request
        /// </summary>
        /// <param name="parameters">Search parameters to be used for report</param>
        private void FetchProfitLossReportData(Dictionary<TradeParameters, string> parameters)
        {
            // Forward Request to Profit Loss Report Manager
            _profitLossReportManager.RequestReport(parameters);
        }

        #endregion
    }
}
