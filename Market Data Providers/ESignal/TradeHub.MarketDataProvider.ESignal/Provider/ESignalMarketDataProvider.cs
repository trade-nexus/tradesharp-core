using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IESignal;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.MarketDataProvider;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataProvider.ESignal.ValueObjects;
using Constants = TradeHub.Common.Core.Constants;

namespace TradeHub.MarketDataProvider.ESignal.Provider
{
    public class ESignalMarketDataProvider :IMarketDataProvider, IHistoricBarDataProvider
    {
        private readonly Type _type = typeof(ESignalMarketDataProvider);

        private Hooks _session = null;
        private readonly ConnectionParameters _parameters;
        private readonly string _marketDataProviderName = Constants.MarketDataProvider.Blackwood;

        // Field to indicate User Logout request
        private bool _logoutRequest = false;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="parameters">Contains parametes to create connection</param>
        public ESignalMarketDataProvider(ConnectionParameters parameters)
        {
            _parameters = parameters;
        }

        #region Implementation of IMarketDataProvider

        public event Action<string> LogonArrived;
        public event Action<string> LogoutArrived;
        public event Action<MarketDataEvent> MarketDataRejectionArrived;

        /// <summary>
        /// Connects/Starts a client
        /// </summary>
        public bool Start()
        {
            try
            {
                // Toggle Field Value
                _logoutRequest = false;

                if (ConnectESignal())
                {
                    // Hook ESignal events
                    RegisterESignalEvents(true);

                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Session successfully created.", _type.FullName, "Start");
                    }

                    // Raise Event
                    if (LogonArrived!=null)
                    {
                        LogonArrived(_marketDataProviderName);
                    }

                    return true;
                }

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Session failed.", _type.FullName, "Start");
                }

                return false;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "Start");
                return false;
            }
        }

        /// <summary>
        /// Disconnects/Stops a client
        /// </summary>
        public bool Stop()
        {
            try
            {
                if (_session != null)
                {
                    // Toggle Field Value
                    _logoutRequest = true;

                    // Disconnect Session
                    DisconnectESignal();

                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Session disconnected successfully.", _type.FullName, "Stop");
                    }

                    // Raise Event
                    if (LogoutArrived != null)
                    {
                        LogoutArrived(_marketDataProviderName);
                    }
                }
                else
                {
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Session no longer exists.", _type.FullName, "Stop");
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "Stop");
                return false;
            }
        }

        /// <summary>
        /// Is Market Data client connected
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            // Check whether the Data session is connected or not
            if (_session != null)
            {
                return true;
            }
            return false;
        }

        #endregion

        #region Implementation of IHistoricBarDataProvider

        public event Action<HistoricBarData> HistoricBarDataArrived;

        /// <summary>
        /// Historic Bar Data Request Message
        /// </summary>
        public bool HistoricBarDataRequest(HistoricDataRequest historicDataRequest)
        {
            try
            {
                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "HistoricBarDataRequest");
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Creates ESignal Session
        /// </summary>
        private bool ConnectESignal()
        {
            try
            {
                // Create session object
                _session = new IESignal.Hooks();

                if (_session != null)
                {
                    // Add Username
                    _session.SetApplication(_parameters.UserName);
                    return true;
                }
                
                return false;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ConnectESignal");
                return false;
            }
        }

        /// <summary>
        /// Disconnects ESignal Session
        /// </summary>
        private bool DisconnectESignal()
        {
            try
            {
                // Kill ESignal Process
                KillProcess();
                _session = null;
                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "DisconnectESignal");
                return false;
            }
        }

        /// <summary>
        /// Hook/unhook ESignal events for martket data
        /// </summary>
        public void RegisterESignalEvents(bool connect)
        {
            try
            {
                // Hook Events if the connected is opened
                if (connect)
                {
                    // Unhook events
                    RemoveESignalEvents();

                    // Hook Events
                    AddESignalEvents();
                }
                // Unhook Events if the connection is closed
                else
                {
                    // Unhook events
                    RemoveESignalEvents();
                }
            }

            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "RegisterESignalEvents");
            }
        }

        /// <summary>
        /// Adds ESignal Events
        /// </summary>
        private void AddESignalEvents()
        {
            try
            {
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "AddESignalEvents");
            }
        }

        /// <summary>
        /// Remove Blackwood Events
        /// </summary>
        private void RemoveESignalEvents()
        {
            try
            {
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "RemoveESignalEvents");
            }
        }

        /// <summary>
        /// Kills processes that need to be restarted with every connection with eSignal
        /// </summary>
        public void KillProcess()
        {
            foreach (Process proc in Process.GetProcesses())
            {
                if (proc.ProcessName.StartsWith("WinSig") || proc.ProcessName.StartsWith("winros"))
                {
                    proc.Kill();
                }
            }
        }
    }
}
