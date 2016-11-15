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
using System.Globalization;
using System.Text;
using Disruptor;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.ValueObjects;

namespace TradeHub.Common.HistoricalDataProvider.Utility
{
    /// <summary>
    /// Listens order requests from strategies and triggers appropriate events
    /// </summary>
    public class OrderRequestListener : IEventHandler<RabbitMqRequestMessage>
    {
        private Type _type = typeof (OrderRequestListener);
        private AsyncClassLogger _asyncClassLogger;

        /// <summary>
        /// Responsible for managing order requests as an order server
        /// </summary>
        private IOrderExecutor _orderExecutor;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="orderExecutor">Provides server side order management</param>
        /// <param name="asyncClassLogger"> </param>
        public OrderRequestListener(IOrderExecutor orderExecutor, AsyncClassLogger asyncClassLogger)
        {
            // Save Instance
            _orderExecutor = orderExecutor;
            _asyncClassLogger = asyncClassLogger;
        }

        #region Incoming Order Handling

        /// <summary>
        /// Called when Market Order Request is receieved
        /// </summary>
        /// <param name="messageArray"></param>
        private void OnMarketOrderReceived(string[] messageArray)
        {
            if (_asyncClassLogger.IsDebugEnabled)
            {
                _asyncClassLogger.Debug("Market Order request received: " + messageArray[1], _type.FullName,
                             "OnMarketOrderReceived");
            }

            MarketOrder marketOrder = new MarketOrder(messageArray[7]);

            // Parse incoming message into Market Order
            if (ParseToMarketOrder(marketOrder, messageArray))
            {
                // Send New Marker Order for execution
                _orderExecutor.NewMarketOrderArrived(marketOrder);
            }
        }

        /// <summary>
        /// Called when Limit Order Request is recieved
        /// </summary>
        /// <param name="messageArray"></param>
        private void OnLimitOrderReceived(string[] messageArray)
        {
            if (_asyncClassLogger.IsDebugEnabled)
            {
                _asyncClassLogger.Debug("Limit Order request received: " + messageArray[1], _type.FullName,
                             "OnLimitOrderReceived");
            }

            LimitOrder limitOrder = new LimitOrder(messageArray[8]);

            // Parse incoming message to Limit Order
            if (ParseToLimitOrder(limitOrder, messageArray))
            {
                // Send new Limit Order for execution
                _orderExecutor.NewLimitOrderArrived(limitOrder);
            }
        }

        /// <summary>
        /// Called when Cancel Order request is received
        /// </summary>
        /// <param name="messageArray"></param>
        private void OnCancelOrderReceived(string[] messageArray)
        {
            if (_asyncClassLogger.IsDebugEnabled)
            {
                _asyncClassLogger.Debug("Cancel Order request received: " + messageArray[1], _type.FullName,
                             "OnCancelOrderReceived");
            }

            Order order = new Order(messageArray[8]);

            // Parse incoming message to Order
            if (ParseToOrder(order, messageArray))
            {
                // Send Order for cancellation
                _orderExecutor.CancelOrderArrived(order);
            }
        }

        #endregion

        #region Order Parsing

        /// <summary>
        /// Creats market order object from incoming string message
        /// </summary>
        /// <param name="marketOrder">Market Order to add values to</param>
        /// <param name="message">Received message</param>
        /// <returns></returns>
        private bool ParseToMarketOrder(MarketOrder marketOrder, string[] message)
        {
            try
            {
                // Get Order ID
                marketOrder.OrderID = message[1];
                // Get Order Side
                marketOrder.OrderSide = message[2];
                // Get Order Size
                marketOrder.OrderSize = Convert.ToInt32(message[3]);
                // Get Order TIF value
                marketOrder.OrderTif = message[4];
                // Get Symbol
                marketOrder.Security = new Security() {Symbol = message[5]};
                // Get Time Value
                marketOrder.OrderDateTime = DateTime.ParseExact(message[6], "M/d/yyyy h:mm:ss.fff tt",
                                                                CultureInfo.InvariantCulture);
                // Get Order Trigger Price
                marketOrder.TriggerPrice = Convert.ToDecimal(message[8]);
                // Get Slippage Value
                marketOrder.Slippage = Convert.ToDecimal(message[9]);
                // Get Order Remarks
                marketOrder.Remarks = message[10];

                return true;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "ParseToMarketOrder");
                return false;
            }
        }

