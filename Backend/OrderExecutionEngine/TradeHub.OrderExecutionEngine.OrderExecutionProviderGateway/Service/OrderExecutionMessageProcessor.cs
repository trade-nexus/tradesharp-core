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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.OrderExecutionProvider;
using TradeHub.Common.Core.Utility;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.OrderExecutionEngine.OrderExecutionProviderGateway.Utility;
using Constants = TradeHub.Common.Core.Constants;

namespace TradeHub.OrderExecutionEngine.OrderExecutionProviderGateway.Service
{
    /// <summary>
    /// Process incoming messages and takes appropariate actions
    /// </summary>
    public class OrderExecutionMessageProcessor
    {
        private Type _type = typeof (OrderExecutionMessageProcessor);

        #region Fields

        /// <summary>
        /// Keeps track of all the provider instances
        /// Key =  Provider Name
        /// Value = Provider Instance
        /// </summary>
        private ConcurrentDictionary<string, IOrderExecutionProvider> _providersMap =
            new ConcurrentDictionary<string, IOrderExecutionProvider>();

        /// <summary>
        /// Keeps track of all the provider login requests
        /// Key =  Provider Name
        /// Value = List containing AppID connected to given Provider
        /// </summary>
        private ConcurrentDictionary<string, List<string>> _providersLoginRequestMap =
            new ConcurrentDictionary<string, List<string>>();

        /// <summary>
        /// Keeps track of all the Login request from the strategies
        /// Key = Market Data Provider Name
        /// Value = List contains strategy ids
        /// </summary>
        private ConcurrentDictionary<string, List<string>> _loginRequestToStrategiesMap =
            new ConcurrentDictionary<string, List<string>>();

        /// <summary>
        /// Keeps track of all the Logout request from the strategies
        /// Key = Market Data Provider Name
        /// Value = List contains strategy ids
        /// </summary>
        private ConcurrentDictionary<string, List<string>> _logoutRequestToStrategiesMap =
            new ConcurrentDictionary<string, List<string>>();

        #endregion

        #region Properties

        /// <summary>
        /// Keeps track of all the provider instances
        /// Key =  Provider Name
        /// Value = Provider Instance
        /// </summary>
        public ReadOnlyConcurrentDictionary<string, IOrderExecutionProvider> ProvidersMap
        {
            get { return new ReadOnlyConcurrentDictionary<string, IOrderExecutionProvider>(_providersMap); }
        }

        /// <summary>
        /// Keeps track of all the provider login requests
        /// Key =  Provider Name
        /// Value = List containing AppID connected to given Provider
        /// </summary>
        public ReadOnlyConcurrentDictionary<string, List<string>> ProvidersLoginRequestMap
        {
            get { return new ReadOnlyConcurrentDictionary<string, List<string>>(_providersLoginRequestMap); }
        }

        #endregion

        #region Events

        /// <summary>
        /// Fired when Logon is recieved from the Order Execution Provider
        /// string = Application ID
        /// string = Order Execution Proivder Name
        /// </summary>
        public event Action<string, string> LogonArrived;

        /// <summary>
        /// Fired when Logout is recieved from the Order Execution Provider
        /// string = Application ID
        /// string = Order Execution Proivder Name
        /// </summary>
        public event Action<string, string> LogoutArrived;

        /// <summary>
        /// Fired when Order is accepted in the Order-Exchange
        /// Order = TradeHub Order Object
        /// string = Application ID
        /// </summary>
        public event Action<Order, string> NewArrived;

        /// <summary>
        /// Fired when Order is cancelled from the Order-Exchange
        /// Order = TradeHub Order Object
        /// string = Application ID
        /// </summary>
        public event Action<Order, string> CancellationArrived;

        /// <summary>
        /// Fired when Order is rejected by the Order-Exchange
        /// Rejection = TradeHub Rejection Object
        /// string = Application ID
        /// </summary>
        public event Action<Rejection, string> RejectionArrived;

        /// <summary>
        /// Fired when Order is executed on the Order-Exchange
        /// ExecutionInfo = TradeHub ExecutionInfo Object
        /// string = Application ID
        /// </summary>
        public event Action<Execution, string> ExecutionArrived;

        /// <summary>
        /// Fired when Locate Message is received from Order-Exchange
        /// LimitOrder = TradeHub LimitOrder Object containing Locate Info
        /// string = Application ID
        /// </summary>
        public event Action<LimitOrder, string> LocateMessageArrived;

