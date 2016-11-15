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
using System.Threading;
using System.Threading.Tasks;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.DataDownloader.Common.ConcreteImplementation;
using TradeHub.DataDownloader.UserInterface.Common;
using TradeHub.DataDownloader.UserInterface.Common.Messages;
using TradeHub.MarketDataEngine.Client.Service;
using TradeHub.StrategyEngine.HistoricalData;
using TradeHub.StrategyEngine.MarketData;

namespace TradeHub.DataDownloader.ApplicationCenter
{
    /// <summary>
    /// This Class is the Main Heart of Our application.
    /// It will perform all kind of Communicatoin between Ui Layer and Backend
    /// </summary>
    public class ApplicationControl
    {
        private Type _oType = typeof(ApplicationControl);

        #region Fields
        private MarketDataResponseHandler _responseHandler;
        private MarketDataService _marketDataService;
        private HistoricalDataService _historicalDataService;
        private object _lock = new object();
        private ConcurrentQueue<Bar> _barsQueue;
        private ConcurrentQueue<Tick> _ticksQueue;
        private ConcurrentQueue<HistoricBarData> _historicBarsQueue;
        private BlockingCollection<Bar> _blockingCollectionBar;
        private BlockingCollection<Tick> _blockingCollectionTick;
        private BlockingCollection<HistoricBarData> _blockingCollectionHistoricBarData;
        #endregion

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="responseHandler"></param>
        /// <param name="marketDataService"></param>
        /// <param name="historicalDataService"></param>
        public ApplicationControl(MarketDataResponseHandler responseHandler, MarketDataService marketDataService, HistoricalDataService historicalDataService )
        {
            try
            {
                #region Initialization of BlockingCollection
                _barsQueue = new ConcurrentQueue<Bar>();
                _blockingCollectionBar = new BlockingCollection<Bar>(_barsQueue);
                _ticksQueue = new ConcurrentQueue<Tick>();
                _blockingCollectionTick = new BlockingCollection<Tick>(_ticksQueue);
                _historicBarsQueue = new ConcurrentQueue<HistoricBarData>();
                _blockingCollectionHistoricBarData = new BlockingCollection<HistoricBarData>(_historicBarsQueue);
                #endregion

                _responseHandler = responseHandler;
                _marketDataService = marketDataService;
                _historicalDataService = historicalDataService;

                #region MarketDataEngine Events

                _marketDataService.LogonArrived += MarketDataEngineClientLogonArrived;
                _marketDataService.LogoutArrived += MarketDataEngineClientLogoutArrived;

                _marketDataService.TickArrived += MarketDataEngineClientTickArrived;
                _marketDataService.BarArrived += MarketDataEngineClientBarArrived;

                _historicalDataService.HistoricalDataArrived += MarketDataEngineClientHistoricBarsArrived;

                #endregion

                _marketDataService.StartService();
                _historicalDataService.StartService();

                #region Event Aggregator

                EventSystem.Subscribe<Login>(SendLogonRequest);
                EventSystem.Subscribe<SecurityPermissions>(RequestDataFromMarketDataEngine);
                EventSystem.Subscribe<Unsubscribe>(UnsubscribeSecurity);
                EventSystem.Subscribe<ProviderPermission>(ChangeInProiderPermission);
                EventSystem.Subscribe<ChangeSecurityPermissionsMessage>(UpdateSecurityPermissions);
                EventSystem.Subscribe<Logout>(Logout);
                EventSystem.Subscribe<BarDataRequest>(RequestBarData);
                EventSystem.Subscribe<UnsubscribeBars>(UnsubscribeBarRequest);
                EventSystem.Subscribe<HistoricDataRequest>(RequestHistoricalData);
                EventSystem.Subscribe<string>(OnApplicationClose);
                #endregion

                #region Queue Initialization

                Task.Factory.StartNew(ReadTicksFromQueue);
                Task.Factory.StartNew(ReadBarsFromQueue);
                Task.Factory.StartNew(ReadHistoricBarFromQueue);

                #endregion
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "ApplicationControl");
            }
        }

