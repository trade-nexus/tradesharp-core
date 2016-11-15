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
using System.Threading;
using System.Threading.Tasks;
using fxcore2;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.MarketDataProvider;
using TradeHub.Common.Core.Utility;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataProvider.Fxcm.ValueObject;
using Constants = TradeHub.Common.Core.Constants;

namespace TradeHub.MarketDataProvider.Fxcm.Provider
{
    public class FxcmMarketDataProvider : ILiveTickDataProvider, IHistoricBarDataProvider
    {
        private Type _type = typeof(FxcmMarketDataProvider);

        private AsyncClassLogger _logger;

        /// <summary>
        /// Indicates if the Provider session is connected or not
        /// </summary>
        private bool _isConnected;

        /// <summary>
        /// Contains Provider name used through out TradeSharp
        /// </summary>
        private readonly string _marketDataProviderName;

        /// <summary>
        /// Contains connection details required for communication
        /// </summary>
        private ConnectionParameters _connectionParameters;

        /// <summary>
        /// Responsible for communicating with FXCM Server
        /// </summary>
        private O2GSession _session = null;

        /// <summary>
        /// Provides data access
        /// </summary>
        private O2GTableManager _tableManager;

        /// <summary>
        /// Handles FXCM session status changes
        /// </summary>
        private SessionStatusListener _statusListener;

        /// <summary>
        /// Provides access to Market data
        /// </summary>
        private PriceListener _priceListener;

        /// <summary>
        ///  Provides access to Historical data
        /// </summary>
        private HistoricalDataListener _historicalDataListener;

        #region Events

        /// <summary>
        /// Fired each time a Logon is arrived
        /// </summary>
        public event Action<string> LogonArrived;

        /// <summary>
        /// Fired each time a Logout is arrived
        /// </summary>
        public event Action<string> LogoutArrived;

        /// <summary>
        /// Fired each time a new tick arrives.
        /// </summary>
        public event Action<Tick> TickArrived;

        /// <summary>
        /// Fired when requested Historic Bar Data arrives
        /// </summary>
        public event Action<HistoricBarData> HistoricBarDataArrived;

        /// <summary>
        /// Fired each time when market data rejection arrives.
        /// </summary>
        public event Action<MarketDataEvent> MarketDataRejectionArrived;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public FxcmMarketDataProvider()
        {
            // Create object for logging details
            _logger = new AsyncClassLogger("FxcmDataProvider");
            _logger.SetLoggingLevel();
            _logger.LogDirectory(Constants.DirectoryStructure.MDE_LOGS_LOCATION);

            // Set provider name
            _marketDataProviderName = Constants.MarketDataProvider.Fxcm;
        }

        #region Connection Methods

        /// <summary>
        /// Indicates if the Provider session is connected or not
        /// </summary>
        public bool IsConnected()
        {
            return _isConnected;
        }

        /// <summary>
        /// Connects/Starts a client
        /// </summary>
        public bool Start()
        {
            try
            {
                // Read account credentials
                ReadConnectionParameters();

                if (_connectionParameters == null)
                {
                    Logger.Info("Connection Parameters unavailable", _type.FullName, "Start");

                    return false;
                }

                // Opens a new FXCM session
                OpenSession();

                return true;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "Start");
            }
            return false;
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
                    // Unsubscribe all price updates
                    _priceListener.UnsubscribeAll();

                    // Close existing session
                    CloseSession();

                    return true;
                }