        /// <summary>
        /// Creats limit order object from incoming string message
        /// </summary>
        /// <param name="limitOrder">Limit Order to add values to</param>
        /// <param name="message">Received message</param>
        /// <returns></returns>
        private bool ParseToLimitOrder(LimitOrder limitOrder, string[] message)
        {
            try
            {
                // Get Order ID
                limitOrder.OrderID = message[1];
                // Get Order Side
                limitOrder.OrderSide = message[2];
                // Get Order Size
                limitOrder.OrderSize = Convert.ToInt32(message[3]);
                // Get Limit Price
                limitOrder.LimitPrice = Convert.ToDecimal(message[4]);
                // Get Order TIF value
                limitOrder.OrderTif = message[5];
                // Get Symbol
                limitOrder.Security = new Security() {Symbol = message[6]};
                // Get Time Value
                limitOrder.OrderDateTime = DateTime.ParseExact(message[7], "M/d/yyyy h:mm:ss.fff tt",
                                                               CultureInfo.InvariantCulture);
                // Get Order Trigger Price
                limitOrder.TriggerPrice = Convert.ToDecimal(message[9]);
                // Get Slippage Price
                limitOrder.Slippage = Convert.ToDecimal(message[10]);
                // Get Order Remarks
                limitOrder.Remarks = message[11];

                return true;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "ParseToLimitOrder");
                return false;
            }
        }

        /// <summary>
        /// Creats order object from incoming string message
        /// </summary>
        /// <param name="order">Order to add values to</param>
        /// <param name="message">Received message</param>
        /// <returns></returns>
        private bool ParseToOrder(Order order, string[] message)
        {
            try
            {
                // Get Order ID
                order.OrderID = message[1];
                // Get Order Side
                order.OrderSide = message[2];
                // Get Order Size
                order.OrderSize = Convert.ToInt32(message[3]);
                // Get Order TIF value
                order.OrderTif = message[4];
                // Get Order TIF value
                order.OrderStatus = message[5];
                // Get Symbol
                order.Security = new Security() {Symbol = message[6]};
                // Get Time Value
                order.OrderDateTime = DateTime.ParseExact(message[7], "M/d/yyyy h:mm:ss.fff tt",
                                                          CultureInfo.InvariantCulture);

                return true;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "ParseToOrder");
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Called when new order request is received
        /// </summary>
        /// <param name="orderRequest"></param>
        public void NewOrderRequest(byte[] orderRequest)
        {
            string message = Encoding.UTF8.GetString(orderRequest);

            var messageArray = message.Split(',');

            if (messageArray[0].Equals("Market"))
                OnMarketOrderReceived(messageArray);
            else if (messageArray[0].Equals("Limit"))
                OnLimitOrderReceived(messageArray);
            else if (messageArray[0].Equals("Cancel"))
                OnCancelOrderReceived(messageArray);
        }
        
        /// <summary>
        /// Called when new Market Order is received
        /// </summary>
        /// <param name="marketOrder"></param>
        public void NewMarketOrderRequest(MarketOrder marketOrder)
        {
            // Send New Marker Order for execution
            _orderExecutor.NewMarketOrderArrived(marketOrder);
        }

        /// <summary>
        /// Called when new Limit Order is receievd
        /// </summary>
        /// <param name="limitOrder"></param>
        public void NewLimitOrderRequest(LimitOrder limitOrder)
        {
            // Send New Limit Order for execution
            _orderExecutor.NewLimitOrderArrived(limitOrder);
        }

        /// <summary>
        /// Called when new Cancel Order request is received
        /// </summary>
        /// <param name="order"></param>
        public void NewCancelOrderRequest(Order order)
        {
            // Send Order for cancellation
            _orderExecutor.CancelOrderArrived(order);
        }

        #region Implementation of IEventHandler<in RabbitMqMessage>

        /// <summary>
        /// Called when a publisher has committed an event to the <see cref="T:Disruptor.RingBuffer`1"/>
        /// </summary>
        /// <param name="data">Data committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="sequence">Sequence number committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="endOfBatch">flag to indicate if this is the last event in a batch from the <see cref="T:Disruptor.RingBuffer`1"/></param>
        public void OnNext(RabbitMqRequestMessage data, long sequence, bool endOfBatch)
        {
            string message = Encoding.UTF8.GetString(data.Message);

            var messageArray = message.Split(',');

            if (messageArray[0].Equals("Market"))
                OnMarketOrderReceived(messageArray);
            else if (messageArray[0].Equals("Limit"))
                OnLimitOrderReceived(messageArray);
            else if (messageArray[0].Equals("Cancel"))
                OnCancelOrderReceived(messageArray);
        }

        #endregion
    }
}
