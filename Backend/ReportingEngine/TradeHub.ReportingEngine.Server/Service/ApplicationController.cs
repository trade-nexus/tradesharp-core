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