        /// <summary>
        /// Fired when Position Message is received from Order-Exchange
        /// Position = TradeHub Position Object containing Position Info
        /// </summary>
        public event Action<Position> PositionMessageArrived;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public OrderExecutionMessageProcessor()
        {
            
        }

        #region Incoming Admin Messages

        /// <summary>
        /// Handles Logon Message Requests from Applications
        /// </summary>
        public void OnLogonMessageRecieved(Login login, string appId)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Login request recieved for: " + login.OrderExecutionProvider, _type.FullName,
                                "OnLogonMessageRecieved");
                }

                // Update Login Request Map
                List<string> strategyList;
                if (_loginRequestToStrategiesMap.TryGetValue(login.OrderExecutionProvider, out strategyList))
                {
                    // Add upon new request from the strategy
                    if (!strategyList.Contains(appId))
                    {
                        strategyList.Add(appId);
                    }
                }
                else
                {
                    strategyList = new List<string>();

                    // Add strategy to the list
                    strategyList.Add(appId);
                }

                // Update Login Request Map
                _loginRequestToStrategiesMap.AddOrUpdate(login.OrderExecutionProvider, strategyList, (key, value) => strategyList);

                // Process New Login Request and make necessary updates in local Maps
                ProcessProviderLogonRequest(login, appId);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnLogonMessageRecieved");
            }
        }

        /// <summary>
        /// Handles Logout Message Requests from Applications
        /// </summary>
        public void OnLogoutMessageRecieved(Logout logout, string appId)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Logout request recieved for: " + logout.OrderExecutionProvider, _type.FullName,
                                "OnLogoutMessageRecieved");
                }

                // Update Logout Request Map
                List<string> strategyList;
                if (_logoutRequestToStrategiesMap.TryGetValue(logout.OrderExecutionProvider, out strategyList))
                {
                    // Add upon new request from the strategy
                    if (!strategyList.Contains(appId))
                    {
                        strategyList.Add(appId);
                    }
                }
                else
                {
                    strategyList = new List<string>();

                    // Add strategy to the list
                    strategyList.Add(appId);
                }

                // Update Logout Request Map
                _logoutRequestToStrategiesMap.AddOrUpdate(logout.OrderExecutionProvider, strategyList,
                                                          (key, value) => strategyList);

                // Process New Logout Request and make necessary updates in local Maps
                ProcessProviderLogoutRequest(logout, appId);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnLogoutMessageRecieved");
            }
        }

        #endregion

        #region Incoming Order Messages from Applications
        
        /// <summary>
        /// Handles New Market Order Request messages from Applications
        /// </summary>
        /// <param name="marketOrder">TradeHub Market Order</param>
        /// <param name="appId">Unique Application ID</param>
        public void MarketOrderRequestReceived(MarketOrder marketOrder, string appId)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug(
                        "New Market Order request received from: " + appId + marketOrder.OrderID + " for: " +
                        marketOrder.OrderExecutionProvider, _type.FullName, "MarketOrderRequestReceived");
                }

                IOrderExecutionProvider orderExecutionProvider;
                if (_providersMap.TryGetValue(marketOrder.OrderExecutionProvider, out orderExecutionProvider))
                {
                    IMarketOrderProvider marketOrderProvider = orderExecutionProvider as IMarketOrderProvider;
                    if (marketOrderProvider != null)
                    {
                        // Modify Order ID by appending Application ID in the front
                        marketOrder.OrderID = appId + "|" +marketOrder.OrderID;
                        
                        // Register Market Order Events
                        RegisterMarketOrderEvents(marketOrderProvider);
                        
                        // Send Market Order to Execution Provider
                        marketOrderProvider.SendMarketOrder(marketOrder);
                    }
                    else
                    {
                        if (Logger.IsInfoEnabled)
                        {
                            Logger.Info("Requested provider doesn't support Market Orders", _type.FullName, "MarketOrderRequestReceived");
                        }
                    }
                }
                else
                {
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info(
                            "Order Execution Provider module not available for: " + marketOrder.OrderExecutionProvider,
                            _type.FullName, "MarketOrderRequestReceived");
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "MarketOrderRequestReceived");
            }
        }

        /// <summary>
        /// Handles New Limit Order Request messages from Applications
        /// </summary>
        /// <param name="limitOrder">TradeHub Limit Order</param>
        /// <param name="appId">Unique Application ID</param>
        public void LimitOrderRequestReceived(LimitOrder limitOrder, string appId)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug(
                        "New Limit Order request received from: " + appId + limitOrder.OrderID + " for: " +
                        limitOrder.OrderExecutionProvider,
                        _type.FullName, "LimitOrderRequestReceived");
                }

                IOrderExecutionProvider orderExecutionProvider;
                if (_providersMap.TryGetValue(limitOrder.OrderExecutionProvider, out orderExecutionProvider))
                {
                    ILimitOrderProvider limitOrderProvider = orderExecutionProvider as ILimitOrderProvider;
                    if (limitOrderProvider != null)
                    {
                        // Modify Order ID by appending Application ID in the front
                        limitOrder.OrderID = appId + "|" + limitOrder.OrderID;

                        // Register Market Order Events
                        RegisterLimitOrderEvents(limitOrderProvider);

                        // Send Limit Order to Execution Provider
                        limitOrderProvider.SendLimitOrder(limitOrder);
                    }
                    else
                    {
                        if (Logger.IsInfoEnabled)
                        {
                            Logger.Info("Requested provider doesn't support Limit Orders", _type.FullName, "LimitOrderRequestReceived");
                        }
                    }
                }
                else
                {
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info(
                            "Order Execution Provider module not available for: " + limitOrder.OrderExecutionProvider,
                            _type.FullName, "LimitOrderRequestReceived");
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "LimitOrderRequestReceived");
            }
        }

        /// <summary>
        /// Handles New Cancel Order Request messages from Applications
        /// </summary>
        /// <param name="order">TradeHub Order</param>
        /// <param name="appId">Unique Application ID</param>
        public void CancelOrderRequestReceived(Order order, string appId)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug(
                        "New Cancel Order request received from: " + appId + order.OrderID + " for: " +
                        order.OrderExecutionProvider,
                        _type.FullName, "CancelOrderRequestReceived");
                }

                IOrderExecutionProvider orderExecutionProvider;
                if (_providersMap.TryGetValue(order.OrderExecutionProvider, out orderExecutionProvider))
                {
                    ILimitOrderProvider limitOrderProvider = orderExecutionProvider as ILimitOrderProvider;
                    if (limitOrderProvider != null)
                    {
                        // Modify Order ID by appending Application ID in the front
                        order.OrderID = appId + "|" + order.OrderID;

                        // Send Order Cancel request to Execution Provider
                        limitOrderProvider.CancelLimitOrder(order);
                    }
                    else
                    {
                        if (Logger.IsInfoEnabled)
                        {
                            Logger.Info("Requested provider doesn't support Order Cancellations", _type.FullName, "CancelOrderRequestReceived");
                        }
                    }
                }
                else
                {
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info(
                            "Order Execution Provider module not available for: " + order.OrderExecutionProvider,
                            _type.FullName, "CancelOrderRequestReceived");
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "CancelOrderRequestReceived");
            }
        }

        /// <summary>
        /// Handles New Locate response messages from Applications
        /// </summary>
        /// <param name="locateResponse">TradeHub LocateResponse</param>
        /// <param name="appId">Unique Application ID</param>
        public void LocateResponseReceived(LocateResponse locateResponse, string appId)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(
                        "Locate Response received from: " + appId + " for: " + locateResponse.OrderExecutionProvider,
                        _type.FullName, "LocateResponseReceived");
                }

                IOrderExecutionProvider orderExecutionProvider;
                if (_providersMap.TryGetValue(locateResponse.OrderExecutionProvider, out orderExecutionProvider))
                {
                    // Send Locate Response to Execution Provider
                    orderExecutionProvider.LocateMessageResponse(locateResponse);
                }
                else
                {
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info(
                            "Order Execution Provider module not available for: " + locateResponse.OrderExecutionProvider,
                            _type.FullName, "LocateResponseReceived");
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "LocateResponseReceived");
            }
        }

        #endregion

        #region Process Admin Messages

        /// <summary>
        /// Processes the new incoming Order Execution Provider Login requests
        /// </summary>
        /// <param name="login">TradeHub Login object</param>
        /// <param name="applicationId">Unique Application ID</param>
        private void ProcessProviderLogonRequest(Login login, string applicationId)
        {
            try
            {
                List<string> appIds;
                // Check if the requested Execution Provider has already recieved login request
                if (_providersLoginRequestMap.TryGetValue(login.OrderExecutionProvider, out appIds))
                {
                    if (!appIds.Contains(applicationId))
                    {
                        // Update List 
                        appIds.Add(applicationId);

                        IOrderExecutionProvider orderExecutionProvider;
                        if (_providersMap.TryGetValue(login.OrderExecutionProvider, out orderExecutionProvider))
                        {
                            if (orderExecutionProvider != null)
                            {
                                if (Logger.IsInfoEnabled)
                                {
                                    Logger.Info("Requested provider: " + login.OrderExecutionProvider + " module successfully loaded",
                                                _type.FullName, "CheckExecutionProviderAvailability");
                                }

                                // If Order Execution Provider is connectd then raise event else wait for the Logon to arrive from Gateway
                                if (orderExecutionProvider.IsConnected())
                                {
                                    // Raise Logon Event
                                    OnLogonEventArrived(login.OrderExecutionProvider);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Logger.IsInfoEnabled)
                        {
                            Logger.Info("Login request is already entertained for the given App: " + applicationId,
                                        _type.FullName, "ProcessProviderLogonRequest");
                        }
                    }
                }
                else
                {
                    // Get a new instance of the requested OrderExecutionProvider
                    IOrderExecutionProvider orderExecutionProvider = GetExecutionProviderInstance(login.OrderExecutionProvider);
                    if (orderExecutionProvider != null)
                    {
                        appIds = new List<string>();
                        appIds.Add(applicationId);

                        // Register events
                        HookConnectionStatusEvents(orderExecutionProvider);

                        // Start the requested OrderExecutionProvider Service
                        orderExecutionProvider.Start();

                        // Update Internal maps if the requested Login Provider instance doesn't exists
                        UpdateMapsOnNewProviderLogin(login, appIds, orderExecutionProvider);
                    }
                    else
                    {
                        if (Logger.IsInfoEnabled)
                        {
                            Logger.Info("Requested provider: " + login.OrderExecutionProvider + " module not found.",
                                        _type.FullName, "ProcessProviderLogonRequest");
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ProcessProviderLogonRequest");
            }
        }

        /// <summary>
        /// Processes the new incoming Order Execution Provider Logout requests
        /// </summary>
        /// <param name="logout">TradeHub Login object</param>
        /// <param name="applicationId">Unique Application ID</param>
        private void ProcessProviderLogoutRequest(Logout logout, string applicationId)
        {
            try
            {
                List<string> appIds;
                // Check if the requested Execution Provider has mutliple login request
                if (_providersLoginRequestMap.TryGetValue(logout.OrderExecutionProvider, out appIds))
                {
                    appIds.Remove(applicationId);

                    // Updates the Internal Maps on Logout Request
                    UpdateMapsOnProviderLogout(logout, appIds);
                }
                else
                {
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info(
                            "Logout cannot be processed as the requested provider: " + logout.MarketDataProvider +
                            " module is not available",
                            _type.FullName, "ProcessProviderLogoutRequest");
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ProcessProviderLogoutRequest");
            }
        }

        /// <summary>
        /// Updates the Internal Maps if the requested login Provider instance doesn't exist
        /// </summary>
        private bool UpdateMapsOnNewProviderLogin(Login login, List<string> count,
                                                 IOrderExecutionProvider orderExecutionProvider)
        {
            try
            {
                // Update the login count dicationary
                _providersLoginRequestMap.AddOrUpdate(login.OrderExecutionProvider, count, (key, value) => count);

                // Add the New OrderExecutionProvider instance to the local dictionary
                _providersMap.TryAdd(login.OrderExecutionProvider, orderExecutionProvider);

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Requested provider: " + login.OrderExecutionProvider + " module successfully Initialized.",
                                _type.FullName, "UpdateMapsOnNewProviderLogin");
                }

                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "UpdateMapsOnNewProviderLogin");
                return false;

            }
        }

        /// <summary>
        /// Updates the Internal Maps on MarketData Provider logout request
        /// </summary>
        private bool UpdateMapsOnProviderLogout(Logout logout, List<string> appIds)
        {
            try
            {
                // Update the Login count dictionary
                _providersLoginRequestMap.AddOrUpdate(logout.OrderExecutionProvider, appIds, (key, value) => appIds);

                // Send logout to the requested OrderExecutionProvider gateway if no other user is connected
                if (appIds.Count == 0)
                {
                    // Get and remove the OrderExecutionProvider Instance from the local dictionary
                    IOrderExecutionProvider orderExecutionProvider;
                    if (_providersMap.TryRemove(logout.OrderExecutionProvider, out orderExecutionProvider))
                    {
                        if (orderExecutionProvider != null)
                        {
                            // Stop the OrderExecutionProvider instance
                            orderExecutionProvider.Stop();

                            List<string> tempValue;
                            // Remove the provider from the login count dicationary
                            _providersLoginRequestMap.TryRemove(logout.OrderExecutionProvider, out tempValue);

                            if (Logger.IsInfoEnabled)
                            {
                                Logger.Info("Logout Request sent to the provider: " + logout.OrderExecutionProvider,
                                            _type.FullName, "UpdateMapsOnProviderLogout");
                            }
                        }
                    }
                }
                else
                {
                    // Send Logout Message to the requesting Application
                    OnLogoutEventArrived(logout.OrderExecutionProvider);
                }
                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "UpdateMapsOnProviderLogout");
                return false;
            }
        }

        #endregion

        #region Order Execution Provider Event Handling

        /// <summary>
        /// Raised when Order Execution Provider Logon is recieved
        /// </summary>
        /// <param name="orderExecutionProvider">Order Execution Provider Name</param>
        private void OnLogonEventArrived(string orderExecutionProvider)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Logon message recieved from: " + orderExecutionProvider, _type.FullName, "OnLogonEventArrived");
                }

                List<string> strategyIds;

                // Get List of Strategies which requested for logon on given provider
                if (_loginRequestToStrategiesMap.TryRemove(orderExecutionProvider, out strategyIds))
                {
                    // Raise Logon Event for each Strategy which requested Logon on given provider
                    foreach (string strategy in strategyIds)
                    {
                        if (LogonArrived != null)
                        {
                            LogonArrived(strategy, orderExecutionProvider);
                        }
                    }
                }
                // Notify Application associated with the OEP that Logon has arrived from Broker without request
                else
                {
                    List<string> appIds;
                    if (_providersLoginRequestMap.TryGetValue(orderExecutionProvider, out appIds))
                    {
                        // Raise Logon Event for each Application which is connected to given given provider
                        foreach (string id in appIds)
                        {
                            if (LogonArrived != null)
                            {
                                LogonArrived(id, orderExecutionProvider);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnLogonEventArrived");
            }
        }

        /// <summary>
        /// Raised when Order Execution Provider Logout is recieved
        /// </summary>
        /// <param name="orderExecutionProvider">Order Execution Provider Name</param>
        private void OnLogoutEventArrived(string orderExecutionProvider)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Logout message recieved from: " + orderExecutionProvider, _type.FullName,
                                "OnLogoutEventArrived");
                }

                List<string> strategyIds;

                // Get List of Strategies which requested for logut on given provider
                if (_logoutRequestToStrategiesMap.TryRemove(orderExecutionProvider, out strategyIds))
                {
                    // Raise Logout Event for each Strategy which requested Logout on given provider
                    foreach (string strategy in strategyIds)
                    {
                        if (LogoutArrived != null)
                        {
                            LogoutArrived(strategy, orderExecutionProvider);
                        }
                    }
                }
                // Notify Application associated with the OEP that Logout has arrived from Broker without request
                else
                {
                    List<string> appIds;
                    if (_providersLoginRequestMap.TryGetValue(orderExecutionProvider, out appIds))
                    {
                        // Raise Logout Event for each Application which is connected to given given provider
                        foreach (string id in appIds)
                        {
                            if (LogoutArrived != null)
                            {
                                LogoutArrived(id, orderExecutionProvider);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnLogoutEventArrived");
            }
        }

        /// <summary>
        /// Raised when Order with status New/Submitted is received from Order Execution Provider
        /// </summary>
        /// <param name="order">TradeHub Order</param>
        private void OnNewArrived(Order order)
        {
            try
            {
                lock (order)
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("New Arrived event received from: " + order.OrderID, _type.FullName,
                                     "OnNewArrived");
                    }

                    // extract Application ID from local Order ID
                    string[] tempArray = order.OrderID.Split('|');

                    if (tempArray.Count() == 2)
                    {
                        // Set Application ID
                        string applicationId = tempArray[0];

                        // Adjust local Order ID
                        order.OrderID = tempArray[1];

                        // Set Order Status
                        order.OrderStatus = Constants.OrderStatus.SUBMITTED;

                        // Raise New Arrived event
                        if (NewArrived != null)
                        {
                            NewArrived(order, applicationId);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnNewArrived");
            }
        }

        /// <summary>
        /// Raised when Order is rejected by the Gateway in Order Execution Provider
        /// </summary>
        /// <param name="rejection">TradeHub Rejection</param>
        private void OnOrderRejectionArrived(Rejection rejection)
        {
            try
            {
                lock (rejection)
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Order Rejection event received: " + rejection,
                                     _type.FullName, "OnOrderRejectedArrived");
                    }

                    // extract Application ID from local Order ID
                    string[] tempArray = rejection.OrderId.Split('|');

                    if (tempArray.Count() == 2)
                    {
                        // Set Application ID
                        string applicationId = tempArray[0];

                        // Adjust local Order ID
                        rejection.OrderId = tempArray[1];

                        // Raise Rejection Arrived event
                        if (RejectionArrived != null)
                        {
                            RejectionArrived(rejection, applicationId);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnOrderRejectedArrived");
            }
        }

        /// <summary>
        /// Rasied when Order Execution is received from the Gateway in Order Execution Provider
        /// </summary>
        /// <param name="executionInfo">TradeHub ExecutionInfo object</param>
        private void OnExecutionArrived(Execution executionInfo)
        {
            try
            {
                lock (executionInfo)
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Order Execution event received from: " + executionInfo.Order.OrderID,
                                     _type.FullName, "OnExecutionArrived");
                    }

                    // extract Application ID from local Order ID
                    string[] tempArray = executionInfo.Order.OrderID.Split('|');

                    if (tempArray.Count() == 2)
                    {
                        // Set Application ID
                        string applicationId = tempArray[0];

                        // Adjust local Order ID
                        executionInfo.Order.OrderID = tempArray[1];

                        // Raise Execution Arrived event
                        if (ExecutionArrived != null)
                        {
                            ExecutionArrived(executionInfo, applicationId);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnExecutionArrived");
            }
        }

        /// <summary>
        /// Rasied when Order is Cancelled in Order-Exchange by Order Execution Provider
        /// </summary>
        /// <param name="order">TradeHub Order object</param>
        private void OnCancellationArrived(Order order)
        {
            try
            {
                lock (order)
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Order Cancellation event received: " + order.OrderID,
                                     _type.FullName, "OnCancellationArrived");
                    }

                    // extract Application ID from local Order ID
                    string[] tempArray = order.OrderID.Split('|');

                    if (tempArray.Count() == 2)
                    {
                        // Set Application ID
                        string applicationId = tempArray[0];

                        // Adjust local Order ID
                        order.OrderID = tempArray[1];

                        // Set Order Status
                        order.OrderStatus = Constants.OrderStatus.CANCELLED;

                        // Raise Cancealltion Arrived event
                        if (CancellationArrived != null)
                        {
                            CancellationArrived(order, applicationId);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnCancellationArrived");
            }
        }

        /// <summary>
        /// Raised when Locate message is received from Gateway in Order Execution Provider
        /// </summary>
        /// <param name="locateOrder">TradeHub Limit Order containing Locate message info</param>
        private void OnLocateMessageArrived(LimitOrder locateOrder)
        {
            try
            {
                lock (locateOrder)
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Locate message received from: " + locateOrder.OrderExecutionProvider,
                                     _type.FullName, "OnLocateMessageArrived");
                    }

                    if (LocateMessageArrived != null)
                    {
                        List<string> appIds;
                        // Get all the strategies connected to Provider which issues Locate Message
                        if (_providersLoginRequestMap.TryGetValue(locateOrder.OrderExecutionProvider, out appIds))
                        {
                            foreach (string appId in appIds)
                            {
                                // Raise event to notify listeners
                                LocateMessageArrived(locateOrder, appId);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnLocateMessageArrived");
            }
        }

        /// <summary>
        /// Raised when Position message is received from Gateway in Order Execution Provider
        /// </summary>
        /// <param name="position">TradeHub Position message info</param>
        private void OnPositionMessageArrived(Position position)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Position message received from: " + position.Provider,
                        _type.FullName, "OnPositionMessageArrived");
                }
                if (PositionMessageArrived != null)
                    PositionMessageArrived(position);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnPositionMessageArrived");
            }

        }

        #endregion

        #region Register/Unregister Order Execution Provider Events

        /// <summary>
        /// Registers Logon and Logout Events for the Order Execution Provider
        /// </summary>
        private void HookConnectionStatusEvents(IOrderExecutionProvider orderExecutionProvider)
        {
            try
            {
                orderExecutionProvider.LogonArrived += OnLogonEventArrived;
                orderExecutionProvider.LogoutArrived += OnLogoutEventArrived;
                orderExecutionProvider.OnLocateMessage += OnLocateMessageArrived;
                orderExecutionProvider.OnPositionMessage += OnPositionMessageArrived;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "HookConnectionStatusEvents");
            }
        }

        /// <summary>
        /// Unhooks Logon and Logout Events for the Order Execution Provider
        /// </summary>
        private void UnhookConnectionStatusEvents(IOrderExecutionProvider orderExecutionProvider)
        {
            try
            {
                orderExecutionProvider.LogonArrived -= OnLogonEventArrived;
                orderExecutionProvider.LogoutArrived -= OnLogoutEventArrived;
                orderExecutionProvider.OnLocateMessage -= OnLocateMessageArrived;
                orderExecutionProvider.OnPositionMessage -= OnPositionMessageArrived;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "UnhookConnectionStatusEvents");
            }
        }

        /// <summary>
        /// Hooks New Order and Order Rejection events from Order Execution Provider
        /// </summary>
        /// <param name="marketOrderProvider">TradeHub Market Order Provider object</param>
        private void RegisterMarketOrderEvents(IMarketOrderProvider marketOrderProvider)
        {
            // Unhook to avoid multiple registrations
            UnregisterMarketOrderEvents(marketOrderProvider);

            marketOrderProvider.NewArrived += OnNewArrived;
            marketOrderProvider.ExecutionArrived += OnExecutionArrived;
            marketOrderProvider.OrderRejectionArrived += OnOrderRejectionArrived;
        }

        /// <summary>
        /// Unhooks New Order and Order Rejection events from Order Execution Provider
        /// </summary>
        /// <param name="marketOrderProvider">TradeHub Market Order Provider object</param>
        private void UnregisterMarketOrderEvents(IMarketOrderProvider marketOrderProvider)
        {
            marketOrderProvider.NewArrived -= OnNewArrived;
            marketOrderProvider.ExecutionArrived -= OnExecutionArrived;
            marketOrderProvider.OrderRejectionArrived -= OnOrderRejectionArrived;
        }

        /// <summary>
        /// Hooks Limit Order events (New/Cancellation/Execution/Rejection)
        /// </summary>
        /// <param name="limitOrderProvider">TradeHub Limit Order Provider object</param>
        private void RegisterLimitOrderEvents(ILimitOrderProvider limitOrderProvider)
        {
            // Unhook to avoid multiple event registration
            UnregisterLimitOrderEvents(limitOrderProvider);

            limitOrderProvider.NewArrived += OnNewArrived;
            limitOrderProvider.CancellationArrived += OnCancellationArrived;
            limitOrderProvider.ExecutionArrived += OnExecutionArrived;
            limitOrderProvider.RejectionArrived += OnOrderRejectionArrived;
        }

        /// <summary>
        /// Unhooks Limit Order events (New/Cancellation/Execution/Rejection)
        /// </summary>
        /// <param name="limitOrderProvider">TradeHub Limit Order Provider object</param>
        private void UnregisterLimitOrderEvents(ILimitOrderProvider limitOrderProvider)
        {
            limitOrderProvider.NewArrived -= OnNewArrived;
            limitOrderProvider.CancellationArrived -= OnCancellationArrived;
            limitOrderProvider.ExecutionArrived -= OnExecutionArrived;
            limitOrderProvider.RejectionArrived -= OnOrderRejectionArrived;
        }

        #endregion

        /// <summary>
        /// Returns the OrderExecutionProvider instance for the requested Provider
        /// </summary>
        private IOrderExecutionProvider GetExecutionProviderInstance(string provideName)
        {
            try
            {
                // Get Order Execution Provider Instance
                return ExecutionProviderInitializer.GetOrderExecutionProviderInstance(provideName);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "GetExecutionProviderInstance");
                return null;
            }
        }

        /// <summary>
        /// Stop Processing messages
        /// </summary>
        public void StopProcessing()
        {
            try
            {
                foreach (IOrderExecutionProvider orderExecutionProvider in _providersMap.Values)
                {
                    orderExecutionProvider.Stop();
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "StopProcessing");
            }
        }
    }
}
