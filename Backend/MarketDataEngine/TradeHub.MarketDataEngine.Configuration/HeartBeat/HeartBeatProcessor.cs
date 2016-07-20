using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TraceSourceLogger;
using TradeHub.Common.Core.ValueObjects.Heartbeat;

namespace TradeHub.MarketDataEngine.Configuration.HeartBeat
{
    /// <summary>
    /// Performs analysis of heartbeat messages and notifies if the heartbeat is missed
    /// </summary>
    internal class HeartBeatProcessor
    {
        private Type _type = typeof (HeartBeatProcessor);

        private readonly string _applicationId;
        private readonly int _heartbeatValidationInterval;

        private readonly int _heartbeatResponseInterval;
        private readonly int _heartbeatInterval;

        private readonly HeartbeatMessage _serverHeartbeat;

        #region Events

        // ReSharper Disable InconsistentNaming
        private event Action<string> _disconnect;
        private event Action<HeartbeatMessage> _response;
        // ReSharper Enable InconsistentNaming

        /// <summary>
        /// Event is raised when heartbeat messages stop
        /// </summary>
        public event Action<string> Disconnect
        {
            add
            {
                if (_disconnect == null)
                {
                    _disconnect += value;
                }
            }
            remove { _disconnect -= value; }
        }

        /// <summary>
        /// Event is raised to send Heartbeat response
        /// </summary>
        public event Action<HeartbeatMessage> Response
        {
            add
            {
                if (_response == null)
                {
                    _response += value;
                }
            }
            remove { _response -= value; }
        }

        #endregion

        /// <summary>
        /// Timer is elapsed after Heartbeat validation interval 
        /// Reset on new heartbeat arrival
        /// </summary>
        private readonly Timer _validationTimer;

        /// <summary>
        /// Timer is elapsed to Send Heartbeat response
        /// </summary>
        private readonly Timer _responseTimer;
 
        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="heartbeat">TradeHub Heartbeat Message</param>
        /// <param name="heartbeatValidationInterval">Time interval to wait for Heartbeat before issuing disconnect event</param>
        /// <param name="heartbeatResponseInterval">Timer Interval after which to send Heartbeat response</param>
        public HeartBeatProcessor(HeartbeatMessage heartbeat, int heartbeatValidationInterval, int heartbeatResponseInterval)
        {
            _applicationId = heartbeat.ApplicationId;
            _heartbeatInterval = heartbeat.HeartbeatInterval;
            _heartbeatValidationInterval = heartbeatValidationInterval;
            _heartbeatResponseInterval = heartbeatResponseInterval;

            // Initialize Server Heartbeat response 
            _serverHeartbeat = new HeartbeatMessage
                {
                    ApplicationId = _applicationId,
                    HeartbeatInterval = _heartbeatResponseInterval,
                    ReplyTo = heartbeat.ReplyTo
                };

            double disconnectInterval = _heartbeatInterval + _heartbeatValidationInterval;

            // Initialize Timers
            _validationTimer = new Timer();
            _responseTimer = new Timer();
            
            // Set Timer Intervals
            _validationTimer.Interval = disconnectInterval;
            _responseTimer.Interval = _heartbeatResponseInterval;

            // Register Elapse Events
            _validationTimer.Elapsed += OnValidationTimerElapsed;
            _responseTimer.Elapsed += OnResponseTimerElapsed;

            // Start Validation Timer
            StartValidationTimer();

            // Start Heartbeat Response Timer
            StartResponseTimer();
        }

        /// <summary>
        /// Called when HeartBeat arrives from the given Application
        /// </summary>
        public void Update()
        {
            try
            {
                // Stop Timer for processing
                StopValidationTimer();

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("New Heartbeat received from: " + _applicationId, _type.FullName, "Update");
                }

                // Start Timer after processing
                StartValidationTimer();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "Update");
            }
        }

        /// <summary>
        /// Raised on Validation Timer Elapse
        /// </summary>
        private void OnValidationTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Stop Response Timer
            StopResponseTimer();

            // Stop Validation Timer
            StopValidationTimer();

            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Validation Timer elapsed for: " + _applicationId, _type.FullName, "OnValidationTimerElapsed");
            }

            // Raise Disconnect Event
            if (_disconnect != null)
            {
                _disconnect(_applicationId);
            }
        }

        /// <summary>
        /// Raised on Response Timer Elapse 
        /// </summary>
        private void OnResponseTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Response Timer elapsed for: " + _applicationId, _type.FullName, "OnResponseTimerElapsed");
            }

            // Raise Heartbeat Response Event
            if (_response != null)
            {
                _response(_serverHeartbeat);
            }
        }

        /// <summary>
        /// Starts Heartbeat Validation Timer
        /// </summary>
        private void StartValidationTimer()
        {
            //// Adjust Heartbeat validation timer
            //_disconnectTimer.Interval = _heartbeatInterval + _heartbeatValidationInterval;

            // Start Validation Timer
            _validationTimer.Start();
        }

        /// <summary>
        /// Stops Heatbeat validation timer
        /// </summary>
        private void StopValidationTimer()
        {
            // Stop Validation Timer
            _validationTimer.Stop();
        }

        /// <summary>
        /// Starts Heartbeat Validation Timer
        /// </summary>
        private void StartResponseTimer()
        {
            // Start Response Timer
            _responseTimer.Start();
        }

        /// <summary>
        /// Stops Heatbeat Response timer
        /// </summary>
        private void StopResponseTimer()
        {
            // Stop Validation Timer
            _responseTimer.Stop();
        }
    }
}
