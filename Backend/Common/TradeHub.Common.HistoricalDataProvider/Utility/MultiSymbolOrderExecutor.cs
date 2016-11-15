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
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;

namespace TradeHub.Common.HistoricalDataProvider.Utility
{
    /// <summary>
    /// Responsible for providing executions for multiple symbols
    /// </summary>
    public class MultiSymbolOrderExecutor: IOrderExecutor
    {
        private Type _type = typeof(MultiSymbolOrderExecutor);

        public event Action<Order> NewOrderArrived;
        public event Action<Execution> ExecutionArrived;
        public event Action<Order> CancellationArrived;
        public event Action<Rejection> RejectionArrived;

        //received Limit order list
        private List<LimitOrder> _limitOrders;

        private DateTime _lastdateTime;
        private decimal _lastTradePrice;
        private decimal _lastTradeSize;

        //tick records for different symbols
        private Dictionary<string,Tick> _latestTicks;
        //bars records for different symbols
        private Dictionary<string,Bar> _latestBars;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public MultiSymbolOrderExecutor()
        {
            _limitOrders = new List<LimitOrder>();
            _latestTicks=new Dictionary<string, Tick>();
            _latestBars=new Dictionary<string, Bar>();
        }
        
        /// <summary>
        /// Called when new tick is recieved
        /// </summary>
        /// <param name="tick">TradeHub Tick</param>
        public void TickArrived(Tick tick)
        {
            if (_latestTicks.ContainsKey(tick.Security.Symbol))
            {
                _latestTicks[tick.Security.Symbol] = tick;
            }
            else
            {
                _latestTicks.Add(tick.Security.Symbol,tick);
            }
        }

        /// <summary>
        /// Called when new Bar is received
        /// </summary>
        /// <param name="bar">TradeHub Bar</param>
        public void BarArrived(Bar bar)
        {
            if (_latestBars.ContainsKey(bar.Security.Symbol))
            {
                _latestBars[bar.Security.Symbol] = bar;
            }
            else
            {
                _latestBars.Add(bar.Security.Symbol,bar);
            }

            foreach (var limitOrder in _limitOrders.ToArray())
            {
                if (limitOrder.Security.Symbol.Equals(bar.Security.Symbol))
                {
                    ExecuteLimitOrder(limitOrder, bar);
                }
            }
        }

        /// <summary>
        /// Called when new market order is received
        /// </summary>
        /// <param name="marketOrder">TardeHub MarketOrder</param>
        public void NewMarketOrderArrived(MarketOrder marketOrder)
        {
            if (ValidMarketOrder(marketOrder))
            {
                var order = new Order(marketOrder.OrderID, marketOrder.OrderSide, marketOrder.OrderSize,
                                          marketOrder.OrderTif,
                                          marketOrder.OrderCurrency, marketOrder.Security,
                                          marketOrder.OrderExecutionProvider);
                //change order status to open
                order.OrderStatus = OrderStatus.OPEN;
                order.StrategyId = marketOrder.StrategyId;

                if (NewOrderArrived != null)
                {
                    NewOrderArrived(order);
                }

                //send the order for execution
                ExecuteMarketOrder(marketOrder);
            }
            else
            {
                Rejection rejection = new Rejection(marketOrder.Security, OrderExecutionProvider.SimulatedExchange)
                {
                    OrderId = marketOrder.OrderID,
                    DateTime = DateTime.Now,
                    RejectioReason = "Invaild Price Or Size"
                };
                
                marketOrder.OrderStatus = OrderStatus.REJECTED;
                
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Rejection :" + rejection, _type.FullName, "NewMarketOrderArrived");
                }
               
                if (RejectionArrived != null)
                {
                    RejectionArrived(rejection);
                }
            }
        }