        /// <summary>
        /// Event Fired When Historic Bars Arrives
        /// </summary>
        /// <param name="obj"></param>
        private void MarketDataEngineClientHistoricBarsArrived(HistoricBarData obj)
        {
            try
            {
                _blockingCollectionHistoricBarData.TryAdd(obj);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "MarketDataEngineClientHistoricBarsArrived");
            }
        }

        /// <summary>
        /// Request Historic data from Provider
        /// </summary>
        /// <param name="historicDataRequest"></param>
        public void RequestHistoricalData(HistoricDataRequest historicDataRequest)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Requesting Historic data for" + historicDataRequest, _oType.FullName,
                                "RequestHistoricalData");
                }

                // Request Historic Data
                _historicalDataService.Subscribe(historicDataRequest);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "RequestHistoricalData");
            }
        }

        /// <summary>
        /// Unsubscribe 
        /// </summary>
        /// <param name="unsubscribeBars"></param>
        public void UnsubscribeBarRequest(UnsubscribeBars unsubscribeBars)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Sending bar data unsubscribe Request" + unsubscribeBars.UnSubscribeBarDataRequest,
                                 _oType.FullName, "UnsubscribeRequest");
                }

                // Send Unsubscription request
                _marketDataService.Unsubscribe(unsubscribeBars.UnSubscribeBarDataRequest);
                _responseHandler.RemoveBarRequest(unsubscribeBars.UnSubscribeBarDataRequest);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "UnsubscribeRequest");
            }
        }

        /// <summary>
        /// Event Fired on Every Tick Update
        /// </summary>
        public void MarketDataEngineClientBarArrived(Bar bar)
        {
            try
            {
                _blockingCollectionBar.TryAdd(bar);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "MarketDataEngineClientTickArrived");
            }
        }

        /// <summary>
        /// User Changes Permission
        /// </summary>
        /// <param name="permissionsMessage"></param>
        public void UpdateSecurityPermissions(ChangeSecurityPermissionsMessage permissionsMessage)
        {
            _responseHandler.ChangeSecurityPermission(permissionsMessage.Permissions);
        }

        /// <summary>
        /// When User Request To Change Provider Permission
        /// </summary>
        /// <param name="providerPermission"></param>
        private void ChangeInProiderPermission(ProviderPermission providerPermission)
        {
            try
            {
                _responseHandler.ChangeProviderPermissions(providerPermission);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "ChangeInProiderPermission");
            }
        }

        /// <summary>
        /// Methord To Unsubscribe Security
        /// </summary>
        /// <param name="unsubscribe"></param>
        private void UnsubscribeSecurity(Unsubscribe unsubscribe)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(unsubscribe.ToString(), _oType.FullName, "UnsubscribeSecurity");
                }

                // Send Tick unsubscription request
                _marketDataService.Unsubscribe(unsubscribe);
                _responseHandler.OnSymbolUnSubscribed(unsubscribe.Security);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "UnsubscribeSecurity");
            }
        }

        /// <summary>
        /// Event Fired on Logout Arrived
        /// </summary>
        /// <param name="obj"></param>
        private void MarketDataEngineClientLogoutArrived(string obj)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug(obj, _oType.FullName, "MarketDataEngineClientLogoutArrived");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "MarketDataEngineClientLogoutArrived");
            }
        }

        /// <summary>
        /// Event Fired When Application Recieves Logon from MarketDataEngine
        /// </summary>
        /// <param name="obj"></param>
        private void MarketDataEngineClientLogonArrived(string obj)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug(obj, _oType.FullName, "MarketDataEngineClientLogonArrived");
                }

                ProviderPermission currentPermission;
                // Try to get existing Permission settings
                if (!_responseHandler.ProviderPermissionDictionary.TryGetValue(obj, out currentPermission))
                {
                    // Create a new Permissions object
                    currentPermission = new ProviderPermission {MarketDataProvider = obj, WriteCsv = true};
                }

                _responseHandler.OnProviderConnected(currentPermission);
                EventSystem.Publish(new LoginArrivedMessage { Provider = new Provider { IsConnected = true, ProviderName = obj } });
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "MarketDataEngineClientLogonArrived");
            }
        }

        /// <summary>
        /// Event Fired When Ever Application Recieves Tick From MarketDataEngine
        /// </summary>
        /// <param name="tick"></param>
        private void MarketDataEngineClientTickArrived(Tick tick)
        {
            try
            {
                _blockingCollectionTick.TryAdd(tick);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "MarketDataEngineClientTickArrived");
            }
        }

        /// <summary>
        /// Sends Login Request to MarketDataEngine
        /// </summary>
        /// <param name="login"></param>
        public void SendLogonRequest(Login login)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug(login.ToString(), _oType.FullName, "MarketDataEngineClientLogonArrived");
                }

                // Send login requests
                _marketDataService.Login(login);
                _historicalDataService.Login(login);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "SendLogonRequest");
            }
        }

        /// <summary>
        /// Get Requset From User And forwards it to MarketDataEngine
        /// </summary>
        /// <param name="subscribe"></param>
        public void RequestDataFromMarketDataEngine(SecurityPermissions subscribe)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug(subscribe.ToString(), _oType.FullName, "RequestDataFromMarketDataEngine");
                }
                _responseHandler.OnSymbolSubscribed(subscribe);
                
                // Send Tick Subscription Request
                _marketDataService.Subscribe(subscribe);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "RequestDataFromMarketDataEngine");
            }
        }

        /// <summary>
        /// Log out Market Data Engine
        /// </summary>
        /// <param name="logout"></param>
        public void Logout(Logout logout)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug(logout.ToString(), _oType.FullName, "Logout");
                }

                // Send Logout Request
                _marketDataService.Logout(logout);
                _historicalDataService.Logout(logout);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "LogOut");
            }
        }

        /// <summary>
        /// Reads Bars From Queue
        /// </summary>
        private void ReadTicksFromQueue()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        Tick tick = _blockingCollectionTick.Take();
                            _responseHandler.HandleTickArrived(tick);
                            EventSystem.Publish(tick);
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(exception, _oType.FullName, "ReadTicksFromQueue");
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "ReadTicksFromQueue");
            }
        }

        /// <summary>
        /// Read Historic Data from Queue
        /// </summary>
        private void ReadHistoricBarFromQueue()
        {
            try
            {
                while (true)
                {
                    HistoricBarData barData = _blockingCollectionHistoricBarData.Take();
                        _responseHandler.SaveHistoricBarData(barData);
                        EventSystem.Publish(barData);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "ReadTicksFromQueue");
            }
        }

        /// <summary>
        /// Reads Bars From Queue
        /// </summary>
        private void ReadBarsFromQueue()
        {
            try
            {
                while (true)
                {

                    try
                    {
                        Bar bar = _blockingCollectionBar.Take();
                            _responseHandler.HandleBarArrived(bar);
                            EventSystem.Publish(bar);
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(exception, _oType.FullName, "ReadBarsFromQueue");
                    }
                    
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "ReadTicksFromQueue");
            }
        }

        /// <summary>
        /// Event Fired When User Request Bar Data
        /// </summary>
        public void RequestBarData(BarDataRequest barDataRequest)
        {
            try
            {
                // Send Live bar subscription requests
                _marketDataService.Subscribe(barDataRequest);

                _responseHandler.NewBarRequestArrived(barDataRequest);
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Sending Bar Request To Market Data Engine" + barDataRequest, _oType.FullName, "RequestBarData");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "RequestBarData");
            }
        }

        /// <summary>
        /// Methord Fired On Application Close
        /// </summary>
        public void OnApplicationClose(string value)
        {
            _marketDataService.UnsubscribeAllSecurities();
            _marketDataService.UnsubscribeAllLiveBars();

            // Wait for the unsubscription requests to be completed
            Thread.Sleep(1000);

            _marketDataService.StopService();
            _historicalDataService.StopService();
        }
    }
}
