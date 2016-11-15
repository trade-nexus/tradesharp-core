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
using System.Text;
using System.Threading.Tasks;
using EasyNetQ;
using Spring.Context.Support;
using TraceSourceLogger;
using  Disruptor;
using  Disruptor.Dsl;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.MarketDataProvider;
using TradeHub.Common.Core.ValueObjects;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.Inquiry;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataEngine.MarketDataProviderGateway.Utility;

namespace TradeHub.MarketDataEngine.MarketDataProviderGateway.Service
{
    /// <summary>
    /// Processes the incoming messages and takes approparitae actions
    /// </summary>
    public class MessageProcessor:IEventHandler<Tick>
    {
        private Type _type = typeof(MessageProcessor);

        private int _ringBufferSize=65536;
        private Disruptor<Tick> _disruptor;
        private RingBuffer<Tick> _ringBuffer;

       
        #region Private Fields

        // Responsible for providing Synthetic Bars
        private LiveBarGenerator _liveBarGenerator;

        /// <summary>
        /// Keeps track of all the provider login requests
        /// Key =  Provider Name
        /// Value = List containing AppID connected to given Provider
        /// </summary>
        private ConcurrentDictionary<string, List<string>> _providersLoginRequestMap = new ConcurrentDictionary<string, List<string>>();

        /// <summary>
        /// Keeps track of all the provider instances
        /// Key =  Provider Name
        /// Value = Provider Instance
        /// </summary>
        private ConcurrentDictionary<string, IMarketDataProvider> _providersMap =
            new ConcurrentDictionary<string, IMarketDataProvider>();

        /// <summary>
        /// Keeps track of all the providers subscription requests
        /// Key =  Provider Name
        /// Value = All Symbols subscribed for the given provider
        /// </summary>
        private ConcurrentDictionary<string, Dictionary<Security, List<string>>> _subscriptionMap =
            new ConcurrentDictionary<string, Dictionary<Security, List<string>>>();

        /// <summary>
        /// Keeps track of all the providers Live Bar Data requests
        /// Key = Provider Name
        /// Value = Live Bar Data request Ids against the AppID
        /// </summary>
        private ConcurrentDictionary<string, Dictionary<string, string>> _liveBarRequestsMap =
            new ConcurrentDictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// Keeps track of all the providers subscription requests made for Bar Generator
        /// Key = Provider Name
        /// Value = Count for each Security subscribed for the given provider
        /// </summary>
        private ConcurrentDictionary<string, Dictionary<Security, int>> _liveBarTicksubscriptionMap =
            new ConcurrentDictionary<string, Dictionary<Security, int>>();

        /// <summary>
        /// Keeps track of all the providers Historic Bar Data requests
        /// Key =  Provider Name
        /// Value =  Historic Bar Data request Ids against the AppID
        /// </summary>
        private ConcurrentDictionary<string, Dictionary<string, string>> _historicBarRequestsMap =
            new ConcurrentDictionary<string, Dictionary<string, string>>();

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

        #region Public Properties

        /// <summary>
        /// Keeps track of all the provider login requests
        /// Key =  Provider Name
        /// Value = List containing AppID connected to given Provider
        /// </summary>
        public ReadOnlyConcurrentDictionary<string, List<string>> ProvidersLoginRequestMap
        {
            get { return new ReadOnlyConcurrentDictionary<string, List<string>>(_providersLoginRequestMap); }
        }

        /// <summary>
        /// Keeps track of all the provider instances
        /// Key =  Provider Name
        /// Value = Provider Instance
        /// </summary>
        public ReadOnlyConcurrentDictionary<string, IMarketDataProvider> ProvidersMap
        {
            get { return new ReadOnlyConcurrentDictionary<string, IMarketDataProvider>(_providersMap); }
        }

        /// <summary>
        /// Keeps track of all the providers subscription requests
        /// Key = Provider Name
        /// Value = All Symbols subscribed for the given provider
        /// </summary>
        public ReadOnlyConcurrentDictionary<string, Dictionary<Security, List<string>>> SubscriptionMap
        {
            get { return new ReadOnlyConcurrentDictionary<string, Dictionary<Security, List<string>>>(_subscriptionMap); }
        }

        /// <summary>
        /// Keeps track of all the providers Live Bar Data requests
        /// Key = Provider Name
        /// Value = Live Bar Data request Ids against the AppID
        /// </summary>
        public ReadOnlyConcurrentDictionary<string, Dictionary<string, string>> LiveBarRequestsMap
        {
            get {return new ReadOnlyConcurrentDictionary<string, Dictionary<string, string>>(_liveBarRequestsMap);}
        }

        /// <summary>
        /// Keeps track of all the providers Historic Bar Data requests
        /// Key = Provider Name
        /// Value = Historic Bar Data request Ids against the AppID
        /// </summary>
        public ReadOnlyConcurrentDictionary<string, Dictionary<string, string>> HistoricBarRequestsMap
        {
            get { return new ReadOnlyConcurrentDictionary<string, Dictionary<string, string>>(_historicBarRequestsMap); }
        }
        
        #endregion

        #region Events

        /// <summary>
        /// Fired when Logon is recieved from the Market Data Provider
        /// string = strategy ID
        /// string = Market Data Proivder Name
        /// </summary>
        public event Action<string, string> LogonArrived;
        /// <summary>
        /// Fired when Logout is recieved from the Market Data Provider
        /// string = strategy ID
        /// string = Market Data Proivder Name
        /// </summary>
        public event Action<string, string> LogoutArrived;
        public event Action<Tick, string> TickArrived;
        public event Action<Bar, string> LiveBarArrived;
        public event Action<HistoricBarData, string> HistoricalBarsArrived;
        
        #endregion

        /// <summary>
        /// Argument Constructor
        /// </summary>
        public MessageProcessor(LiveBarGenerator liveBarGenerator)
        {
            _liveBarGenerator = liveBarGenerator;

            // Register Live Bar Generator Events
            _liveBarGenerator.LiveBarArrived += OnLiveBarGeneratorBarArrived;

            _disruptor=new Disruptor<Tick>(()=>new Tick(),_ringBufferSize,TaskScheduler.Default );
            _disruptor.HandleEventsWith(this);
            _ringBuffer = _disruptor.Start();


        }

        #region Admin Messages