        /// <summary>
        /// Called when new limit order is recieved
        /// </summary>
        /// <param name="limitOrder">TradeHub LimitOrder</param>
        public void NewLimitOrderArrived(LimitOrder limitOrder)
        {
            try
            {
                if (ValideLimitOrder(limitOrder))
                {
                    var order = new Order(limitOrder.OrderID,
                                          limitOrder.OrderSide,
                                          limitOrder.OrderSize,
                                          limitOrder.OrderTif,
                                          limitOrder.OrderCurrency,
                                          limitOrder.Security,
                                          limitOrder.OrderExecutionProvider);
                    //change order status to open
                    order.OrderStatus = OrderStatus.OPEN;
                    order.StrategyId = limitOrder.StrategyId;
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("New Arrived :" + order, _type.FullName, "NewLimitOrderArrived");
                    }

                    //add limit order to list
                    _limitOrders.Add(limitOrder);

                    if (NewOrderArrived != null)
                    {
                        NewOrderArrived(order);
                    }
                }
                else
                {
                    Rejection rejection = new Rejection(limitOrder.Security, OrderExecutionProvider.SimulatedExchange) { OrderId = limitOrder.OrderID, DateTime = DateTime.Now, RejectioReason = "Invaild Price Or Size" };
                    limitOrder.OrderStatus = OrderStatus.REJECTED;
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Rejection :" + rejection, _type.FullName, "NewLimitOrderArrived");
                    }
                    if (RejectionArrived != null)
                    {
                        RejectionArrived.Invoke(rejection);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "NewLimitOrderArrived");
            }
        }

        /// <summary>
        /// Called when new cancel order request is recieved
        /// </summary>
        /// <param name="cancelOrder"></param>
        public void CancelOrderArrived(Order cancelOrder)
        {
            try
            {
                foreach (var order in _limitOrders.ToArray())
                {
                    if (order.OrderID.Equals(cancelOrder.OrderID))
                    {
                        _limitOrders.Remove(order);
                        //set order status to cancel
                        cancelOrder.OrderStatus = OrderStatus.CANCELLED;
                        
                        if (CancellationArrived != null)
                        {
                            CancellationArrived(cancelOrder);
                        }

                        if (Logger.IsDebugEnabled)
                        {
                            Logger.Debug("Cancelled the Order: " + cancelOrder, _type.FullName,
                                                    "CancelOrderArrived");
                        }
                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "CancelOrderArrived");
            }

        }

        /// <summary>
        /// Validates market order fields
        /// </summary>
        /// <param name="marketOrder">TradeHub MarketOrder</param>
        /// <returns></returns>
        private bool ValidMarketOrder(MarketOrder marketOrder)
        {
            return !string.IsNullOrEmpty(marketOrder.OrderID) && !string.IsNullOrEmpty(marketOrder.OrderSide) && !string.IsNullOrEmpty(marketOrder.OrderExecutionProvider) && marketOrder.OrderSize>0;
        }

        /// <summary>
        /// Validates limit order fields
        /// </summary>
        /// <param name="limitOrder">TradeHub LimitOrder</param>
        /// <returns></returns>
        private bool ValideLimitOrder(LimitOrder limitOrder)
        {
            //Conditions which can cause rejection
            return limitOrder.OrderSize > 0 && !string.IsNullOrWhiteSpace(limitOrder.Security.Symbol) && limitOrder.LimitPrice > 0 && !string.IsNullOrEmpty(limitOrder.OrderID) && !string.IsNullOrEmpty(limitOrder.OrderSide);
        }

        /// <summary>
        /// Executes accepted market order
        /// </summary>
        /// <param name="marketOrder">TradeHub Market Order</param>
        private void ExecuteMarketOrder(MarketOrder marketOrder)
        {
            try
            {
                #region Create Execution Message

                decimal price = 0;
                DateTime dateTime = marketOrder.OrderDateTime;
                if(_latestTicks.ContainsKey(marketOrder.Security.Symbol))
                {
                    price = _latestTicks[marketOrder.Security.Symbol].LastPrice;
                    dateTime = _latestTicks[marketOrder.Security.Symbol].DateTime;
                }
                else if (_latestBars.ContainsKey(marketOrder.Security.Symbol))
                {
                    price = _latestBars[marketOrder.Security.Symbol].Close;
                    dateTime = _latestBars[marketOrder.Security.Symbol].DateTime;
                }
                marketOrder.OrderStatus = OrderStatus.EXECUTED;
                var newExecution =
                    new Execution(
                        new Fill(marketOrder.Security, marketOrder.OrderExecutionProvider,
                                 marketOrder.OrderID)
                            {
                                ExecutionDateTime = dateTime,
                                ExecutionPrice = price,
                                ExecutionSize = marketOrder.OrderSize,
                                AverageExecutionPrice = price,
                                ExecutionId = Guid.NewGuid().ToString(),
                                ExecutionSide = marketOrder.OrderSide,
                                ExecutionType = ExecutionType.Fill,
                                LeavesQuantity = 0,
                                OrderExecutionProvider = OrderExecutionProvider.SimulatedExchange,
                                CummalativeQuantity = marketOrder.OrderSize
                            }, marketOrder);
                
                if (ExecutionArrived != null)
                {
                    ExecutionArrived(newExecution);
                }

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug(
                        "Market Order Executed using:" + "DateTime=" + _latestBars[marketOrder.Security.Symbol].DateTime + "| Price= " + price,
                        _type.FullName, "ExecuteMarketOrder");
                }

                #endregion
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ExecuteMarketOrder");
            }
        }

        /// <summary>
        /// Called to check and execute limitorder
        /// </summary>
        /// <param name="limitOrder"></param>
        /// <param name="bar"></param>
        private bool ExecuteLimitOrder(LimitOrder limitOrder, Bar bar)
        {
            if(limitOrder.OrderDateTime.ToString().Equals(bar.DateTime.ToString()))
                return false;

            #region BUY Order

            if (limitOrder.OrderSide == OrderSide.BUY)
            {
                if (bar.Low <= limitOrder.LimitPrice)
                {
                    decimal price = BuySidePriceCalculation(limitOrder, bar);
                    if (price != 0)
                    {
                        // Remove from local list
                        _limitOrders.Remove(limitOrder);
                        limitOrder.OrderStatus = OrderStatus.EXECUTED;
                        var newExecution =
                            new Execution(
                                new Fill(limitOrder.Security, limitOrder.OrderExecutionProvider,
                                         limitOrder.OrderID)
                                    {
                                        ExecutionDateTime = bar.DateTime,
                                        ExecutionPrice = price + limitOrder.Slippage,
                                        ExecutionSize = limitOrder.OrderSize,
                                        AverageExecutionPrice = price - limitOrder.Slippage,
                                        ExecutionId = Guid.NewGuid().ToString(),
                                        ExecutionSide = limitOrder.OrderSide,
                                        ExecutionType = ExecutionType.Fill,
                                        LeavesQuantity = 0,
                                        OrderExecutionProvider = OrderExecutionProvider.SimulatedExchange,
                                        CummalativeQuantity = limitOrder.OrderSize
                                    }, limitOrder);
                        //newExecution.BarClose = bar.Close;

                        if (Logger.IsDebugEnabled)
                        {
                            Logger.Debug("Limit Order: " + limitOrder.OrderID + " Executed using: " + bar,
                                                    _type.FullName, "ExecuteLimitOrder");
                        }
                        
                        // Raise Event to notify listeners
                        if (ExecutionArrived != null)
                        {
                            ExecutionArrived(newExecution);
                        }

                        return true;
                    }
                }
            }

            #endregion

            #region SELL Order

            if (limitOrder.OrderSide == OrderSide.SELL)
            {
                if (bar.High >= limitOrder.LimitPrice)
                {
                    decimal price = SellSidePriceCalculation(limitOrder, bar);
                    if (price != 0)
                    {
                        // Remove order from local list
                        _limitOrders.Remove(limitOrder);
                        
                        limitOrder.OrderStatus = OrderStatus.EXECUTED;
                        var newExecution =
                            new Execution(
                                new Fill(limitOrder.Security, limitOrder.OrderExecutionProvider,
                                         limitOrder.OrderID)
                                    {
                                        ExecutionDateTime = bar.DateTime,
                                        ExecutionPrice = price - limitOrder.Slippage,
                                        ExecutionSize = limitOrder.OrderSize,
                                        AverageExecutionPrice = price + limitOrder.Slippage,
                                        ExecutionId = Guid.NewGuid().ToString(),
                                        ExecutionSide = limitOrder.OrderSide,
                                        ExecutionType = ExecutionType.Fill,
                                        LeavesQuantity = 0,
                                        OrderExecutionProvider = OrderExecutionProvider.SimulatedExchange,
                                        CummalativeQuantity = limitOrder.OrderSize
                                    }, limitOrder);

                        if (Logger.IsDebugEnabled)
                        {
                            Logger.Debug("Limit Order: " + limitOrder.OrderID + " Executed using: " + bar,
                                                    _type.FullName, "ExecuteLimitOrder");
                        }
                        
                        // Raise Event to notify listeners
                        if (ExecutionArrived != null)
                        {
                            ExecutionArrived.Invoke(newExecution);
                        }
                        return true;
                    }
                }
            }

            #endregion

            return false;
        }

        /// <summary>
        /// Calculation for buy side limit orders
        /// </summary>
        /// <returns></returns>
        private decimal BuySidePriceCalculation(LimitOrder limitOrder, Bar bar)
        {
            decimal price = 0;
            if (bar.Open == bar.Low)
            {
                price = bar.Low;
            }
            else if (bar.Low < bar.Open)
            {
                if (bar.Open <= limitOrder.LimitPrice)
                    price = bar.Open;
                else if (limitOrder.LimitPrice < bar.Open)
                    price = limitOrder.LimitPrice;
            }
            return price;
        }

        /// <summary>
        /// Calculation for sell side limit orders
        /// </summary>
        /// <returns></returns>
        private decimal SellSidePriceCalculation(LimitOrder limitOrder,Bar bar)
        {
            decimal price = 0;
            if (bar.Open == bar.High)
            {
                price = bar.Open;
            }
            else if (bar.Open < bar.High)
            {
                if (limitOrder.LimitPrice<=bar.Open)
                    price = bar.Open;
                else if (limitOrder.LimitPrice > bar.Open)
                    price = limitOrder.LimitPrice;
            }

            return price;
        }

        /// <summary>
        /// Removes the order data stored in local order maps
        /// </summary>
        public void Clear()
        {
            _limitOrders.Clear();
        }
    }
}
