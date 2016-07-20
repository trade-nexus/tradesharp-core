using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.OrderExecutionProvider;
using TradeHub.OrderExecutionProviders.Simulator.Service;
using TradeHub.OrderExecutionProviders.Simulator.Utility;
using TradeHubConstants = TradeHub.Common.Core.Constants;

namespace TradeHub.OrderExecutionProviders.Simulator.Provider
{
    /// <summary>
    /// Provider Simulated Order Executions
    /// </summary>
    public class SimulatedOrderExecutionProvider: IMarketOrderProvider, ILimitOrderProvider
    {
        private Type _type = typeof(SimulatedOrderExecutionProvider);

        // Name of the Order Execution Provider
        private readonly string _orderExecutionProviderName = TradeHubConstants.OrderExecutionProvider.Simulated;

        // Responsible for Transforming incoming message to TradeHub messages
        private readonly SimulatedOrderProcessor _orderProcessor;

        /// <summary>
        /// Contains all the new Order which are yet to be accepted
        /// KEY = Cleint Order ID
        /// Value = TradeHub Order
        /// </summary>
        private Dictionary<string, Order> _newOrders= new Dictionary<string, Order>();

        /// <summary>
        /// Contains all the accepted Orders
        /// KEY = Cleint Order ID
        /// Value = TradeHub Order
        /// </summary>
        private Dictionary<string, Order> _acceptedOrders = new Dictionary<string, Order>();

        /// <summary>
        /// Contains all the filled Orders
        /// KEY = Cleint Order ID
        /// Value = TradeHub Order
        /// </summary>
        private Dictionary<string, Order> _filledOrders = new Dictionary<string, Order>();

        /// <summary>
        /// Contains all Locate Order waiting for a response
        /// KEY = BW Client Order ID
        /// Value = <see cref="LimitOrder"/>
        /// </summary>
        private ConcurrentDictionary<string, LimitOrder> _locateOrders = new ConcurrentDictionary<string, LimitOrder>();

        /// <summary>
        /// Contains Blackwood Locate Order Symbols to Local IDs Map
        /// KEY = Blackwood Symbol
        /// VALUE = Local ID (Strategy + "|" + OrderID)
        /// </summary>
        private ConcurrentDictionary<string, string> _locateOrdersToLocalIdsMap = new ConcurrentDictionary<string, string>();

        // Shows the state of the simulator
        private bool _isConnected = false;

        // Dedicated Thread to read user input
        private Thread _readerThread;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="orderProcessor">SimulatedOrderProcessor used for handling custom orders</param>
        public SimulatedOrderExecutionProvider(SimulatedOrderProcessor orderProcessor)
        {
            _orderProcessor = orderProcessor;

            // Hook Simulated Order Processor Events
            RegisterOrderProcessorEvents();
        }

        /// <summary>
        /// Registers Simulated Order Processor Events
        /// </summary>
        private void RegisterOrderProcessorEvents()
        {
            _orderProcessor.NewArrived += OnNewArrived;
            _orderProcessor.CancellationArrived += OnCancellationArrived;
            _orderProcessor.ExecutionArrived += OnExecutionArrived;
            _orderProcessor.RejectionArrived += OnRejectionArrived;
            _orderProcessor.LocateMessageArrived += OnLocateMessageArrived;
            _orderProcessor.PositionArrived += OnPositionArrived;
        }

        #region Implementation of IOrderExecutionProvider

        private event Action<Position> _onPositionMessage;

        public event Action<string> LogonArrived;
        public event Action<string> LogoutArrived;
        public event Action<Rejection> OrderRejectionArrived;
        public event Action<LimitOrder> OnLocateMessage;
        public event Action<Position> OnPositionMessage
        {
            add
            {
                if (_onPositionMessage == null)
                    _onPositionMessage += value;
            }
            remove
            {
                _onPositionMessage -= value;
            }

        }

        /// <summary>
        /// Connects/Starts a client
        /// </summary>
        public bool Start()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Starting Simualtor", _type.FullName, "Start");
                }

                ConsoleWriter.WriteLine(ConsoleColor.Green, "Starting simulator");

                // Start Listening to input data
                _readerThread = new Thread(ReadInput);
                _readerThread.Start();

                _isConnected = true;

                // Raise Logon Event
                OnLogonArrived();

