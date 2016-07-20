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
