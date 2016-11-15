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
using TraceSourceLogger;
using TradeHub.Common.Core.Repositories;
using TradeHub.Common.Core.Repositories.Parameters;
using TradeHub.Infrastructure.Nhibernate.Repositories;

namespace TradeHub.ReportingEngine.OrderReporter
{
    /// <summary>
    /// Responsible for generating all order related reports
    /// </summary>
    public class OrderReportManager
    {
        private Type _type = typeof (OrderReportManager);

        /// <summary>
        /// Provides Access to DataBase
        /// </summary>
        private IOrderRepository _orderRespository;

        #region Events

        public event Action<IList<object[]>> DataReceived; 

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="orderRespository"></param>
        public OrderReportManager(IOrderRepository orderRespository)
        {
            _orderRespository = orderRespository;
        }

        /// <summary>
        /// Requests infrastructure for specified information
        /// </summary>
        /// <param name="arguments"></param>
        public void RequestReport(Dictionary<OrderParameters, string> arguments)
        {
            try
            {
                // Request required information from DB
                IList<object[]> result = _orderRespository.Find(arguments);

                // Raise Event to notify Listeners
                if (result != null)
                {
                    DataReceived(result);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "RequestReport");
            }
        }
    }
}
