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
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.StrategyEngine.OrderExecution;
using TradeSharp.ServiceControllers.Managers;
using TradeSharp.UI.Common;
using TradeSharp.UI.Common.Constants;
using TradeSharp.UI.Common.Models;
using TradeSharp.UI.Common.Utility;
using TradeSharp.UI.Common.ValueObjects;
using OrderExecutionProvider = TradeSharp.UI.Common.Models.OrderExecutionProvider;

namespace TradeSharp.ServiceControllers.Services
{
    /// <summary>
    /// Provides access for Order Execution queries and response
    /// </summary>
    public class OrderExecutionController
    {
        private Type _type = typeof(OrderExecutionController);

        /// <summary>
        /// Responsible for providing requested order execution functionality
        /// </summary>
        private OrderExecutionManager _orderExecutionManager;

        /// <summary>
        /// Keeps tracks of all the Providers
        /// KEY = Provider Name
        /// Value = Provider details <see cref="OrderExecutionProvider"/>
        /// </summary>
        private IDictionary<string, OrderExecutionProvider> _providersMap;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="orderExecutionService">Provides communication access with Order Execution Server</param>
        public OrderExecutionController(OrderExecutionService orderExecutionService)
        {
            // Initialize Manager
            _orderExecutionManager = new OrderExecutionManager(orderExecutionService);

            // Intialize local maps
            _providersMap = new Dictionary<string, OrderExecutionProvider>();

            // Subscribe Application events
            SubscribeEvents();

            // Subscribe Order Execution Manager events
            SubscribeManagerEvents();
        }

        /// <summary>
        /// Subscribe events to receive incoming Order Execution requests
        /// </summary>
        private void SubscribeEvents()
        {
            // Register Event to receive connect/disconnect requests
            EventSystem.Subscribe<OrderExecutionProvider>(NewConnectionRequest);
            EventSystem.Subscribe<OrderRequest>(NewOrderRequest);

            // Register Event to receive Service notifications
            EventSystem.Subscribe<ServiceDetails>(OnServiceStatusModification);
        }

        /// <summary>
        /// Subscribe events to receive incoming data and responses from Order Execution Manager
        /// </summary>
        private void SubscribeManagerEvents()
        {
            _orderExecutionManager.LogonArrivedEvent += OnLogonArrived;
            _orderExecutionManager.LogoutArrivedEvent += OnLogoutArrived;

            _orderExecutionManager.OrderAcceptedEvent += OnOrderAccepted;
            _orderExecutionManager.ExecutionArrivedEvent += OnExecutionArrived;
            _orderExecutionManager.CancellationArrivedEvent += OnCancellationArrived;
            _orderExecutionManager.RejectionArrivedEvent += OnRejectionArrived;
        }

        #region Incoming Requests

        /// <summary>
        /// Called when new Connection request is made by the user
        /// </summary>
        /// <param name="orderExecutionProvider"></param>
        private void NewConnectionRequest(OrderExecutionProvider orderExecutionProvider)
        {
            // Only entertain 'Order Execution Provider' related calls
            if (!orderExecutionProvider.ProviderType.Equals(ProviderType.OrderExecution))
                return;

            if (orderExecutionProvider.ConnectionStatus.Equals(ConnectionStatus.Disconnected))
            {
                // Open a new order execution connection
                ConnectOrderExecutionProvider(orderExecutionProvider);
            }
            else if (orderExecutionProvider.ConnectionStatus.Equals(ConnectionStatus.Connected))
            {
                // Close existing connection
                DisconnectOrderExecutionProvider(orderExecutionProvider);
            }
        }

