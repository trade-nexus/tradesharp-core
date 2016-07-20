
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;

namespace TradeHub.SimulatedExchange.SimulatorControler
{
    public class SimulateLimitOrder
    {
        private Type _type = typeof(SimulateLimitOrder);
        private IList<LimitOrder> _limitOrders;
        public event Action<Order> NewArrived;
        public event Action<Execution> NewExecution;
        public event Action<Rejection> LimitOrderRejection;

        /// <summary>
        /// Key = DateTime
        /// Value = TradeHub Tick
        /// </summary>
        private Dictionary<DateTime, Tick> _ticksCollection = new Dictionary<DateTime, Tick>();

        /// <summary>
        /// Key = DateTime
        /// Value = TradeHub Tick
        /// </summary>
        public Dictionary<DateTime, Tick> TicksCollection
        {
            get { return _ticksCollection; }
            set { _ticksCollection = value; }
        }

        /// <summary>
        /// Constructor:
        /// Initialize Limit Order List.  
        /// </summary>
        public SimulateLimitOrder()
        {
            try
            {
                _limitOrders=new List<LimitOrder>();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SimulateLimitOrder");
            }
        }

        /// <summary>
        /// Adds new LimitOrder to list
        /// If validation of order is successful it sends NewArrived Message.
        /// In case of failure it sends sends rejection. 
        /// </summary>
        public void NewLimitOrderArrived(LimitOrder limitOrder)
        {
            try
            {
                if (ValidateMarketOrder(limitOrder))
                {
                    var order = new Order(limitOrder.OrderID,
                                          limitOrder.OrderSide, 
                                          limitOrder.OrderSize,
                                          limitOrder.OrderTif,
                                          limitOrder.OrderCurrency,
                                          limitOrder.Security,
                                          limitOrder.OrderExecutionProvider);
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("New Arrived :" + order, _type.FullName, "NewLimitOrderArrived");
                    }
                    if (NewArrived != null)
                    {
                        NewArrived.Invoke(order);
                    }

                    // Execute Limit Order
                    ExecuteLimitOrder(limitOrder);

                    //// Add to List
                    //_limitOrders.Add(limitOrder);
                }
                else
                {
                    Rejection rejection=new Rejection(limitOrder.Security,OrderExecutionProvider.SimulatedExchange){OrderId = limitOrder.OrderID,DateTime = DateTime.Now,RejectioReason = "Invaild Price Or Size"};
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Rejection :" + rejection, _type.FullName, "NewLimitOrderArrived");
                    }
                    if (LimitOrderRejection!=null)
                    {
                        LimitOrderRejection.Invoke(rejection);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "NewLimitOrderArrived");
            }
        }

        /// <summary>
        /// Validates Limit Order 
        /// </summary>
        /// <param name="limitOrder"> </param>
        /// <returns></returns>
        private bool ValidateMarketOrder(LimitOrder limitOrder)
        {
            //Conditions which can cause rejection
            return limitOrder.OrderSize > 0 && !string.IsNullOrWhiteSpace(limitOrder.Security.Symbol) && limitOrder.LimitPrice > 0;
        }

