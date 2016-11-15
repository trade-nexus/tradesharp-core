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
using System.Threading.Tasks;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.Repositories.Parameters;
using TradeHub.ReportingEngine.CommunicationManager.Service;

namespace TradeHub.ReportingEngine.Client.Service
{
    /// <summary>
    /// Provides communication medium with Reporting Engine Server
    /// </summary>
    public class ReportingEngineClient : ICommunicator
    {
        private Type _type = typeof (ReportingEngineClient);

        /// <summary>
        /// Provides direct access to Reporting Engine Server
        /// </summary>
        private readonly Communicator _serverCommunicator;

        /// <summary>
        /// Raised when Order Report is received from Reporting Engine
        /// </summary>
        public event Action<IList<object[]>> OrderReportReceivedEvent;

        /// <summary>
        /// Raised when Profit Loss Report is received from Reporting Engine
        /// </summary>
        public event Action<ProfitLossStats> ProfitLossReportReceivedEvent;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="serverCommunicator"></param>
        public ReportingEngineClient(Communicator serverCommunicator)
        {
            // Save Instance
            _serverCommunicator = serverCommunicator;

            // Register Server Events
            SubscribeServerEvents();
        }

        /// <summary>
        /// Subscribes Server events to received reporting data
        /// </summary>
        private void SubscribeServerEvents()
        {
            // Makes sure that events are not hooked multiple times
            UnSubscribeServerEvents();

            // Register Events
            _serverCommunicator.OrderReportReceivedEvent += OrderReportReceived;
            _serverCommunicator.ProfitLossReportReceivedEvent += ProfitLossReportReceived;
        }

        /// <summary>
        /// Un-Subscribes Server events to received reporting data
        /// </summary>
        private void UnSubscribeServerEvents()
        {
            // Un-Subscribe Events
            _serverCommunicator.OrderReportReceivedEvent -= OrderReportReceived;
            _serverCommunicator.ProfitLossReportReceivedEvent -= ProfitLossReportReceived;
        }

        /// <summary>
        /// Indicates if the communication medium is open or not
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            if (_serverCommunicator != null)
            {
                return true;
            }

            return false;
        }

        #region Start/Stop

        /// <summary>
        /// Opens necessary connections to start communication
        /// </summary>
        public void Connect()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(IsConnected() ? "Connection established" : "Connection failed", _type.FullName,
                        "Connect");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "Connect");
            }
        }

        /// <summary>
        /// Closes communication channels
        /// </summary>
        public void Disconnect()
        {
            // NOTE: No operation needed as direct access to Server is used.
        }

        #endregion

        #region Request Messages

        /// <summary>
        /// Send Order Report request to Reporting Engine
        /// </summary>
        /// <param name="parameters">Search parameters to be used for report</param>
        public void RequestOrderReport(Dictionary<OrderParameters, string> parameters)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("New order report request received.", _type.FullName, "RequestOrderReport");
                }

                // Send Request to Reporting Engine
                _serverCommunicator.RequestOrderReport(parameters);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "RequestOrderReport");
            }
        }

        /// <summary>
        /// Send Profit Loss Report request to Reporting Engine
        /// </summary>
        /// <param name="parameters">Search parameters to be used for report</param>
        public void RequestProfitLossReport(Dictionary<TradeParameters, string> parameters)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("New profit loss report request received.", _type.FullName, "RequestProfitLossReport");
                }

                // Send Request to Reporting Engine
                _serverCommunicator.RequestProfitLossReport(parameters);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "RequestProfitLossReport");
            }
        }

        #endregion

        #region 

        /// <summary>
        /// Called when Order report is received
        /// </summary>
        /// <param name="report">Contains requested report information</param>
        private void OrderReportReceived(IList<object[]> report)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Order report received", _type.FullName, "OrderReportReceived");
                }

                // Raise event to notify listeners
                if (OrderReportReceivedEvent!=null)
                {
                    OrderReportReceivedEvent(report);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OrderReportReceived");
            }
        }

        /// <summary>
        /// Called when Profit Loss report is received
        /// </summary>
        /// <param name="report">Contains requested report information</param>
        private void ProfitLossReportReceived(ProfitLossStats report)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Profit Loss report received", _type.FullName, "ProfitLossReportReceived");
                }

                // Raise event to notify listeners
                if (ProfitLossReportReceivedEvent != null)
                {
                    ProfitLossReportReceivedEvent(report);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ProfitLossReportReceived");
            }
        }

        #endregion
    }
}
