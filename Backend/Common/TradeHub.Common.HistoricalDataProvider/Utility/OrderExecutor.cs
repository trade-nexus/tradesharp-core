﻿using System;
using System.Collections.Generic;
using System.Linq;
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.Repositories;
using OrderStatus = TradeHub.Common.Core.Constants.OrderStatus;

namespace TradeHub.Common.HistoricalDataProvider.Utility
{
    /// <summary>
    /// Responsible for providing order management (Server Side) Modified for PETER/TED
    /// </summary>
    public class OrderExecutor : IOrderExecutor
    {
        private Type _type = typeof (OrderExecutor);
        private AsyncClassLogger _asyncClassLogger;

        public event Action<Order> NewOrderArrived;
        public event Action<Execution> ExecutionArrived;
        public event Action<Order> CancellationArrived;
        public event Action<Rejection> RejectionArrived;

        //received Limit order list
        private SortedList<int, LimitOrder> _limitOrders;

        private DateTime _lastdateTime;
        private decimal _lastTradePrice;
        private decimal _lastTradeSize;

        private Bar _latestBar;

        /// <summary>
        /// Argument Constructor without persistence
        /// </summary>
        /// <param name="asyncClassLogger"> </param>
        public OrderExecutor(AsyncClassLogger asyncClassLogger )
        {
            _asyncClassLogger = asyncClassLogger;
            _limitOrders = new SortedList<int, LimitOrder>();
        }
        
        /// <summary>
        /// Called when new tick is recieved
        /// </summary>
        /// <param name="tick">TradeHub Tick</param>
        public void TickArrived(Tick tick)
        {
            //if (tick.HasTrade)
            //{
            //    _lastTradePrice = Math.Round(tick.LastPrice, 4);
            //    _lastTradeSize = tick.LastSize;

            //}
            //_lastdateTime = tick.DateTime;

            //foreach (var limitOrder in _limitOrders.ToArray())
            //{
            //    ExecuteLimitOrder(limitOrder,tick);
            //}
        }

        /// <summary>
        /// Called when new Bar is received
        /// </summary>
        /// <param name="bar">TradeHub Bar</param>
        public void BarArrived(Bar bar)
        {
            _latestBar = bar;

            foreach (KeyValuePair<int, LimitOrder> limitOrder in _limitOrders.ToArray())
            {
                if(limitOrder.Key.Equals(3))
                    if (_limitOrders.ContainsKey(2))
                        continue;
                if(ExecuteLimitOrder(limitOrder.Value, bar))
                    break;
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
                
                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Rejection :" + rejection, _type.FullName, "NewMarketOrderArrived");
                }
               
                if (RejectionArrived != null)
                {
                    RejectionArrived(rejection);
                }
            }
            //if (_orderRepository != null)
            //{
            //    _orderRepository.AddUpdate(marketOrder);
            //}
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
                    if (_asyncClassLogger.IsInfoEnabled)
                    {
                        _asyncClassLogger.Info("New Arrived :" + order, _type.FullName, "NewLimitOrderArrived");
                    }


                    // get index
                    int index;
                    int.TryParse(limitOrder.Remarks.Split('-')[1], out index);

                    //add limit order to list
                    _limitOrders.Add(index, limitOrder);

                    if (NewOrderArrived != null)
                    {
                        NewOrderArrived(order);
                    }

                    //Tick tick=new Tick();
                    //tick.LastPrice = _lastTradePrice;
                    //tick.LastSize = _lastTradeSize;
                    //tick.DateTime = _lastdateTime;
                    //tick.Security = new Security() {Symbol = limitOrder.Security.Symbol};