        /// <summary>
        /// Called when connection request is received for given Order Execution Provider
        /// </summary>
        /// <param name="orderExecutionProvider">Contains provider details</param>
        private void ConnectOrderExecutionProvider(OrderExecutionProvider orderExecutionProvider)
        {
            // Check if the provider already exists in the local map
            if (!_providersMap.ContainsKey(orderExecutionProvider.ProviderName))
            {
                // Add incoming provider to local map
                _providersMap.Add(orderExecutionProvider.ProviderName, orderExecutionProvider);
            }

            // Check current provider status
            if (orderExecutionProvider.ConnectionStatus.Equals(ConnectionStatus.Disconnected))
            {
                // Forward connection request
                _orderExecutionManager.Connect(orderExecutionProvider.ProviderName);
            }
            else
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(orderExecutionProvider.ProviderName + " connection status is already set to connected.",
                        _type.FullName, "ConnectOrderExecutionProvider");
                }
            }
        }

        /// <summary>
        /// Called when disconnect request is received for given Order Execution Provider
        /// </summary>
        /// <param name="orderExecutionProvider">Contains provider details</param>
        private void DisconnectOrderExecutionProvider(OrderExecutionProvider orderExecutionProvider)
        {
            // Check current provider status
            if (orderExecutionProvider.ConnectionStatus.Equals(ConnectionStatus.Connected))
            {
                // Forward disconnect request
                _orderExecutionManager.Disconnect(orderExecutionProvider.ProviderName);
            }
            else
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(orderExecutionProvider.ProviderName + " connection status is already set to dis-connected.",
                        _type.FullName, "DisconnectOrderExecutionProvider");
                }
            }
        }

        /// <summary>
        /// Called when new order related request is made by the user
        /// </summary>
        /// <param name="orderRequest">Contains Order details</param>
        private void NewOrderRequest(OrderRequest orderRequest)
        {
            OrderExecutionProvider provider;
            //Find respective Provider
            if (_providersMap.TryGetValue(orderRequest.OrderDetails.Provider, out provider))
            {
                // Only entertain request if provider is connected
                if (provider.ConnectionStatus.Equals(ConnectionStatus.Connected))
                {
                    // Hanlde New Order Requests
                    if (orderRequest.RequestType.Equals(OrderRequestType.New))
                    {
                        // Handle Market Order Request
                        if (orderRequest.OrderDetails.Type.Equals(OrderType.Market))
                        {
                            MarketOrderRequest(orderRequest.OrderDetails);      
                        }
                        // Handle Limit Order Request
                        else if (orderRequest.OrderDetails.Type.Equals(OrderType.Limit))
                        {
                            LimitOrderRequest(orderRequest.OrderDetails);
                        }
                    }
                    // Hanlde Order Cancellation Requests
                    else if (orderRequest.RequestType.Equals(OrderRequestType.Cancel))
                    {
                        CancelOrderRequest(orderRequest.OrderDetails);
                    }
                }
                else
                {
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info(orderRequest.OrderDetails.Provider + " provider not connected.",
                            _type.FullName, "NewOrderRequest");
                    }
                }

            }
            else
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(orderRequest.OrderDetails.Provider + " provider not found.",
                        _type.FullName, "NewOrderRequest");
                }
            }
        }

        /// <summary>
        /// Called if the incoming order request is for Market Order
        /// </summary>
        /// <param name="orderDetails">Contains all order details</param>
        private void MarketOrderRequest(OrderDetails orderDetails)
        {
            // Forward market order request
            _orderExecutionManager.MarketOrderRequests(orderDetails);
        }

        /// <summary>
        /// Called if the incoming order request is for Limit Order
        /// </summary>
        /// <param name="orderDetails">Contains all order details</param>
        private void LimitOrderRequest(OrderDetails orderDetails)
        {
            // Forward limit order request
            _orderExecutionManager.LimitOrderRequest(orderDetails);
        }

        /// <summary>
        /// Called if the incoming request is to cancel an existing order
        /// </summary>
        /// <param name="orderDetails">Contains all order details</param>
        private void CancelOrderRequest(OrderDetails orderDetails)
        {
            // Forward Cancellation request
            _orderExecutionManager.CancelOrderRequest(orderDetails.ID);
        }

        #endregion

        #region Order Execution Manager Events

        /// <summary>
        /// Called when requested provider is successfully 'Logged ON'
        /// </summary>
        /// <param name="providerName"></param>
        private void OnLogonArrived(string providerName)
        {
            OrderExecutionProvider provider;
            if (_providersMap.TryGetValue(providerName, out provider))
            {
                provider.ConnectionStatus = ConnectionStatus.Connected;

                // Raise event to update UI
                EventSystem.Publish<UiElement>(new UiElement());
            }
        }

        /// <summary>
        /// Called when requested market data provider is successfully 'Logged OUT'
        /// </summary>
        /// <param name="providerName"></param>
        private void OnLogoutArrived(string providerName)
        {
            OrderExecutionProvider provider;
            if (_providersMap.TryGetValue(providerName, out provider))
            {
                provider.ConnectionStatus = ConnectionStatus.Disconnected;

                // Raise event to update UI
                EventSystem.Publish<UiElement>(new UiElement());
            }
        }

        /// <summary>
        /// Called when the requested order is accepted
        /// </summary>
        /// <param name="order">Contains accepted order details</param>
        private void OnOrderAccepted(Order order)
        {
            OrderExecutionProvider provider;

            // Get Order Execution Provider
            if (_providersMap.TryGetValue(order.OrderExecutionProvider, out provider))
            {
                OrderDetails orderDetails = provider.GetOrderDetail(order.OrderID, order.OrderStatus);

                // Update order status
                if (orderDetails != null) 
                    orderDetails.Status = order.OrderStatus;
            }
        }

        /// <summary>
        /// Called when order execution is receievd
        /// </summary>
        /// <param name="execution">Contains execution details</param>
        private void OnExecutionArrived(Execution execution)
        {
            OrderExecutionProvider provider;

            // Get Order Execution Provider
            if (_providersMap.TryGetValue(execution.OrderExecutionProvider, out provider))
            {
                OrderDetails orderDetails = null;

                // Find 'Order Details' object 
                foreach (OrderDetails tempOrderDetails in provider.OrdersCollection)
                {
                    if (tempOrderDetails.ID.Equals(execution.Order.OrderID))
                    {
                        orderDetails = tempOrderDetails;
                        break;
                    }
                }

                // Update order parameters
                if (orderDetails != null)
                {
                    orderDetails.Status = execution.Order.OrderStatus;

                    // Create fill information
                    var fillDetail = new FillDetail();

                    fillDetail.FillId = execution.Fill.ExecutionId;
                    fillDetail.FillPrice = execution.Fill.ExecutionPrice;
                    fillDetail.FillQuantity = execution.Fill.ExecutionSize;
                    fillDetail.FillDatetime = execution.Fill.ExecutionDateTime;
                    fillDetail.FillType = execution.Fill.ExecutionType;

                    // Add to order details object
                    provider.AddFill(orderDetails, fillDetail);

                    // Use incoming information to update position statistics
                    provider.UpdatePosition(orderDetails);
                }
            }
        }

        /// <summary>
        /// Called when requested order is rejected
        /// </summary>
        /// <param name="rejection">Contains rejection details</param>
        private void OnRejectionArrived(Rejection rejection)
        {
            OrderExecutionProvider provider;

            // Get Order Execution Provider
            if (_providersMap.TryGetValue(rejection.OrderExecutionProvider, out provider))
            {
                OrderDetails orderDetails = provider.GetOrderDetail(rejection.OrderId, OrderStatus.REJECTED);

                // Update order parameters
                if (orderDetails != null)
                    orderDetails.Status = OrderStatus.REJECTED;
            }
        }

        /// <summary>
        /// Called when order cancellaiton request is successful
        /// </summary>
        /// <param name="order">Contains cancelled order details</param>
        private void OnCancellationArrived(Order order)
        {
            OrderExecutionProvider provider;

            // Get Order Execution Provider
            if (_providersMap.TryGetValue(order.OrderExecutionProvider, out provider))
            {
                OrderDetails orderDetails = provider.GetOrderDetail(order.OrderID, OrderStatus.CANCELLED);

                // Update order parameters
                if (orderDetails != null)
                    orderDetails.Status = OrderStatus.CANCELLED;
            }
        }

        #endregion
        
        /// <summary>
        /// Called when Service status is modified
        /// </summary>
        /// <param name="serviceDetails"></param>
        private void OnServiceStatusModification(ServiceDetails serviceDetails)
        {
            if (serviceDetails.ServiceName.Equals(GetEnumDescription.GetValue(UI.Common.Constants.Services.OrderExecutionService)))
            {
                if (serviceDetails.Status.Equals(ServiceStatus.Running))
                {
                    _orderExecutionManager.Connect();
                }
                else if (serviceDetails.Status.Equals(ServiceStatus.Stopped))
                {
                    _orderExecutionManager.Disconnect();
                }
            }
        }

        /// <summary>
        /// Stops all order execution related activities
        /// </summary>
        public void Stop()
        {
            // Send logout for each connected order execution provider
            foreach (KeyValuePair<string, OrderExecutionProvider> keyValuePair in _providersMap)
            {
                if (keyValuePair.Value.ConnectionStatus.Equals(ConnectionStatus.Connected))
                {
                    _orderExecutionManager.Disconnect(keyValuePair.Key);
                }
            }

            _orderExecutionManager.Stop();
        }
    }
}
