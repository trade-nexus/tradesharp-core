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
using System.Text;
using System.Threading;
using fxcore2;
using TraceSourceLogger;

namespace TradeHub.MarketDataProvider.Fxcm.Provider
{
    internal class SessionStatusListener : IO2GSessionStatus
    {
        private Type _type = typeof (SessionStatusListener);

        /// <summary>
        /// Holds reference to the calling class logger object
        /// </summary>
        private readonly AsyncClassLogger _logger;

        private bool _connected;
        private bool _error;
        private O2GSession _session;
        private EventWaitHandle _syncSessionEvent;

        #region Events

        // ReSharper Disable InconsistentNaming
        private event Action<bool> _connectionEvent;
        // ReSharper Enable InconsistentNaming

        /// <summary>
        /// Event is raised when connection state changes
        /// </summary>
        public event Action<bool> ConnectionEvent
        {
            add
            {
                if (_connectionEvent == null)
                {
                    _connectionEvent += value;
                }
            }
            remove { _connectionEvent -= value; }
        }

        #endregion

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="session"></param>
        /// <param name="logger"></param>
        public SessionStatusListener(O2GSession session, AsyncClassLogger logger)
        {
            _session = session;
            _logger = logger;
            Reset();
            _syncSessionEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
        }

        public bool Connected
        {
            get { return _connected; }
        }

        public bool Error
        {
            get { return _error; }
        }

        /// <summary>
        /// Resets local variables to default values
        /// </summary>
        public void Reset()
        {
            _connected = false;
            _error = false;
        }

        /// <summary>
        /// Adds delay
        /// </summary>
        /// <returns></returns>
        public bool WaitEvents()
        {
            return _syncSessionEvent.WaitOne(30000);
        }

        /// <summary>
        /// Called when FXCM session status changes
        /// </summary>
        /// <param name="status"></param>
        public void onSessionStatusChanged(O2GSessionStatusCode status)
        {
            switch (status)
            {
                case O2GSessionStatusCode.Connected:
                    _connected = true;
                    _syncSessionEvent.Set();

                    // Raise event to notify listeners
                    if (_connectionEvent != null)
                    {
                        _connectionEvent(true);
                    }

                    break;
                case O2GSessionStatusCode.Disconnected:
                    _connected = false;
                    _syncSessionEvent.Set();

                    // Raise event to notify listeners
                    if (_connectionEvent != null)
                    {
                        _connectionEvent(false);
                    }

                    break;
            }
        }

        /// <summary>
        /// Called when login fails
        /// </summary>
        /// <param name="error"></param>
        public void onLoginFailed(string error)
        {
            _error = true;
            _logger.Error(error, _type.FullName, "onLoginFailed");
        }
    }
}