                return false;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "Stop");
                return false;
            }
        }

        #endregion

        #region Subscription

        /// <summary>
        /// Market data request message
        /// </summary>
        public bool SubscribeTickData(Subscribe request)
        {
            try
            {
                if (_isConnected)
                {
                    // Send subscription request
                    _priceListener.Subscribe(request.Security.Symbol);

                    return true;
                }

                return false;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "SubscribeTickData");
                return false;
            }
        }

        /// <summary>
        /// Unsubscribe Market data message
        /// </summary>
        public bool UnsubscribeTickData(Unsubscribe request)
        {
            try
            {
                if (_isConnected)
                {
                    // unsubscribe price feed
                    _priceListener.Unsubscribe(request.Security.Symbol);

                    return true;
                }

                return false;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "UnsubscribeTickData");
                return false;
            }
        }

        /// <summary>
        /// Historic Bar Data Request Message
        /// </summary>
        public bool HistoricBarDataRequest(HistoricDataRequest historicDataRequest)
        {
            try
            {
                if (_isConnected)
                {
                    // Forward request
                    _historicalDataListener.GetHistoricalData(historicDataRequest);

                    return true;
                }

                return false;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "HistoricBarDataRequest");
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Open Session to FXCM
        /// </summary>
        private void OpenSession()
        {
            try
            {
                // Create a new session object
                _session = O2GTransport.createSession();

                _session.useTableManager(O2GTableManagerMode.Yes, null);
                // Initialize listener
                _statusListener = new SessionStatusListener(_session, _logger);
                
                // Register event
                _statusListener.ConnectionEvent += OnSessionStatusChanged;

                // Register local listener
                _session.subscribeSessionStatus(_statusListener);

                _statusListener.Reset();
                
                // Send login request
                _session.login(_connectionParameters.LoginId, _connectionParameters.Password, _connectionParameters.Url,
                    _connectionParameters.Connection);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "OpenSession");
            }
        }

        /// <summary>
        /// Close FXCM session
        /// </summary>
        private void CloseSession()
        {
            try
            {
                // Send logout request
                _session.logout();
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "CloseSession");
            }
        }

        /// <summary>
        /// Called when FXCM session status changes
        /// </summary>
        /// <param name="status"></param>
        private void OnSessionStatusChanged(bool status)
        {
            _isConnected = status;

            if (_logger.IsInfoEnabled)
            {
                _logger.Info("Data Feed Server connected: " + _isConnected, _type.FullName, "OnSessionStatusChanged");
            }

            if (_isConnected)
            {
                // Initialize Table Manager for price updates
                _tableManager = _session.getTableManager();

                // Initialize Price listener
                _priceListener = new PriceListener(_tableManager, _logger);

                // Initialize Historical Data listener
                _historicalDataListener = new HistoricalDataListener(_session, _logger);
                _session.subscribeResponse(_historicalDataListener);

                _priceListener.DataEvent += OnDataArrived;
                _historicalDataListener.HistoricalDataEvent += OnHistoricalDataArrived;

                // Raise event to notify listeners
                if (LogonArrived != null)
                {
                    LogonArrived(_marketDataProviderName);
                }
            }
            else
            {
                // Unsubscribe Events
                _statusListener.ConnectionEvent -= OnSessionStatusChanged; 
                _priceListener.DataEvent -= OnDataArrived;
                _historicalDataListener.HistoricalDataEvent -= OnHistoricalDataArrived;

                _session.unsubscribeSessionStatus(_statusListener);
                _session.unsubscribeResponse(_historicalDataListener);

                _session.Dispose();
                _session = null;
                _statusListener = null;
                _tableManager = null;

                // Raise event to notify listeners
                if (LogoutArrived != null)
                {
                    LogoutArrived(_marketDataProviderName);
                }
            }
        }

        /// <summary>
        /// Reads required connections pamareters from stored file
        /// </summary>
        private void ReadConnectionParameters()
        {
            // Read parameters from given file
            var parameters = ParameterReader.ReadParamters("FxcmParams.xml", "Fxcm");

            if (parameters.Count == 5)
            {
                string loginId;
                parameters.TryGetValue("Login", out loginId);

                string password;
                parameters.TryGetValue("Password", out password);

                string account;
                parameters.TryGetValue("Account", out account);

                string connection;
                parameters.TryGetValue("Connection", out connection);

                string url;
                parameters.TryGetValue("Url", out url);

                // Create new object
                _connectionParameters = new ConnectionParameters(loginId, password, account, connection, url);

                return;
            }

            _connectionParameters = null;
        }

        /// <summary>
        /// Called when new Tick data is received
        /// </summary>
        /// <param name="tick"></param>
        private void OnDataArrived(Tick tick)
        {
            // Raise Event to notify listeners
            if (TickArrived != null)
            {
                TickArrived(tick);
            }
        }

        /// <summary>
        /// Called when requested Historical data is received
        /// </summary>
        /// <param name="historicBarData"></param>
        private void OnHistoricalDataArrived(HistoricBarData historicBarData)
        {
            // Raise Event to notify listeners
            if (HistoricBarDataArrived != null)
            {
                HistoricBarDataArrived(historicBarData);
            }
        }
    }
}
