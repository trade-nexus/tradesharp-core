
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
    /// <summary>
    /// Handles all Market Order Executions
    /// </summary>
    public class SimulateMarketOrder
    {
        private Type _type = typeof(SimulateMarketOrder);
        public event Action<Order> NewArrived;
        public event Action<Execution> NewExecution; 
        private IList<MarketOrder> _marketOrders;
        public event Action<Rejection> MarketOrderRejection;

        /// <summary>
        /// Key = DateTime
        /// Value = TradeHub Tick
        /// </summary>
        private Dictionary<DateTime, Tick> _ticksCollection = new Dictionary<DateTime, Tick>();
        
        /// <summary>
        /// Key = DateTime
        /// Value = TradeHub Tick
        /// </summary>
        //public Dictionary<DateTime, Tick> TicksCollection  
        public Dictionary<DateTime, Tick> TicksCollection
        {
            get { return _ticksCollection; }
            set { _ticksCollection = value; }
        }
        /// <summary>
        /// Constructor:
        /// Initialize list Of Market Order
        /// </summary>
        public SimulateMarketOrder()
        {
            try
            {
                _marketOrders = new List<MarketOrder>();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SimulateMarketOrder");
            }
        }

        /// <summary>
        /// Adds new MarketOrder to list
        /// If validation of order is successful it sends NewArrived Message.
        /// In case of failure it sends sends rejection. 
        /// </summary>
        /// <param name="marketOrder"></param>
        public void NewMarketOrderArrived(MarketOrder marketOrder)
        {
            try
            {
                if (ValidateMarketOrder(marketOrder))
                {
                    var order = new Order(marketOrder.OrderID, marketOrder.OrderSide, marketOrder.OrderSize,
                                          marketOrder.OrderTif,
                                          marketOrder.OrderCurrency, marketOrder.Security,
                                          marketOrder.OrderExecutionProvider);

                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("New Market Order Received :" + marketOrder, _type.FullName, "NewMarketOrderArrived");
                    }

                    if (NewArrived != null)
                    {
                        NewArrived(order);
                    }

                    // Execute Market Order
                    ExecuteMarketOrder(marketOrder);

                    //// Add to collection
                    //_marketOrders.Add(marketOrder);
                }
                else
                {
                    Rejection rejection = new Rejection(marketOrder.Security, OrderExecutionProvider.SimulatedExchange)
                        {
                            OrderId = marketOrder.OrderID,
                            DateTime = DateTime.Now,
                            RejectioReason = "Invaild Price Or Size"
                        };
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Rejection :" + rejection, _type.FullName, "NewMarketOrderArrived");
                    }
                    if (MarketOrderRejection != null)
                    {
                        MarketOrderRejection.Invoke(rejection);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "NewMarketOrderArrived");
            }
        }

        /// <summary>
        /// Validates Market Order 
        /// </summary>
        /// <param name="marketOrder"></param>
        /// <returns></returns>
        private bool ValidateMarketOrder(MarketOrder marketOrder)
        {
            return !string.IsNullOrEmpty(marketOrder.OrderID) && !string.IsNullOrEmpty(marketOrder.OrderSide) && !string.IsNullOrEmpty(marketOrder.OrderExecutionProvider);
        }

        /// <summary>
        /// New Bar Arrived From Simulated Market Data Exchange.
        /// </summary>
        /// <param name="bar"></param>
        public void NewBarArrived(Bar bar)
        {
            try
            {
                lock (_marketOrders)
                {
                    var selectedlist = _marketOrders.Where(x => x.Security.Symbol == bar.Security.Symbol);
                    foreach (var marketOrder in selectedlist)
                    {
                        var newExecution =
                            new Execution(
                                new Fill(marketOrder.Security, marketOrder.OrderExecutionProvider,
                                         marketOrder.OrderID)
                                    {
                                        DateTime = bar.DateTime,
                                        ExecutionPrice = bar.Close,
                                        ExecutionSize = marketOrder.OrderSize,
                                        AverageExecutionPrice = bar.Close,
                                        ExecutionId = Guid.NewGuid().ToString(),
                                        ExecutionSide = marketOrder.OrderSide,
                                        ExecutionType = ExecutionType.Fill,
                                        LeavesQuantity = 0,
                                        OrderExecutionProvider = OrderExecutionProvider.SimulatedExchange,
                                        CummalativeQuantity = marketOrder.OrderSize
                                    }, marketOrder);
                        if (NewExecution != null)
                        {
                            NewExecution.Invoke(newExecution);
                        }
                        _marketOrders.Remove(marketOrder);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "NewBarArrived");
            }
        }

        /// <summary>
        /// New Tick Arrived from Simulated Market Data Exchange
        /// </summary>
        /// <param name="tick"></param>
        public void NewTickArrived(Tick tick)
        {
            try
            {
                lock (_marketOrders)
                {
                    var selectedlist = _marketOrders.Where(x => x.Security.Symbol == tick.Security.Symbol);
                    foreach (var marketOrder in selectedlist)
                    {
                        _marketOrders.Remove(marketOrder);

                        var newExecution =
                            new Execution(
                                new Fill(marketOrder.Security, marketOrder.OrderExecutionProvider,
                                         marketOrder.OrderID)
                                    {
                                        ExecutionDateTime = tick.DateTime,
                                        ExecutionPrice = tick.LastPrice,
                                        ExecutionSize = marketOrder.OrderSize,
                                        AverageExecutionPrice = tick.LastPrice,
                                        ExecutionId = Guid.NewGuid().ToString(),
                                        ExecutionSide = marketOrder.OrderSide,
                                        ExecutionType = ExecutionType.Fill,
                                        LeavesQuantity = 0,
                                        OrderExecutionProvider = OrderExecutionProvider.SimulatedExchange,
                                        CummalativeQuantity = marketOrder.OrderSize
                                    }, marketOrder);
                        if (NewExecution != null)
                        {
                            NewExecution.Invoke(newExecution);
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
        /// <param name="marketOrder">TradeHub Market Order</param>
        private void ExecuteMarketOrder(MarketOrder marketOrder)
        {
            try
            {
                Tick tick = null;
                DateTime key;

                if (marketOrder.OrderDateTime.Hour.Equals(15) && marketOrder.OrderDateTime.Minute.Equals(58) && marketOrder.OrderDateTime.Second.Equals(56))
                {
                    key = marketOrder.OrderDateTime.AddHours(17);
                    key = key.AddMinutes(32);
                    key = key.AddSeconds(-42);
                }
                else if (marketOrder.OrderDateTime.Second == 56 || marketOrder.OrderDateTime.Second == 11)
                {
                    key = marketOrder.OrderDateTime.AddSeconds(18);
                }
                else
                {
                    key = marketOrder.OrderDateTime.AddSeconds(14);
                }

                #region Create Execution Message

                while (true)
                {
                    if (_ticksCollection.TryGetValue(key, out tick))
                    {
                        var newExecution =
                            new Execution(
                                new Fill(marketOrder.Security, marketOrder.OrderExecutionProvider,
                                         marketOrder.OrderID)
                                    {
                                        ExecutionDateTime = tick.DateTime,
                                        ExecutionPrice = tick.LastPrice,
                                        ExecutionSize = marketOrder.OrderSize,
                                        AverageExecutionPrice = tick.LastPrice,
                                        ExecutionId = Guid.NewGuid().ToString(),
                                        ExecutionSide = marketOrder.OrderSide,
                                        ExecutionType = ExecutionType.Fill,
                                        LeavesQuantity = 0,
                                        OrderExecutionProvider = OrderExecutionProvider.SimulatedExchange,
                                        CummalativeQuantity = marketOrder.OrderSize
                                    }, marketOrder);
                        if (NewExecution != null)
                        {
                            NewExecution.Invoke(newExecution);
                        }

                        if (Logger.IsDebugEnabled)
                        {
                            Logger.Debug("Market Order Executed using:" + tick, _type.FullName, "ExecuteMarketOrder");
                        }
                        break;
                    }

                    // Try to get the tick from next minute if the key is not found
                    key = key.AddMinutes(1);
                }

                #endregion
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ExecuteMarketOrder");
            }
        }

        /// <summary>
        /// When Market Get Disconnected
        /// </summary>
        public void OnSimulatedMarketDisconnect()
        {
            try
            {
                _marketOrders.Clear();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnSimulatedMarketDisconnect");
            }
        }
    }
}
