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


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using TradeHub.Common.Core.Constants;

namespace TradeSharp.UI.Common.Models
{
    /// <summary>
    /// Generic Provider class used for Order Execution Provider
    /// </summary>
    public class OrderExecutionProvider : Provider
    {
        #region Fields

        /// <summary>
        /// Hold UI thread reference
        /// </summary>
        private Dispatcher _currentDispatcher;

        /// <summary>
        /// Contains current active orders for the given provider
        /// </summary>
        private List<OrderDetails> _activeOrdersList;

        /// <summary>
        /// Contains all orders information during the current application session
        /// </summary>
        private ObservableCollection<OrderDetails> _ordersCollection;

        /// <summary>
        /// Contains positions stats for all traded securities Grouped by symbol
        /// KEY = Symbol
        /// VALUE = Position information <see cref="PositionStatistics"/>
        /// </summary>
        private Dictionary<string, PositionStatistics> _positionStatisticsDictionary;

        /// <summary>
        ///  Contains positions stats for all traded securities to be used for UI reference
        /// </summary>
        private ObservableCollection<PositionStatistics> _positionStatisticsCollection; 

        #endregion

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="currentDispatcher">UI Dispatcher to be used</param>
        public OrderExecutionProvider(Dispatcher currentDispatcher)
        {
            _currentDispatcher = currentDispatcher;

            _activeOrdersList = new List<OrderDetails>();
            _ordersCollection = new ObservableCollection<OrderDetails>();
            _positionStatisticsCollection = new ObservableCollection<PositionStatistics>();
            _positionStatisticsDictionary = new Dictionary<string, PositionStatistics>();
        }

        #region Properties

        /// <summary>
        /// Contains all orders information during the current application session
        /// </summary>
        public ObservableCollection<OrderDetails> OrdersCollection
        {
            get { return _ordersCollection; }
            set
            {
                _ordersCollection = value;
                OnPropertyChanged("OrdersCollection");
            }
        }

        /// <summary>
        /// Contains positions stats for all traded securities
        /// </summary>
        public ObservableCollection<PositionStatistics> PositionStatisticsCollection
        {
            get { return _positionStatisticsCollection; }
            set
            {
                _positionStatisticsCollection = value;
                OnPropertyChanged("PositionStatisticsCollection");
            }
        }

        #endregion

        /// <summary>
        /// Adds a new order to existing orders list/collection
        /// </summary>
        /// <param name="orderDetails">Contains order information</param>
        public void AddOrder(OrderDetails orderDetails)
        {
            // Add to active orders list
            _activeOrdersList.Add(orderDetails);

            // Add to global orders collection
            OrdersCollection.Add(orderDetails);
        }

        /// <summary>
        /// Adds a Fill information to existing collection
        /// </summary>
        /// <param name="orderDetails">Order in which to add Fill information</param>
        /// <param name="fillDetail">Contains Fill information</param>
        public void AddFill(OrderDetails orderDetails, FillDetail fillDetail)
        {
            _currentDispatcher.Invoke(DispatcherPriority.Background, (Action)(() =>
            {
                // Add to given orders fill list
                orderDetails.FillDetails.Add(fillDetail);
            }));
        }

        /// <summary>
        /// Returns Order Details object for the given 'Order-ID'
        /// </summary>
        /// <param name="orderId">Unique ID to identity order</param>
        /// <param name="orderStatus">Order Status</param>
        /// <returns>Order Details object</returns>
        public OrderDetails GetOrderDetail(string orderId, string orderStatus)
        {
            OrderDetails orderDetails = null;

            // Find 'Order Details' object 
            foreach (OrderDetails tempOrderDetails in _activeOrdersList)
            {
                if (tempOrderDetails.ID.Equals(orderId))
                {
                    orderDetails = tempOrderDetails;

                    // Remove from active Orders list if no more updates are expected
                    if (orderStatus.Equals(OrderStatus.EXECUTED) || orderStatus.Equals(OrderStatus.CANCELLED) || orderStatus.Equals(OrderStatus.REJECTED))
                    {
                        _activeOrdersList.Remove(tempOrderDetails);
                    }

                    // terminate loop
                    break;
                }
            }

            return orderDetails;
        }

        /// <summary>
        /// Update position statistics for a given Security (Symbol)
        /// </summary>
        /// <param name="orderDetails">Order details</param>
        public void UpdatePosition(OrderDetails orderDetails)
        {
            PositionStatistics statistics;
            // Find Existing Statistics for the incoming order security
            if (!_positionStatisticsDictionary.TryGetValue(orderDetails.Security.Symbol, out statistics))
            {
                // Create a new object 
                statistics = new PositionStatistics(orderDetails.Security);

                // Add to dictionary collection
                _positionStatisticsDictionary.Add(orderDetails.Security.Symbol, statistics);

                _currentDispatcher.Invoke(DispatcherPriority.Background, (Action)(() =>
                {
                    // Add to Collection to dispaly on UI
                    PositionStatisticsCollection.Add(statistics);
                }));
            }

            // Handle BUY Order
            if (orderDetails.Side.Equals(OrderSide.BUY))
            {
                statistics.Position += orderDetails.Quantity;
                statistics.Pnl -= orderDetails.Quantity*orderDetails.FillDetails.Last().FillPrice;
            }
            else
            {
                statistics.Position -= orderDetails.Quantity;
                statistics.Pnl += orderDetails.Quantity * orderDetails.FillDetails.Last().FillPrice;
            }
        }

        /// <summary>
        /// Returns Position Statistics for the requested Symbol
        /// </summary>
        /// <param name="symbol">Symbol Name</param>
        /// <returns></returns>
        public PositionStatistics GetPositionStatistics(string symbol)
        {
            PositionStatistics positionStatistics = null;

            // Get information from local dictionary
            _positionStatisticsDictionary.TryGetValue(symbol, out positionStatistics);

            return positionStatistics;
        }
    }
}
