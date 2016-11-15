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

namespace TradeHub.Common.Core.DomainModels.OrderDomain
{
    /// <summary>
    /// Contains <see cref="Fill"/> and <see cref="Order"/> objects to provide Execution Information
    /// </summary>
    [Serializable]
    public class Execution
    {
        private Order _order;
        private Fill _fill;
        private string _orderExecutionProvider;
        //public decimal BarClose;

        /// <summary>
        /// Default Constructor
        /// </summary>
        private Execution()
        {
            
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="fill">TradeHub Fill Message</param>
        /// <param name="order">TradeHub Order Message</param>
        public Execution(Fill fill, Order order)
        {
            _fill = fill;
            _order = order;

            // Set Order Execution Provider
            _orderExecutionProvider = fill.OrderExecutionProvider;
        }

        /// <summary>
        /// Gets TradeHub Order Object
        /// </summary>
        public Order Order
        {
            get { return _order; }
            set { _order = value; }
        }

        /// <summary>
        /// Gets TradeHub Execution Object
        /// </summary>
        public Fill Fill
        {
            get { return _fill; }
            set { _fill = value; }
        }

        /// <summary>
        /// Gets/Sets the Name of Order Execution Provider
        /// </summary>
        public string OrderExecutionProvider
        {
            get { return _orderExecutionProvider; }
            set { _orderExecutionProvider = value; }
        }

        /// <summary>
        /// ToString Override for Execution Info
        /// </summary>
        public override string ToString()
        {
            StringBuilder stringBuilder= new StringBuilder();

            stringBuilder.Append("Execution Info :: ");
            stringBuilder.Append(_order);
            stringBuilder.Append(" | ");
            stringBuilder.Append(_fill);
            stringBuilder.Append(" | Order Execution Proivder: " + _orderExecutionProvider);

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Provides basic execution info
        /// </summary>
        public string BasicExecutionInfo()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("Execution Info :: ");
            stringBuilder.Append(" Symbol: ");
            stringBuilder.Append(_order.Security.Symbol);
            stringBuilder.Append(" | ");
            stringBuilder.Append(" OrderID: ");
            stringBuilder.Append(_order.OrderID);
            stringBuilder.Append(" | ");
            stringBuilder.Append(" Side: ");
            stringBuilder.Append(_order.OrderSide);
            stringBuilder.Append(" | ");
            stringBuilder.Append(" Size ");
            stringBuilder.Append(_order.OrderSize);
            stringBuilder.Append(" | ");
            stringBuilder.Append(" Execution Price: ");
            stringBuilder.Append(_fill.ExecutionPrice);
            stringBuilder.Append(" | ");
            stringBuilder.Append(" Signal Price: ");
            stringBuilder.Append(_order.TriggerPrice);
            stringBuilder.Append(" | ");
            stringBuilder.Append(" Remarks: ");
            stringBuilder.Append(_order.Remarks);
            stringBuilder.Append(" | ");
            stringBuilder.Append(" Time: ");
            stringBuilder.Append(_fill.ExecutionDateTime);
            
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Creates a string which is to be published and converted back to Execution on receiver end
        /// </summary>
        public string DataToPublish()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(_order.OrderID);
            stringBuilder.Append(",");
            stringBuilder.Append(_order.OrderSide);
            stringBuilder.Append(",");
            stringBuilder.Append(_order.OrderSize);
            stringBuilder.Append(",");
            stringBuilder.Append(_order.Security.Symbol);
            stringBuilder.Append(",");
            stringBuilder.Append(_fill.ExecutionPrice);
            stringBuilder.Append(",");
            stringBuilder.Append(_fill.ExecutionSize);
            stringBuilder.Append(",");
            stringBuilder.Append(_fill.AverageExecutionPrice);
            stringBuilder.Append(",");
            stringBuilder.Append(_fill.LeavesQuantity);
            stringBuilder.Append(",");
            stringBuilder.Append(_fill.CummalativeQuantity);
            stringBuilder.Append(",");
            stringBuilder.Append(_fill.OrderExecutionProvider);
            stringBuilder.Append(",");
            stringBuilder.Append(_fill.ExecutionDateTime.ToString("M/d/yyyy h:mm:ss.fff tt"));
            stringBuilder.Append(",");
            stringBuilder.Append(_order.TriggerPrice);
            stringBuilder.Append(",");
            stringBuilder.Append(_fill.ExecutionId);
            stringBuilder.Append(",");
            stringBuilder.Append(_fill.ExecutionSide);

            return stringBuilder.ToString();
        }
    }
}