                    //ExecuteLimitOrder(limitOrder,_latestBar);
                }
                else
                {
                    Rejection rejection = new Rejection(limitOrder.Security, OrderExecutionProvider.SimulatedExchange) { OrderId = limitOrder.OrderID, DateTime = DateTime.Now, RejectioReason = "Invaild Price Or Size" };
                    limitOrder.OrderStatus = OrderStatus.REJECTED;
                    if (_asyncClassLogger.IsInfoEnabled)
                    {
                        _asyncClassLogger.Info("Rejection :" + rejection, _type.FullName, "NewLimitOrderArrived");
                    }
                    if (RejectionArrived != null)
                    {
                        RejectionArrived.Invoke(rejection);
                    }
                }
                //if (_orderRepository != null)
                //{
                //    _orderRepository.AddUpdate(limitOrder);
                //}
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "NewLimitOrderArrived");
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
                    if (order.Value.OrderID.Equals(cancelOrder.OrderID))
                    {
                        _limitOrders.Remove(order.Key);
                        //set order status to cancel
                        cancelOrder.OrderStatus = OrderStatus.CANCELLED;
                        
                        if (CancellationArrived != null)
                        {
                            CancellationArrived(cancelOrder);
                        }

                        if (_asyncClassLogger.IsDebugEnabled)
                        {
                            _asyncClassLogger.Debug("Cancelled the Order: " + cancelOrder, _type.FullName,
                                                    "CancelOrderArrived");
                        }
                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception,_type.FullName,"CancelOrderArrived");
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

                // Adjust Slippage
                if (marketOrder.OrderSide == OrderSide.BUY||marketOrder.OrderSide==OrderSide.COVER)
                {
                    price = marketOrder.TriggerPrice + marketOrder.Slippage;
                }
                else if (marketOrder.OrderSide == OrderSide.SELL||marketOrder.OrderSide==OrderSide.SHORT)
                {
                    price = marketOrder.TriggerPrice - marketOrder.Slippage;
                }

                // Get Signal Price from ENTRY/EXIT Order
                //if (marketOrder.Remarks.Contains("ENTRY"))
                {
                    string[] remarksItems = marketOrder.Remarks.Split(':');
                    if (remarksItems.Length > 1)
                    {
                        decimal triggerPrice;
                        if(Decimal.TryParse(remarksItems[1], out triggerPrice))
                        {
                            marketOrder.TriggerPrice = triggerPrice;
                        }
                        marketOrder.Remarks = (marketOrder.Remarks.Replace(remarksItems[1], "")).Replace(":", "");
                    }
                }

                // Add OHLC values
                marketOrder.Remarks += (" | OHLC: " + _latestBar.Open + ":" + _latestBar.High + ":" + _latestBar.Low + ":" + _latestBar.Close);
                marketOrder.OrderStatus = OrderStatus.EXECUTED;
                var newExecution =
                    new Execution(
                        new Fill(marketOrder.Security, marketOrder.OrderExecutionProvider,
                                 marketOrder.OrderID)
                            {
                                ExecutionDateTime = _latestBar.DateTime, //marketOrder.OrderDateTime,
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
                //newExecution.BarClose = _latestBar.Close;
                
                if (ExecutionArrived != null)
                {
                    ExecutionArrived(newExecution);
                }

                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug(
                        "Market Order Executed using:" + "DateTime=" + _latestBar.DateTime + "| Price= " + price,
                        _type.FullName, "ExecuteMarketOrder");
                }

                #endregion
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "ExecuteMarketOrder");
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
                        // Get Order Index
                        var index = _limitOrders.IndexOfValue(limitOrder);

                        // Remove from local list
                        _limitOrders.RemoveAt(index);

                        // Add OHLC values
                        limitOrder.Remarks += (" | OHLC: " + _latestBar.Open + ":" + _latestBar.High + ":" + _latestBar.Low +
                                               ":" + _latestBar.Close);
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

                        if (_asyncClassLogger.IsDebugEnabled)
                        {
                            _asyncClassLogger.Debug("Limit Order: " + limitOrder.OrderID + " Executed using: " + bar,
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
                        // Get Order Index
                        var index = _limitOrders.IndexOfValue(limitOrder);

                        // Remove order from local list
                        _limitOrders.RemoveAt(index);

                        // Add OHLC values
                        limitOrder.Remarks += (" | OHLC: " + _latestBar.Open + ":" + _latestBar.High + ":" + _latestBar.Low +
                                               ":" + _latestBar.Close);
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
                        //newExecution.BarClose = bar.Close;

                        if (_asyncClassLogger.IsDebugEnabled)
                        {
                            _asyncClassLogger.Debug("Limit Order: " + limitOrder.OrderID + " Executed using: " + bar,
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
