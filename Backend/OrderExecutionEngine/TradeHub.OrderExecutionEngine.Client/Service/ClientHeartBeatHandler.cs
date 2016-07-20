using System;
using System.Timers;
using Spring.Context.Support;
using TraceSourceLogger;
using TradeHub.Common.Core.ValueObjects.Heartbeat;

namespace TradeHub.OrderExecutionEngine.Client.Service
{
    /// <summary>
    /// Responsible for creating heartbeat to keep the connection alive
    /// </summary>
    internal class ClientHeartBeatHandler
    {
        private Type _type = typeof (ClientHeartBeatHandler);
        private AsyncClassLogger _asyncClassLogger;

        /// <summary>
        /// Duration between successive Heartbeats in milliseconds
        /// </summary>
        private int _heartbeatInterval;

        /// <summary>
        /// Timer responsible for generating periodic heartbeat messages
        /// </summary>
        private readonly Timer _heartbeatTimer;

        /// <summary>
        /// Timer responsible for keeping Track of Server Heartbeat Response
        /// </summary>
        private readonly Timer _serverResponseTimer;

        /// <summary>
        /// Heartbeat Message to be Published
        /// </summary>
        private HeartbeatMessage _heartbeatMessage;

        private int _heartbeatValidationInterval = 10000;

        /// <summary>
        /// Notifies listeners to send new Heartbeat message
        /// </summary>
        private event Action<HeartbeatMessage> _sendHeartbeat;

        /// <summary>
        /// Notifies listeners about MDE disconnection
        /// </summary>
        private event Action _serverDisconnected; 

        /// <summary>
        /// Notifies listener to send new Heartbeat message
        /// </summary>
        public event Action<HeartbeatMessage> SendHeartbeat
        {
            add
            {
                if (_sendHeartbeat == null)
                {
                    _sendHeartbeat += value;
                }
            }
            remove { _sendHeartbeat -= value; }
        }

        /// <summary>
        /// Notifies listener about MDE disconnection
        /// </summary>
        public event Action ServerDisconnected
        {
            add
            {
                if (_serverDisconnected == null)
                {
                    _serverDisconnected += value;
                }
            }
            remove { _serverDisconnected -= value; }
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="applicationId">Unique Application ID</param>
        /// <param name="asyncClassLogger">Class level logger for order client</param>
        /// <param name="heartbeatInterval">Duration between successive Heartbeats in milliseconds</param>
        public ClientHeartBeatHandler(string applicationId, AsyncClassLogger asyncClassLogger, int heartbeatInterval = 60000)
        {
            _asyncClassLogger = asyncClassLogger;

            _heartbeatInterval = heartbeatInterval;

            // Initialize
            _heartbeatTimer = new Timer();
            _serverResponseTimer = new Timer();
            _heartbeatMessage= new HeartbeatMessage();

            _heartbeatMessage.ApplicationId = applicationId;

            _heartbeatTimer.Elapsed += OnHeartbeatTimerElapsed;
            _serverResponseTimer.Elapsed += OnServerResponseTimerElapsed;
        }

        /// <summary>
        /// Starts generating Heartbeat requests after specified intervals
        /// </summary>
        public void StartHandler()
        {
            // Add/Update Heartbeat Message Info
            _heartbeatMessage.HeartbeatInterval = _heartbeatInterval;
            
            // Set Heartbeat Interval
            _heartbeatTimer.Interval = _heartbeatInterval;
            // Start Heartbeat Timer
            _heartbeatTimer.Start();
        }

        /// <summary>
        /// Stops generating Heartbeat requests 
        /// </summary>
        public void StopHandler()
        {
            // Stop Heartbeat Timer
            _heartbeatTimer.Stop();
        }

        /// <summary>
        /// Handles the new incoming Server Heartbeat
        /// </summary>
        /// <param name="serverHeartbeatInterval">Time Interval after which MDE will send response Heartbeat</param>
        public void Update(int serverHeartbeatInterval)
        {
            try
            {
                // Stop Timer for processing
                StopValidationTimer();

                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Server Heartbeat received", _type.FullName, "Update");
                }

                // Start Timer after processing
                StartValidationTimer(serverHeartbeatInterval);
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "Update");
            }
        }

        /// <summary>
        /// Starts Heartbeat Validation Timer
        /// </summary>
        public void StartValidationTimer(int serverHeartbeatInterval = 120000)
        {
            // Adjust Heartbeat validation timer
            _serverResponseTimer.Interval = serverHeartbeatInterval + _heartbeatValidationInterval;

            // Start Validation Timer
            _serverResponseTimer.Start();
        }

        /// <summary>
        /// Stops Heatbeat validation timer
        /// </summary>
        public void StopValidationTimer()
        {
            // Stop Validation Timer
            _serverResponseTimer.Stop();
        }

        /// <summary>
        /// Raised when Heartbeat Timer is Elapsed
        /// </summary>
        private void OnHeartbeatTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Timer elapsed for heartbeat message.", _type.FullName, "OnHeartbeatTimerElapsed");
                }

                // Raise event to send new heartbeat message
                if (_sendHeartbeat != null)
                {
                    _sendHeartbeat(_heartbeatMessage);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnHeartbeatTimerElapsed");
            }
        }

        /// <summary>
        /// Raised when Server Heartbeat Timer Elapse
        /// </summary>
        void OnServerResponseTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Stop Response Timer
            StopHandler();

            // Stop Validation Timer
            StopValidationTimer();

            if (_asyncClassLogger.IsDebugEnabled)
            {
                _asyncClassLogger.Debug("Expected Server shutdown due to missed heartbeat", _type.FullName, "OnServerResponseTimerElapsed");
            }

            // Raise Disconnect Event
            if (_serverDisconnected != null)
            {
                _serverDisconnected();
            }
        }
        
    }
}
