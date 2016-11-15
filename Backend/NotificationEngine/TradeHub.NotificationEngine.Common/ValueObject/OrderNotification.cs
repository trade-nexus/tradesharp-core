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
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.NotificationEngine.Common.Constants;

namespace TradeHub.NotificationEngine.Common.ValueObject
{
    /// <summary>
    /// Contains information regarding order messages to be for notifications
    /// </summary>
    [Serializable]
    public class OrderNotification
    {
        #region Fields

        /// <summary>
        /// Indicates the type of notification to be sent
        /// </summary>
        private readonly NotificationType _notificationType;

        /// <summary>
        /// Indicates what type of order is being used for notification
        /// </summary>
        private readonly OrderNotificationType _orderNotificationType;

        /// <summary>
        /// Contains basic order information
        /// </summary>
        private Order _order;

        /// <summary>
        /// Contains order fill details
        /// </summary>
        private Fill _fill;

        /// <summary>
        /// Contains order rejection details
        /// </summary>
        private Rejection _rejection;

        /// <summary>
        /// Limit price if the order type is Limit
        /// </summary>
        private decimal _limitPrice = default(decimal);

        #endregion

        #region Properties

        /// <summary>
        /// Contains basic order information
        /// </summary>
        public Order Order
        {
            get { return _order; }
            set { _order = value; }
        }

        /// <summary>
        /// Contains order fill details
        /// </summary>
        public Fill Fill
        {
            get { return _fill; }
            set { _fill = value; }
        }

        /// <summary>
        /// Limit price if the order type is Limit
        /// </summary>
        public decimal LimitPrice
        {
            get { return _limitPrice; }
        }

        /// <summary>
        /// Indicates the type of notification to be sent
        /// </summary>
        public NotificationType NotificationType
        {
            get { return _notificationType; }
        }

        /// <summary>
        /// Indicates what type of order is being used for notification
        /// </summary>
        public OrderNotificationType OrderNotificationType
        {
            get { return _orderNotificationType; }
        }

        /// <summary>
        /// Contains order rejection details
        /// </summary>
        public Rejection Rejection
        {
            get { return _rejection; }
        }

        #endregion

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="notificationType"></param>
        /// <param name="orderNotificationType"></param>
        public OrderNotification(NotificationType notificationType, OrderNotificationType orderNotificationType)
        {
            _notificationType = notificationType;
            _orderNotificationType = orderNotificationType;
        }

        /// <summary>
        /// Uses the order object to set necessary properties to be used for notification
        /// </summary>
        /// <param name="order"></param>
        public void SetOrder(Order order)
        {
            // Set order by duplicating
            _order = (Order)order.Clone();
        }

        /// <summary>
        /// Sets limit price value for the limit orders
        /// </summary>
        /// <param name="limitPrice"></param>
        public void SetLimitPrice(decimal limitPrice)
        {
            _limitPrice = limitPrice;
        }

        /// <summary>
        /// Uses fill object to set necessary properties to be used for notification
        /// </summary>
        /// <param name="fill"></param>
        public void SetFill(Fill fill)
        {
            // Set fill by duplicating
            _fill = (Fill) fill.Clone();
        }

        public void SetRejection(Rejection rejection)
        {
            // Set rejection by duplicating
            _rejection = (Rejection) rejection.Clone();
        }
    }
}