                return _isConnected;
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "Start");
                return false;
            }
        }

        /// <summary>
        /// Disconnects/Stops a client
        /// </summary>
        public bool Stop()
        {
            try
            {
                if (_isConnected)
                {
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Stopping Simualtor", _type.FullName, "Stop");
                    }
                    ConsoleWriter.WriteLine(ConsoleColor.Red, "Stopping simulator");

                    // Raise Logout Event
                    OnLogoutArrived();

                    _isConnected = false;
                }
                else
                {
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Simualtor already stopped", _type.FullName, "Stop");
                    }
                    ConsoleWriter.WriteLine(ConsoleColor.Red, "Simualtor already stopped");
                }
                return true;
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "Stop");
                return false;
            }
        }

        /// <summary>
        /// Is Order Execution client connected
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            return _isConnected;
        }

        /// <summary>
        /// Sends Locate message Accepted/Rejected response to Broker
        /// </summary>
        /// <param name="locateResponse"> </param>
        /// <returns></returns>
        public bool LocateMessageResponse(LocateResponse locateResponse)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Locate Response received: " + locateResponse, _type.FullName, "LocateMessageResponse");
                }

                LimitOrder locateMsg;
                if (_locateOrders.TryRemove(locateResponse.OrderId, out locateMsg))
                {
                    if (locateResponse.Accepted)
                    {
                        _locateOrdersToLocalIdsMap.AddOrUpdate(locateMsg.Security.Symbol, locateResponse.StrategyId,
                                                               (key, value) => locateResponse.StrategyId);

                        if (Logger.IsInfoEnabled)
                        {
                            Logger.Info("Accepted locate message: " + locateMsg.ToString(), _type.FullName, "LocateMessageResponse");
                        }
                        return true;
                    }
                    else
                    {
                        if (Logger.IsInfoEnabled)
                        {
                            Logger.Info("Rejected locate message: " + locateMsg.ToString(), _type.FullName, "LocateMessageResponse");
                        }
                        return true;
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "LocateMessageResponse");
                return false;
            }
        }

        #endregion

        #region Implementation of IXOrderProvider

        public event Action<Order> NewArrived;
        public event Action<Execution> ExecutionArrived;
        public event Action<Order> CancellationArrived;
        public event Action<Rejection> RejectionArrived;

        /// <summary>
        /// Sends Limit Order on the given Order Execution Provider
        /// </summary>
        /// <param name="limitOrder">TradeHub LimitOrder</param>
        public void SendLimitOrder(LimitOrder limitOrder)
        {
            try
            {
                if(Logger.IsInfoEnabled)
                {
                    Logger.Info("Limit Order request received: " + limitOrder, _type.FullName, "SendLimitOrder");
                    ConsoleWriter.WriteLine(ConsoleColor.Green, "Limit order request received: " + limitOrder);
                }

                // Add to new orders map
                if (!(_newOrders.ContainsKey(limitOrder.OrderID) || _acceptedOrders.ContainsKey(limitOrder.OrderID)))
                {
                    _newOrders.Add(limitOrder.OrderID, limitOrder);

                    {
                        Order order = new Order(TradeHubConstants.OrderExecutionProvider.Simulated);

                        // Add Order ID
                        order.OrderID = limitOrder.OrderID;
                        // Set Order status
                        order.OrderStatus = TradeHubConstants.OrderStatus.SUBMITTED;
                        // Send information
                        OnNewArrived(order);
                    }

                    return;
                }

                ConsoleWriter.WriteLine(ConsoleColor.Red, "Order with same ID already exists");
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendLimitOrder");
            }
        }

        /// <summary>
        /// Sends Limit Order Cancallation on the given Order Execution Provider
        /// </summary>
        /// <param name="order">TradeHub Order</param>
        public void CancelLimitOrder(Order order)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Cancel Order request received: " + order, _type.FullName, "CancelLimitOrder");
                    ConsoleWriter.WriteLine(ConsoleColor.Green, "Cancel order request received: " + order);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "CancelLimitOrder");
            }
        }

        /// <summary>
        /// Sends Market Order on the given Order Execution Provider
        /// </summary>
        /// <param name="marketOrder">TradeHub MarketOrder</param>
        public void SendMarketOrder(MarketOrder marketOrder)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Market Order request received: " + marketOrder, _type.FullName, "SendMarketOrder");
                    ConsoleWriter.WriteLine(ConsoleColor.Green, "Market order request received: " + marketOrder);
                }

                // Add to new orders map
                if (!(_newOrders.ContainsKey(marketOrder.OrderID) || _acceptedOrders.ContainsKey(marketOrder.OrderID)))
                {
                    _newOrders.Add(marketOrder.OrderID, marketOrder);

                    {
                        Order order = new Order(TradeHubConstants.OrderExecutionProvider.Simulated);

                        // Add Order ID
                        order.OrderID = marketOrder.OrderID;
                        // Set Order status
                        order.OrderStatus = TradeHubConstants.OrderStatus.SUBMITTED;
                        // Send information
                        OnNewArrived(order);
                    }

                    {
                        // Create Order Execution
                        // Create TradeHub Order containing baisc info
                        Order order = new Order(TradeHubConstants.OrderExecutionProvider.Simulated);
                        // Add Order ID
                        order.OrderID = marketOrder.OrderID;
                        // Set Order Status
                        order.OrderStatus = TradeHubConstants.OrderStatus.EXECUTED;

                        // Create TradeHub Execution object containing execution details
                        Fill fil = new Fill(new Security { Symbol = marketOrder.Security.Symbol },
                            TradeHubConstants.OrderExecutionProvider.Simulated, order.OrderID);

                        // Set Execution ID
                        fil.ExecutionId = DateTime.UtcNow.ToString("HHmmssfff");

                        // Set Execution Time
                        fil.ExecutionDateTime = DateTime.UtcNow;

                        // Set Execution Price
                        fil.ExecutionPrice = Convert.ToDecimal(12.32);

                        // Set Execution Quantity
                        fil.ExecutionSize = Convert.ToInt32(marketOrder.OrderSize);

                        //Set Commulative Quantity 
                        fil.CummalativeQuantity = Convert.ToInt32(marketOrder.OrderSize);

                        // Set Leaves Quantity
                        fil.CummalativeQuantity = 0;

                        // Create TradeHub Excution Info Object
                        Execution executionInfo = new Execution(fil, order);
                        executionInfo.OrderExecutionProvider = TradeHubConstants.OrderExecutionProvider.Simulated;

                        OnExecutionArrived(executionInfo);
                    }
                    return;
                }

                ConsoleWriter.WriteLine(ConsoleColor.Red, "Order with same ID already exists");
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendMarketOrder");
            }
        }

        #endregion

        /// <summary>
        /// Start reading for input data
        /// </summary>
        private void ReadInput()
        {
            while (_isConnected)
            {
                {
                    ConsoleWriter.WriteLine(ConsoleColor.Green, "Enter Input");
                    string response = ConsoleWriter.Prompt();
                    if (response != null)
                    {
                        if (response.ToLower().Equals("exit"))
                        {
                            Stop();
                            break;
                        }

                        // Process incoming message
                        switch (response.Trim().ToLower())
                        {
                            case "help":
                                _orderProcessor.DisplayHelp();
                                break;
                            case "info new":
                                _orderProcessor.DisplayOrdersInfo(_newOrders, "received");
                                break;
                            case "info accepted":
                                _orderProcessor.DisplayOrdersInfo(_acceptedOrders, "accepted");
                                break;
                            case "info filled":
                                _orderProcessor.DisplayOrdersInfo(_filledOrders, "filled");
                                break;
                            default:
                                _orderProcessor.ProcessIncomingMessage(response);
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Raised when Logon is received from the Gateway
        /// </summary>
        private void OnLogonArrived()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Logon Arrived for Order Execution Simulator", _type.FullName, "OnLogonArrived");
                }

                // Raise Logon Event
                if (LogonArrived != null)
                {
                    LogonArrived(_orderExecutionProviderName);
                }
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "OnLogonArrived");
            }
        }

        /// <summary>
        /// Raised when Logout is received from the Gateway
        /// </summary>
        private void OnLogoutArrived()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Logout Arrived for Order Execution Simulator", _type.FullName, "OnLogoutArrived");
                }

                // Raise Logout Event
                if (LogoutArrived != null)
                {
                    LogoutArrived(_orderExecutionProviderName);
                }
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "OnLogoutArrived");
            }
        }

        /// <summary>
        /// Raised when Order with status New/Submitted is received
        /// </summary>
        /// <param name="order">TradeHub Order</param>
        private void OnNewArrived(Order order)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Order Status New received.", _type.FullName, "OnNewArrived");
                }

                Order origOrder;
                if (_newOrders.TryGetValue(order.OrderID, out origOrder))
                {
                    // Remove from waiting map
                    _newOrders.Remove(order.OrderID);

                    // Add to Accepted orders map
                    _acceptedOrders.Add(order.OrderID, origOrder);

                    origOrder.OrderStatus = TradeHubConstants.OrderStatus.SUBMITTED;

                    // Raise Event
                    if (NewArrived != null)
                    {
                        NewArrived((Order)origOrder.Clone());
                    }

                    return;
                }

                ConsoleWriter.WriteLine(ConsoleColor.Red, "No request received for Order with ID: " + order.OrderID);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnNewArrived");
            }
        }

        /// <summary>
        /// Raised when Order cancellation is received
        /// </summary>
        /// <param name="order">TradeHub Order</param>
        private void OnCancellationArrived(Order order)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Order cancellation received. " + order, _type.FullName, "OnCancellationArrived");
                }

                Order origOrder;
                if (_acceptedOrders.TryGetValue(order.OrderID, out origOrder))
                {
                    // Remove from Accepted Order map
                    _acceptedOrders.Remove(order.OrderID);

                    // Update Order status
                    origOrder.OrderStatus = order.OrderStatus;

                    // Raise Event
                    if (CancellationArrived != null)
                    {
                        CancellationArrived((Order)origOrder.Clone());
                    }

                    return;
                }

                ConsoleWriter.WriteLine(ConsoleColor.Red, "No accepted Order with ID: " + order.OrderID);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnCancellationArrived");
            }
        }

        /// <summary>
        /// Raised when Order execution is received
        /// </summary>
        /// <param name="execution">TradeHub Execution Info</param>
        private void OnExecutionArrived(Execution execution)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Order execution received. " + execution, _type.FullName, "OnExecutionArrived");
                }
                Order origOrder;
                if (_acceptedOrders.TryGetValue(execution.Order.OrderID, out origOrder))
                {
                    // Remove from Accepted Order map
                    _acceptedOrders.Remove(execution.Order.OrderID);

                    // Add order to Fill Map
                    _filledOrders.Add(execution.Order.OrderID, origOrder);

                    // Update Order Status
                    origOrder.OrderStatus = execution.Order.OrderStatus;

                    // Set Orignal Order to send back information
                    execution.Order = (Order)origOrder.Clone();

                    // Set execution side
                    execution.Fill.ExecutionSide = origOrder.OrderSide;

                    // Raise Event
                    if (ExecutionArrived != null)
                    {
                        ExecutionArrived(execution);
                    }

                    return;
                }

                ConsoleWriter.WriteLine(ConsoleColor.Red, "No accepted Order with ID: " + execution.Order.OrderID);

                // Check for Locate Message
                string localId;
                if (_locateOrdersToLocalIdsMap.TryRemove(execution.Fill.Security.Symbol, out localId))
                {
                    execution.Order.OrderID = localId;

                    // Raise Event
                    if (ExecutionArrived != null)
                    {
                        ExecutionArrived(execution);
                    }
                    return;
                }

                ConsoleWriter.WriteLine(ConsoleColor.Red, "No strategy awaiting for locate fill: " + execution.Fill.Security.Symbol);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnExecutionArrived");
            }
        }

        /// <summary>
        /// Raised when Rejection event is received
        /// </summary>
        /// <param name="rejection">TradeHub Rejection</param>
        private void OnRejectionArrived(Rejection rejection)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Rejection event received. " + rejection, _type.FullName, "OnRejectionArrived");
                }


                Order origOrder;
                if (!_acceptedOrders.TryGetValue(rejection.OrderId, out origOrder))
                {
                    if(_newOrders.TryGetValue(rejection.OrderId, out origOrder))
                    {
                        // Remove order from waiting orders map
                        _newOrders.Remove(rejection.OrderId);
                    }
                }
                else
                {
                    // Remove from Accepted Order map
                    _acceptedOrders.Remove(rejection.OrderId);
                }

                if (origOrder != null)
                {
                    // Raise Event
                    if (OrderRejectionArrived != null)
                    {
                        OrderRejectionArrived(rejection);
                    }

                    return;
                }

                ConsoleWriter.WriteLine(ConsoleColor.Red, "No Order request received with ID: " + rejection.OrderId);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnRejectionArrived");
            }
        }

        /// <summary>
        /// Raised when Rejection event is received
        /// </summary>
        /// <param name="locateMessage">TradeHub LimitOrder containing Locate Message Info</param>
        private void OnLocateMessageArrived(LimitOrder locateMessage)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Locate message event received. " + locateMessage, _type.FullName,
                                "OnLocateMessageArrived");
                }

                // Raise Event
                if (OnLocateMessage != null)
                {
                    // Update BW Locate Orders Map
                    _locateOrders.AddOrUpdate(locateMessage.OrderID, locateMessage, (key, value) => locateMessage);

                    OnLocateMessage(locateMessage);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnLocateMessageArrived");
            }
        }

        private void OnPositionArrived(Position position)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Position Arrived for Order Execution Simulator", _type.FullName, "PositionArrived");
                }

                // Raise Position Event
                if (_onPositionMessage != null)
                {
                    _onPositionMessage(position);
                }
            }
            catch (Exception exception)
            {
                ConsoleWriter.WriteLine(ConsoleColor.DarkRed, exception.ToString());
                Logger.Error(exception, _type.FullName, "OnLogonArrived");
            }
        }

    }
}
