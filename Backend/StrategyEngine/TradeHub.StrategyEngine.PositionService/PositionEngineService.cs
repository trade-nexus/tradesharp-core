using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.PositionEngine.Client.Service;

namespace TradeHub.StrategyEngine.PositionService
{
    /// <summary>
    /// Responsible for handling Position Queries
    /// </summary>
    public class PositionEngineService
    {
        private Type _type = typeof(PositionEngineService);

        // Class level logger
        private AsyncClassLogger _asyncClassLogger;

        /// <summary>
        /// Indicates whether the position engine service is connected to PE-Server or not
        /// </summary>
        private bool _isConnected = false;

        /// <summary>
        /// Communicates with Position Engine
        /// </summary>
        private readonly PositionEngineClient _positionEngineClient;

        #region Events

        // ReSharper disable InconsistentNaming
        private event Action _connected;
        private event Action _disconnected;
        private event Action<string> _logonArrived;
        private event Action<string> _logoutArrived;
        private event Action<Position> _positionArrived; 
        // ReSharper restore InconsistentNaming

        public event Action Connected
        {
            add
            {
                if (_connected == null)
                {
                    _connected += value;
                }
            }
            remove { _connected -= value; }
        }

        public event Action Disconnected
        {
            add
            {
                if (_disconnected == null)
                {
                    _disconnected += value;
                }
            }
            remove { _disconnected -= value; }
        }

        public event Action<string> LogonArrived
        {
            add
            {
                if (_logonArrived == null)
                {
                    _logonArrived += value;
                }
            }
            remove { _logonArrived -= value; }
        }

        public event Action<string> LogoutArrived
        {
            add
            {
                if (_logoutArrived == null)
                {
                    _logoutArrived += value;
                }
            }
            remove { _logoutArrived -= value; }
        }

        public event Action<Position> PositionArrived
        {
            add
            {
                if (_positionArrived == null)
                {
                    _positionArrived += value;
                }
            }
            remove { _positionArrived -= value; }
        } 

        #endregion

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="positionEngineClient">Client to communicates with Position Engine</param>
        public PositionEngineService(PositionEngineClient positionEngineClient)
        {
            _positionEngineClient = positionEngineClient;

            _asyncClassLogger = new AsyncClassLogger("PEServiceLogger");
            _asyncClassLogger.SetLoggingLevel();

            // Subscribe Client Events
            RegisterClientEvents();
        }

        #region Start/Stop

        /// <summary>
        /// Start Position Engine Service
        /// </summary>
        public bool StartService()
        {
            try
            {
                if (_isConnected)
                {
                    if (_asyncClassLogger.IsInfoEnabled)
                    {
                        _asyncClassLogger.Info("Position engine service already running.", _type.FullName, "StartService");
                    }

                    return true;
                }

                // Start PE-Client
                _positionEngineClient.Initialize();

                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Position engine service started.", _type.FullName, "StartService");
                }

                return true;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "StartService");
                return false;
            }
        }

        /// <summary>
        /// Stop Position Engine Service
        /// </summary>
        public bool StopService()
        {
            try
            {
                if (_positionEngineClient != null)
                {
                    // Stop PE-Client
                    _positionEngineClient.Shutdown();

                    // Unsubscribe Events
                    UnregisterClientEvents();
                }

                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Position engine service stopped.", _type.FullName, "StopService");
                }
                return true;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "StopService");
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Subscribe Position Engine Client Events
        /// </summary>
        private void RegisterClientEvents()
        {
            // Makes sure events are not hooked multiple times
            UnregisterClientEvents();

            // Subscribe events
            _positionEngineClient.ServerConnected += OnServerConnected;
            _positionEngineClient.PositionArrived += OnPositionArrived;
        }

        /// <summary>
        /// Un-Subscribe Position Engine Client Events
        /// </summary>
        private void UnregisterClientEvents()
        {
            // Un-Subscribe events
            _positionEngineClient.ServerConnected -= OnServerConnected;
            _positionEngineClient.PositionArrived -= OnPositionArrived;
        }
        
        #region PE-Client Events

        /// <summary>
        /// Called when connection is successful with PE-Server
        /// </summary>
        private void OnServerConnected()
        {
            try
            {
                // Toggle connection status value
                _isConnected = true;

                if (_asyncClassLogger.IsInfoEnabled)
                {
                    _asyncClassLogger.Info("Successfully connected with PE-Server.", _type.FullName, "OnServerConnected");
                }

                // Raise Event to notify listeners
                if (_connected != null)
                {
                    _connected();
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnServerConnected");
            }
        }

        /// <summary>
        /// Called when new Position Information is received
        /// </summary>
        /// <param name="position">Position</param>
        private void OnPositionArrived(Position position)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Info("New Position arrived " + position, _type.FullName, "OnPositionArrived");
                }

                // Raise Event to notify listeners
                if (_positionArrived != null)
                {
                    _positionArrived(position);
                }
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "OnPositionArrived");
            }
        }

        #endregion

        #region Incoming Requests for PE Server

        /// <summary>
        /// Sends Positon info request to PE-Server
        /// </summary>
        /// <param name="provider">Broker from which to receive position updates</param>
        /// <returns></returns>
        public bool Subscribe(string provider)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("New subscription request received", _type.FullName, "Subscribe");
                }

                // Check for existing connection
                if (_isConnected)
                {
                    // Forward Request to PE-Client
                    _positionEngineClient.SubscribeProviderPosition(provider);

                    return true;
                }

                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Request not sent to PE as PE-Client is not connected.", _type.FullName, "Subscribe");
                }
                return false;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "Subscribe");
                return false;
            }
        }

        /// <summary>
        /// Sends un-subscription request to PE-Server
        /// </summary>
        /// <param name="provider">Broker from which to receive position updates</param>
        /// <returns></returns>
        public bool UnSubscribe(string provider)
        {
            try
            {
                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("New un-subscription request received", _type.FullName, "UnSubscribe");
                }

                // Check for existing connection
                if (_isConnected)
                {
                    // Forward Request to PE-Client
                    _positionEngineClient.UnSubscribeProviderPosition(provider);

                    return true;
                }

                if (_asyncClassLogger.IsDebugEnabled)
                {
                    _asyncClassLogger.Debug("Request not sent to PE as PE-Client is not connected.", _type.FullName, "UnSubscribe");
                }
                return false;
            }
            catch (Exception exception)
            {
                _asyncClassLogger.Error(exception, _type.FullName, "UnSubscribe");
                return false;
            }
        }

        #endregion
    }
}