        /// <summary>
        /// Handles Logon Messages
        /// </summary>
        public void OnLogonMessageRecieved(Login login, string appId)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Login request recieved for: " + login.MarketDataProvider, _type.FullName,
                                "OnLogonMessageRecieved");
                }

                // Update Login Request Map
                List<string> strategyList;
                if ( _loginRequestToStrategiesMap.TryGetValue(login.MarketDataProvider, out strategyList))
                {
                    // Add upon new request from the strategy
                    if(!strategyList.Contains(appId))
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
                _loginRequestToStrategiesMap.AddOrUpdate(login.MarketDataProvider, strategyList, (key, value) => strategyList);

                // Process New Login Request and make necessary updates in local Maps
                ProcessProviderLoginRequest(login, appId);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnLogonMessageRecieved");
            }
        }

        /// <summary>
        /// Handles Logout Messages
        /// </summary>
        public void OnLogoutMessageRecieved(Logout logout, string appId)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Logout request recieved for: " + logout.MarketDataProvider, _type.FullName,
                                "OnLogoutMessageRecieved");
                }

                // Update Logout Request Map
                List<string> strategyList;
                if (_logoutRequestToStrategiesMap.TryGetValue(logout.MarketDataProvider, out strategyList))
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
                _logoutRequestToStrategiesMap.AddOrUpdate(logout.MarketDataProvider, strategyList,
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

        #region Tick Subscription Messages

        /// <summary>
        /// Handles incoming Tick Subscribe messages from Application Controller
        /// </summary>
        public void OnTickSubscribeRecieved(Subscribe subscribe, string appId)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(
                        "Subscribe call recieved for: " + subscribe.Security.Symbol + " | On: " + subscribe.MarketDataProvider,
                        _type.FullName, "OnTickSubscribeRecieved");
                }

                // Handle incoming request
                OnTickSubscribe(subscribe, appId);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnTickSubscribeRecieved");
            }
        }

        /// <summary>
        /// Handles incoming Tick Unsubscribe messages from Appliation Controller
        /// </summary>
        public void OnTickUnsubscribeRecieved(Unsubscribe unsubscribe, string appId)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(
                        "Unsubscribe call recieved for: " + unsubscribe.Security.Symbol + " | On: " +
                        unsubscribe.MarketDataProvider,
                        _type.FullName, "OnTickUnsubscribe");
                }

                // Handle incoming request
                OnTickUnsubscribe(unsubscribe, appId);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnTickUnsubscribe");
            }
        }

        /// <summary>
        /// Handles incoming Tick Subscribe messages
        /// </summary>
        private void OnTickSubscribe(Subscribe subscribe, string appId)
        {
            try
            {
                // Process New Subscription request and make necessary updates to internal Maps
                ProcessMarketDataSubscription(subscribe, appId);
            }
            catch (Exception exception) 
            {
                Logger.Error(exception, _type.FullName, "OnTickSubscribe");
            }
        }

        /// <summary>
        /// Handles incoming Tick Unsubscribe messages
        /// </summary>
        private void OnTickUnsubscribe(Unsubscribe unsubscribe, string appId)
        {
            try
            {
                // Process New Unsubscription request and make necessary updates to internal Maps
                ProcessMarketDataUnscubscription(unsubscribe, appId);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnTickUnsubscribe");
            }
        }

        #endregion

        #region Live Bar Data Request Messages

        /// <summary>
        /// Handles incoming Live Bar Data Subscribe Request Messages from Application Controller
        /// </summary>
        public void OnLiveBarSubscribeRequestRecieved(BarDataRequest barDataRequest, string appId)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(
                        "Live Bar Data request recieved for: " + barDataRequest.Security.Symbol + " | On: " + barDataRequest.MarketDataProvider,
                        _type.FullName, "OnLiveBarSubscribeRequestRecieved");
                }

                // Process request
                OnLiveBarSubscribeRequest(barDataRequest, appId);

            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnLiveBarSubscribeRequestRecieved");
            }
        }


        /// <summary>
        /// Handles incoming Live Bar Data Unsubscribe Request Messages from Application Controller
        /// </summary>
        public void OnLiveBarUnsubscribeRequestRecieved(BarDataRequest barDataRequest, string appId)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(
                        "Live Bar Data unsubscribe request recieved for: " + barDataRequest.Security.Symbol + " | On: " + barDataRequest.MarketDataProvider,
                        _type.FullName, "OnLiveBarUnsubscribeRequestRecieved");
                }

                OnLiveBarUnsubscribeRequest(barDataRequest, appId);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnLiveBarUnsubscribeRequestRecieved");
            }
        }


        /// <summary>
        /// Handles Live Bar Data Subscribe Request Message
        /// </summary>
        public void OnLiveBarSubscribeRequest(BarDataRequest barDataRequest, string appId)
        {
            try
            {
                // Process New Live Bar Data request and make necessary updates to internal Maps
                ProcessLiveBarSubscriptionRequest(barDataRequest, appId);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnLiveBarSubscribeRequest");
            }
        }

        /// <summary>
        /// Handles Live Bar Data Unsubscribe Request Message
        /// </summary>
        public void OnLiveBarUnsubscribeRequest(BarDataRequest barDataRequest, string appId)
        {
            try
            {
                // Process New Live Bar Data Unsubscribe request and make necessary updates to internal Maps
                ProcessLiveBarUnsubscriptionRequest(barDataRequest, appId);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnLiveBarUnsubscribeRequest");
            }
        }

        #endregion

        #region Historic Bar Data Request Messages

        /// <summary>
        /// Handles incoming Historic Bar Data Request Messages from Application Controller
        /// </summary>
        public void OnHistoricBarDataRequestRecieved(HistoricDataRequest historicDataRequest, string appId)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(
                        "Historic Bar Data request recieved for: " + historicDataRequest.Security.Symbol + " | On: " + historicDataRequest.MarketDataProvider,
                        _type.FullName, "OnHistoricBarDataRequestRecieved");
                }
                
                // Handle incoming request
                OnHistoricBarDataRequest(historicDataRequest, appId);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnHistoricBarDataRequestRecieved");
            }
        }
        
        /// <summary>
        /// Handles Historic Bar Data Request Message
        /// </summary>
        public void OnHistoricBarDataRequest(HistoricDataRequest historicDataRequest, string appId)
        {
            try
            {
                // Process New Historic Bar Data request and make necessary updates to internal Maps
                ProcessHistoricBarDataRequest(historicDataRequest, appId);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnHistoricBarDataRequest");
            }
        }

        #endregion

        #region Process Admin Messages

        /// <summary>
        /// Processes the New incoming MarketData Provider Login Request
        /// </summary>
        private bool ProcessProviderLoginRequest(Login login, string appId)
        {
            try
            {
                List<string> appIds;
                // Check if the requested Data Provider has already recieved login request
                if (_providersLoginRequestMap.TryGetValue(login.MarketDataProvider, out appIds))
                {
                    if (!appIds.Contains(appId))
                    {
                        // Update List 
                        appIds.Add(appId);

                        IMarketDataProvider marketDataProvider;
                        if (_providersMap.TryGetValue(login.MarketDataProvider, out marketDataProvider))
                        {
                            if (marketDataProvider != null)
                            {
                                if (Logger.IsInfoEnabled)
                                {
                                    Logger.Info("Requested provider: " + login.MarketDataProvider + " module successfully loaded",
                                                _type.FullName, "ProcessProviderLoginRequest");
                                }

                                // If Market Data Provider is connectd then raise event else wait for the Logon to arrive from Gateway
                                if (marketDataProvider.IsConnected())
                                {
                                    // Raise Logon Event
                                    OnLogonEventArrived(login.MarketDataProvider);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Logger.IsInfoEnabled)
                        {
                            Logger.Info("Login request is already entertained for the given App: " + appId,
                                        _type.FullName, "ProcessProviderLoginRequest");
                        }
                    }
                }
                else
                {
                    // Get a new instance of the requested MarketDataProvider
                    IMarketDataProvider marketDataProvider = GetMarketDataInstance(login.MarketDataProvider);
                    if (marketDataProvider != null)
                    {
                        appIds = new List<string>();
                        appIds.Add(appId);

                        // Register events
                        HookConnectionStatusEvents(marketDataProvider);

                        // Start the requested MarketDataProvider Service
                        marketDataProvider.Start();

                        // Update Internal maps if the requested Login Provider instance doesn't exists
                        UpdateMapsOnNewProviderLogin(login, appIds, marketDataProvider);
                    }
                    else
                    {
                        if (Logger.IsInfoEnabled)
                        {
                            Logger.Info("Requested provider: " + login.MarketDataProvider + " module not found.",
                                        _type.FullName, "ProcessProviderLoginRequest");
                        }
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ProcessProviderLoginRequest");
                return false;
            }
        }

        /// <summary>
        /// Processes the New incoming MarketData Provider Logout Request
        /// </summary>
        private bool ProcessProviderLogoutRequest(Logout logout, string appId)
        {
            try
            {
                List<string> appIds;
                // Check if the requested Data Provider has mutliple login request
                if (_providersLoginRequestMap.TryGetValue(logout.MarketDataProvider, out appIds))
                {
                    appIds.Remove(appId);

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
                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ProcessProviderLogoutRequest");
                return false;
            }
        }

        #endregion

        #region Process Tick Subscription Messages

        /// <summary>
        /// Processes New incoming Market Data Subscription Request
        /// </summary>
        private bool ProcessMarketDataSubscription(Subscribe subscribe, string strategyId)
        {
            try
            {
                IMarketDataProvider marketDataProvider;
                if (_providersMap.TryGetValue(subscribe.MarketDataProvider, out marketDataProvider))
                {
                    if (marketDataProvider != null)
                    {
                        ILiveTickDataProvider liveDataProvider = marketDataProvider as ILiveTickDataProvider;
                        if (liveDataProvider != null)
                        {
                            Dictionary<Security, List<string>> symbols;
                            if (_subscriptionMap.TryGetValue(subscribe.MarketDataProvider, out symbols))
                            {
                                List<string> strategies;

                                // Check if the Security is already subscribed
                                if (symbols.TryGetValue(subscribe.Security, out strategies))
                                {
                                    // Update the internal Subscriptions Map if the requested security is already subscribed
                                    UpdateMapsOnExistingSubscription(subscribe, strategies, symbols, strategyId);
                                }
                                else
                                {
                                    // Update the  internal Subscriptions Map if the requested security is not subscribed
                                    UpdateMapsOnNewSubscription(subscribe, strategyId, symbols);

                                    // Send Subscription Request
                                    SendTickSubscriptionRequest(liveDataProvider, subscribe);
                                }

                            }
                            else
                            {
                                // Initialize local Dictionary
                                symbols = new Dictionary<Security, List<string>>();

                                // Update the  internal Subscriptions Map if the requested security is not subscribed
                                UpdateMapsOnNewSubscription(subscribe, strategyId, symbols);

                                // Send Subscription Request
                                SendTickSubscriptionRequest(liveDataProvider, subscribe);
                            }

                        }
                        else
                        {
                            if (Logger.IsInfoEnabled)
                            {
                                Logger.Info("Requested Market Data Provider doesn't support Live Data.", _type.FullName,
                                            "SendSubscriptionRequest");
                            }
                        }
                    }
                }
                else
                {
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Data provide module not found for: " + subscribe.MarketDataProvider, _type.FullName,
                                    "OnSubscribe");
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ProcessMarketDataSubscription");
                return false;
            }
        }

        /// <summary>
        /// Processes New incoming Market Data Unsubscription Requests
        /// </summary>
        private bool ProcessMarketDataUnscubscription(Unsubscribe unsubscribe, string strategyId)
        {
            try
            {
                IMarketDataProvider marketDataProvider;
                if (_providersMap.TryGetValue(unsubscribe.MarketDataProvider, out marketDataProvider))
                {
                    if (marketDataProvider != null)
                    {
                        ILiveTickDataProvider liveDataProvider = marketDataProvider as ILiveTickDataProvider;
                        if (liveDataProvider != null)
                        {
                            Dictionary<Security, List<string>> symbols;
                            if (_subscriptionMap.TryGetValue(unsubscribe.MarketDataProvider, out symbols))
                            {
                                List<string> strategies;
                                if (symbols.TryGetValue(unsubscribe.Security, out strategies))
                                {
                                    strategies.Remove(strategyId);
                                    if (strategies.Count.Equals(0))
                                    {
                                        // Updates securties list
                                        symbols.Remove(unsubscribe.Security);

                                        // Update the internal Subscriptions Map 
                                        UpdateMapsOnUnsubscription(unsubscribe, symbols);

                                        // Send Unsubscription request
                                        SendTickUnsubcriptionRequest(liveDataProvider, unsubscribe);
                                    }
                                    else
                                    {
                                        // Updates securties list
                                        symbols[unsubscribe.Security] = strategies;

                                        // Update the subscription map
                                        _subscriptionMap.AddOrUpdate(unsubscribe.MarketDataProvider, symbols,
                                                                     (key, value) => symbols);
                                    }
                                }
                            }

                        }
                        else
                        {
                            if (Logger.IsInfoEnabled)
                            {
                                Logger.Info("Requested Market Data Provider doesn't support Live Data.", _type.FullName,
                                            "ProcessMarketDataUnscubscription");
                            }
                        }
                    }
                }
                else
                {
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Data provide module not found for: " + unsubscribe.MarketDataProvider,
                                    _type.FullName,
                                    "ProcessMarketDataUnscubscription");
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ProcessMarketDataUnscubscription");
                return false;
            }
        }

        #endregion

        #region Process Live Bar Data Request Messages

        /// <summary>
        /// Processes New Incoming Live Bar Data Request Message
        /// </summary>
        private void ProcessLiveBarSubscriptionRequest(BarDataRequest barDataRequest, string strategyId)
        {
            try
            {
                IMarketDataProvider marketDataProvider;
                if (_providersMap.TryGetValue(barDataRequest.MarketDataProvider, out marketDataProvider))
                {
                    if (marketDataProvider != null)
                    {
                        var liveBarDataProvider = marketDataProvider as ILiveBarDataProvider;
                        if (liveBarDataProvider != null)
                        {
                            Dictionary<string, string> idsMap;
                            if (_liveBarRequestsMap.TryGetValue(barDataRequest.MarketDataProvider, out idsMap))
                            {
                                // Update related local Maps
                                UpdateLiveBarRequestMapOnSubscription(idsMap, strategyId, barDataRequest);
                            }
                            else
                            {
                                idsMap = new Dictionary<string, string>();

                                // Update related local Maps
                                UpdateLiveBarRequestMapOnSubscription(idsMap, strategyId, barDataRequest);
                            }

                            // Send Request to the Market Data Provide Gateway
                            SendLiveBarSubscriptionRequest(liveBarDataProvider, barDataRequest);
                        }
                        else
                        {
                            if (Logger.IsInfoEnabled)
                            {
                                Logger.Info(
                                    "Requested Provider " + barDataRequest.MarketDataProvider +
                                    " does not support Lova Bar Data therefore using Bar Factory.", _type.FullName,
                                    "ProcessLiveBarSubscriptionRequest");
                            }
                           
                            // Get bars form BarGererator
                            SendSubscriptiontToBarGenerator(barDataRequest, strategyId);
                        }
                    }
                }
                else
                {
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Data provide module not found for: " + barDataRequest.MarketDataProvider, _type.FullName,
                                    "ProcessLiveBarSubscriptionRequest");
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ProcessLiveBarSubscriptionRequest");
            }
        }

        /// <summary>
        /// Processes New incoming Live Bar Unsubscription Requests
        /// </summary>
        private void ProcessLiveBarUnsubscriptionRequest(BarDataRequest barDataRequest, string strategyId)
        {
            try
            {
                IMarketDataProvider marketDataProvider;
                if (_providersMap.TryGetValue(barDataRequest.MarketDataProvider, out marketDataProvider))
                {
                    if (marketDataProvider != null)
                    {
                        var liveBarDataProvider = marketDataProvider as ILiveBarDataProvider;
                        if (liveBarDataProvider != null)
                        {
                            Dictionary<string, string> idsMap;
                            if (_liveBarRequestsMap.TryGetValue(barDataRequest.MarketDataProvider, out idsMap))
                            {
                                // Update related local Maps
                                UpdateLiveBarRequestMapOnUnsubscription(idsMap, strategyId, barDataRequest);

                                // Send Request to the Market Data Provide Gateway
                                SendLiveBarUnsubscriptionRequest(liveBarDataProvider, barDataRequest);
                            }
                        }
                        else
                        {
                            if (Logger.IsInfoEnabled)
                            {
                                Logger.Info(
                                    "Requested Provider " + barDataRequest.MarketDataProvider +
                                    " does not support Live Bar Data therefore using Bar Factory.", _type.FullName,
                                    "ProcessLiveBarUnsubscriptionRequest");
                            }
                            
                            // Unsubscribe Bars from BarGenerator
                            SendUnsubscriptiontToBarGenerator(barDataRequest);
                        }
                    }
                }
                else
                {
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Data provide module not found for: " + barDataRequest.MarketDataProvider, _type.FullName,
                                    "ProcessLiveBarUnsubscriptionRequest");
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ProcessLiveBarUnsubscriptionRequest");
            }
        }

        /// <summary>
        /// Send Bar Subscription request to Live Bar Generator
        /// </summary>
        /// <param name="barDataRequest">TradeHub Bar Data Request</param>
        /// <param name="appId">Unique Application ID</param>
        private void SendSubscriptiontToBarGenerator(BarDataRequest barDataRequest, string appId)
        {
            try
            {
                if (_liveBarGenerator!=null)
                {
                    Subscribe subscribe = new Subscribe()
                        {
                            MarketDataProvider = barDataRequest.MarketDataProvider,
                            Security = barDataRequest.Security,
                            Id = barDataRequest.Id
                        };

                    Dictionary<Security, int> securities;
                    if (_liveBarTicksubscriptionMap.TryGetValue(barDataRequest.MarketDataProvider, out securities))
                    {
                        int count;
                        if(securities.TryGetValue(barDataRequest.Security,out count))
                        {
                            count++;
                            securities[barDataRequest.Security] = count;
                        }
                        else
                        {
                            // Update Count
                            securities.Add(barDataRequest.Security, 1);

                            // Subscribe Tick Data to create Bars
                            OnTickSubscribe(subscribe, "TradeHub");
                        }
                    }
                    else
                    {
                        securities= new Dictionary<Security, int>();
                        // Update Count
                        securities.Add(barDataRequest.Security, 1);

                        // Subscribe Tick Data to create Bars
                        OnTickSubscribe(subscribe, "TradeHub");
                    }

                    // Update Map
                    _liveBarTicksubscriptionMap.AddOrUpdate(barDataRequest.MarketDataProvider, securities,
                                                            (key, value) => securities);

                    Dictionary<string, string> idsMap;
                    if (!_liveBarRequestsMap.TryGetValue(barDataRequest.MarketDataProvider, out idsMap))
                    {
                        idsMap = new Dictionary<string, string>();
                    }

                    // Updates IDs Dictionary
                    idsMap.Add(barDataRequest.Id, appId);

                    // Update Internal Map
                    _liveBarRequestsMap.AddOrUpdate(barDataRequest.MarketDataProvider, idsMap, (key, value) => idsMap);

                    // Send subscription request
                    _liveBarGenerator.SubscribeBars(barDataRequest);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendBarSubscriptiontToBarGenerator");
            }
        }

        /// <summary>
        /// Send Bar Unsubscription request to Live Bar Generator
        /// </summary>
        /// <param name="barDataRequest">TradeHub Bar Data Request</param>
        private void SendUnsubscriptiontToBarGenerator(BarDataRequest barDataRequest)
        {
            try
            {
                if (_liveBarGenerator != null)
                {
                    Unsubscribe unsubscribe = new Unsubscribe()
                    {
                        MarketDataProvider = barDataRequest.MarketDataProvider,
                        Security = barDataRequest.Security,
                        Id = barDataRequest.Id
                    };

                    Dictionary<Security, int> securities;
                    if (_liveBarTicksubscriptionMap.TryGetValue(barDataRequest.MarketDataProvider, out securities))
                    {
                        int count;
                        if (securities.TryGetValue(barDataRequest.Security, out count))
                        {
                            count--;
                            if (count == 0)
                            {
                                securities.Remove(barDataRequest.Security);

                                if (securities.Count.Equals(0))
                                {
                                    _liveBarTicksubscriptionMap.TryRemove(barDataRequest.MarketDataProvider,
                                                                          out securities);
                                }
                                // Unsubscribe Tick data which was used to create bars
                                OnTickUnsubscribe(unsubscribe, "TradeHub");
                            }
                            else
                            {
                                // Update Count
                                securities[barDataRequest.Security] = count;

                                // Update Map
                                _liveBarTicksubscriptionMap.AddOrUpdate(barDataRequest.MarketDataProvider, securities,
                                                                        (key, value) => securities);
                            }
                        }

                        Dictionary<string, string> idsMap;
                        if (_liveBarRequestsMap.TryGetValue(barDataRequest.MarketDataProvider, out idsMap))
                        {
                            // Updates IDs Dictionary
                            idsMap.Remove(barDataRequest.Id);

                            // Remove Market Data Provider from the Map if there are no Bar Request
                            if (idsMap.Count == 0)
                            {
                                _liveBarRequestsMap.TryRemove(barDataRequest.MarketDataProvider, out idsMap);
                            }

                            // Send unsubscription request
                            _liveBarGenerator.UnsubscribeBars(barDataRequest);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendUnsubscriptiontToBarGenerator");
            }
        }

        #endregion

        #region Process Historic Bar Data

        /// <summary>
        /// Processes new incoming Historic Bar Data Request Message
        /// </summary>
        private bool ProcessHistoricBarDataRequest(HistoricDataRequest historicDataRequest, string strategyId)
        {
            try
            {
                IMarketDataProvider marketDataProvider;
                if (_providersMap.TryGetValue(historicDataRequest.MarketDataProvider, out marketDataProvider))
                {
                    if (marketDataProvider != null)
                    {
                        var barDataProvider = marketDataProvider as IHistoricBarDataProvider;
                        if (barDataProvider !=null)
                        {
                            Dictionary<string, string> securitiesDic;
                            if (_historicBarRequestsMap.TryGetValue(historicDataRequest.MarketDataProvider, out securitiesDic))
                            {
                                // Update related local Maps
                                UpdateHistoricBarRequestMap(securitiesDic, strategyId, historicDataRequest);
                            }
                            else
                            {
                                securitiesDic = new Dictionary<string, string>();

                                // Update related local Maps
                                UpdateHistoricBarRequestMap(securitiesDic, strategyId, historicDataRequest);
                            }

                            // Send Request to the Market Data Provide Gateway
                            SendHistoricBarDataRequest(barDataProvider, historicDataRequest);
                        }
                        else
                        {
                            if(Logger.IsInfoEnabled)
                            {
                                Logger.Info("Requested Provider " + historicDataRequest.MarketDataProvider + " does not support Historic Bar Data.", _type.FullName, "ProcessHistoricBarDataRequest");
                            }
                        }
                    }
                }
                else
                {
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Data provide module not found for: " + historicDataRequest.MarketDataProvider, _type.FullName,
                                    "ProcessHistoricBarDataRequest");
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ProcessHistoricBarDataRequest");
                return false;
            }
        }

        #endregion

        #region Update Login Maps

        /// <summary>
        /// Updates the Internal Maps if the requested login Provider instance doesn't exist
        /// </summary>
        private bool UpdateMapsOnNewProviderLogin(Login login, List<string> count,
                                                 IMarketDataProvider marketDataProvider)
        {
            try
            {
                // Update the login count dicationary
                _providersLoginRequestMap.AddOrUpdate(login.MarketDataProvider, count, (key, value) => count);

                // Add the New MarketDataProvider instance to the local dictionary
                _providersMap.TryAdd(login.MarketDataProvider, marketDataProvider);

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Requested provider: " + login.MarketDataProvider + " module successfully Initialized.",
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
                _providersLoginRequestMap.AddOrUpdate(logout.MarketDataProvider, appIds, (key, value) => appIds);

                // Send logout to the requested MarketDataProvider gateway if no other user is connected
                if (appIds.Count == 0)
                {
                    // Get and remove the MarketDataProvider Instance from the local dictionary
                    IMarketDataProvider marketDataProvider;
                    if (_providersMap.TryRemove(logout.MarketDataProvider, out marketDataProvider))
                    {
                        if (marketDataProvider != null)
                        {
                            // Stop the MarketDataProvider instance
                            marketDataProvider.Stop();

                            List<string> tempValue;
                            // Remove the provider from the login count dicationary
                            _providersLoginRequestMap.TryRemove(logout.MarketDataProvider, out tempValue);

                            if (Logger.IsInfoEnabled)
                            {
                                Logger.Info("Logout Request sent to the provider: " + logout.MarketDataProvider,
                                            _type.FullName, "UpdateMapsOnProviderLogout");
                            }
                        }
                    }
                }
                else
                {
                    // Send Logout Message to the requesting Application
                    OnLogoutEventArrived(logout.MarketDataProvider);
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

        #region Update Subscription Maps

        /// <summary>
        /// Updates the Internal Subscriptions Map on Subscription if the requested security is already subscribed
        /// </summary>
        private void UpdateMapsOnExistingSubscription(Subscribe subscribe, List<string> strategies,
                                                      Dictionary<Security, List<string>> symbols, string strategyId)
        {
            try
            {
                // Check if the requesting strategy has already subscribed to the given security
                if (!strategies.Contains(strategyId))
                {
                    // Add Strategy to given security subscriptions
                    strategies.Add(strategyId);
                }
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(
                        "Data alreaedy subscribed for : " + subscribe.Security.Symbol + " | On: " +
                        subscribe.MarketDataProvider,
                        _type.FullName, "UpdateMapsOnExistingSubscription");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "UpdateMapsOnExistingSubscription");
            }
        }

        /// <summary>
        /// Updates the Internal Subscriptions Map on Subscription if the requested security is not yet subscribed
        /// </summary>
        private void UpdateMapsOnNewSubscription(Subscribe subscribe, string strategyId,
                                                 Dictionary<Security, List<string>> symbols)
        {
            try
            {
                // Add new Security to the local dictionary and add requesting strategyId to its subscriptions
                symbols.Add(subscribe.Security, new List<string>() {strategyId});

                // Update the subscription map
                _subscriptionMap.AddOrUpdate(subscribe.MarketDataProvider, symbols, (key, value) => symbols);

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(
                        "Sending data request for : " + subscribe.Security.Symbol + " | On: " +
                        subscribe.MarketDataProvider,
                        _type.FullName, "UpdateMapsOnNewSubscription");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "UpdateMapsOnNewSubscription");
            }
        }

        /// <summary>
        /// Updates the Internak Subscriptions Map on Unsubcription Request
        /// </summary>
        private void UpdateMapsOnUnsubscription(Unsubscribe unsubscribe, Dictionary<Security, List<string>> symbols)
        {
            try
            {
                if (symbols.Count.Equals(0))
                {
                    // Update the subscription map
                    _subscriptionMap.TryRemove(unsubscribe.MarketDataProvider, out symbols);
                }

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(
                        "Sending unsubscription request for : " + unsubscribe.Security.Symbol +
                        " | On: " + unsubscribe.MarketDataProvider, _type.FullName, "UpdateMapsOnUnsubscription");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "UpdateMapsOnUnsubscription");
            }
        }

        #endregion

        #region Update Live Bar Request Map

        /// <summary>
        /// Updates the Live Bar Request Map on new Subscription
        /// </summary>
        /// <param name="idsMap">Request Ids Map</param>
        /// <param name="appId">Unique Application ID</param>
        /// <param name="barDataRequest">TradeHub BarDataRequest Message</param>
        private void UpdateLiveBarRequestMapOnSubscription(Dictionary<string, string> idsMap, string appId,
                                             BarDataRequest barDataRequest)
        {
            try
            {
                // Updates IDs Dictionary
                idsMap.Add(barDataRequest.Id, appId);

                // Update Internal Map
                _liveBarRequestsMap.AddOrUpdate(barDataRequest.MarketDataProvider, idsMap, (key, value) => idsMap);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "UpdateLiveBarRequestMapOnSubscription");
            }
        }

        /// <summary>
        /// Updates the Live Bar Request Map on Unsubscription request
        /// </summary>
        /// <param name="idsMap">Request Ids Map</param>
        /// <param name="appId">Unique Application ID</param>
        /// <param name="barDataRequest">TradeHub BarDataRequest Message</param>
        private void UpdateLiveBarRequestMapOnUnsubscription(Dictionary<string, string> idsMap, string appId,
                                             BarDataRequest barDataRequest)
        {
            try
            {
                // Updates IDs Dictionary
                idsMap.Remove(barDataRequest.Id);

                // Remove Market Data Provider from the Map if there are no Bar Request
                if (idsMap.Count == 0)
                {
                    _liveBarRequestsMap.TryRemove(barDataRequest.MarketDataProvider, out idsMap);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "UpdateLiveBarRequestMapOnUnsubscription");
            }
        }

        #endregion
        
        #region Update Historic Bar Requests Maps

        /// <summary>
        /// Updates HistoricBarRequestMap upon new request
        /// </summary>
        private void UpdateHistoricBarRequestMap(Dictionary<string, string> idsDic, string strategyId,
                                                            HistoricDataRequest historicDataRequest)
        {
            try
            {
                // Updates Securites Dictionary
                idsDic.Add(historicDataRequest.Id, strategyId);

                // Update the Historic Bar Requests Map
                _historicBarRequestsMap.AddOrUpdate(historicDataRequest.MarketDataProvider, idsDic,
                                                    (key, value) => idsDic);

            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "UpdateHistoricBarRequestMap");
            }
        }

        #endregion

        #region Register/Unregister Market Data Events

        /// <summary>
        /// Registers Logon and Logout Events for the Market Data Provider
        /// </summary>
        private void HookConnectionStatusEvents(IMarketDataProvider marketDataProvider)
        {
            try
            {
                marketDataProvider.LogonArrived += OnLogonEventArrived;
                marketDataProvider.LogoutArrived += OnLogoutEventArrived;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "HookConnectionStatusEvents");
            }
        }

        /// <summary>
        /// Unhooks Logon and Logout Events for the Market Data Provider
        /// </summary>
        private void UnhookConnectionStatusEvents(IMarketDataProvider marketDataProvider)
        {
            try
            {
                marketDataProvider.LogonArrived -= OnLogonEventArrived;
                marketDataProvider.LogoutArrived -= OnLogoutEventArrived;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "UnhookConnectionStatusEvents");
            }
        }

        /// <summary>
        /// Hooks Tick data events
        /// </summary>
        private void HookTickDataEvents(ILiveTickDataProvider liveDataProvider)
        {
            try
            {
                // Avoid multiple registration
                UnhookTickDataEvents(liveDataProvider);

                // Register Event
                liveDataProvider.TickArrived += OnTickArrived;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "HookTickDataEvents");
            }
        }

        /// <summary>
        /// Unhooks Tick data events
        /// </summary>
        private void UnhookTickDataEvents(ILiveTickDataProvider liveDataProvider)
        {
            try
            {
                // Unregister Event
                liveDataProvider.TickArrived -= OnTickArrived;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "UnhookTickDataEvents");
            }
        }

        /// <summary>
        /// Hooks Live Bar data events
        /// </summary>
        private void HookLiveBarEvents(ILiveBarDataProvider liveBarDataProvider)
        {
            try
            {
                // Avoid multiple registration
                UnhookLiveBarEvents(liveBarDataProvider);

                // Register Event
                liveBarDataProvider.BarArrived += OnLiveBarArrived;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "HookLiveBarEvents");
            }
        }

        /// <summary>
        /// Unhooks Live Bar data events
        /// </summary>
        private void UnhookLiveBarEvents(ILiveBarDataProvider liveBarDataProvider)
        {
            try
            {
                // Unregister Event
                liveBarDataProvider.BarArrived -= OnLiveBarArrived;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "UnhookLiveBarEvents");
            }
        }

        /// <summary>
        /// Hooks Historic Bar data event
        /// </summary>
        private void HookHistoricBarDataEvents(IHistoricBarDataProvider barDataProvider)
        {
            try
            {
                // Avoid multiple registrations
                UnhookHistoricBarDataEvents(barDataProvider);

                // Register Event
                barDataProvider.HistoricBarDataArrived += OnHistoricBarDataArrived;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "HookHistoricBarDataEvents");
            }
        }

        /// <summary>
        /// Hooks Historic Bar data event
        /// </summary>
        private void UnhookHistoricBarDataEvents(IHistoricBarDataProvider barDataProvider)
        {
            try
            {
                // Unregister Event
                barDataProvider.HistoricBarDataArrived -= OnHistoricBarDataArrived;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "UnhookHistoricBarDataEvents");
            }
        }

        #endregion

        #region Market Data Provider Event Handling

        /// <summary>
        /// Raised when Market Data Provider Logon is recieved
        /// </summary>
        private void OnLogonEventArrived(string marketDataProvider)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Logon message recieved from: " + marketDataProvider, _type.FullName, "OnLogonArrived");
                }

                List<string> strategyIds;

                // Get List of Strategies which requested for logon on given provider
                if (_loginRequestToStrategiesMap.TryRemove(marketDataProvider, out strategyIds))
                {
                    // Raise Logon Event for each Strategy which requested Logon on given provider
                    foreach (string strategy in strategyIds)
                    {
                        if (LogonArrived != null)
                        {
                            LogonArrived(strategy, marketDataProvider);
                        }
                    }
                }
                // Notify Application associated with the MDP that Logon has arrived from Broker without request
                else
                {
                    List<string> appIds;
                    if(_providersLoginRequestMap.TryGetValue(marketDataProvider, out appIds))
                    {
                        // Raise Logon Event for each Application which is connected to given given provider
                        foreach (string id in appIds)
                        {
                            if (LogonArrived != null)
                            {
                                LogonArrived(id, marketDataProvider);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnLogonArrived");
            }    
        }

        /// <summary>
        /// Raised when Market Data Logout is recieved
        /// </summary>
        private void OnLogoutEventArrived(string marketDataProvider)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Logout message recieved from: " + marketDataProvider, _type.FullName, "OnLogoutEventArrived");
                }

                List<string> strategyIds;

                // Get List of Strategies which requested for logut on given provider
                if (_logoutRequestToStrategiesMap.TryRemove(marketDataProvider, out strategyIds))
                {
                    // Raise Logout Event for each Strategy which requested Logout on given provider
                    foreach (string strategy in strategyIds)
                    {
                        if (LogoutArrived != null)
                        {
                            LogoutArrived(strategy, marketDataProvider);
                        }
                    }
                }
                // Notify Application associated with the MDP that Logout has arrived from Broker without request
                else
                {
                    List<string> appIds;
                    if (_providersLoginRequestMap.TryGetValue(marketDataProvider, out appIds))
                    {
                        // Raise Logout Event for each Application which is connected to given given provider
                        foreach (string id in appIds)
                        {
                            if (LogoutArrived != null)
                            {
                                LogoutArrived(id, marketDataProvider);
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
        /// Raised when new Tick arrived
        /// </summary>
        private void OnTickArrived(Tick tick)
        {
            try
            {
              //  if (Logger.IsDebugEnabled)
             //   {
               //     Logger.Debug(tick.ToString(), _type.FullName, "OnTickArrived");
              //  }
               
                //Task.Factory.StartNew(() => { SendTickUpdateToBarGenerator(tick); }); //TODO: To check this

                Dictionary<Security, List<string>> symbols;
                //if (_subscriptionMap.TryGetValue(tick.MarketDataProvider, out symbols))
                //{
                //    List<string> strategies;
                //    var res = (from id in symbols[new Security() {Symbol = tick.Security.Symbol}]
                //        where id.Equals("TradeHub")
                //        select id).FirstOrDefault();
                //    if (res != null)
                //    {
                //        long sequenceNumber = _ringBuffer.Next();
                //        Tick temp = _ringBuffer[sequenceNumber];
                //        // CopyTickValue(tick, temp);
                //        temp = tick;
                //        _ringBuffer.Publish(sequenceNumber);
                //    }

                //    // Get the List of Strategies which have subscribed for the current security
                //    //if (symbols.TryGetValue(new Security() { Symbol = tick.Security.Symbol }, out strategies))
                //    //{
                //    //    var result = (from id in strategies where id.Equals("TradeHub") select id).FirstOrDefault();
                //    //    if (result != null)
                //    //    {
                            
                //    //    }
                //    //}
                //}
                if (TickArrived != null)
                {
                    TickArrived(tick, "");
                }




                //Dictionary<Security, List<string>> symbols;

                //// Get Symbols details for the specified provider
                if (_subscriptionMap.TryGetValue(tick.MarketDataProvider, out symbols))
                {
                    List<string> strategies;

                    // Get the List of Strategies which have subscribed for the current security
                    if (symbols.TryGetValue(new Security() { Symbol = tick.Security.Symbol }, out strategies))
                    {
                        // Raise Tick Arrived event for all the requesting strategies
                        foreach (var strategyId in strategies)
                        {
                            if (strategyId.Equals("TradeHub"))
                            {
                                long sequenceNumber = _ringBuffer.Next();
                                Tick temp = _ringBuffer[sequenceNumber];
                                CopyTickValue(tick, temp);
                                _ringBuffer.Publish(sequenceNumber);
                            }
                            
                        }
                        
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnTickArrived");
            }
        }

        /// <summary>
        /// Raised when new Live Bar arrives
        /// </summary>
        private void OnLiveBarArrived(Bar bar, string requestID)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug(bar.ToString(), _type.FullName, "OnLiveBarArrived");
                }

                Dictionary<string, string> idsMap;
                // Get Symbols details for the specified provider
                if (_liveBarRequestsMap.TryGetValue(bar.MarketDataProvider, out idsMap))
                {
                    string appId;

                    // Get the List of Strategies which have subscribed for the current security
                    if (idsMap.TryGetValue(requestID, out appId))
                    {
                        if (LiveBarArrived != null)
                        {
                            LiveBarArrived(bar, appId);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnLiveBarArrived");
            }
        }

        /// <summary>
        /// Raised when Historic Bar Data arrives
        /// </summary>
        private void OnHistoricBarDataArrived(HistoricBarData historicBarData)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Historic Bars recieved from Gateway for request ID: " + historicBarData.ReqId,
                                 _type.FullName, "OnHistoricBarDataArrived");
                }

                Dictionary<string, string> idsDic;
                if(_historicBarRequestsMap.TryGetValue(historicBarData.MarketDataProvider, out idsDic))
                {
                    string appId;
                    if (idsDic.TryGetValue(historicBarData.ReqId, out appId))
                    {
                        // Raise Event
                        if (HistoricalBarsArrived != null)
                        {
                            HistoricalBarsArrived(historicBarData, appId);
                        }

                        // Remove request from the internal map
                        idsDic.Remove(historicBarData.ReqId);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnHistoricBarDataArrived");
            }
        }

        #endregion

        #region Send requests to Market Data Provider

        /// <summary>
        /// Sends Subscription Reuqest to the Market Data Provider Gateway
        /// </summary>
        private void SendTickSubscriptionRequest(ILiveTickDataProvider liveDataProvider, Subscribe subscribe)
        {
            try
            {
                // Register Market Data Events
                HookTickDataEvents(liveDataProvider);
             
                // Send Subscription Call to the Market Data Provider Gateway
                liveDataProvider.SubscribeTickData(subscribe);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendTickSubscriptionRequest");
            }
        }

        /// <summary>
        /// Sends Unsubscription Request to the Market Data Provier Gateway
        /// </summary>
        private void SendTickUnsubcriptionRequest(ILiveTickDataProvider liveDataProvider, Unsubscribe unsubscribe)
        {
            try
            {
                // Send Unsubscription Call to the Market Data Provider Gateway
                liveDataProvider.UnsubscribeTickData(unsubscribe);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendTickUnsubcriptionRequest");
            }
        }

        /// <summary>
        /// Send Live Bar subscription request to the Market Data Provider Gateway
        /// </summary>
        private void SendLiveBarSubscriptionRequest(ILiveBarDataProvider liveBarDataProvider, BarDataRequest barDataRequest)
        {
            try
            {
                // Register Events
                HookLiveBarEvents(liveBarDataProvider);

                // Send subscription request
                liveBarDataProvider.SubscribeBars(barDataRequest);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendLiveBarSubscriptionRequest");
            }
        }

        /// <summary>
        /// Send Live Bar unsubscription request to the Market Data Provider Gateway
        /// </summary>
        private void SendLiveBarUnsubscriptionRequest(ILiveBarDataProvider liveBarDataProvider, BarDataRequest barDataRequest)
        {
            try
            {
                // Send unsubscription request
                liveBarDataProvider.UnsubscribeBars(barDataRequest);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendLiveBarUnsubscriptionRequest");
            }
        }

        /// <summary>
        /// Sends Historic Bar Data request to the Market Data Provider gateway
        /// </summary>
        private void SendHistoricBarDataRequest(IHistoricBarDataProvider barDataProvider,
                                                HistoricDataRequest historicDataRequest)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Sending Historic Bar Data request on: " + historicDataRequest.MarketDataProvider,
                                _type.FullName, "SendHistoricBarDataRequest");
                }

                // Hook Events
                HookHistoricBarDataEvents(barDataProvider);

                // Send request to the gateway
                barDataProvider.HistoricBarDataRequest(historicDataRequest);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendHistoricBarDataRequest");
            }
        }

        #endregion

        /// <summary>
        /// Proivdes required information regarding the given market data provider
        /// </summary>
        /// <param name="marketDataProvider">Name of market data provider</param>
        public InquiryResponse GetMarketDataProviderInfo(string marketDataProvider)
        {
            try
            {
                // Get instance of given market data provider
                var dataProvider = GetMarketDataInstance(marketDataProvider);

                if (dataProvider == null)
                {
                    return null;
                }

                InquiryResponse inquiryResponse = new InquiryResponse();

                // Check if Tick data is supported
                if (dataProvider is ILiveTickDataProvider)
                {
                    inquiryResponse.MarketDataProviderInfo.Add(typeof (ILiveTickDataProvider));
                }
                // Check if live Bar data is supported
                if (dataProvider is ILiveBarDataProvider)
                {
                    inquiryResponse.MarketDataProviderInfo.Add(typeof (ILiveBarDataProvider));
                }
                // check if historical Bar data is supported
                if (dataProvider is IHistoricBarDataProvider)
                {
                    inquiryResponse.MarketDataProviderInfo.Add(typeof (IHistoricBarDataProvider));
                }

                return inquiryResponse;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "GetMarketDataProviderInfo");
                return null;
            }
        }

        /// <summary>
        /// Send new incoming Tick to Live Bar Generator to update Bars
        /// </summary>
        /// <param name="tick">TradeHub Tick</param>
        private void SendTickUpdateToBarGenerator(Tick tick)
        {
            // Send new Tick value
            _liveBarGenerator.UpdateBar(tick);
        }

        /// <summary>
        /// Raised when a new Bar is Recieved from the Live Bar Generator
        /// </summary>
        /// <param name="bar">TradeHub Bar</param>
        /// <param name="reqIds">List of Request IDs for the Given Bar</param>
        private void OnLiveBarGeneratorBarArrived(Bar bar, List<string> reqIds)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("New Bar Recieved from the Bar Generator: " + bar, _type.FullName, "OnLiveBarGeneratorBarArrived");
                }

                Dictionary<string, string> idsMap;

                // Get Symbols details for the specified provider
                if (_liveBarRequestsMap.TryGetValue(bar.MarketDataProvider, out idsMap))
                {
                    if (LiveBarArrived != null)
                    {
                        // Raise event for all the Requested Clients
                        foreach (string reqId in reqIds)
                        {
                            if (Logger.IsDebugEnabled)
                            {
                                Logger.Debug("Adding request ID: " + reqId, _type.FullName, "OnLiveBarGeneratorBarArrived");
                            }

                            // Add Requested ID to the Bar
                            bar.RequestId = reqId;

                            LiveBarArrived(bar, idsMap[reqId]);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "OnLiveBarGeneratorBarArrived");
            }
        }

        /// <summary>
        /// Returns the MarketDataProvider instance for the requested Provider
        /// </summary>
        private IMarketDataProvider GetMarketDataInstance(string provideName)
        {
            try
            {
                // Get Market Data Provider Instance
                return DataProviderInitializer.GetMarketDataProviderInstance(provideName);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "GetMarketDataInstance");
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
                foreach (IMarketDataProvider marketDataProvider in _providersMap.Values)
                {
                    marketDataProvider.Stop();
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "StopProcessing");
            }
        }

        public void OnNext(Tick data, long sequence, bool endOfBatch)
        {
            SendTickUpdateToBarGenerator(data);
        }

        private void CopyTickValue(Tick origTick,Tick tempTick)
        {
            //tempTick = origTick;
            tempTick.AskPrice = origTick.AskPrice;
            tempTick.AskSize = origTick.AskSize;
            tempTick.BidPrice = origTick.BidPrice;
            tempTick.BidSize = origTick.BidSize;
            tempTick.LastPrice = origTick.LastPrice;
            tempTick.LastSize = origTick.LastSize;
            tempTick.MarketDataProvider = origTick.MarketDataProvider;
            tempTick.Security = new Security() { Symbol = origTick.Security.Symbol };
            tempTick.DateTime = origTick.DateTime;


        }
    }
}