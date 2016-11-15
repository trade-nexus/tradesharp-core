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
using System.Text;

namespace TradeHub.Common.Core.DomainModels.OrderDomain
{
    /// <summary>
    /// This is base class for any type of Order for a particular security.
    /// </summary>
    [Serializable]
    public class Order : ICloneable
    {
        #region Members

        private string _orderID = string.Empty;
        private string _brokerOrderID = string.Empty;
        private Security _security;
        private string _orderSide = string.Empty;
        private DateTime _orderDateTime = default(DateTime);
        private int _orderSize = default(int);
        private string _orderCurrency = string.Empty;
        private string _orderTif = string.Empty;
        private string _orderExecutionProvider = string.Empty;
        private string _orderStatus = string.Empty;
        private string _exchange = string.Empty;
        private decimal _triggerPrice = default(decimal);
        private decimal _slippage = default(decimal);
        private string _remarks = string.Empty;
        private IList<Fill> _fills;
        private Rejection _rejection;
        private int _strategyId = default(int);
        private int _id = default(int);
        #endregion

        private Order()
        {
        }

        /// <summary>
        /// Argument constructor
        /// </summary>
        public Order(string orderExecutionProivder)
        {
            this._orderDateTime = DateTime.UtcNow;
            OrderExecutionProvider = orderExecutionProivder;
        }


        /// <summary>
        /// Argument constructor
        /// </summary>
        public Order(string orderID, string orderSide, int orderSize, string orderTif, string orderCurrency, Security security, string orderExecutionProivder)
        {
            _orderID = orderID;
            _orderSide = orderSide;
            _orderSize = orderSize;
            _orderTif = orderTif;
            _orderCurrency = orderCurrency;
            _security = security;
            OrderExecutionProvider = orderExecutionProivder;
            _orderDateTime = DateTime.UtcNow; 
        }

        #region Properties

        /// <summary>
        /// Order Fills
        /// </summary>
        public IList<Fill> Fills
        {
            get { return _fills; }
            set { _fills = value; }
        }

        /// <summary>
        /// Order Rejection
        /// </summary>
        public Rejection Rejection
        {
            get { return _rejection; }
            set { _rejection = value; }
        }

        /// <summary>
        /// Order ID
        /// </summary>
        public string OrderID
        {
            get { return this._orderID; }
            set { this._orderID = value; }
        }

        /// <summary>
        /// Order side
        /// </summary>
        public string OrderSide
        {
            get { return this._orderSide; }
            set { this._orderSide = value; }
        }
        
        /// <summary>
        /// Order size
        /// </summary>
        public int OrderSize
        {
            get { return this._orderSize; }
            set { this._orderSize = value; }
        }

        /// <summary>
        /// Order date time
        /// </summary>
        public DateTime OrderDateTime
        {
            get { return this._orderDateTime; }
            set { this._orderDateTime = value; }
        }

        /// <summary>
        /// Order Currency
        /// </summary>
        public string OrderCurrency
        {
            get { return this._orderCurrency; }
            set { this._orderCurrency = value; }
        }

        /// <summary>
        /// Gets/Sets TradeHub Security
        /// </summary>
        public Security Security
        {
            get { return _security; }
            set { _security = value; }
        }

        /// <summary>
        /// Order TIF Value
        /// </summary>
        public string OrderTif
        {
            get { return _orderTif; }
            set { _orderTif = value; }
        }

        /// <summary>
        /// Order ID Provided by Broker
        /// </summary>
        public string BrokerOrderID
        {
            get { return _brokerOrderID; }
            set { _brokerOrderID = value; }
        }

        /// <summary>
        /// Order Execution Provider Name
        /// </summary>
        public string OrderExecutionProvider
        {
            get { return _orderExecutionProvider; }
            set { _orderExecutionProvider = value; }
        }

        /// <summary>
        /// Gets/Sets Current Order Status
        /// </summary>
        public string OrderStatus
        {
            get { return _orderStatus; }
            set { _orderStatus = value; }
        }

        /// <summary>
        /// Gets/Sets Order Exchange/Venue
        /// </summary>
        public string Exchange
        {
            get { return _exchange; }
            set { _exchange = value; }
        }

        /// <summary>
        /// The Signal Price at which the Order was generated
        /// </summary>
        public decimal TriggerPrice
        {
            get { return _triggerPrice; }
            set { _triggerPrice = value; }
        }

        /// <summary>
        /// Price slippage to be used while executing the order
        /// </summary>
        public decimal Slippage
        {
            get { return _slippage; }
            set { _slippage = value; }
        }

        /// <summary>
        /// Contains additional remarks regarding the given order
        /// </summary>
        public string Remarks
        {
            get { return _remarks; }
            set { _remarks = value; }
        }

        /// <summary>
        /// Strategy ID (FK)
        /// </summary>
        public int StrategyId
        {
            get { return _strategyId; }
            set { _strategyId = value; }
        }

        /// <summary>
        /// Auto increment id
        /// </summary>
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        #endregion

        /// <summary>
        /// Compares on the basis of OrderID
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (this.GetType() != obj.GetType())
            {
                return false;
            }

            // Safe because of the GetType check
            var order = (Order)obj;

            // Use this pattern to compare reference members
            return Equals(OrderID, order.OrderID);
        }

        /// <summary>
        /// Hash Code Overrider for Order
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        public object Clone()
        {
            return MemberwiseClone();
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
            stringBuilder.Append(" | Order ID: " + _orderID);
            stringBuilder.Append(" | Broker ID: " + BrokerOrderID);
            stringBuilder.Append(" | Side: " + _orderSide);
            stringBuilder.Append(" | Size: " + _orderSize);
            stringBuilder.Append(" | TIF: " + _orderTif);
            stringBuilder.Append(" | Trigger Price: " + _triggerPrice);
            stringBuilder.Append(" | Slippage: " + _slippage);
            stringBuilder.Append(" | Status: " + _orderStatus);
            stringBuilder.Append(" | Currency: " + _orderCurrency);
            stringBuilder.Append(" | Exchange: " + _exchange);
            stringBuilder.Append(" | Date Time: " + _orderDateTime);

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Creates a string which is to be published and converted back to Order on receiver end
        /// </summary>
        /// <param name="type">Order Type</param>
        public string DataToPublish(string type)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(type);
            stringBuilder.Append("," + OrderID);
            stringBuilder.Append("," + OrderSide);
            stringBuilder.Append("," + OrderSize);
            stringBuilder.Append("," + OrderTif);
            stringBuilder.Append("," + OrderStatus);
            stringBuilder.Append("," + Security.Symbol);
            stringBuilder.Append("," + OrderDateTime.ToString("M/d/yyyy h:mm:ss.fff tt"));
            stringBuilder.Append("," + OrderExecutionProvider);
            stringBuilder.Append("," + TriggerPrice);
            stringBuilder.Append("," + Slippage);
            stringBuilder.Append("," + Exchange);

            return stringBuilder.ToString();
        }
    }
}
