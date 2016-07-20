using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.MarketDataProvider;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataProvider.Redi.Utility;
using TradeHub.MarketDataProvider.Redi.ValueObject;
using Constants = TradeHub.Common.Core.Constants;

namespace TradeHub.MarketDataProvider.Redi.Provider
{
    public class RediMarketDataProvider : ILiveTickDataProvider
    {
        private readonly Type _type = typeof(RediMarketDataProvider);

        private AsyncClassLogger _logger;

        private readonly string _marketDataProviderName;

        private object _lock = new object();

        /// <summary>
        /// Indicates if the Provider session is connected or not
        /// </summary>
        private bool _isConnected;

        /// <summary>
        /// Holds parameters required for connectivity
        /// </summary>
        private Credentials _credentials;
        
        private SocketCommunication _rediSocketConnection;

        /// <summary>
        /// Is Market Data client connected
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            return _isConnected;
        }

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
        /// Fired each time when market data rejection arrives.
        /// </summary>
        public event Action<MarketDataEvent> MarketDataRejectionArrived;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public RediMarketDataProvider()
        {
            // Set provider name to be used in all calls
            _marketDataProviderName = Constants.MarketDataProvider.Redi;

            // Create object for logging details
            _logger= new AsyncClassLogger("RediDataProvider");
            _logger.SetLoggingLevel();
            _logger.LogDirectory(Constants.DirectoryStructure.MDE_LOGS_LOCATION);
        }

        #region Connect/Disconnect

        /// <summary>
        /// Connects/Starts a client
        /// </summary>
        public bool Start()
        {
            try
            {
                // Read account credentials
                _credentials = CredentialReader.ReadCredentials("RediParams.xml");

                if (!CheckParameterValidity())
                {
                    return false;
                }

                // Create TCP Request
                _rediSocketConnection = new SocketCommunication(new Queue(), _credentials.Username,
                    _credentials.Password, _credentials.IpAddress, Convert.ToInt32(_credentials.Port), _logger);

                _rediSocketConnection.SendMessage += DataRecieved;
                _rediSocketConnection.ErrorInTcp += new SocketCommunication.ConnectionError(RediSocketConnectionErrorInTcp);
                
                _isConnected = true;
                _rediSocketConnection.Connect();

                if (_logger.IsInfoEnabled)
                {
                    _logger.Info("Session is available.", _type.FullName, "Start");
                }

                if (LogonArrived != null)
                {
                    LogonArrived(_marketDataProviderName);
                }

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
                _isConnected = false;
                _rediSocketConnection.Disconnect();
                
                if (LogoutArrived != null)
                {
                    LogoutArrived(_marketDataProviderName);
                }

                if (_logger.IsInfoEnabled)
                {
                    _logger.Info("Session closed.", _type.FullName, "Stop");
                }

                return true;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "Stop");
                return false;
            }
        }

        #endregion

        #region Subscribe Data

        /// <summary>
        /// Market data request message
        /// </summary>
        public bool SubscribeTickData(Subscribe subscribe)
        {
            try
            {
                // Send Request
                _rediSocketConnection.SubscribeMarketData(subscribe.Security.Symbol);

                return true;
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
        public bool UnsubscribeTickData(Unsubscribe unsubscribe)
        {
            try
            {
                // Send Request
                _rediSocketConnection.UnsubscribeMarketData(unsubscribe.Security.Symbol);

                return true;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "UnsubscribeTickData");
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Function fired on every message arrived
        /// </summary>
        /// <param name="dataMessage"></param>
        public void DataRecieved(string dataMessage)
        {
            try
            {
                lock (_lock)
                {
                    string data = dataMessage;
                    Tick tick = new Tick();
                    if (Regex.IsMatch(data, Parser.RegularExp("8")) && Regex.IsMatch(data, Parser.RegularExp("5")))
                    {
                        Match tradePriceMatch = Regex.Match(data, Parser.RegularExp("8"));
                        tick.LastPrice = Parser.DecimalValue(tradePriceMatch.Value);
                        Match symbolMatch = Regex.Match(data, Parser.RegularExp("5"));
                        tick.Security.Symbol = Parser.StringValue(symbolMatch.Value);
                        tick.DateTime = DateTime.Now;
                        tick.LastSize = 1;

                        if (TickArrived != null)
                        {
                            TickArrived(tick);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "DataRecieved");
            }
        }

        private void RediSocketConnectionErrorInTcp(bool connect)
        {
            if (!connect)
            {
                Stop();
            }
        }

        /// <summary>
        /// Checks if the parameters read are valid or not
        /// </summary>
        /// <returns></returns>
        private bool CheckParameterValidity()
        {
            if (String.IsNullOrEmpty(_credentials.Username) || String.IsNullOrWhiteSpace(_credentials.Password)
                || String.IsNullOrEmpty(_credentials.IpAddress) || String.IsNullOrWhiteSpace(_credentials.Port))
            {
                return false;
            }

            return true;
        }
    }
}