        /// <summary>
        /// New Are Arrived From Simulated data service.
        /// </summary>
        /// <param name="bar"></param>
        public void NewBarArrived(Bar bar)
        {
            try
            {
                lock (_limitOrders)
                {
                    var selectedlist = _limitOrders.Where(x => x.Security.Symbol == bar.Security.Symbol);
                    foreach (var limitOrder in selectedlist.ToList())
                    {
                        if (limitOrder.Security.Symbol == bar.Security.Symbol)
                        {
                            if (limitOrder.OrderSide == OrderSide.BUY && bar.Low <= limitOrder.LimitPrice)
                            {
                                var newExecution =
                                    new Execution(
                                        new Fill(limitOrder.Security, limitOrder.OrderExecutionProvider,
                                                 limitOrder.OrderID)
                                            {
                                                DateTime = bar.DateTime,
                                                ExecutionPrice = limitOrder.LimitPrice,
                                                ExecutionSize = limitOrder.OrderSize,
                                                AverageExecutionPrice = bar.Close,
                                                ExecutionId = Guid.NewGuid().ToString(),
                                                ExecutionSide = limitOrder.OrderSide,
                                                ExecutionType = ExecutionType.Fill,
                                                LeavesQuantity = 0,
                                                OrderExecutionProvider = OrderExecutionProvider.SimulatedExchange,
                                                CummalativeQuantity = limitOrder.OrderSize
                                            }, limitOrder);
                                if (NewExecution != null)
                                {
                                    NewExecution.Invoke(newExecution);
                                }
                                _limitOrders.Remove(limitOrder);
                            }

                            if (limitOrder.OrderSide == OrderSide.SELL && bar.Low > limitOrder.LimitPrice)
                            {
                                var newExecution =
                                    new Execution(
                                        new Fill(limitOrder.Security, limitOrder.OrderExecutionProvider,
                                                 limitOrder.OrderID)
                                            {
                                                DateTime = bar.DateTime,
                                                ExecutionPrice = limitOrder.LimitPrice,
                                                ExecutionSize = limitOrder.OrderSize,
                                                AverageExecutionPrice = bar.Close,
                                                ExecutionId = Guid.NewGuid().ToString(),
                                                ExecutionSide = limitOrder.OrderSide,
                                                ExecutionType = ExecutionType.Fill,
                                                LeavesQuantity = 0,
                                                OrderExecutionProvider = OrderExecutionProvider.SimulatedExchange,
                                                CummalativeQuantity = limitOrder.OrderSize
                                            }, limitOrder);
                                if (NewExecution != null)
                                {
                                    NewExecution.Invoke(newExecution);
                                }
                                _limitOrders.Remove(limitOrder);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "NewBarArrived");
            }
        }

        /// <summary>
        /// New Tick Arrived From Simulated data service.
        /// </summary>
        /// <param name="tick"></param>
        public void NewTickArrived(Tick tick)
        {
            try
            {
                lock (_limitOrders)
                {
                    var selectedlist = _limitOrders.Where(x => x.Security.Symbol == tick.Security.Symbol);
                    foreach (var limitOrder in selectedlist.ToList())
                    {
                        if (limitOrder.Security.Symbol == tick.Security.Symbol)
                        {
                            if (limitOrder.OrderSide == OrderSide.BUY && tick.LastPrice <= limitOrder.LimitPrice)
                            {
                                var newExecution =
                                    new Execution(
                                        new Fill(limitOrder.Security, limitOrder.OrderExecutionProvider,
                                                 limitOrder.OrderID)
                                            {
                                                ExecutionDateTime = tick.DateTime,
                                                ExecutionPrice = limitOrder.LimitPrice,
                                                ExecutionSize = limitOrder.OrderSize,
                                                AverageExecutionPrice = tick.LastPrice,
                                                ExecutionId = Guid.NewGuid().ToString(),
                                                ExecutionSide = limitOrder.OrderSide,
                                                ExecutionType = ExecutionType.Fill,
                                                LeavesQuantity = 0,
                                                OrderExecutionProvider = OrderExecutionProvider.SimulatedExchange,
                                                CummalativeQuantity = limitOrder.OrderSize
                                            }, limitOrder);
                                if (NewExecution != null)
                                {
                                    NewExecution.Invoke(newExecution);
                                }
                                _limitOrders.Remove(limitOrder);
                            }

                            if (limitOrder.OrderSide == OrderSide.SELL && tick.LastPrice > limitOrder.LimitPrice)
                            {
                                var newExecution =
                                    new Execution(
                                        new Fill(limitOrder.Security, limitOrder.OrderExecutionProvider,
                                                 limitOrder.OrderID)
                                            {
                                                ExecutionDateTime = tick.DateTime,
                                                ExecutionPrice = limitOrder.LimitPrice,
                                                ExecutionSize = limitOrder.OrderSize,
                                                AverageExecutionPrice = tick.LastPrice,
                                                ExecutionId = Guid.NewGuid().ToString(),
                                                ExecutionSide = limitOrder.OrderSide,
                                                ExecutionType = ExecutionType.Fill,
                                                LeavesQuantity = 0,
                                                OrderExecutionProvider = OrderExecutionProvider.SimulatedExchange,
                                                CummalativeQuantity = limitOrder.OrderSize
                                            }, limitOrder);
                                if (NewExecution != null)
                                {
                                    NewExecution.Invoke(newExecution);
                                }
                                _limitOrders.Remove(limitOrder);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "NewTickArrived");
            }
        }

        /// <summary>
        /// Executes accepted market order
        /// </summary>
        /// <param name="limitOrder">TradeHub Limit Order</param>
        private void ExecuteLimitOrder(LimitOrder limitOrder)
        {
            try
            {
                DateTime[] ticksCollectionKeys;
                lock (_ticksCollection)
                {
                    ticksCollectionKeys = _ticksCollection.Keys.ToArray();
                }

                foreach (DateTime key in ticksCollectionKeys)
                {
                    if (key > limitOrder.OrderDateTime)
                    {
                        Tick tick;
                        if (_ticksCollection.TryGetValue(key, out tick))
                        {
                            #region BUY Order

                            if (limitOrder.OrderSide == OrderSide.BUY &&
                                tick.LastPrice <= limitOrder.LimitPrice)
                            {
                                var newExecution =
                                    new Execution(
                                        new Fill(limitOrder.Security, limitOrder.OrderExecutionProvider,
                                                 limitOrder.OrderID)
                                            {
                                                ExecutionDateTime = tick.DateTime,
                                                ExecutionPrice = limitOrder.LimitPrice,
                                                ExecutionSize = limitOrder.OrderSize,
                                                AverageExecutionPrice = tick.LastPrice,
                                                ExecutionId = Guid.NewGuid().ToString(),
                                                ExecutionSide = limitOrder.OrderSide,
                                                ExecutionType = ExecutionType.Fill,
                                                LeavesQuantity = 0,
                                                OrderExecutionProvider = OrderExecutionProvider.SimulatedExchange,
                                                CummalativeQuantity = limitOrder.OrderSize
                                            }, limitOrder);
                                if (NewExecution != null)
                                {
                                    NewExecution.Invoke(newExecution);
                                }

                                if (Logger.IsDebugEnabled)
                                {
                                    Logger.Debug("Limit Order Executed using:" + tick, _type.FullName, "ExecuteLimitOrder");
                                }

                                return;
                            }

                            #endregion

                            #region SELL Order

                            if (limitOrder.OrderSide == OrderSide.SELL &&
                                tick.LastPrice > limitOrder.LimitPrice)
                            {
                                var newExecution =
                                    new Execution(
                                        new Fill(limitOrder.Security, limitOrder.OrderExecutionProvider,
                                                 limitOrder.OrderID)
                                            {
                                                ExecutionDateTime = tick.DateTime,
                                                ExecutionPrice = limitOrder.LimitPrice,
                                                ExecutionSize = limitOrder.OrderSize,
                                                AverageExecutionPrice = tick.LastPrice,
                                                ExecutionId = Guid.NewGuid().ToString(),
                                                ExecutionSide = limitOrder.OrderSide,
                                                ExecutionType = ExecutionType.Fill,
                                                LeavesQuantity = 0,
                                                OrderExecutionProvider = OrderExecutionProvider.SimulatedExchange,
                                                CummalativeQuantity = limitOrder.OrderSize
                                            }, limitOrder);
                                if (NewExecution != null)
                                {
                                    NewExecution.Invoke(newExecution);
                                }

                                if (Logger.IsDebugEnabled)
                                {
                                    Logger.Debug("Limit Order Executed using:" + tick, _type.FullName, "ExecuteLimitOrder");
                                }

                                return;
                            }

                            #endregion
                        }
                    }
                }

                //foreach (KeyValuePair<DateTime, Tick> keyValuePair in _ticksCollection)
                //{
                //    if (keyValuePair.Key > limitOrder.OrderDateTime)
                //    {
                //        #region BUY Order

                //        if (limitOrder.OrderSide == OrderSide.BUY &&
                //            keyValuePair.Value.LastPrice <= limitOrder.LimitPrice)
                //        {
                //            var newExecution =
                //                new Execution(
                //                    new Fill(limitOrder.Security, limitOrder.OrderExecutionProvider,
                //                             limitOrder.OrderID)
                //                        {
                //                            ExecutionDateTime = keyValuePair.Value.DateTime,
                //                            ExecutionPrice = limitOrder.LimitPrice,
                //                            ExecutionSize = limitOrder.OrderSize,
                //                            AverageExecutionPrice = keyValuePair.Value.LastPrice,
                //                            ExecutionId = Guid.NewGuid().ToString(),
                //                            ExecutionSide = limitOrder.OrderSide,
                //                            ExecutionType = ExecutionType.Fill,
                //                            LeavesQuantity = 0,
                //                            OrderExecutionProvider = OrderExecutionProvider.SimulatedExchange,
                //                            CummalativeQuantity = limitOrder.OrderSize
                //                        }, limitOrder);
                //            if (NewExecution != null)
                //            {
                //                NewExecution.Invoke(newExecution);
                //            }

                //            if (Logger.IsDebugEnabled)
                //            {
                //                Logger.Debug("Limit Order Executed using:" + keyValuePair.Value, _type.FullName,
                //                             "ExecuteLimitOrder");
                //            }

                //            return;
                //        }

                //        #endregion

                //        #region SELL Order

                //        if (limitOrder.OrderSide == OrderSide.SELL &&
                //            keyValuePair.Value.LastPrice > limitOrder.LimitPrice)
                //        {
                //            var newExecution =
                //                new Execution(
                //                    new Fill(limitOrder.Security, limitOrder.OrderExecutionProvider,
                //                             limitOrder.OrderID)
                //                        {
                //                            ExecutionDateTime = keyValuePair.Value.DateTime,
                //                            ExecutionPrice = limitOrder.LimitPrice,
                //                            ExecutionSize = limitOrder.OrderSize,
                //                            AverageExecutionPrice = keyValuePair.Value.LastPrice,
                //                            ExecutionId = Guid.NewGuid().ToString(),
                //                            ExecutionSide = limitOrder.OrderSide,
                //                            ExecutionType = ExecutionType.Fill,
                //                            LeavesQuantity = 0,
                //                            OrderExecutionProvider = OrderExecutionProvider.SimulatedExchange,
                //                            CummalativeQuantity = limitOrder.OrderSize
                //                        }, limitOrder);
                //            if (NewExecution != null)
                //            {
                //                NewExecution.Invoke(newExecution);
                //            }

                //            if (Logger.IsDebugEnabled)
                //            {
                //                Logger.Debug("Limit Order Executed using:" + keyValuePair.Value, _type.FullName,
                //                             "ExecuteLimitOrder");
                //            }

                //            return;
                //        }

                //        #endregion
                //    }
                //}

                // Add to List
                _limitOrders.Add(limitOrder);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ExecuteLimitOrder");
            }
        }

        /// <summary>
        /// When Market Get Disconnected
        /// </summary>
        public void OnSimulatedMarketDisconnect()
        {
            try
            {
                _limitOrders.Clear();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnSimulatedMarketDisconnect");
            }
        }
    }
}
