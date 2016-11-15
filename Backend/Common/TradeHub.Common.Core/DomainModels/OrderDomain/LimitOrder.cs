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


﻿

using System;
using System.Text;

namespace TradeHub.Common.Core.DomainModels.OrderDomain
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class LimitOrder : Order
    {
        private decimal _limitPrice = default(decimal);

        private LimitOrder()
            : base("")
        {

        }

        public LimitOrder(string orderExecutionProivder) : base(orderExecutionProivder)
        {
        }

        public LimitOrder(string orderID, string orderSide, int orderSize, string orderTif, string orderCurrency, Security security, string orderExecutionProivder)
            : base(orderID, orderSide, orderSize, orderTif, orderCurrency, security, orderExecutionProivder)
        {
        }

        /// <summary>
        /// Gets/Sets Order Limit Price
        /// </summary>
        public decimal LimitPrice
        {
            get { return _limitPrice; }
            set { _limitPrice = value; }
        }

        /// <summary>
        /// Overrider ToString for Order
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("Order :: ");
            stringBuilder.Append(Security);
            stringBuilder.Append(" | Order ID: " + OrderID);
            stringBuilder.Append(" | Broker ID: " + BrokerOrderID);
            stringBuilder.Append(" | Side: " + OrderSide);
            stringBuilder.Append(" | Size: " + OrderSize);
            stringBuilder.Append(" | Limit Price: " + LimitPrice);
            stringBuilder.Append(" | Trigger Price: " + TriggerPrice);
            stringBuilder.Append(" | Slippage: " + Slippage);
            stringBuilder.Append(" | TIF: " + OrderTif);
            stringBuilder.Append(" | Status: " + OrderStatus);
            stringBuilder.Append(" | Currency: " + OrderCurrency);
            stringBuilder.Append(" | Exchange: " + Exchange);
            stringBuilder.Append(" | Date Time: " + OrderDateTime);

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Creates a string which is to be published and converted back to LimitOrder on receiver end
        /// </summary>
        public string DataToPublish()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("Limit");
            stringBuilder.Append("," + OrderID);
            stringBuilder.Append("," + OrderSide);
            stringBuilder.Append("," + OrderSize); 
            stringBuilder.Append("," + LimitPrice);
            stringBuilder.Append("," + OrderTif);
            stringBuilder.Append("," + Security.Symbol);
            stringBuilder.Append("," + OrderDateTime.ToString("M/d/yyyy h:mm:ss.fff tt"));
            stringBuilder.Append("," + OrderExecutionProvider);
            stringBuilder.Append("," + TriggerPrice);
            stringBuilder.Append("," + Slippage);
            stringBuilder.Append("," + Remarks);
            stringBuilder.Append("," + Exchange);

            return stringBuilder.ToString();
        }
    }
}
